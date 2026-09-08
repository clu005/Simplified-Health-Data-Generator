using EduDataGen.DAL.Models;
using EduDataGen.Engine.Domain;
using EduDataGen.Engine.Math;
using FluentAssertions;

namespace EduDataGen.Tests.EngineTests;

public class AttenuatedAccumulatorTests
{
    [Fact]
    public void CalculateMetricValue_AppliesNonLinearAttenuation_WhenMultipleConditionsPresent()
    {
        var accumulator = new AttenuatedAccumulator();
        var sampler = new GaussianSampler(555);

        var metric = new BaselineMetric
        {
            Distribution = "Gaussian",
            Mean = 37.0,
            StdDev = 0.0 // Zero std dev for exact deterministic check
        };

        var activeConditions = new List<ActiveConditionState>
        {
            new()
            {
                ConditionName = "Fever1",
                SeverityFactor = 1.0,
                Effects = new Dictionary<string, ConditionEffect>
                {
                    ["BodyTemp"] = new() { DeltaMean = 2.0, DeltaStdDev = 0.0, Mode = "Attenuated" }
                }
            },
            new()
            {
                ConditionName = "Fever2",
                SeverityFactor = 1.0,
                Effects = new Dictionary<string, ConditionEffect>
                {
                    ["BodyTemp"] = new() { DeltaMean = 1.0, DeltaStdDev = 0.0, Mode = "Attenuated" }
                }
            }
        };

        // Delta 1 = 2.0, Delta 2 = 1.0
        // Expected total delta = 2.0 + 1.0 * (0.35^1) = 2.35
        // Expected metric value = 37.0 + 2.35 = 39.35
        double calculated = accumulator.CalculateMetricValue("BodyTemp", metric, activeConditions, sampler, gamma: 0.35);

        calculated.Should().BeApproximately(39.35, 0.001);
    }

    [Fact]
    public void CalculateMetricValue_OverrideMode_OverridesOtherEffects()
    {
        var accumulator = new AttenuatedAccumulator();
        var sampler = new GaussianSampler(777);

        var metric = new BaselineMetric
        {
            Distribution = "Gaussian",
            Mean = 120.0,
            StdDev = 0.0
        };

        var activeConditions = new List<ActiveConditionState>
        {
            new()
            {
                ConditionName = "MildHypertension",
                Priority = 1,
                SeverityFactor = 1.0,
                Effects = new Dictionary<string, ConditionEffect>
                {
                    ["SystolicBP"] = new() { DeltaMean = 20.0, DeltaStdDev = 0.0, Mode = "Attenuated" }
                }
            },
            new()
            {
                ConditionName = "Shock",
                Priority = 10,
                SeverityFactor = 1.0,
                Effects = new Dictionary<string, ConditionEffect>
                {
                    ["SystolicBP"] = new() { DeltaMean = -50.0, DeltaStdDev = 0.0, Mode = "Override" }
                }
            }
        };

        // Expected = 120.0 + (-50.0) = 70.0
        double calculated = accumulator.CalculateMetricValue("SystolicBP", metric, activeConditions, sampler);

        calculated.Should().BeApproximately(70.0, 0.001);
    }
}
