using System.Text.Json;
using FiftyTwoWeekClimbPlan.Configuration;
using FiftyTwoWeekClimbPlan.Models;

namespace FiftyTwoWeekClimbPlan.Services;

public class ClerusyncService
{
    private readonly HttpClient httpClient;

    private readonly JsonSerializerOptions jsonOptions;

    private readonly BotSettings settings;


    public ClerusyncService(
        HttpClient httpClient,
        JsonSerializerOptions jsonOptions,
        BotSettings settings)
    {
        this.httpClient = httpClient;

        this.jsonOptions = jsonOptions;

        this.settings = settings;
    }


    public async Task<PriceMovementResponse>
        GetPriceMovementsAsync()
    {
        using HttpRequestMessage request =
            new HttpRequestMessage(
                HttpMethod.Get,
                settings.ClerusyncUrl
            );

        request.Headers.Add(
            "x-api-key",
            settings.ClerusyncApiKey
        );

        request.Headers.Add(
            "Accept",
            "application/json"
        );

        using HttpResponseMessage response =
            await httpClient.SendAsync(
                request
            );

        string content =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Clerusync error " +
                $"{(int)response.StatusCode}: " +
                $"{content}"
            );
        }

        PriceMovementResponse? result =
            JsonSerializer.Deserialize<
                PriceMovementResponse
            >(
                content,
                jsonOptions
            );

        return result
            ??
            new PriceMovementResponse();
    }
}