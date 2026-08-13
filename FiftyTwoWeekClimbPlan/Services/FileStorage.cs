using System.Text.Json;
using FiftyTwoWeekClimbPlan.Models;

namespace FiftyTwoWeekClimbPlan.Services;

public class FileStorage
{
    private readonly JsonSerializerOptions _jsonOptions =
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

    private readonly string _dataDirectory;
    private readonly string _tradesPath;
    private readonly string _cooldownsPath;

    public FileStorage()
    {
        _dataDirectory =
            Path.Combine(
            AppContext.BaseDirectory,
                "Data"
            );

        Directory.CreateDirectory(
            _dataDirectory
        );

        _tradesPath =
            Path.Combine(
                _dataDirectory,
                "bot-trades.json"
            );

        _cooldownsPath =
            Path.Combine(
                _dataDirectory,
                "stop-loss-cooldown.json"
            );
    }

    public async Task<List<Trade>>
        LoadTradesAsync()
    {
        if (!System.IO.File.Exists(_tradesPath))
        {
            return new List<Trade>();
        }

        string json =
            await System.IO.File.ReadAllTextAsync(
                _tradesPath
            );

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<Trade>();
        }

        return JsonSerializer.Deserialize<List<Trade>>(
            json,
            _jsonOptions
        ) ?? new List<Trade>();
    }

    public async Task SaveTradesAsync(
        List<Trade> trades)
    {
        string json =
            JsonSerializer.Serialize(
                trades,
                _jsonOptions
            );

        await System.IO.File.WriteAllTextAsync(
            _tradesPath,
            json
        );
    }

    public async Task<List<StopLossCooldown>>
        LoadStopLossCooldownsAsync()
    {
        if (!System.IO.File.Exists(_cooldownsPath))
        {
            return new List<StopLossCooldown>();
        }

        string json =
            await System.IO.File.ReadAllTextAsync(
                _cooldownsPath
            );

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<StopLossCooldown>();
        }

        return JsonSerializer.Deserialize<
            List<StopLossCooldown>
        >(
            json,
            _jsonOptions
        ) ?? new List<StopLossCooldown>();
    }

    public async Task SaveStopLossCooldownsAsync(
        List<StopLossCooldown> cooldowns)
    {
        string json =
            JsonSerializer.Serialize(
                cooldowns,
                _jsonOptions
            );

        await System.IO.File.WriteAllTextAsync(
            _cooldownsPath,
            json
        );
    }
}