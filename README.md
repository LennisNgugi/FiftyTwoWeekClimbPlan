# Fifty Two Week Climb Plan

A .NET 10 console application that monitors price-movement signals from Clerusync API and automatically places buy and sell orders through the Trading 212 API.

> **Warning:** This software can place real orders. Use it at your own risk and test before using a live Trading 212 account.

## Function

The bot runs continuously and checks the market every **60 seconds**.

### Buy Signal

A buy signal is generated when:

* Clerusync reports a stock making a new high.
* The stock exists in `instruments.json`.
* The stock is not already tracked by the bot.
* The stock is not within a stop-loss cooldown period.

The bot attempts to invest approximately **£5 per order**.

### Sell Rules

The bot monitors open positions and automatically sells when:

* **Profit target:** `+5%`
* **Stop loss:** `-20%`

After a stop-loss sale, the ticker is placed on a **12-hour cooldown** before another buy can be made.

## Configuration

Create a `.env` file with the file path `FiftyTwoWeekClimbPlan\.env` the following environment variables are required in file:

```text
BASE_URL=your-trading212-api-base-url
API_KEY=your-trading212-api-key
API_SECRET=your-trading212-api-secret
CLERUSYNC_API_KEY=your-clerusync-investment-api-key
```

The application expects `.env` to be copied to:

```text
bin\Debug\net10.0\.env
```
The application expects `bot-trades.json` to be copied to:

```text
bin\Debug\net10.0\bot-trades.json
```
The application expects `stop-loss-cooldown.json` to be copied to:

```text
bin\Debug\net10.0\stop-loss-cooldown.json
```
The application expects `instruments.json` to be copied to:

```text
bin\Debug\net10.0\instruments.json
```
## Packages Dependencies

```dependencies
    DotNetEnv
    Microsoft.Extensions.Configuration
    Microsoft.Extensions.Configuration.EnvironmentVariables
    Microsoft.Extensions.Configuration.Json
    Microsoft.Net.Http.Headers
    System.Net.Http
```
The solution requires the following NuGet packages:
 
| Package                                                   | Purpose                                                    |
| --------------------------------------------------------- | ---------------------------------------------------------- |
| `DotNetEnv`                                               | Loads configuration values from the `.env` file            |
| `Microsoft.Extensions.Configuration`                      | Provides application configuration support                 |
| `Microsoft.Extensions.Configuration.EnvironmentVariables` | Allows configuration to be read from environment variables |
| `Microsoft.Extensions.Configuration.Json`                 | Allows configuration to be read from JSON files            |
| `Microsoft.Net.Http.Headers`                              | Provides HTTP header functionality                         |
| `System.Net.Http`                                         | Provides HTTP client functionality for API communication   | 

## Investing Configuration

The main investing parameters are defined in the BotSettings class of `BotSettings.cs`:

```csharp
    public int CheckIntervalSeconds { get; } = 60;
    public decimal ProfitTarget { get; } = 5m;
    public decimal StopLoss { get; } = -20m;
    public int StopLossCooldownHours { get; } = 12;
    public decimal BuyAmount { get; } = 5m;
    public int QuantityDecimalPlaces { get; } = 3;
```

These control:

| Setting            |      Value | Description                       |
| ------------------ | ---------: | --------------------------------- |
| Check interval     | 60 seconds | Time between market checks        |
| Profit target      |        +5% | Sells when this profit is reached |
| Stop loss          |       -20% | Sells when this loss is reached   |
| Stop-loss cooldown |   12 hours | Stops immediate re-entry          |
| Buy amount         |         £5 | Approximate amount per order      |
| Quantity precision | 3 decimals | Trading212 quantity rounding      | 

## Required Files

### `Program.cs`

Starts the bot with program.cs and the main application logic is contained in TradingBot.cs.
Loading the env file, creating the bot configuration, API communication, file storage, 
passes the required services into the investing bot. Also handles unexpected errors, 
displaying them on console rather than immediately closing the application.

### `instruments.json`

Contains the Trading 212 instrument list used to match Clerusync tickers with Trading 212 tickers. 
(Due to copyright laws, I can not publish Trading212 instruments list used for maping short name and trading212 tickers from a json file. However, the list can be accessed via:)

```text
curl -X GET "https://live.trading212.com/api/v0/equity/metadata/instruments" \
  -u "YOUR_API_KEY:YOUR_API_SECRET" \
  -H "Accept: application/json"
```

### `bot-trades.json`

Stores orders opened by the bot so that it can continue monitoring them after restarting.

### `stop-loss-cooldown.json`

Stores tickers that are temporarily blocked after hitting the stop loss.

## Running the Bot

Build and run the application from Visual Studio or the command line:

```text
dotnet run
```

On startup the bot displays its configuration and loads the available Trading 212 instruments.

It then performs an immediate market check followed by checks every 60 seconds.

Example:

```text
=====================================
Fifty Two Week Climb Plan Bot
=====================================
BASE_URL: ...
Trading212 API: ...
Buy amount: £5
Profit target: 5%
Stop loss: -20%
Check interval: 60s

Loaded 15534 instruments
Trading bot started
```

## Important Considerations

The bot uses the **Clerusync api `newHigh` price as the reference price** when calculating the quantity to purchase. The actual Trading 212 execution price may differ.

The bot also relies on Trading 212's reported `currentPrice`, `averagePricePaid` and `quantity` when calculating profit and creating sell orders.

Trading 212 may reject orders because of instrument restrictions, minimum position sizes, available funds, market conditions or other API rules. Failed orders are logged to the console and the order remains tracked by the bot.

## Disclaimer

This project is provided for educational and automation purposes. It is an automated investing system and can result in financial losses, would recommend testing in demo before live environment BaseUrl to verify the Trading 212 API behaviour.

## Future Improvements

Fix minimum-position/quantity handling: Some tickets have sell limits fix using error handling message. 

Dashboard/monitoring: View positions, signals, orders and bot health remotely.

Add a database: SQLite instead of JSON files might speed up application.

Use Renewable solar energy: Add bash script that automates running on raspberry pi. 
