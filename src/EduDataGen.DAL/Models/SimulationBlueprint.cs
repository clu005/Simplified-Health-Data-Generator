namespace EduDataGen.DAL.Models;

public class SimulationBlueprint
{
    public string Scenario { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Seed { get; set; }
    public int TotalRecords { get; set; }
    public string UnitSystem { get; set; } = "Metric";
    public CohortSettings CohortSettings { get; set; } = new();
    public Dictionary<string, BaselineMetric> BaselineMetrics { get; set; } = new();
    public Dictionary<string, ConditionDefinition> Conditions { get; set; } = new();
    public AnomalySettings Anomalies { get; set; } = new();
}

public class CohortSettings
{
    public double HealthyRatio { get; set; } = 0.5;
}

public class BaselineMetric
{
    public string Distribution { get; set; } = "Gaussian";
    public double Mean { get; set; }
    public double StdDev { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public string Unit { get; set; } = string.Empty;
    public MetricLimits Limits { get; set; } = new();
}

public class MetricLimits
{
    public double HardMin { get; set; }
    public double HardMax { get; set; }
    public double? SoftSaturationThreshold { get; set; }
}

public class ConditionDefinition
{
    public double OccurrenceProbability { get; set; }
    public int Priority { get; set; } = 0;
    public SeverityDistributionSettings SeverityDistribution { get; set; } = new();
    public Dictionary<string, ConditionEffect> Effects { get; set; } = new();
}

public class SeverityDistributionSettings
{
    public string Distribution { get; set; } = "Uniform";
    public double MinFactor { get; set; } = 0.2;
    public double MaxFactor { get; set; } = 1.0;
    public List<DiscreteGrade>? DiscreteGrades { get; set; }
}

public class DiscreteGrade
{
    public string GradeName { get; set; } = string.Empty;
    public double Factor { get; set; }
    public double Weight { get; set; }
}

public class ConditionEffect
{
    public double DeltaMean { get; set; }
    public double DeltaStdDev { get; set; } = 0;
    public string Mode { get; set; } = "Attenuated";
    public double Weight { get; set; } = 1.0;
}

public class AnomalySettings
{
    public double MissingValueRate { get; set; }
    public List<OutlierRule>? Outliers { get; set; }
    public List<TypoRule>? TypoInconsistencies { get; set; }
}

public class OutlierRule
{
    public string Field { get; set; } = string.Empty;
    public object? Value { get; set; }
    public int Count { get; set; }
}

public class TypoRule
{
    public string Field { get; set; } = string.Empty;
    public string OriginalValue { get; set; } = string.Empty;
    public List<string> CorruptedValues { get; set; } = new();
}
