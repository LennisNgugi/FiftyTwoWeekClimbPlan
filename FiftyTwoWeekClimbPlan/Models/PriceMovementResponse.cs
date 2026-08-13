using System.Text.Json.Serialization;

namespace FiftyTwoWeekClimbPlan.Models;

public class PriceMovementResponse
{
    [JsonPropertyName("changes")]
    public List<PriceMovement>? Changes { get; set; }
}