using EduDataGen.DAL.Models;
using EduDataGen.DAL.Repositories;
using EduDataGen.DAL.Workspace;
using EduDataGen.WebAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace EduDataGen.Tests.WebApiTests;

public class BlueprintsControllerTests
{
    [Fact]
    public async Task Controller_ListAndGetAndSaveBlueprint_FunctionsCorrectly()
    {
        string testWorkspace = Path.Combine(Path.GetTempPath(), "EduDataGen_WebAPI_BlueprintTest_" + Guid.NewGuid().ToString("N"));
        var workspaceManager = new WorkspaceManager(testWorkspace);
        workspaceManager.InitializeWorkspace();

        var repo = new BlueprintRepository(workspaceManager);
        var controller = new BlueprintsController(repo);

        var blueprint = new SimulationBlueprint
        {
            Scenario = "api_test_scenario",
            Seed = 1001,
            TotalRecords = 50,
            CohortSettings = new CohortSettings { HealthyRatio = 0.6 }
        };

        // 1. Save blueprint
        var saveResult = await controller.SaveBlueprint(blueprint, "api_test.json");
        var okSaveResult = saveResult as OkObjectResult;
        okSaveResult.Should().NotBeNull();

        // 2. List blueprints
        var listResult = await controller.ListBlueprints();
        var okListResult = listResult as OkObjectResult;
        okListResult.Should().NotBeNull();

        // 3. Get blueprint
        var getResult = await controller.GetBlueprint("api_test.json");
        var okGetResult = getResult as OkObjectResult;
        okGetResult.Should().NotBeNull();
        var loadedBlueprint = okGetResult!.Value as SimulationBlueprint;
        loadedBlueprint.Should().NotBeNull();
        loadedBlueprint!.Scenario.Should().Be("api_test_scenario");

        // Cleanup
        if (Directory.Exists(testWorkspace))
        {
            Directory.Delete(testWorkspace, true);
        }
    }
}
