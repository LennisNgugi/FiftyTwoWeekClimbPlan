using System.Text.Json.Serialization;

namespace FiftyTwoWeekClimbPlan.Models;

public class StopLossCooldown
{
    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = "";

    [JsonPropertyName("blockedUntil")]
    public DateTimeOffset BlockedUntil { get; set; }
}