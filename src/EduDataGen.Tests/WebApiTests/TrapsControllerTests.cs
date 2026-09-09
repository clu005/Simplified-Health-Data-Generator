using EduDataGen.DAL.Exporters;
using EduDataGen.DAL.Models;
using EduDataGen.DAL.Workspace;
using EduDataGen.Engine.Traps;
using EduDataGen.WebAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace EduDataGen.Tests.WebApiTests;

public class TrapsControllerTests
{
    [Fact]
    public async Task InjectTraps_GroundTruthFilePassed_ReturnsBadRequest()
    {
        string tempWorkspace = Path.Combine(Path.GetTempPath(), "EduDataGen_TrapsTest_" + Guid.NewGuid().ToString("N"));
        var workspaceManager = new WorkspaceManager(tempWorkspace);
        workspaceManager.InitializeWorkspace();

        var exporter = new DualTableCsvExporter(workspaceManager);
        var injector = new PedagogicalTrapInjector();
        var controller = new TrapsController(exporter, workspaceManager, injector);

        var request = new InjectTrapsRequest
        {
            SourceFileName = "medical_triage_ground_truth.csv"
        };

        var result = await controller.InjectTraps(request);
        var badRequest = result as BadRequestObjectResult;
        badRequest.Should().NotBeNull();

        if (Directory.Exists(tempWorkspace))
        {
            Directory.Delete(tempWorkspace, true);
        }
    }

    [Fact]
    public async Task InjectTraps_ValidFeaturesFile_InjectsTrapsAndSavesCorruptedFile()
    {
        string tempWorkspace = Path.Combine(Path.GetTempPath(), "EduDataGen_TrapsTest_" + Guid.NewGuid().ToString("N"));
        var workspaceManager = new WorkspaceManager(tempWorkspace);
        workspaceManager.InitializeWorkspace();

        var exporter = new DualTableCsvExporter(workspaceManager);
        var injector = new PedagogicalTrapInjector();
        var controller = new TrapsController(exporter, workspaceManager, injector);

        // Prepare sample features CSV
        var featuresRecords = new List<Dictionary<string, object?>>
        {
            new() { { "Patient_ID", "PAT-0001" }, { "Body_Temp_C", "36.8" }, { "Heart_Rate", "72" } },
            new() { { "Patient_ID", "PAT-0002" }, { "Body_Temp_C", "37.1" }, { "Heart_Rate", "80" } }
        };
        var gtRecords = new List<Dictionary<string, object?>>
        {
            new() { { "Patient_ID", "PAT-0001" }, { "Is_Patient", false } },
            new() { { "Patient_ID", "PAT-0002" }, { "Is_Patient", true } }
        };

        await exporter.ExportDualTableAsync("medical_triage", featuresRecords, gtRecords, "20260329_120000");

        var request = new InjectTrapsRequest
        {
            SourceFileName = "medical_triage_20260329_120000_features.csv",
            TargetFileName = "medical_triage_20260329_120000_corrupted_features.csv",
            Anomalies = new AnomalySettings
            {
                Outliers = new List<OutlierRule>
                {
                    new OutlierRule { Field = "Body_Temp_C", Value = "99.0", Count = 1 }
                }
            }
        };

        var result = await controller.InjectTraps(request);
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();

        string corruptedPath = workspaceManager.GetSanitizedPath("datasets", "medical_triage_20260329_120000_corrupted_features.csv");
        File.Exists(corruptedPath).Should().BeTrue();

        var corruptedData = await exporter.ReadCsvAsDictionariesAsync(corruptedPath);
        corruptedData.Count.Should().Be(2);
        corruptedData.Any(r => r["Body_Temp_C"] == "99.0").Should().BeTrue();

        if (Directory.Exists(tempWorkspace))
        {
            Directory.Delete(tempWorkspace, true);
        }
    }
}
