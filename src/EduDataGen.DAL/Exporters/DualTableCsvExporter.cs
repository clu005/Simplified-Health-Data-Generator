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
    Task<List<Dictionary<string, string?>>> ReadCsvAsDictionariesAsync(string filePath);
    Task WriteCsvFromDictionariesAsync(string filePath, List<Dictionary<string, string?>> records);
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

    public async Task<List<Dictionary<string, string?>>> ReadCsvAsDictionariesAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("CSV file not found.", filePath);
        }

        using var reader = new StreamReader(filePath, new UTF8Encoding(true));
        using var csv = new CsvReader(reader, CsvConfig);

        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.HeaderRecord;
        var result = new List<Dictionary<string, string?>>();

        while (await csv.ReadAsync())
        {
            var dict = new Dictionary<string, string?>();
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    dict[header] = csv.GetField(header);
                }
            }
            result.Add(dict);
        }

        return result;
    }

    public async Task WriteCsvFromDictionariesAsync(string filePath, List<Dictionary<string, string?>> records)
    {
        using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));
        using var csv = new CsvWriter(writer, CsvConfig);

        if (records.Count > 0)
        {
            var headers = records[0].Keys.ToList();
            foreach (var header in headers)
            {
                csv.WriteField(header);
            }
            await csv.NextRecordAsync();

            foreach (var dict in records)
            {
                foreach (var header in headers)
                {
                    csv.WriteField(dict.TryGetValue(header, out var val) ? val : null);
                }
                await csv.NextRecordAsync();
            }
        }
    }

    private static async Task WriteCsvFileAsync<T>(string filePath, IEnumerable<T> records)
    {
        using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));
        using var csv = new CsvWriter(writer, CsvConfig);

        if (records is IEnumerable<IDictionary<string, object?>> dictRecords)
        {
            var list = dictRecords.ToList();
            if (list.Count > 0)
            {
                var keys = list[0].Keys.ToList();
                foreach (var key in keys)
                {
                    csv.WriteField(key);
                }
                await csv.NextRecordAsync();

                foreach (var dict in list)
                {
                    foreach (var key in keys)
                    {
                        csv.WriteField(dict.TryGetValue(key, out var val) ? val : null);
                    }
                    await csv.NextRecordAsync();
                }
            }
        }
        else
        {
            await csv.WriteRecordsAsync(records);
        }
    }
}
