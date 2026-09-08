# EduDataGen - Overall Project Plan & Feature Tracker

## 1. Project Overview & Vision

**EduDataGen** is an open-source synthetic data engine designed for K-12 data science and AI education. It provides teachers and students with realistic, dual-table medical/physiological datasets (Observables vs. Ground Truth) for data cleaning, pattern discovery, and model validation.

This document serves as the **living project tracker** for the overall software development lifecycle across Phase 1, Phase 2, and Phase 3.

---

## 2. Milestone Roadmap

```
┌─────────────────────────────────────────────────────────────┐
│ Phase 1: Core Engine, DAL & Web API Foundation (MVP)         │
│ • 5-Project C# Solution Setup                               │
│ • Deterministic PRNG Math Engine & 3-Tier Pipeline          │
│ • DAL Workspace Disk I/O & Dual-Table CSV Exporters          │
│ • Web API Controllers for Blueprint & Rendering             │
│ • xUnit Test Suite for Engine & DAL                         │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│ Phase 2: AI Blueprint Compiler, Traps Studio & Web UI Tester │
│ • Gemini API Integration (EduDataGen.ConnectedService)      │
│ • Pedagogical Anomaly Injector (Outliers, Typos, Missing)   │
│ • Static HTML Manual Tester UI (wwwroot/index.html)         │
│ • Integration Testing & Web API Swagger Polish              │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│ Phase 3: Presets, Open Source Package & CODAP Teaching Kit  │
│ • Built-in Presets (Medical Triage, Soccer, Growth Curves)   │
│ • CODAP & Google Colab Integration Guide                    │
│ • CI/CD GitHub Actions & Repository Release                 │
└──────────────────────────────┬──────────────────────────────┘
```

---

## 3. Work Breakdown Structure & Feature Tracker

### Phase 1: Core Engine, DAL & Web API Foundation (MVP)
| Task ID | Component | Task Description | Status | Target Deliverable |
| :--- | :--- | :--- | :---: | :--- |
| **P1-01** | `Solution` | Initialize 5-project C# solution (`DAL`, `Engine`, `ConnectedService`, `WebAPI`, `Tests`) | ✅ Completed | `EduDataGen.sln` |
| **P1-02** | `DAL` | Implement Workspace Disk Initializer (`blueprints/`, `presets/`, `datasets/`, `configs/`) | ✅ Completed | `WorkspaceManager.cs` |
| **P1-03** | `DAL` | Implement Blueprint JSON Repository (Load/Save Simulation Blueprints) | ✅ Completed | `BlueprintRepository.cs` |
| **P1-04** | `DAL` | Implement Dual-Table CSV Exporters via `CsvHelper` (RFC 4180 + UTF-8 with BOM) | ✅ Completed | `DualTableCsvExporter.cs` |
| **P1-05** | `Engine` | Implement Domain Entities & JSON Blueprint Schema Models | ✅ Completed | `Domain/` models |
| **P1-06** | `Engine` | Implement Seeded PRNG & Box-Muller Gaussian Distribution Sampler | ✅ Completed | `GaussianSampler.cs` |
| **P1-07** | `Engine` | Implement 3-Tier Stochastic Pathogenesis Pipeline (Cohort roll -> Condition roll -> Severity) | ✅ Completed | `StochasticPipeline.cs` |
| **P1-08** | `Engine` | Implement Non-Linear Attenuated Accumulator ($\gamma = 0.35$) & Priority Override | ✅ Completed | `AttenuatedAccumulator.cs` |
| **P1-09** | `Engine` | Implement Physiological Boundary Clamping (HardMin / HardMax) | ✅ Completed | `PhysiologicalClamper.cs` |
| **P1-10** | `WebAPI` | Implement `BlueprintsController` (Get all, Get by ID, Save blueprint) | ✅ Completed | `BlueprintsController.cs` |
| **P1-11** | `WebAPI` | Implement `DatasetsController` (Render dual-table CSVs offline) | ✅ Completed | `DatasetsController.cs` |
| **P1-12** | `Tests` | Write xUnit tests for PRNG determinism, 3-tier pipeline, and math formulas | ✅ Completed | `EngineTests/` |
| **P1-13** | `Tests` | Write xUnit tests for DAL CSV serialization & workspace I/O | ✅ Completed | `DalTests/` |

---

### Phase 2: AI Blueprint Compiler, Traps Studio & Manual Tester Web UI
| Task ID | Component | Task Description | Status | Target Deliverable |
| :--- | :--- | :--- | :---: | :--- |
| **P2-01** | `Connected` | Implement Google Gemini REST API Client with Structured Outputs JSON Schema | ⏳ Backlog | `GeminiBlueprintCompiler.cs` |
| **P2-02** | `WebAPI` | Implement AI Blueprint compilation endpoint (`POST /api/blueprints/generate-ai`) | ⏳ Backlog | `BlueprintsController.cs` |
| **P2-03** | `Engine` | Implement Pedagogical Anomaly Injector (Outliers, Missing Values, Typo Inconsistencies) | ⏳ Backlog | `PedagogicalTrapInjector.cs` |
| **P2-04** | `WebAPI` | Implement `TrapsController` (`POST /api/datasets/inject-traps`) | ⏳ Backlog | `TrapsController.cs` |
| **P2-05** | `WebAPI` | Implement Static File Server & `wwwroot/index.html` Single-Page Manual Tester UI | ⏳ Backlog | `wwwroot/index.html` |
| **P2-06** | `WebAPI` | Implement `AssetsController` (Browse & side-by-side preview Features vs Ground Truth) | ⏳ Backlog | `AssetsController.cs` |
| **P2-07** | `Tests` | Integration tests for Web API controllers and Gemini compiler mock | ⏳ Backlog | `WebApiTests/` |

---

### Phase 3: Presets, Open Source Packaging & CODAP Teaching Kit
| Task ID | Component | Task Description | Status | Target Deliverable |
| :--- | :--- | :--- | :---: | :--- |
| **P3-01** | `Presets` | Create `medical-triage.json` preset template | ⏳ Backlog | `presets/medical-triage.json` |
| **P3-02** | `Presets` | Create `youth-soccer.json` preset template | ⏳ Backlog | `presets/youth-soccer.json` |
| **P3-03** | `Presets` | Create `pediatric-vitals.json` preset template | ⏳ Backlog | `presets/pediatric-vitals.json` |
| **P3-04** | `Docs` | Write CODAP & Google Colab student data mining alignment guide | ⏳ Backlog | `Documents/CODAP_GUIDE.md` |
| **P3-05** | `CI/CD` | Create GitHub Actions workflow for `dotnet build` & `dotnet test` | ⏳ Backlog | `.github/workflows/ci.yml` |

---

## 4. Status Legend

- ⏳ **Backlog**: Scheduled for future execution.
- 🚧 **In Progress**: Currently under active development.
- ✅ **Completed**: Implemented, verified with unit tests, and reviewed.
