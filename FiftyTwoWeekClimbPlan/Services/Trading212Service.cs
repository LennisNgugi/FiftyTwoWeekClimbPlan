using System.Text;
using System.Text.Json;
using FiftyTwoWeekClimbPlan.Configuration;
using FiftyTwoWeekClimbPlan.Models;

namespace FiftyTwoWeekClimbPlan.Services;

public class Trading212Service
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions;
    private readonly BotSettings settings;

    public Trading212Service(
        HttpClient httpClient,
        JsonSerializerOptions jsonOptions,
        BotSettings settings)
    {
        this.httpClient = httpClient;
        this.jsonOptions = jsonOptions;
        this.settings = settings;
    }

    // Authentication Headers
    private Dictionary<string, string>
        Trading212Headers()
    {
        string credentials =
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{settings.ApiKey}:{settings.ApiSecret}"
                )
            );

        return new Dictionary<string, string>
        {
            ["Authorization"] =
                $"Basic {credentials}",

            ["Accept"] =
                "application/json"
        };
    }


    // Get Positions
    public async Task<List<Position>>
        GetPositionsAsync()
    {
        using HttpRequestMessage request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"{settings.Trading212BaseUrl}/positions"
            );

        foreach (
            KeyValuePair<string, string> header
            in Trading212Headers())
        {
            request.Headers.TryAddWithoutValidation(
                header.Key,
                header.Value
            );
        }

        using HttpResponseMessage response =
            await httpClient.SendAsync(
                request
            );

        string content =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(content);

            throw new Exception(
                $"Positions error " +
                $"{(int)response.StatusCode}"
            );
        }

        return
            JsonSerializer.Deserialize<List<Position>>(
                content,
                jsonOptions
            )
            ??
            new List<Position>();
    }


    // Buy
    public async Task<OrderResponse?>
        CreateBuyOrderAsync(
            string ticker,
            decimal price)
    {
        decimal numericPrice =
            price;


        if (numericPrice <= 0)
        {
            Console.WriteLine(
                $"Cannot buy {ticker}: " +
                $"invalid price {price}"
            );

            return null;
        }


        Console.WriteLine(
            $"BUYING approximately £" +
            $"{settings.BuyAmount} OF {ticker} " +
            $"using reference price £" +
            $"{numericPrice}"
        );


        // Calculate quantity
        decimal rawQuantity =
            settings.BuyAmount /
            numericPrice;

        Console.WriteLine(
            $"Raw quantity: {rawQuantity}"
        );


        decimal quantity =
            Quantity.RoundQuantity(
                rawQuantity,
                settings
            );

        Console.WriteLine(
            $"Rounded quantity: {quantity}"
        );


        if (quantity <= 0)
        {
            Console.WriteLine(
                $"Invalid quantity calculated " +
                $"for {ticker}"
            );

            return null;
        }


        decimal estimatedValue =
            quantity *
            numericPrice;

        Console.WriteLine(
            $"Estimated order value: " +
            $"£{estimatedValue:F2}"
        );


        // Order
        var orderBody =
            new
            {
                ticker,

                quantity,

                extendedHours = false
            };

        string json =
            JsonSerializer.Serialize(
                orderBody
            );


        using HttpRequestMessage request =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"{settings.Trading212BaseUrl}/orders/market"
            );


        foreach (
            KeyValuePair<string, string> header
            in Trading212Headers())
        {
            request.Headers.TryAddWithoutValidation(
                header.Key,
                header.Value
            );
        }


        request.Content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );


        using HttpResponseMessage response =
            await httpClient.SendAsync(
                request
            );


        string responseText =
            await response.Content.ReadAsStringAsync();


        Console.WriteLine(
            $"Trading212 response: " +
            $"{responseText}"
        );


        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(
                $"Buy order failed for {ticker}: " +
                $"HTTP {(int)response.StatusCode}"
            );

            return null;
        }


        Console.WriteLine(
            $"Buy order accepted for {ticker}"
        );


        try
        {
            return
                JsonSerializer.Deserialize<
                    OrderResponse
                >(
                    responseText,
                    jsonOptions
                );
        }
        catch
        {
            return new OrderResponse
            {
                Ticker = ticker,

                Quantity = quantity
            };
        }
    }


    // Sell
    public async Task<bool>
        CreateSellOrderAsync(
            string ticker,
            decimal quantity)
    {
        Console.WriteLine(
            $"SELLING {ticker} " +
            $"quantity {quantity}"
        );


        if (quantity <= 0)
        {
            Console.WriteLine(
                $"Invalid sell quantity " +
                $"for {ticker}: {quantity}"
            );

            return false;
        }


        decimal roundedQuantity =
            Quantity.RoundQuantity(
                quantity,
                settings
            );


        if (roundedQuantity <= 0)
        {
            Console.WriteLine(
                $"Rounded sell quantity " +
                $"is zero for {ticker}"
            );

            return false;
        }


        var orderBody =
            new
            {
                ticker,

                quantity =
                    -Math.Abs(
                        roundedQuantity
                    ),

                extendedHours = false
            };


        string json =
            JsonSerializer.Serialize(
                orderBody
            );


        using HttpRequestMessage request =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"{settings.Trading212BaseUrl}/orders/market"
            );


        foreach (
            KeyValuePair<string, string> header
            in Trading212Headers())
        {
            request.Headers.TryAddWithoutValidation(
                header.Key,
                header.Value
            );
        }


        request.Content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );


        using HttpResponseMessage response =
            await httpClient.SendAsync(
                request
            );


        string responseText =
            await response.Content.ReadAsStringAsync();


        Console.WriteLine(
            $"Trading212 response: " +
            $"{responseText}"
        );


        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(
                $"Sell order failed for {ticker}: " +
                $"HTTP {(int)response.StatusCode}"
            );

            return false;
        }


        Console.WriteLine(
            $"Sell order accepted for {ticker}"
        );

        return true;
    }
}