using EduDataGen.DAL.Exporters;
using EduDataGen.DAL.Workspace;
using EduDataGen.WebAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace EduDataGen.Tests.WebApiTests;

public class AssetsControllerTests
{
    [Fact]
    public async Task AssetsController_ListAndPreviewDatasets_FunctionsCorrectly()
    {
        string tempWorkspace = Path.Combine(Path.GetTempPath(), "EduDataGen_AssetsTest_" + Guid.NewGuid().ToString("N"));
        var workspaceManager = new WorkspaceManager(tempWorkspace);
        workspaceManager.InitializeWorkspace();

        var exporter = new DualTableCsvExporter(workspaceManager);
        var controller = new AssetsController(workspaceManager, exporter);

        // 1. Create a pair of CSV files
        var featuresRecords = new List<Dictionary<string, object?>>
        {
            new() { { "Patient_ID", "PAT-0001" }, { "Body_Temp_C", "36.8" } }
        };
        var gtRecords = new List<Dictionary<string, object?>>
        {
            new() { { "Patient_ID", "PAT-0001" }, { "Primary_Diagnosis", "Healthy" } }
        };

        await exporter.ExportDualTableAsync("assets_test", featuresRecords, gtRecords, "20260329_150000");

        // 2. Test ListDatasets
        var listResult = controller.ListDatasets();
        var okListResult = listResult as OkObjectResult;
        okListResult.Should().NotBeNull();

        // 3. Test PreviewDataset
        var previewResult = await controller.PreviewDataset("assets_test_20260329_150000_features.csv", limit: 5);
        var okPreviewResult = previewResult as OkObjectResult;
        okPreviewResult.Should().NotBeNull();

        if (Directory.Exists(tempWorkspace))
        {
            Directory.Delete(tempWorkspace, true);
        }
    }
}
