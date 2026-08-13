using System.Text.Json;
using FiftyTwoWeekClimbPlan.Configuration;
using FiftyTwoWeekClimbPlan.Models;

namespace FiftyTwoWeekClimbPlan.Services;

public class ClerusyncClient
{
    private const string Url = "https://clerusyncinvestment.com/public/api/pricemovement";
    private readonly HttpClient _httpClient;
    private readonly BotSettings _settings;

    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

    public ClerusyncClient(
        BotSettings settings)
    {
        _settings = settings;

        _httpClient =
            new HttpClient();
    }

    public async Task<PriceMovementResponse>
        GetPriceMovementsAsync()
    {
        using HttpRequestMessage request =
            new HttpRequestMessage(
                HttpMethod.Get,
                Url
            );

        request.Headers.Add(
            "x-api-key",
            _settings.ClerusyncApiKey
        );

        request.Headers.Add(
            "Accept",
            "application/json"
        );

        using HttpResponseMessage response =
            await _httpClient.SendAsync(
                request
            );

        string content =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Clerusync error " + $"{(int)response.StatusCode}: " + $"{content}");
        }

        return
            JsonSerializer.Deserialize<
                PriceMovementResponse
            >(
                content,
                _jsonOptions
            )
            ??
            new PriceMovementResponse();
    }
}