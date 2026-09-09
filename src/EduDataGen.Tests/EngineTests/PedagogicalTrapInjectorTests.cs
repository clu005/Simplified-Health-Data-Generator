using EduDataGen.DAL.Models;
using EduDataGen.Engine.Traps;
using FluentAssertions;

namespace EduDataGen.Tests.EngineTests;

public class PedagogicalTrapInjectorTests
{
    private readonly PedagogicalTrapInjector _injector = new();

    [Fact]
    public void InjectTraps_OutliersRule_ReplacesSpecifiedCountOfRows()
    {
        var records = new List<Dictionary<string, string?>>
        {
            new() { { "Patient_ID", "PAT-0001" }, { "Body_Temp_C", "36.8" } },
            new() { { "Patient_ID", "PAT-0002" }, { "Body_Temp_C", "37.0" } },
            new() { { "Patient_ID", "PAT-0003" }, { "Body_Temp_C", "36.5" } },
            new() { { "Patient_ID", "PAT-0004" }, { "Body_Temp_C", "36.9" } }
        };

        var settings = new AnomalySettings
        {
            Outliers = new List<OutlierRule>
            {
                new OutlierRule { Field = "Body_Temp_C", Value = "99.0", Count = 2 }
            }
        };

        var corrupted = _injector.InjectTraps(records, settings, seed: 12345);

        corrupted.Count.Should().Be(4);
        int outlierCount = corrupted.Count(r => r["Body_Temp_C"] == "99.0");
        outlierCount.Should().Be(2);
    }

    [Fact]
    public void InjectTraps_TypoRule_ReplacesMatchingValuesWithCorruptedVariants()
    {
        var records = new List<Dictionary<string, string?>>
        {
            new() { { "Patient_ID", "PAT-0001" }, { "Severity", "Mild" } },
            new() { { "Patient_ID", "PAT-0002" }, { "Severity", "Mild" } },
            new() { { "Patient_ID", "PAT-0003" }, { "Severity", "Severe" } }
        };

        var settings = new AnomalySettings
        {
            TypoInconsistencies = new List<TypoRule>
            {
                new TypoRule
                {
                    Field = "Severity",
                    OriginalValue = "Mild",
                    CorruptedValues = new List<string> { "Mild ", "mlid" }
                }
            }
        };

        var corrupted = _injector.InjectTraps(records, settings, seed: 12345);

        corrupted[0]["Severity"].Should().Match(s => s == "Mild " || s == "mlid");
        corrupted[1]["Severity"].Should().Match(s => s == "Mild " || s == "mlid");
        corrupted[2]["Severity"].Should().Be("Severe");
    }

    [Fact]
    public void InjectTraps_MissingValueRate_SetsFieldsToEmptyAndPreservesPatientId()
    {
        var records = new List<Dictionary<string, string?>>();
        for (int i = 1; i <= 20; i++)
        {
            records.Add(new Dictionary<string, string?>
            {
                { "Patient_ID", $"PAT-{i:D4}" },
                { "Age", "15" },
                { "Heart_Rate", "80" }
            });
        }

        var settings = new AnomalySettings
        {
            MissingValueRate = 0.5
        };

        var corrupted = _injector.InjectTraps(records, settings, seed: 12345);

        // Patient_ID must never be blanked
        corrupted.All(r => !string.IsNullOrEmpty(r["Patient_ID"])).Should().BeTrue();

        // Some cells in Age or Heart_Rate should be missing ("")
        int missingCount = corrupted.Sum(r => (r["Age"] == "" ? 1 : 0) + (r["Heart_Rate"] == "" ? 1 : 0));
        missingCount.Should().BeGreaterThan(0);
    }
}
