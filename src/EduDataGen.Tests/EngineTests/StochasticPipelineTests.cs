using EduDataGen.DAL.Models;
using EduDataGen.Engine.Math;
using EduDataGen.Engine.Pipeline;
using FluentAssertions;

namespace EduDataGen.Tests.EngineTests;

public class StochasticPipelineTests
{
    [Fact]
    public void DeterminePathogenesis_HealthyRatio_DistributesHealthyVsDiseased()
    {
        var pipeline = new StochasticPipeline();
        var blueprint = new SimulationBlueprint
        {
            CohortSettings = new CohortSettings { HealthyRatio = 0.8 },
            Conditions = new Dictionary<string, ConditionDefinition>
            {
                ["Infection"] = new()
                {
                    OccurrenceProbability = 1.0,
                    Priority = 1,
                    SeverityDistribution = new SeverityDistributionSettings { MinFactor = 0.5, MaxFactor = 1.0 }
                }
            }
        };

        var sampler = new GaussianSampler(42);
        int healthyCount = 0;
        int patientCount = 0;

        for (int i = 0; i < 1000; i++)
        {
            var sample = pipeline.DeterminePathogenesis(blueprint, sampler, $"PAT-{i:D4}");
            if (!sample.IsPatient) healthyCount++;
            else patientCount++;
        }

        // Approximately 80% healthy (800) and 20% patient (200)
        healthyCount.Should().BeGreaterThan(700);
        patientCount.Should().BeGreaterThan(100);
    }

    [Fact]
    public void DeterminePathogenesis_AssignsFallbackCondition_IfDiseasedPoolHitsNoCondition()
    {
        var pipeline = new StochasticPipeline();
        var blueprint = new SimulationBlueprint
        {
            CohortSettings = new CohortSettings { HealthyRatio = 0.0 }, // 100% diseased pool
            Conditions = new Dictionary<string, ConditionDefinition>
            {
                ["RareCondition"] = new()
                {
                    OccurrenceProbability = 0.0, // 0% chance of activation in normal roll
                    Priority = 1,
                    SeverityDistribution = new SeverityDistributionSettings { MinFactor = 0.5, MaxFactor = 1.0 }
                }
            }
        };

        var sampler = new GaussianSampler(100);
        var sample = pipeline.DeterminePathogenesis(blueprint, sampler, "PAT-0001");

        sample.IsPatient.Should().BeTrue();
        sample.ActiveConditions.Should().HaveCount(1);
        sample.ActiveConditions[0].ConditionName.Should().Be("RareCondition");
        sample.PrimaryDiagnosis.Should().Be("RareCondition");
    }
}
