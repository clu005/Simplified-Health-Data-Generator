using EduDataGen.DAL.Exporters;
using EduDataGen.DAL.Models;
using EduDataGen.DAL.Workspace;
using EduDataGen.Engine.Traps;
using Microsoft.AspNetCore.Mvc;

namespace EduDataGen.WebAPI.Controllers;

[ApiController]
[Route("api/datasets")]
public class TrapsController : ControllerBase
{
    private readonly IDualTableCsvExporter _exporter;
    private readonly IWorkspaceManager _workspaceManager;
    private readonly IPedagogicalTrapInjector _trapInjector;

    public TrapsController(
        IDualTableCsvExporter exporter,
        IWorkspaceManager workspaceManager,
        IPedagogicalTrapInjector trapInjector)
    {
        _exporter = exporter;
        _workspaceManager = workspaceManager;
        _trapInjector = trapInjector;
    }

    /// <summary>
    /// Injects pedagogical dirty data traps (outliers, missing values, typos) into a Features CSV dataset.
    /// Ground Truth files are strictly protected from modification.
    /// </summary>
    [HttpPost("inject-traps")]
    public async Task<IActionResult> InjectTraps([FromBody] InjectTrapsRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.SourceFileName))
        {
            return BadRequest(new { error = "Source features CSV file name is required." });
        }

        if (request.SourceFileName.Contains("ground_truth", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Traps can only be injected into Features tables (*_features.csv). Ground Truth tables cannot be modified." });
        }

        string sourceFilePath = _workspaceManager.GetSanitizedPath("datasets", request.SourceFileName);
        if (!System.IO.File.Exists(sourceFilePath))
        {
            return NotFound(new { error = $"Source features CSV file '{request.SourceFileName}' was not found." });
        }

        string? targetFileName = request.TargetFileName;
        if (string.IsNullOrWhiteSpace(targetFileName))
        {
            string nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(request.SourceFileName);
            targetFileName = $"{nameWithoutExt}_corrupted.csv";
        }

        if (targetFileName.Contains("ground_truth", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Target file name cannot overwrite Ground Truth tables." });
        }

        string targetFilePath = _workspaceManager.GetSanitizedPath("datasets", targetFileName);

        try
        {
            var records = await _exporter.ReadCsvAsDictionariesAsync(sourceFilePath);
            if (records == null || records.Count == 0)
            {
                return BadRequest(new { error = "Source CSV file is empty." });
            }

            var corruptedRecords = _trapInjector.InjectTraps(records, request.Anomalies ?? new AnomalySettings(), request.Seed);
            await _exporter.WriteCsvFromDictionariesAsync(targetFilePath, corruptedRecords);

            return Ok(new
            {
                message = "Pedagogical traps injected successfully into features dataset.",
                sourceFileName = request.SourceFileName,
                targetFileName = System.IO.Path.GetFileName(targetFilePath),
                recordsCount = corruptedRecords.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to inject traps: {ex.Message}" });
        }
    }
}

public class InjectTrapsRequest
{
    public string SourceFileName { get; set; } = string.Empty;
    public string? TargetFileName { get; set; }
    public AnomalySettings Anomalies { get; set; } = new();
    public int Seed { get; set; } = 2026;
}
