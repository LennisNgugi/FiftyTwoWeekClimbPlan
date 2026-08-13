using System.Text.Json;
using System.Text.Json.Serialization;

namespace FiftyTwoWeekClimbPlan.Models;

public class OrderResponse
{
    [JsonPropertyName("ticker")]
    public string? Ticker { get; set; }

    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>?
        Extra
    { get; set; }
}