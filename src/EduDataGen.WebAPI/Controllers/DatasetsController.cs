using EduDataGen.DAL.Repositories;
using EduDataGen.DAL.Workspace;
using EduDataGen.Engine;
using Microsoft.AspNetCore.Mvc;

namespace EduDataGen.WebAPI.Controllers;

public class RenderDatasetRequest
{
    public string FileName { get; set; } = string.Empty;
    public int? OverrideTotalRecords { get; set; }
    public int? OverrideSeed { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class DatasetsController : ControllerBase
{
    private readonly IBlueprintRepository _blueprintRepository;
    private readonly IDatasetGenerator _datasetGenerator;
    private readonly IWorkspaceManager _workspaceManager;

    public DatasetsController(
        IBlueprintRepository blueprintRepository,
        IDatasetGenerator datasetGenerator,
        IWorkspaceManager workspaceManager)
    {
        _blueprintRepository = blueprintRepository;
        _datasetGenerator = datasetGenerator;
        _workspaceManager = workspaceManager;
    }

    /// <summary>
    /// Renders dual-table CSV dataset (Features vs Ground Truth) offline based on a blueprint.
    /// </summary>
    [HttpPost("render")]
    public async Task<IActionResult> RenderDataset([FromBody] RenderDatasetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return BadRequest(new { error = "FileName is required." });
        }

        var blueprint = await _blueprintRepository.LoadBlueprintAsync(request.FileName);
        if (blueprint == null)
        {
            return NotFound(new { error = $"Blueprint '{request.FileName}' not found." });
        }

        var result = await _datasetGenerator.RenderDatasetAsync(
            blueprint,
            request.OverrideTotalRecords,
            request.OverrideSeed);

        return Ok(new
        {
            message = "Dataset rendered successfully.",
            scenario = result.Scenario,
            timestamp = result.Timestamp,
            featuresPath = result.FeaturesPath,
            groundTruthPath = result.GroundTruthPath,
            totalRecords = result.GroundTruthRecords.Count,
            previewFeatures = result.FeaturesRecords.Take(5),
            previewGroundTruth = result.GroundTruthRecords.Take(5)
        });
    }

    /// <summary>
    /// Lists all generated CSV dataset files.
    /// </summary>
    [HttpGet]
    public IActionResult ListDatasets()
    {
        if (!Directory.Exists(_workspaceManager.DatasetsPath))
        {
            return Ok(new { datasets = Enumerable.Empty<object>() });
        }

        var files = Directory.GetFiles(_workspaceManager.DatasetsPath, "*.csv")
                             .Select(f => new FileInfo(f))
                             .Select(fi => new
                             {
                                 FileName = fi.Name,
                                 SizeBytes = fi.Length,
                                 LastModified = fi.LastWriteTimeUtc
                             });

        return Ok(new { datasets = files });
    }
}
