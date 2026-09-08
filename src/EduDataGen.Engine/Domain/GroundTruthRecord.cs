namespace EduDataGen.Engine.Domain;

/// <summary>
/// Represents standard ground truth record for teacher evaluation and model validation.
/// </summary>
public class GroundTruthRecord
{
    public string Patient_ID { get; set; } = string.Empty;
    public bool Is_Patient { get; set; }
    public int Condition_Count { get; set; }
    public string Active_Conditions { get; set; } = "None";
    public string Severity_Details { get; set; } = "None";
    public string Primary_Diagnosis { get; set; } = "Healthy";
}
