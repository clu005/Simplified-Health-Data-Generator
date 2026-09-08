using EduDataGen.DAL.Exporters;
using EduDataGen.DAL.Workspace;
using FluentAssertions;
using Xunit;

namespace EduDataGen.Tests.DalTests;

public class DualTableCsvExporterTests : IDisposable
{
    private readonly string _tempWorkspaceDir;
    private readonly WorkspaceManager _workspaceManager;
    private readonly DualTableCsvExporter _exporter;

    public DualTableCsvExporterTests()
    {
        _tempWorkspaceDir = Path.Combine(Path.GetTempPath(), "EduDataGen_CsvTest_" + Guid.NewGuid());
        _workspaceManager = new WorkspaceManager(_tempWorkspaceDir);
        _exporter = new DualTableCsvExporter(_workspaceManager);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempWorkspaceDir))
        {
            Directory.Delete(_tempWorkspaceDir, true);
        }
    }

    public class FeatureRecord
    {
        public string Patient_ID { get; set; } = string.Empty;
        public double Body_Temp_C { get; set; }
        public int Heart_Rate { get; set; }
    }

    public class GroundTruthRecord
    {
        public string Patient_ID { get; set; } = string.Empty;
        public bool Is_Patient { get; set; }
        public string Primary_Diagnosis { get; set; } = string.Empty;
    }

    [Fact]
    public async Task ExportDualTableAsync_ShouldCreateBothCsvFilesWithUtf8Bom()
    {
        // Arrange
        var features = new List<FeatureRecord>
        {
            new() { Patient_ID = "PAT-0001", Body_Temp_C = 36.8, Heart_Rate = 72 },
            new() { Patient_ID = "PAT-0002", Body_Temp_C = 38.5, Heart_Rate = 95 }
        };

        var groundTruth = new List<GroundTruthRecord>
        {
            new() { Patient_ID = "PAT-0001", Is_Patient = false, Primary_Diagnosis = "Healthy" },
            new() { Patient_ID = "PAT-0002", Is_Patient = true, Primary_Diagnosis = "Acute_Infection" }
        };

        // Act
        var (featuresPath, groundTruthPath) = await _exporter.ExportDualTableAsync(
            "medical_triage",
            features,
            groundTruth,
            "20260101_000000");

        // Assert
        File.Exists(featuresPath).Should().BeTrue();
        File.Exists(groundTruthPath).Should().BeTrue();

        byte[] featuresBytes = await File.ReadAllBytesAsync(featuresPath);
        // Verify UTF-8 BOM: 0xEF, 0xBB, 0xBF
        featuresBytes.Length.Should().BeGreaterThan(3);
        featuresBytes[0].Should().Be(0xEF);
        featuresBytes[1].Should().Be(0xBB);
        featuresBytes[2].Should().Be(0xBF);

        var readFeatures = await _exporter.ReadCsvAsync(featuresPath);
        readFeatures.Should().HaveCount(2);
    }
}
