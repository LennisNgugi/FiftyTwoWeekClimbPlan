using FiftyTwoWeekClimbPlan.Configuration;

namespace FiftyTwoWeekClimbPlan.Services;

public static class Quantity
{
    public static decimal RoundQuantity(
        decimal quantity,
        BotSettings settings)
    {
        decimal multiplier =
            (decimal)Math.Pow(
                10,
                settings.QuantityDecimalPlaces
            );

        return Math.Floor(
            quantity * multiplier
        ) / multiplier;
    }
}