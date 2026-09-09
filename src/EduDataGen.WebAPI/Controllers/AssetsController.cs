using EduDataGen.DAL.Exporters;
using EduDataGen.DAL.Workspace;
using Microsoft.AspNetCore.Mvc;

namespace EduDataGen.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly IWorkspaceManager _workspaceManager;
    private readonly IDualTableCsvExporter _exporter;

    public AssetsController(IWorkspaceManager workspaceManager, IDualTableCsvExporter exporter)
    {
        _workspaceManager = workspaceManager;
        _exporter = exporter;
    }

    /// <summary>
    /// Lists all dataset files and pairs in the datasets directory.
    /// </summary>
    [HttpGet("datasets")]
    public IActionResult ListDatasets()
    {
        string datasetsDir = _workspaceManager.DatasetsPath;
        if (!Directory.Exists(datasetsDir))
        {
            return Ok(new { files = Array.Empty<object>(), pairs = Array.Empty<object>() });
        }

        var dirInfo = new DirectoryInfo(datasetsDir);
        var csvFiles = dirInfo.GetFiles("*.csv")
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .ToList();

        var fileDtos = csvFiles.Select(f => new
        {
            fileName = f.Name,
            size = f.Length,
            lastModifiedUtc = f.LastWriteTimeUtc,
            isFeatures = f.Name.Contains("features", StringComparison.OrdinalIgnoreCase),
            isGroundTruth = f.Name.Contains("ground_truth", StringComparison.OrdinalIgnoreCase)
        }).ToList();

        var pairs = new List<object>();
        var featuresFiles = csvFiles.Where(f => f.Name.EndsWith("_features.csv", StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var featureFile in featuresFiles)
        {
            string expectedGtName = featureFile.Name.Replace("_features.csv", "_ground_truth.csv");
            var gtFile = csvFiles.FirstOrDefault(f => string.Equals(f.Name, expectedGtName, StringComparison.OrdinalIgnoreCase));

            pairs.Add(new
            {
                featuresFile = featureFile.Name,
                groundTruthFile = gtFile?.Name,
                lastModifiedUtc = featureFile.LastWriteTimeUtc
            });
        }

        return Ok(new
        {
            files = fileDtos,
            pairs
        });
    }

    /// <summary>
    /// Fetches side-by-side top records comparing Features CSV and Ground Truth CSV.
    /// </summary>
    [HttpGet("datasets/preview")]
    public async Task<IActionResult> PreviewDataset(
        [FromQuery] string? featuresFile,
        [FromQuery] string? groundTruthFile = null,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(featuresFile) && string.IsNullOrWhiteSpace(groundTruthFile))
        {
            return BadRequest(new { error = "Either featuresFile or groundTruthFile parameter must be provided." });
        }

        limit = System.Math.Clamp(limit, 1, 100);

        List<Dictionary<string, string?>>? featuresRecords = null;
        List<Dictionary<string, string?>>? groundTruthRecords = null;

        if (!string.IsNullOrWhiteSpace(featuresFile))
        {
            string path = _workspaceManager.GetSanitizedPath("datasets", featuresFile);
            if (System.IO.File.Exists(path))
            {
                var all = await _exporter.ReadCsvAsDictionariesAsync(path);
                featuresRecords = all.Take(limit).ToList();
            }
            else
            {
                return NotFound(new { error = $"Features file '{featuresFile}' not found." });
            }

            if (string.IsNullOrWhiteSpace(groundTruthFile) && featuresFile.EndsWith("_features.csv", StringComparison.OrdinalIgnoreCase))
            {
                string inferredGt = featuresFile.Replace("_features.csv", "_ground_truth.csv");
                string gtPath = _workspaceManager.GetSanitizedPath("datasets", inferredGt);
                if (System.IO.File.Exists(gtPath))
                {
                    groundTruthFile = inferredGt;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(groundTruthFile))
        {
            string gtPath = _workspaceManager.GetSanitizedPath("datasets", groundTruthFile);
            if (System.IO.File.Exists(gtPath))
            {
                var allGt = await _exporter.ReadCsvAsDictionariesAsync(gtPath);
                groundTruthRecords = allGt.Take(limit).ToList();
            }
        }

        return Ok(new
        {
            featuresFile,
            groundTruthFile,
            features = featuresRecords ?? new List<Dictionary<string, string?>>(),
            groundTruth = groundTruthRecords ?? new List<Dictionary<string, string?>>()
        });
    }
}
