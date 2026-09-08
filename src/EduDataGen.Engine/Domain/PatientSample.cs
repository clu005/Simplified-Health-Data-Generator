using System.Globalization;

namespace EduDataGen.Engine.Domain;

/// <summary>
/// Represents a generated patient sample prior to CSV export.
/// </summary>
public class PatientSample
{
    public string PatientId { get; set; } = string.Empty;
    public bool IsPatient { get; set; }
    public List<ActiveConditionState> ActiveConditions { get; set; } = new();
    public Dictionary<string, double> MetricValues { get; set; } = new();
    public string PrimaryDiagnosis { get; set; } = "Healthy";

    /// <summary>
    /// Converts patient sample metrics into dynamic key-value dictionary for features table CSV output.
    /// </summary>
    public Dictionary<string, object?> ToFeatureDictionary(List<string>? metricOrder = null)
    {
        var dict = new Dictionary<string, object?>
        {
            ["Patient_ID"] = PatientId
        };

        if (metricOrder != null && metricOrder.Count > 0)
        {
            foreach (var key in metricOrder)
            {
                if (MetricValues.TryGetValue(key, out var val))
                {
                    dict[key] = val;
                }
            }
        }
        else
        {
            foreach (var kvp in MetricValues)
            {
                dict[kvp.Key] = kvp.Value;
            }
        }

        return dict;
    }

    /// <summary>
    /// Converts patient sample into GroundTruthRecord.
    /// </summary>
    public GroundTruthRecord ToGroundTruthRecord()
    {
        if (!IsPatient || ActiveConditions.Count == 0)
        {
            return new GroundTruthRecord
            {
                Patient_ID = PatientId,
                Is_Patient = false,
                Condition_Count = 0,
                Active_Conditions = "None",
                Severity_Details = "None",
                Primary_Diagnosis = "Healthy"
            };
        }

        string activeConditions = string.Join(";", ActiveConditions.Select(c => c.ConditionName));
        string severityDetails = string.Join(";", ActiveConditions.Select(c =>
            $"{c.ConditionName}:{c.SeverityFactor.ToString("0.00", CultureInfo.InvariantCulture)}"));

        return new GroundTruthRecord
        {
            Patient_ID = PatientId,
            Is_Patient = true,
            Condition_Count = ActiveConditions.Count,
            Active_Conditions = activeConditions,
            Severity_Details = severityDetails,
            Primary_Diagnosis = PrimaryDiagnosis
        };
    }
}
