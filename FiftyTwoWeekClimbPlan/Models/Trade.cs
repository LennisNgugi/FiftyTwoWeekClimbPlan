using System.Text.Json.Serialization;

namespace FiftyTwoWeekClimbPlan.Models;

public class Trade
{
    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = "";

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("boughtAt")]
    public DateTimeOffset BoughtAt { get; set; }
}