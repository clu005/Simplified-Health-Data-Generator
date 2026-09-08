using EduDataGen.DAL.Exporters;
using EduDataGen.DAL.Models;
using EduDataGen.DAL.Workspace;
using EduDataGen.Engine;
using EduDataGen.Engine.Clamping;
using EduDataGen.Engine.Math;
using EduDataGen.Engine.Pipeline;
using FluentAssertions;

namespace EduDataGen.Tests.EngineTests;

public class DatasetGeneratorTests
{
    [Fact]
    public async Task RenderDatasetAsync_GeneratesReproducibleDualTableDatasets_AndAppliesAnomaliesToFeaturesOnly()
    {
        string testWorkspace = Path.Combine(Path.GetTempPath(), "EduDataGen_Engine_Test_" + Guid.NewGuid().ToString("N"));
        var workspaceManager = new WorkspaceManager(testWorkspace);
        workspaceManager.InitializeWorkspace();

        var exporter = new DualTableCsvExporter(workspaceManager);
        var pipeline = new StochasticPipeline();
        var accumulator = new AttenuatedAccumulator();
        var clamper = new PhysiologicalClamper();

        var generator = new DatasetGenerator(pipeline, accumulator, clamper, exporter);

        var blueprint = new SimulationBlueprint
        {
            Scenario = "medical_triage_test",
            Seed = 2026,
            TotalRecords = 20,
            CohortSettings = new CohortSettings { HealthyRatio = 0.5 },
            BaselineMetrics = new Dictionary<string, BaselineMetric>
            {
                ["BodyTemp"] = new()
                {
                    Distribution = "Gaussian",
                    Mean = 36.8,
                    StdDev = 0.4,
                    Limits = new MetricLimits { HardMin = 34.0, HardMax = 42.0 }
                }
            },
            Conditions = new Dictionary<string, ConditionDefinition>
            {
                ["Fever"] = new()
                {
                    OccurrenceProbability = 0.8,
                    Priority = 1,
                    SeverityDistribution = new SeverityDistributionSettings { MinFactor = 0.5, MaxFactor = 1.0 },
                    Effects = new Dictionary<string, ConditionEffect>
                    {
                        ["BodyTemp"] = new() { DeltaMean = 2.0, DeltaStdDev = 0.2, Mode = "Attenuated" }
                    }
                }
            },
            Anomalies = new AnomalySettings
            {
                Outliers = new List<OutlierRule>
                {
                    new() { Field = "BodyTemp", Value = 99.0, Count = 1 }
                }
            }
        };

        var result1 = await generator.RenderDatasetAsync(blueprint, overrideSeed: 2026, customTimestamp: "run1");
        var result2 = await generator.RenderDatasetAsync(blueprint, overrideSeed: 2026, customTimestamp: "run2");

        // Ground Truth should be 100% reproducible for same seed
        result1.GroundTruthRecords.Should().HaveCount(20);
        result2.GroundTruthRecords.Should().HaveCount(20);

        for (int i = 0; i < 20; i++)
        {
            result1.GroundTruthRecords[i].Patient_ID.Should().Be(result2.GroundTruthRecords[i].Patient_ID);
            result1.GroundTruthRecords[i].Is_Patient.Should().Be(result2.GroundTruthRecords[i].Is_Patient);
            result1.GroundTruthRecords[i].Active_Conditions.Should().Be(result2.GroundTruthRecords[i].Active_Conditions);
        }

        // Outlier 99.0 or 99 should exist in Features table
        var outlierFeatureRow = result1.FeaturesRecords.FirstOrDefault(f => f.ContainsKey("BodyTemp") && f["BodyTemp"]?.ToString()?.StartsWith("99") == true);
        outlierFeatureRow.Should().NotBeNull();

        // Cleanup
        if (Directory.Exists(testWorkspace))
        {
            Directory.Delete(testWorkspace, true);
        }
    }
}
