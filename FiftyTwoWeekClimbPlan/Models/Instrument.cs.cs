using System.Text.Json;
using System.Text.Json.Serialization;

namespace FiftyTwoWeekClimbPlan.Models;

public class Instrument
{
    [JsonPropertyName("shortName")]
    public string? ShortName { get; set; }

    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = "";

    [JsonExtensionData]
    public Dictionary<string, JsonElement>?
        Extra
    { get; set; }
}