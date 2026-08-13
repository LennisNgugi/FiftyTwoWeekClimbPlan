using System.Text.Json;
using FiftyTwoWeekClimbPlan.Configuration;
using FiftyTwoWeekClimbPlan.Models;

namespace FiftyTwoWeekClimbPlan.Services;

public class TradingBot
{
    private readonly BotSettings settings;
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions;
    private readonly InstrumentService instrumentService;
    private readonly TradeStorageService tradeStorage;
    private readonly CooldownService cooldownService;
    private readonly ClerusyncService clerusyncService;
    private readonly Trading212Service trading212Service;

    // Constructor
    public TradingBot()
    {
        settings =
            new BotSettings();

        httpClient =
            new HttpClient();

        jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,

                WriteIndented = true
            };


        instrumentService =
            new InstrumentService(
                jsonOptions
            );

        tradeStorage =
            new TradeStorageService(
                jsonOptions
            );

        cooldownService =
            new CooldownService(
                jsonOptions,
                settings
            );

        clerusyncService =
            new ClerusyncService(
                httpClient,
                jsonOptions,
                settings
            );

        trading212Service =
            new Trading212Service(
                httpClient,
                jsonOptions,
                settings
            );
    }


    
    // Start
    public async Task StartAsync()
    {
        Console.WriteLine();
        Console.WriteLine("=====================================");
        Console.WriteLine("Fifty Two Week Climb Plan Bot");
        Console.WriteLine("=====================================");
        Console.WriteLine($"BASE_URL: {settings.BaseUrl}");
        Console.WriteLine($"Trading212 API: " + $"{settings.Trading212BaseUrl}");
        Console.WriteLine($"Buy amount: " + $"£{settings.BuyAmount}");
        Console.WriteLine($"Profit target: " + $"{settings.ProfitTarget}%");
        Console.WriteLine($"Stop loss: " + $"{settings.StopLoss}%");
        Console.WriteLine($"Check interval: " + $"{settings.CheckInterval / 1000} seconds");
        Console.WriteLine();


        await instrumentService.LoadAsync();


        Console.WriteLine("Investing bot started");

        await RunAsync();


        using PeriodicTimer timer =
            new PeriodicTimer(
                TimeSpan.FromMilliseconds(
                    settings.CheckInterval
                )
            );


        while (
            await timer.WaitForNextTickAsync()
        )
        {
            await RunAsync();
        }
    }

    // Run
    private async Task RunAsync()
    {
        Console.WriteLine();

        Console.WriteLine(
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
            "Checking market..."
        );


        try
        {
            await CheckBuySignalsAsync();

            await CheckProfitTargetsAsync();
        }
        catch (Exception error)
        {
            Console.WriteLine(
                $"Trading cycle error: " +
                $"{error}"
            );
        }


        Console.WriteLine(
            "Cycle complete"
        );
    }


    
    // Buy Signals
    private async Task CheckBuySignalsAsync()
    {
        PriceMovementResponse movement =
            await clerusyncService
                .GetPriceMovementsAsync();


        List<Trade> trades =
            await tradeStorage.LoadAsync();


        HashSet<string> owned =
            trades
                .Select(
                    trade =>
                        trade.Ticker
                )
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase
                );


        foreach (
            PriceMovement stock
            in movement.Changes
                ??
                new List<PriceMovement>())
        {
            if (
                string.IsNullOrWhiteSpace(
                    stock.Ticker
                )
            )
            {
                Console.WriteLine(
                    "Skipping movement without ticker"
                );

                continue;
            }


            // New high
            if (
                stock.NewHigh <=
                stock.OldHigh
            )
            {
                continue;
            }


            // Instrument
            if (
                !instrumentService.TryGet(
                    stock.Ticker,
                    out Instrument? instrument
                )
            )
            {
                Console.WriteLine(
                    $"No Trading212 ticker " +
                    $"for {stock.Ticker}"
                );

                continue;
            }


            // Cooldown
            if (
                await cooldownService
                    .IsOnCooldownAsync(
                        instrument.Ticker
                    )
            )
            {
                Console.WriteLine(
                    $"{instrument.Ticker} " +
                    "blocked by stop loss cooldown"
                );

                continue;
            }


            // Already owned
            if (
                owned.Contains(
                    instrument.Ticker
                )
            )
            {
                Console.WriteLine(
                    $"{instrument.Ticker} " +
                    "already owned by bot"
                );

                continue;
            }


            decimal price =
                stock.NewHigh;


            if (price <= 0)
            {
                Console.WriteLine(
                    $"Invalid Clerusync price " +
                    $"for {stock.Ticker}: {price}"
                );

                continue;
            }

            // Buy
            OrderResponse? order =
                await trading212Service
                    .CreateBuyOrderAsync(
                        instrument.Ticker,
                        price
                    );


            // Save trade
            if (order != null)
            {
                decimal quantity =
                    order.Quantity
                    ??
                    Quantity.RoundQuantity(
                        settings.BuyAmount / price,
                        settings
                    );


                string ticker =
                    order.Ticker
                    ??
                    instrument.Ticker;


                trades.Add(
                    new Trade
                    {
                        Ticker =
                            ticker,

                        Quantity =
                            quantity,

                        BoughtAt =
                            DateTimeOffset.UtcNow
                    }
                );


                await tradeStorage.SaveAsync(
                    trades
                );


                owned.Add(
                    ticker
                );


                Console.WriteLine(
                    $"{ticker} trade saved"
                );
            }
        }
    }


    
    // Profit Targets
    private async Task
        CheckProfitTargetsAsync()
    {
        List<Trade> trades =
            await tradeStorage.LoadAsync();


        if (trades.Count == 0)
        {
            Console.WriteLine(
                "No bot trades"
            );

            return;
        }


        List<Position> positions =
            await trading212Service
                .GetPositionsAsync();


        List<Trade> remaining =
            new List<Trade>();


        foreach (
            Trade trade
            in trades)
        {
            Position? position =
                positions.FirstOrDefault(
                    p =>
                        p.Instrument != null
                        &&
                        string.Equals(
                            p.Instrument.Ticker,
                            trade.Ticker,
                            StringComparison.OrdinalIgnoreCase
                        )
                );


            if (position == null)
            {
                Console.WriteLine(
                    $"{trade.Ticker} " +
                    "waiting for position"
                );

                remaining.Add(
                    trade
                );

                continue;
            }


            decimal currentPrice =
                position.CurrentPrice;

            decimal buyPrice =
                position.AveragePricePaid;


            if (
                currentPrice <= 0
                ||
                buyPrice <= 0
            )
            {
                Console.WriteLine(
                    $"Invalid position pricing " +
                    $"for {trade.Ticker}"
                );

                remaining.Add(
                    trade
                );

                continue;
            }


            decimal profit =
                (
                    (
                        currentPrice -
                        buyPrice
                    )
                    /
                    buyPrice
                )
                *
                100m;


            Console.WriteLine(
                $"{trade.Ticker}: " +
                $"{profit:F2}%"
            );

            // TAKE PROFIT
            if (
                profit >=
                settings.ProfitTarget
            )
            {
                Console.WriteLine(
                    $"{trade.Ticker} reached " +
                    $"+{settings.ProfitTarget}% profit"
                );


                bool sold =
                    await trading212Service
                        .CreateSellOrderAsync(
                            trade.Ticker,
                            position.Quantity
                        );


                if (!sold)
                {
                    remaining.Add(
                        trade
                    );
                }
                else
                {
                    Console.WriteLine(
                        $"{trade.Ticker} sold successfully - " +
                        "available for new buys"
                    );
                }


                continue;
            }

            // STOP LOSS
            if (
                profit <=
                settings.StopLoss
            )
            {
                Console.WriteLine(
                    $"{trade.Ticker} hit stop loss " +
                    $"{settings.StopLoss}%"
                );


                bool sold =
                    await trading212Service
                        .CreateSellOrderAsync(
                            trade.Ticker,
                            position.Quantity
                        );


                if (sold)
                {
                    await cooldownService.AddAsync(
                        trade.Ticker
                    );


                    Console.WriteLine(
                        $"{trade.Ticker} blocked for " +
                        $"{settings.StopLossCooldownHours} hours"
                    );
                }
                else
                {
                    remaining.Add(
                        trade
                    );
                }


                continue;
            }


            // Still holding
            remaining.Add(
                trade
            );
        }


        await tradeStorage.SaveAsync(
            remaining
        );
    }
}
