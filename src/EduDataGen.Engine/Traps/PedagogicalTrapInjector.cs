namespace EduDataGen.Engine.Traps;

using System;
using EduDataGen.DAL.Models;

public class PedagogicalTrapInjector : IPedagogicalTrapInjector
{
    public List<Dictionary<string, string?>> InjectTraps(
        List<Dictionary<string, string?>> records,
        AnomalySettings settings,
        int seed = 2026)
    {
        if (records == null || records.Count == 0 || settings == null)
        {
            return records ?? new List<Dictionary<string, string?>>();
        }

        var corruptedRecords = records.Select(r => new Dictionary<string, string?>(r)).ToList();
        var rng = new Random(seed);

        // 1. Outlier Rules
        if (settings.Outliers != null)
        {
            foreach (var rule in settings.Outliers)
            {
                if (string.IsNullOrWhiteSpace(rule.Field) || rule.Count <= 0)
                {
                    continue;
                }

                if (!corruptedRecords[0].ContainsKey(rule.Field))
                {
                    continue;
                }

                int targetCount = System.Math.Min(rule.Count, corruptedRecords.Count);
                var targetIndices = Enumerable.Range(0, corruptedRecords.Count)
                    .OrderBy(_ => rng.Next())
                    .Take(targetCount)
                    .ToList();

                string valStr = rule.Value?.ToString() ?? "";
                foreach (int idx in targetIndices)
                {
                    corruptedRecords[idx][rule.Field] = valStr;
                }
            }
        }

        // 2. Typo / Category Inconsistencies
        if (settings.TypoInconsistencies != null)
        {
            foreach (var rule in settings.TypoInconsistencies)
            {
                if (string.IsNullOrWhiteSpace(rule.Field) ||
                    rule.CorruptedValues == null ||
                    rule.CorruptedValues.Count == 0)
                {
                    continue;
                }

                if (!corruptedRecords[0].ContainsKey(rule.Field))
                {
                    continue;
                }

                foreach (var row in corruptedRecords)
                {
                    string? currentVal = row[rule.Field];
                    if (currentVal != null && string.Equals(currentVal, rule.OriginalValue, StringComparison.OrdinalIgnoreCase))
                    {
                        int corruptedIdx = rng.Next(rule.CorruptedValues.Count);
                        row[rule.Field] = rule.CorruptedValues[corruptedIdx];
                    }
                }
            }
        }

        // 3. Missing Value Rate
        if (settings.MissingValueRate > 0)
        {
            double rate = System.Math.Clamp(settings.MissingValueRate, 0.0, 1.0);
            var fields = corruptedRecords[0].Keys
                .Where(k => !string.Equals(k, "Patient_ID", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(k, "PatientID", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(k, "ID", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var row in corruptedRecords)
            {
                foreach (var field in fields)
                {
                    if (rng.NextDouble() < rate)
                    {
                        row[field] = "";
                    }
                }
            }
        }

        return corruptedRecords;
    }
}
