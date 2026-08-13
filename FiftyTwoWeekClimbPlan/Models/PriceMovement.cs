using System.Text.Json.Serialization;

namespace FiftyTwoWeekClimbPlan.Models;

public class PriceMovement
{
    [JsonPropertyName("ticker")]
    public string? Ticker { get; set; }

    [JsonPropertyName("newHigh")]
    public decimal NewHigh { get; set; }

    [JsonPropertyName("oldHigh")]
    public decimal OldHigh { get; set; }
}