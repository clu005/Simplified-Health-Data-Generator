using EduDataGen.DAL.Exporters;
using EduDataGen.DAL.Models;
using EduDataGen.DAL.Repositories;
using EduDataGen.DAL.Workspace;
using EduDataGen.Engine;
using EduDataGen.Engine.Clamping;
using EduDataGen.Engine.Math;
using EduDataGen.Engine.Pipeline;
using EduDataGen.WebAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace EduDataGen.Tests.WebApiTests;

public class DatasetsControllerTests
{
    [Fact]
    public async Task RenderDataset_LoadsBlueprintAndGeneratesCSV_ReturnsPreviewAndPaths()
    {
        string testWorkspace = Path.Combine(Path.GetTempPath(), "EduDataGen_WebAPI_DatasetTest_" + Guid.NewGuid().ToString("N"));
        var workspaceManager = new WorkspaceManager(testWorkspace);
        workspaceManager.InitializeWorkspace();

        var repo = new BlueprintRepository(workspaceManager);
        var exporter = new DualTableCsvExporter(workspaceManager);
        var pipeline = new StochasticPipeline();
        var accumulator = new AttenuatedAccumulator();
        var clamper = new PhysiologicalClamper();
        var generator = new DatasetGenerator(pipeline, accumulator, clamper, exporter);

        var controller = new DatasetsController(repo, generator, workspaceManager);

        // Save a test blueprint first
        var blueprint = new SimulationBlueprint
        {
            Scenario = "render_api_test",
            Seed = 2026,
            TotalRecords = 10,
            CohortSettings = new CohortSettings { HealthyRatio = 0.5 },
            BaselineMetrics = new Dictionary<string, BaselineMetric>
            {
                ["Age"] = new() { Distribution = "IntegerUniform", Mean = 12, Min = 10, Max = 15, Limits = new MetricLimits { HardMin = 5, HardMax = 20 } }
            }
        };
        await repo.SaveBlueprintAsync(blueprint, "render_test.json");

        // Call render endpoint
        var request = new RenderDatasetRequest
        {
            FileName = "render_test.json",
            OverrideTotalRecords = 15,
            OverrideSeed = 2026
        };

        var actionResult = await controller.RenderDataset(request);
        var okResult = actionResult as OkObjectResult;
        okResult.Should().NotBeNull();

        // Check list endpoint
        var listResult = controller.ListDatasets() as OkObjectResult;
        listResult.Should().NotBeNull();

        // Cleanup
        if (Directory.Exists(testWorkspace))
        {
            Directory.Delete(testWorkspace, true);
        }
    }
}
