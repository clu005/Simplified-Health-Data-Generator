namespace EduDataGen.Engine.Domain;

/// <summary>
/// Holds the generated dataset result details and exported CSV paths.
/// </summary>
public class DatasetGenerationResult
{
    public string Scenario { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string FeaturesPath { get; set; } = string.Empty;
    public string GroundTruthPath { get; set; } = string.Empty;
    public List<Dictionary<string, object?>> FeaturesRecords { get; set; } = new();
    public List<GroundTruthRecord> GroundTruthRecords { get; set; } = new();
}
