using System.Globalization;
using EduDataGen.DAL.Exporters;
using EduDataGen.DAL.Models;
using EduDataGen.Engine.Clamping;
using EduDataGen.Engine.Domain;
using EduDataGen.Engine.Math;
using EduDataGen.Engine.Pipeline;

namespace EduDataGen.Engine;

public class DatasetGenerator : IDatasetGenerator
{
    private readonly IStochasticPipeline _pipeline;
    private readonly IAttenuatedAccumulator _accumulator;
    private readonly IPhysiologicalClamper _clamper;
    private readonly IDualTableCsvExporter _csvExporter;

    public DatasetGenerator(
        IStochasticPipeline pipeline,
        IAttenuatedAccumulator accumulator,
        IPhysiologicalClamper clamper,
        IDualTableCsvExporter csvExporter)
    {
        _pipeline = pipeline;
        _accumulator = accumulator;
        _clamper = clamper;
        _csvExporter = csvExporter;
    }

    public async Task<DatasetGenerationResult> RenderDatasetAsync(
        SimulationBlueprint blueprint,
        int? overrideTotalRecords = null,
        int? overrideSeed = null,
        string? customTimestamp = null)
    {
        int seed = overrideSeed ?? blueprint.Seed;
        int totalRecords = overrideTotalRecords ?? blueprint.TotalRecords;
        if (totalRecords <= 0) totalRecords = 100;

        var sampler = new GaussianSampler(seed);
        var samples = new List<PatientSample>();

        var metricOrder = blueprint.BaselineMetrics?.Keys.ToList() ?? new List<string>();

        // Generate patient records
        for (int i = 1; i <= totalRecords; i++)
        {
            string patientId = $"PAT-{i:D4}";
            var sample = _pipeline.DeterminePathogenesis(blueprint, sampler, patientId);

            if (blueprint.BaselineMetrics != null)
            {
                foreach (var (metricName, metricDef) in blueprint.BaselineMetrics)
                {
                    double rawValue = _accumulator.CalculateMetricValue(
                        metricName,
                        metricDef,
                        sample.ActiveConditions,
                        sampler);

                    double clampedValue = _clamper.ClampValue(
                        rawValue,
                        metricDef.Limits,
                        metricDef.Distribution);

                    sample.MetricValues[metricName] = clampedValue;
                }
            }

            samples.Add(sample);
        }

        // Convert to Ground Truth and Features records
        var groundTruthRecords = samples.Select(s => s.ToGroundTruthRecord()).ToList();
        var featureRecords = samples.Select(s => s.ToFeatureDictionary(metricOrder)).ToList();

        // Apply Anomalies ONLY to Features Table (Never touch Ground Truth)
        ApplyAnomalies(featureRecords, blueprint.Anomalies, sampler);

        string scenario = string.IsNullOrWhiteSpace(blueprint.Scenario) ? "simulation" : blueprint.Scenario;
        string timestamp = customTimestamp ?? DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        var (featuresPath, groundTruthPath) = await _csvExporter.ExportDualTableAsync(
            scenario,
            featureRecords,
            groundTruthRecords,
            timestamp);

        return new DatasetGenerationResult
        {
            Scenario = scenario,
            Timestamp = timestamp,
            FeaturesPath = featuresPath,
            GroundTruthPath = groundTruthPath,
            FeaturesRecords = featureRecords,
            GroundTruthRecords = groundTruthRecords
        };
    }

    private static void ApplyAnomalies(List<Dictionary<string, object?>> features, AnomalySettings? anomalies, IGaussianSampler sampler)
    {
        if (anomalies == null || features.Count == 0) return;

        // 1. Missing Values
        if (anomalies.MissingValueRate > 0)
        {
            foreach (var record in features)
            {
                var keys = record.Keys.Where(k => k != "Patient_ID").ToList();
                foreach (var key in keys)
                {
                    if (sampler.NextDouble() < anomalies.MissingValueRate)
                    {
                        record[key] = string.Empty;
                    }
                }
            }
        }

        // 2. Outliers
        if (anomalies.Outliers != null && anomalies.Outliers.Count > 0)
        {
            foreach (var rule in anomalies.Outliers)
            {
                if (string.IsNullOrWhiteSpace(rule.Field) || rule.Count <= 0) continue;

                int countToInject = System.Math.Min(rule.Count, features.Count);
                for (int c = 0; c < countToInject; c++)
                {
                    int targetIndex = sampler.SampleIntegerUniform(0, features.Count - 1);
                    if (features[targetIndex].ContainsKey(rule.Field))
                    {
                        features[targetIndex][rule.Field] = rule.Value;
                    }
                }
            }
        }

        // 3. Typo Inconsistencies
        if (anomalies.TypoInconsistencies != null && anomalies.TypoInconsistencies.Count > 0)
        {
            foreach (var rule in anomalies.TypoInconsistencies)
            {
                if (string.IsNullOrWhiteSpace(rule.Field) ||
                    rule.CorruptedValues == null ||
                    rule.CorruptedValues.Count == 0) continue;

                var matchingRecords = features
                    .Where(f => f.ContainsKey(rule.Field) && f[rule.Field] != null &&
                                f[rule.Field]!.ToString()!.Equals(rule.OriginalValue, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var rec in matchingRecords)
                {
                    int typoIdx = sampler.SampleIntegerUniform(0, rule.CorruptedValues.Count - 1);
                    rec[rule.Field] = rule.CorruptedValues[typoIdx];
                }
            }
        }
    }
}
