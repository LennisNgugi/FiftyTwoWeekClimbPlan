using System.Text.Json;
using System.Text.Json.Serialization;

namespace FiftyTwoWeekClimbPlan.Models;

public class Position
{
    [JsonPropertyName("instrument")]
    public PositionInstrument? Instrument { get; set; }

    [JsonPropertyName("currentPrice")]
    public decimal CurrentPrice { get; set; }

    [JsonPropertyName("averagePricePaid")]
    public decimal AveragePricePaid { get; set; }

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>?
        Extra
    { get; set; }
}