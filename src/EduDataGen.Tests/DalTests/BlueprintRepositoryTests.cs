using EduDataGen.DAL.Models;
using EduDataGen.DAL.Repositories;
using EduDataGen.DAL.Workspace;
using FluentAssertions;
using Xunit;

namespace EduDataGen.Tests.DalTests;

public class BlueprintRepositoryTests : IDisposable
{
    private readonly string _tempWorkspaceDir;
    private readonly WorkspaceManager _workspaceManager;
    private readonly BlueprintRepository _repository;

    public BlueprintRepositoryTests()
    {
        _tempWorkspaceDir = Path.Combine(Path.GetTempPath(), "EduDataGen_RepoTest_" + Guid.NewGuid());
        _workspaceManager = new WorkspaceManager(_tempWorkspaceDir);
        _repository = new BlueprintRepository(_workspaceManager);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempWorkspaceDir))
        {
            Directory.Delete(_tempWorkspaceDir, true);
        }
    }

    [Fact]
    public async Task SaveAndLoadBlueprint_ShouldPersistAndDeserializeCorrectly()
    {
        // Arrange
        var blueprint = new SimulationBlueprint
        {
            Scenario = "medical_triage",
            Seed = 2026,
            TotalRecords = 100,
            UnitSystem = "Metric",
            CohortSettings = new CohortSettings { HealthyRatio = 0.4 },
            BaselineMetrics = new Dictionary<string, BaselineMetric>
            {
                ["Body_Temp_C"] = new BaselineMetric
                {
                    Distribution = "Gaussian",
                    Mean = 36.8,
                    StdDev = 0.4,
                    Unit = "°C",
                    Limits = new MetricLimits { HardMin = 30.0, HardMax = 45.0 }
                }
            }
        };

        // Act
        await _repository.SaveBlueprintAsync(blueprint);
        var loaded = await _repository.LoadBlueprintAsync("medical_triage_2026.json");

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Scenario.Should().Be("medical_triage");
        loaded.Seed.Should().Be(2026);
        loaded.TotalRecords.Should().Be(100);
        loaded.CohortSettings.HealthyRatio.Should().Be(0.4);
        loaded.BaselineMetrics.Should().ContainKey("Body_Temp_C");
        loaded.BaselineMetrics["Body_Temp_C"].Mean.Should().Be(36.8);
    }

    [Fact]
    public async Task ListBlueprintsAsync_ShouldReturnSavedBlueprintFiles()
    {
        // Arrange
        var blueprint = new SimulationBlueprint { Scenario = "youth_soccer", Seed = 123 };
        await _repository.SaveBlueprintAsync(blueprint, "youth_soccer_123.json");

        // Act
        var blueprints = await _repository.ListBlueprintsAsync();

        // Assert
        blueprints.Should().Contain("youth_soccer_123.json");
    }
}
