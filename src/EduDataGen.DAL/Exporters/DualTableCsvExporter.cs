using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using EduDataGen.DAL.Workspace;

namespace EduDataGen.DAL.Exporters;

public interface IDualTableCsvExporter
{
    Task<(string featuresPath, string groundTruthPath)> ExportDualTableAsync<TFeature, TGroundTruth>(
        string scenario,
        IEnumerable<TFeature> featuresRecords,
        IEnumerable<TGroundTruth> groundTruthRecords,
        string? customTimestamp = null);

    Task<IEnumerable<dynamic>> ReadCsvAsync(string filePath);
}

public class DualTableCsvExporter : IDualTableCsvExporter
{
    private readonly IWorkspaceManager _workspaceManager;
    private static readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        Encoding = new UTF8Encoding(true) // UTF-8 with BOM
    };

    public DualTableCsvExporter(IWorkspaceManager workspaceManager)
    {
        _workspaceManager = workspaceManager;
        _workspaceManager.InitializeWorkspace();
    }

    public async Task<(string featuresPath, string groundTruthPath)> ExportDualTableAsync<TFeature, TGroundTruth>(
        string scenario,
        IEnumerable<TFeature> featuresRecords,
        IEnumerable<TGroundTruth> groundTruthRecords,
        string? customTimestamp = null)
    {
        string timestamp = customTimestamp ?? DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        string featuresFileName = $"{scenario}_{timestamp}_features.csv";
        string groundTruthFileName = $"{scenario}_{timestamp}_ground_truth.csv";

        string featuresPath = _workspaceManager.GetSanitizedPath("datasets", featuresFileName);
        string groundTruthPath = _workspaceManager.GetSanitizedPath("datasets", groundTruthFileName);

        await WriteCsvFileAsync(featuresPath, featuresRecords);
        await WriteCsvFileAsync(groundTruthPath, groundTruthRecords);

        return (featuresPath, groundTruthPath);
    }

    public async Task<IEnumerable<dynamic>> ReadCsvAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("CSV file not found.", filePath);
        }

        using var reader = new StreamReader(filePath, new UTF8Encoding(true));
        using var csv = new CsvReader(reader, CsvConfig);
        var records = csv.GetRecords<dynamic>().ToList();
        return await Task.FromResult(records);
    }

    private static async Task WriteCsvFileAsync<T>(string filePath, IEnumerable<T> records)
    {
        // Write UTF-8 with BOM explicitly for Excel/CODAP compatibility
        using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));
        using var csv = new CsvWriter(writer, CsvConfig);
        await csv.WriteRecordsAsync(records);
    }
}
