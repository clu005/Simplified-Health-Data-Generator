namespace EduDataGen.ConnectedService.Gemini;

using System.Net.Http.Json;
using System.Text.Json;
using EduDataGen.DAL.Models;
using Microsoft.Extensions.Configuration;

public class GeminiBlueprintCompiler : IGeminiBlueprintCompiler
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GeminiBlueprintCompiler(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<SimulationBlueprint> CompileBlueprintAsync(
        string prompt,
        int? seed = null,
        int? totalRecords = null,
        double? healthyRatio = null,
        double? missingValueRate = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));
        }

        string? apiKey = _configuration["GEMINI_API_KEY"]
            ?? _configuration["Gemini:ApiKey"]
            ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Gemini API Key is missing. Please configure GEMINI_API_KEY environment variable or Gemini:ApiKey in configuration.");
        }

        string systemInstruction = """
        You are an expert Clinical Physiologist and Senior Data Architect.
        Your task is to generate a deterministic "Simulation Blueprint" JSON based on the user's scenario request.

        CRITICAL CONSTRAINTS:
        1. Always use the METRIC system (e.g., Celsius for temperature, kg for weight, cm/m for height, km/h for speed). Never use American imperial units under any circumstances.
        2. Establish a realistic baseline (Mean, StdDev, Limits) for the target demographic in BaselineMetrics.
        3. Configure stochastic cohort parameters: Specify a reasonable HealthyRatio (e.g., 0.3-0.5 for triage, 0.8 for general screenings) in CohortSettings.
        4. For each condition in Conditions, define its OccurrenceProbability among the diseased population, Priority, and define its SeverityDistribution (Distribution, MinFactor, MaxFactor).
        5. For diseased or altered conditions, specify non-linear Delta shifts and attenuation factors in Effects (DeltaMean, DeltaStdDev, Mode, Weight) to prevent biological impossibilities. Mode can be "Attenuated", "Linear", or "Override".
        6. Define pedagogically valuable anomalies for middle-school data cleaning exercises in Anomalies (MissingValueRate, Outliers, TypoInconsistencies).
        7. Output ONLY valid JSON matching the SimulationBlueprint schema.
        """;

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = $"Generate a simulation blueprint for scenario: {prompt}" }
                    }
                }
            },
            systemInstruction = new
            {
                parts = new[]
                {
                    new { text = systemInstruction }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json"
            }
        };

        string model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";
        string requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var response = await _httpClient.PostAsJsonAsync(requestUrl, requestBody, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Gemini API request failed with status code {response.StatusCode}: {errorContent}");
        }

        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var jsonDoc = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

        string? jsonText = jsonDoc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(jsonText))
        {
            throw new InvalidOperationException("Gemini API returned empty text response.");
        }

        jsonText = CleanJsonText(jsonText);

        var blueprint = JsonSerializer.Deserialize<SimulationBlueprint>(jsonText, JsonOptions);
        if (blueprint == null)
        {
            throw new InvalidOperationException("Failed to deserialize Gemini response to SimulationBlueprint.");
        }

        if (seed.HasValue)
        {
            blueprint.Seed = seed.Value;
        }
        else if (blueprint.Seed <= 0)
        {
            blueprint.Seed = 2026;
        }

        if (totalRecords.HasValue)
        {
            blueprint.TotalRecords = totalRecords.Value;
        }
        else if (blueprint.TotalRecords <= 0)
        {
            blueprint.TotalRecords = 100;
        }

        if (healthyRatio.HasValue)
        {
            blueprint.CohortSettings ??= new();
            blueprint.CohortSettings.HealthyRatio = healthyRatio.Value;
        }

        if (missingValueRate.HasValue)
        {
            blueprint.Anomalies ??= new();
            blueprint.Anomalies.MissingValueRate = missingValueRate.Value;
        }

        if (string.IsNullOrWhiteSpace(blueprint.Scenario))
        {
            blueprint.Scenario = "ai_generated_scenario";
        }

        return blueprint;
    }

    private static string CleanJsonText(string text)
    {
        text = text.Trim();
        if (text.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            text = text[7..];
        }
        else if (text.StartsWith("```"))
        {
            text = text[3..];
        }

        if (text.EndsWith("```"))
        {
            text = text[..^3];
        }

        return text.Trim();
    }
}
