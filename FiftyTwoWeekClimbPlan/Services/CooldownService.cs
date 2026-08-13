using System.Text.Json;
using FiftyTwoWeekClimbPlan.Configuration;
using FiftyTwoWeekClimbPlan.Models;

namespace FiftyTwoWeekClimbPlan.Services;

public class CooldownService
{
    
    // Fields
    private readonly JsonSerializerOptions jsonOptions;

    private readonly BotSettings settings;

    // Constructor
    public CooldownService(
        JsonSerializerOptions jsonOptions,
        BotSettings settings)
    {
        this.jsonOptions = jsonOptions;

        this.settings = settings;
    }

    
    // File Path
    private string GetCooldownPath()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "stop-loss-cooldown.json"
        );
    }


    // Load Cooldowns
    public async Task<List<StopLossCooldown>>
        LoadAsync()
    {
        try
        {
            string path =
                GetCooldownPath();


            
            // File does not exist
            if (!File.Exists(path))
            {
                return new List<StopLossCooldown>();
            }


            
            // Read file
            string json =
                await File.ReadAllTextAsync(
                    path
                );


            
            // Deserialize
            return
                JsonSerializer.Deserialize<
                    List<StopLossCooldown>
                >(
                    json,
                    jsonOptions
                )
                ??
                new List<StopLossCooldown>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not load " + $"stop-loss-cooldown.json: " + $"{ex.Message}");

            return new List<StopLossCooldown>();
        }
    }


    
    // Save Cooldowns
    public async Task SaveAsync(
        List<StopLossCooldown> data)
    {
        string json =
            JsonSerializer.Serialize(
                data,
                jsonOptions
            );


        await File.WriteAllTextAsync(
            GetCooldownPath(),
            json
        );
    }


    
    // Check Cooldown
    public async Task<bool>
        IsOnCooldownAsync(
            string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            return false;
        }


        List<StopLossCooldown> cooldowns =
            await LoadAsync();


        StopLossCooldown? entry =
            cooldowns.FirstOrDefault(
                item =>
                    string.Equals(
                        item.Ticker,
                        ticker,
                        StringComparison.OrdinalIgnoreCase
                    )
            );


        
        // No cooldown entry
        if (entry == null)
        {
            return false;
        }


        
        // Cooldown expired
        if (
            entry.BlockedUntil
            <=
            DateTimeOffset.UtcNow
        )
        {
            return false;
        }


        
        // Still blocked
        return true;
    }


    
    // Add Stop Loss Cooldown
    public async Task AddAsync(
        string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            return;
        }


        ticker =
            ticker.Trim();


        List<StopLossCooldown> cooldowns =
            await LoadAsync();


        
        // Calculate expiry
        DateTimeOffset blockedUntil =
            DateTimeOffset.UtcNow.AddHours(
                settings.StopLossCooldownHours
            );

    
        // Find existing entry
        StopLossCooldown? existing =
            cooldowns.FirstOrDefault(
                item =>
                    string.Equals(
                        item.Ticker,
                        ticker,
                        StringComparison.OrdinalIgnoreCase
                    )
            );


        // Update existing
        if (existing != null)
        {
            existing.BlockedUntil =
                blockedUntil;
        }

 
        // Create new
        else
        {
            cooldowns.Add(
                new StopLossCooldown
                {
                    Ticker =
                        ticker,

                    BlockedUntil =
                        blockedUntil
                }
            );
        }


        // Save
        await SaveAsync(
            cooldowns
        );


        Console.WriteLine($"{ticker} stop loss cooldown " + 
            $"until {blockedUntil:yyyy-MM-dd HH:mm:ss} UTC"
        );
    }


    
    // Remove Expired Cooldowns
    public async Task RemoveExpiredAsync()
    {
        List<StopLossCooldown> cooldowns =
            await LoadAsync();


        DateTimeOffset now =
            DateTimeOffset.UtcNow;


        int originalCount =
            cooldowns.Count;


        cooldowns =
            cooldowns
                .Where(
                    item =>
                        item.BlockedUntil > now
                )
                .ToList();


        if (
            cooldowns.Count !=
            originalCount
        )
        {
            await SaveAsync(
                cooldowns
            );
        }
    }
}