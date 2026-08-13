using DotNetEnv;

namespace FiftyTwoWeekClimbPlan.Configuration;

public class BotSettings
{
    // Investing Configuration

    /// How often the bot checks the market, Value is in milliseconds.
    public int CheckInterval { get; } = 60 * 1000;
    /// Profit percentage at which a position is sold  
    public decimal ProfitTarget { get; } = 5m;
    /// Loss percentage at which a position is sold   
    public decimal StopLoss { get; } = -20m;
    /// Number of hours a ticker remains blocked after a stop-loss sale.
    public int StopLossCooldownHours { get; } = 12;
    /// Approximate amount spent on each trade.
    public decimal BuyAmount { get; } = 5m;
    /// Number of decimal places supported for Trading212 quantities.
    public int QuantityDecimalPlaces { get; } = 3;
    /// Clerusync price movement API endpoint.    
    public string ClerusyncUrl { get; } = "https://clerusyncinvestment.com/public/api/pricemovement";

    // Environment Variables    
    /// Base URL used for the Trading212 API.
    public string BaseUrl { get; }
    /// Trading212 API key.
    public string ApiKey { get; }
    /// Trading212 API secret.
    public string ApiSecret { get; }
    /// Clerusync API key.    
    public string ClerusyncApiKey { get; }
    /// Trading212 equity API base URL.
    public string Trading212BaseUrl { get; }


    // Constructor
    public BotSettings()
    {
        // Locate .env
        string envFile =
            Path.Combine(
                AppContext.BaseDirectory,
                ".env"
            );


        // Validate .env
        if (!File.Exists(envFile))
        {
            throw new FileNotFoundException(
                $"Could not find .env file at: {envFile}"
            );
        }


        // Load .env
        Env.Load(
            envFile
        );


        // Environment Variables
        BaseUrl = GetRequiredEnvironmentVariable("BASE_URL").TrimEnd('/');
        ApiKey = GetRequiredEnvironmentVariable("API_KEY");
        ApiSecret = GetRequiredEnvironmentVariable("API_SECRET");
        ClerusyncApiKey = GetRequiredEnvironmentVariable("CLERUSYNC_API_KEY");

        // Trading212 URL
        Trading212BaseUrl = $"{BaseUrl}/equity";}


    // Environment Helper
    private static string
        GetRequiredEnvironmentVariable(
            string name)
    {
        string? value =
            Environment.GetEnvironmentVariable(
                name
            );


        if (string.IsNullOrWhiteSpace(value))
        {
            throw new Exception(
                $"Environment variable '{name}' is missing."
            );
        }


        return value;
    }
}