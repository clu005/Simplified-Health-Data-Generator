using EduDataGen.DAL.Models;
using EduDataGen.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EduDataGen.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlueprintsController : ControllerBase
{
    private readonly IBlueprintRepository _repository;

    public BlueprintsController(IBlueprintRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Lists all available blueprints and preset templates.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListBlueprints()
    {
        var blueprints = await _repository.ListBlueprintsAsync();
        var presets = await _repository.ListPresetsAsync();

        return Ok(new
        {
            Blueprints = blueprints,
            Presets = presets
        });
    }

    /// <summary>
    /// Gets a specific blueprint by file name.
    /// </summary>
    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetBlueprint(string fileName)
    {
        var blueprint = await _repository.LoadBlueprintAsync(fileName);
        if (blueprint == null)
        {
            return NotFound(new { error = $"Blueprint file '{fileName}' not found." });
        }

        return Ok(blueprint);
    }

    /// <summary>
    /// Saves a new or updated simulation blueprint.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SaveBlueprint([FromBody] SimulationBlueprint blueprint, [FromQuery] string? fileName = null)
    {
        if (blueprint == null || string.IsNullOrWhiteSpace(blueprint.Scenario))
        {
            return BadRequest(new { error = "Invalid blueprint payload or missing scenario name." });
        }

        await _repository.SaveBlueprintAsync(blueprint, fileName);
        string savedName = fileName ?? $"{blueprint.Scenario}_{blueprint.Seed}.json";

        return Ok(new
        {
            message = "Blueprint saved successfully.",
            fileName = savedName,
            blueprint
        });
    }
}
