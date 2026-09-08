using EduDataGen.DAL.Models;

namespace EduDataGen.Engine.Domain;

/// <summary>
/// Represents an active condition assigned to a generated patient sample.
/// </summary>
public class ActiveConditionState
{
    public string ConditionName { get; set; } = string.Empty;
    public double SeverityFactor { get; set; } = 1.0;
    public string? GradeName { get; set; }
    public int Priority { get; set; }
    public Dictionary<string, ConditionEffect> Effects { get; set; } = new();
}
