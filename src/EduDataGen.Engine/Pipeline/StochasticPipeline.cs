using EduDataGen.DAL.Models;
using EduDataGen.Engine.Domain;
using EduDataGen.Engine.Math;

namespace EduDataGen.Engine.Pipeline;

public class StochasticPipeline : IStochasticPipeline
{
    public PatientSample DeterminePathogenesis(SimulationBlueprint blueprint, IGaussianSampler sampler, string patientId)
    {
        var sample = new PatientSample
        {
            PatientId = patientId,
            IsPatient = false,
            PrimaryDiagnosis = "Healthy"
        };

        double healthyRatio = blueprint.CohortSettings?.HealthyRatio ?? 0.5;
        double r1 = sampler.NextDouble();

        // Tier 1: Healthy vs Diseased Roll
        if (r1 < healthyRatio)
        {
            return sample;
        }

        sample.IsPatient = true;

        if (blueprint.Conditions == null || blueprint.Conditions.Count == 0)
        {
            return sample;
        }

        // Tier 2 & Tier 3: Phenotypic Condition Roll & Severity Factor Sampling
        foreach (var (conditionName, conditionDef) in blueprint.Conditions)
        {
            double r2 = sampler.NextDouble();
            if (r2 < conditionDef.OccurrenceProbability)
            {
                double severityFactor = sampler.SampleSeverityFactor(conditionDef.SeverityDistribution);
                string? gradeName = GetGradeNameForFactor(conditionDef.SeverityDistribution, severityFactor);

                sample.ActiveConditions.Add(new ActiveConditionState
                {
                    ConditionName = conditionName,
                    SeverityFactor = severityFactor,
                    GradeName = gradeName,
                    Priority = conditionDef.Priority,
                    Effects = conditionDef.Effects ?? new()
                });
            }
        }

        // Fallback: If no condition was activated in diseased pool, assign mandatory condition
        if (sample.ActiveConditions.Count == 0)
        {
            var fallback = blueprint.Conditions
                .OrderBy(c => c.Value.Priority)
                .ThenByDescending(c => c.Value.OccurrenceProbability)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(fallback.Key))
            {
                double severityFactor = sampler.SampleSeverityFactor(fallback.Value.SeverityDistribution);
                string? gradeName = GetGradeNameForFactor(fallback.Value.SeverityDistribution, severityFactor);

                sample.ActiveConditions.Add(new ActiveConditionState
                {
                    ConditionName = fallback.Key,
                    SeverityFactor = severityFactor,
                    GradeName = gradeName,
                    Priority = fallback.Value.Priority,
                    Effects = fallback.Value.Effects ?? new()
                });
            }
        }

        // Determine Primary Diagnosis (Highest Priority, then highest SeverityFactor)
        var primaryCondition = sample.ActiveConditions
            .OrderByDescending(c => c.Priority)
            .ThenByDescending(c => c.SeverityFactor)
            .FirstOrDefault();

        if (primaryCondition != null)
        {
            sample.PrimaryDiagnosis = primaryCondition.ConditionName;
        }

        return sample;
    }

    private static string? GetGradeNameForFactor(SeverityDistributionSettings settings, double severityFactor)
    {
        if (settings.DiscreteGrades != null && settings.DiscreteGrades.Count > 0)
        {
            var matchedGrade = settings.DiscreteGrades
                .OrderBy(g => System.Math.Abs(g.Factor - severityFactor))
                .FirstOrDefault();
            return matchedGrade?.GradeName;
        }
        return null;
    }
}
