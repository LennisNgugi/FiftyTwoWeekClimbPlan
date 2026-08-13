using System.Text.Json;
using System.Text.Json.Serialization;

namespace FiftyTwoWeekClimbPlan.Models;

public class PositionInstrument
{
    [JsonPropertyName("ticker")]
    public string? Ticker { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>?
        Extra
    { get; set; }
}