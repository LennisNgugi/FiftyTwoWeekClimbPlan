using System.Text.Json;
using FiftyTwoWeekClimbPlan.Models;

namespace FiftyTwoWeekClimbPlan.Services;

public class InstrumentService
{
    private readonly Dictionary<string, Instrument>
        instrumentMap =
            new(StringComparer.OrdinalIgnoreCase);

    private readonly JsonSerializerOptions
        jsonOptions;


    public InstrumentService(
        JsonSerializerOptions jsonOptions)
    {
        this.jsonOptions = jsonOptions;
    }


    
    // Load Instruments
    public async Task LoadAsync()
    {
        string instrumentsPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "instruments.json"
            );

        if (!File.Exists(instrumentsPath))
        {
            throw new FileNotFoundException(
                $"instruments.json was not found at: {instrumentsPath}"
            );
        }

        string json =
            await File.ReadAllTextAsync(
                instrumentsPath
            );

        List<Instrument>? instruments =
            JsonSerializer.Deserialize<List<Instrument>>(
                json,
                jsonOptions
            );

        if (instruments == null)
        {
            throw new Exception(
                "Unable to parse instruments.json"
            );
        }

        foreach (
            Instrument instrument
            in instruments)
        {
            if (
                !string.IsNullOrWhiteSpace(
                    instrument.ShortName
                )
            )
            {
                instrumentMap[
                    instrument.ShortName.ToUpperInvariant()
                ] = instrument;
            }
        }

        Console.WriteLine(
            $"Loaded {instrumentMap.Count} instruments"
        );
    }


    
    // Find Instrument
    public bool TryGet(
        string shortName,
        out Instrument? instrument)
    {
        return instrumentMap.TryGetValue(
            shortName.ToUpperInvariant(),
            out instrument
        );
    }
}