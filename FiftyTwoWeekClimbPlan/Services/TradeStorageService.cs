using System.Text.Json;
using FiftyTwoWeekClimbPlan.Models;

namespace FiftyTwoWeekClimbPlan.Services;

public class TradeStorageService
{
    private readonly JsonSerializerOptions
        jsonOptions;


    public TradeStorageService(
        JsonSerializerOptions jsonOptions)
    {
        this.jsonOptions = jsonOptions;
    }


    // File Path
    private string GetTradesPath()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "bot-trades.json"
        );
    }


    // Load Trades
    public async Task<List<Trade>> LoadAsync()
    {
        try
        {
            string path =
                GetTradesPath();

            if (!File.Exists(path))
            {
                return new List<Trade>();
            }

            string json =
                await File.ReadAllTextAsync(
                    path
                );

            return JsonSerializer.Deserialize<List<Trade>>(
                json,
                jsonOptions
            )
            ??
            new List<Trade>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Could not load bot-trades.json: " +
                $"{ex.Message}"
            );

            return new List<Trade>();
        }
    }


    // Save Trades
    public async Task SaveAsync(
        List<Trade> trades)
    {
        string json =
            JsonSerializer.Serialize(
                trades,
                jsonOptions
            );

        await File.WriteAllTextAsync(
            GetTradesPath(),
            json
        );
    }
}