using System.Net;
using System.Text;
using System.Text.Json;
using FiftyTwoWeekClimbPlan.Configuration;
using FiftyTwoWeekClimbPlan.Models;

namespace FiftyTwoWeekClimbPlan.Services;

public class Trading212Client
{
    private readonly BotSettings _settings;

    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonOptions =
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

    public Trading212Client(
        BotSettings settings)
    {
        _settings = settings;

        _httpClient =
            new HttpClient();
    }

    private Dictionary<string, string>
        GetHeaders()
    {
        string credentials =
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{_settings.ApiKey}:{_settings.ApiSecret}"
                )
            );

        return new Dictionary<string, string>
        {
            ["Authorization"] = $"Basic {credentials}",

            ["Accept"] = "application/json"
        };
    }

    private void AddHeaders(
        HttpRequestMessage request)
    {
        foreach (
            KeyValuePair<string, string> header
            in GetHeaders())
        {
            request.Headers.TryAddWithoutValidation(
                header.Key,
                header.Value
            );
        }
    }

    public async Task<bool>
        TestAuthenticationAsync()
    {
        using HttpRequestMessage request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"{_settings.Trading212BaseUrl}/positions"
            );

        AddHeaders(request);

        using HttpResponseMessage response =
            await _httpClient.SendAsync(
                request
            );

        string content =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Trading212 authentication HTTP status: " + $"{(int)response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Trading212 authentication failed: " + $"{content}");

            return false;
        }

        Console.WriteLine("Trading212 authentication successful.");

        return true;
    }

    public async Task<List<Position>>
        GetPositionsAsync()
    {
        using HttpRequestMessage request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"{_settings.Trading212BaseUrl}/positions"
            );

        AddHeaders(request);

        using HttpResponseMessage response =
            await _httpClient.SendAsync(
                request
            );

        string content =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Positions error " +
                $"{(int)response.StatusCode}: " +
                $"{content}"
            );
        }

        return
            JsonSerializer.Deserialize<
                List<Position>
            >(
                content,
                _jsonOptions
            )
            ??
            new List<Position>();
    }

    public async Task<OrderResponse?>
        CreateBuyOrderAsync(
            string ticker,
            decimal price)
    {
        if (price <= 0)
        {
            Console.WriteLine($"Invalid price for {ticker}: {price}");

            return null;
        }

        decimal rawQuantity =
            _settings.BuyAmount /
            price;

        decimal quantity =
            RoundQuantity(
                rawQuantity
            );

        if (quantity <= 0)
        {
            Console.WriteLine($"Calculated quantity is zero " + $"for {ticker}");

            return null;
        }

        decimal estimatedValue =
            quantity *
            price;

        Console.WriteLine($"BUYING approximately £" + $"{_settings.BuyAmount} OF {ticker}");
        Console.WriteLine($"Reference price: £{price}");
        Console.WriteLine($"Quantity: {quantity}");
        Console.WriteLine($"Estimated value: £{estimatedValue:F2}");

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
                $"{_settings.Trading212BaseUrl}/orders/market"
            );

        AddHeaders(request);

        request.Content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

        using HttpResponseMessage response =
            await _httpClient.SendAsync(
                request
            );

        string responseText =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Trading212 order HTTP status: " + $"{(int)response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Buy order failed for {ticker}: " + $"HTTP {(int)response.StatusCode}");

            Console.WriteLine($"Response: {responseText}");

            return null;
        }

        Console.WriteLine($"Buy order accepted for {ticker}");

        try
        {
            return JsonSerializer.Deserialize<
                OrderResponse
            >(
                responseText,
                _jsonOptions
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

    public async Task<bool>
        CreateSellOrderAsync(
            string ticker,
            decimal quantity)
    {
        decimal roundedQuantity =
            RoundQuantity(
                quantity
            );

        if (roundedQuantity <= 0)
        {
            Console.WriteLine($"Invalid sell quantity for {ticker}");

            return false;
        }

        Console.WriteLine($"SELLING {ticker} " + $"quantity {roundedQuantity}");

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
                $"{_settings.Trading212BaseUrl}/orders/market"
            );

        AddHeaders(request);

        request.Content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

        using HttpResponseMessage response =
            await _httpClient.SendAsync(
                request
            );

        string responseText =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Trading212 sell HTTP status: " + $"{(int)response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Sell order failed for {ticker}: " + $"HTTP {(int)response.StatusCode}");

            Console.WriteLine($"Response: {responseText}");

            return false;
        }

        Console.WriteLine($"Sell order accepted for {ticker}");

        return true;
    }

    private decimal RoundQuantity(
        decimal quantity)
    {
        decimal multiplier =
            (decimal)Math.Pow(
                10,
                _settings.QuantityDecimalPlaces
            );

        return
            Math.Floor(
                quantity * multiplier
            )
            /
            multiplier;
    }
}