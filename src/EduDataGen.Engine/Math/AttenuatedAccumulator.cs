using EduDataGen.DAL.Models;
using EduDataGen.Engine.Domain;

namespace EduDataGen.Engine.Math;

public class AttenuatedAccumulator : IAttenuatedAccumulator
{
    public double CalculateMetricValue(
        string metricName,
        BaselineMetric metric,
        List<ActiveConditionState> activeConditions,
        IGaussianSampler sampler,
        double gamma = 0.35)
    {
        // 1. Sample baseline metric value
        double baseVal = SampleBaselineValue(metric, sampler);

        if (activeConditions == null || activeConditions.Count == 0)
        {
            return baseVal;
        }

        // 2. Gather active effects on this metric
        var relevantEffects = activeConditions
            .Where(c => c.Effects != null && c.Effects.ContainsKey(metricName))
            .Select(c => new
            {
                Condition = c,
                Effect = c.Effects[metricName]
            })
            .ToList();

        if (relevantEffects.Count == 0)
        {
            return baseVal;
        }

        // 3. Check for Override Mode
        var overrideEffect = relevantEffects
            .Where(e => e.Effect.Mode != null && e.Effect.Mode.Equals("Override", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.Condition.Priority)
            .ThenByDescending(e => e.Condition.SeverityFactor)
            .FirstOrDefault();

        if (overrideEffect != null)
        {
            double s = overrideEffect.Condition.SeverityFactor;
            double overrideDelta = (overrideEffect.Effect.DeltaMean * s) +
                                   sampler.SampleGaussian(0, overrideEffect.Effect.DeltaStdDev * s);
            return baseVal + overrideDelta;
        }

        // 4. Calculate Linear & Attenuated deltas
        double linearDeltaSum = 0;
        List<double> attenuatedDeltas = new();

        foreach (var item in relevantEffects)
        {
            double s = item.Condition.SeverityFactor;
            double delta = (item.Effect.DeltaMean * s) +
                           sampler.SampleGaussian(0, item.Effect.DeltaStdDev * s);

            if (item.Effect.Mode != null && item.Effect.Mode.Equals("Linear", StringComparison.OrdinalIgnoreCase))
            {
                linearDeltaSum += delta;
            }
            else
            {
                attenuatedDeltas.Add(delta);
            }
        }

        // 5. Apply Non-Linear Attenuation: Delta_total = Delta_(1) + sum_{i=2}^N (Delta_(i) * gamma^(i-1))
        double totalAttenuatedDelta = 0;
        if (attenuatedDeltas.Count > 0)
        {
            var sortedDeltas = attenuatedDeltas
                .OrderByDescending(d => System.Math.Abs(d))
                .ToList();

            totalAttenuatedDelta = sortedDeltas[0];
            for (int i = 1; i < sortedDeltas.Count; i++)
            {
                totalAttenuatedDelta += sortedDeltas[i] * System.Math.Pow(gamma, i);
            }
        }

        return baseVal + linearDeltaSum + totalAttenuatedDelta;
    }

    private static double SampleBaselineValue(BaselineMetric metric, IGaussianSampler sampler)
    {
        string dist = metric.Distribution ?? "Gaussian";

        if (dist.Equals("Uniform", StringComparison.OrdinalIgnoreCase))
        {
            double min = metric.Min ?? (metric.Mean - metric.StdDev);
            double max = metric.Max ?? (metric.Mean + metric.StdDev);
            return sampler.SampleUniform(min, max);
        }

        if (dist.Equals("IntegerUniform", StringComparison.OrdinalIgnoreCase))
        {
            int min = (int)(metric.Min ?? metric.Mean);
            int max = (int)(metric.Max ?? metric.Mean);
            return sampler.SampleIntegerUniform(min, max);
        }

        // Default: Gaussian
        return sampler.SampleGaussian(metric.Mean, metric.StdDev);
    }
}
