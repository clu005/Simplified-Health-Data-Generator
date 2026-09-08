# EduDataGen - Approved Architecture & Solution Design

## 1. Executive Summary

**EduDataGen** is an open-source synthetic data engine designed for K-12 data science and AI education. It produces realistic dual-table datasets (Observables vs. Ground Truth) for teaching data cleaning, pattern recognition, and machine learning model validation.

This document outlines the **Approved System Architecture**, detailing the 5-project C# .NET solution structure, layer responsibilities, REST API contracts, and the static HTML manual tester UI.

---

## 2. High-Level Architecture & Layer Breakdown

The system is organized into 5 decoupled C# projects:

```
┌───────────────────────────────────────────────────────────────────┐
│              Static HTML Manual Tester (Web Client)               │
│               (wwwroot/index.html - Single Page UI)               │
└─────────────────────────────────┬─────────────────────────────────┘
                                  │ HTTP REST Calls
                                  ▼
┌───────────────────────────────────────────────────────────────────┐
│                      1. EduDataGen.WebAPI                         │
│   • ASP.NET Core Web API Controllers                              │
│   • Swagger/OpenAPI Documentation                                 │
│   • Static File Server (Hosting manual tester UI)                 │
└───────────┬─────────────────────┬─────────────────────┬───────────┘
            │                     │                     │
            ▼                     ▼                     ▼
┌───────────────────────┐ ┌───────────────────┐ ┌───────────────────┐
│ EduDataGen.Connected  │ │ EduDataGen.Engine │ │  EduDataGen.DAL   │
│       Service         │ │                   │ │                   │
│ • Gemini API Client   │ │ • Stochastic Path-│ │ • Disk Workspace  │
│ • Structured Output   │ │   ogenesis (3-Tier│ │   Manager         │
│   Blueprint Compiler  │ │ • Attenuated Math │ │ • JSON Blueprint  │
│ • External Services   │ │ • Physiological   │ │   Repository      │
│                       │ │   Clamping        │ │ • Dual-Table CSV  │
│                       │ │ • Anomaly Injector│ │   Exporter/Reader │
└───────────┬───────────┘ └─────────┬─────────┘ └─────────┬─────────┘
            │                       │                     │
            └───────────────────────┼─────────────────────┘
                                    ▼
┌───────────────────────────────────────────────────────────────────┐
│                       2. EduDataGen.Tests                         │
│   • xUnit Unit Tests & Integration Tests                          │
└───────────────────────────────────────────────────────────────────┘
```

---

## 3. Project Directory & Solution Layout

```
EduDataGen/
├── Documents/
│   ├── EduDataGen (Synthetic Data Engine) 需求与架构规范文档.md
│   ├── APPROVED_ARCHITECTURE.md
│   ├── CODING_STANDARDS.md
│   └── PROJECT_PLAN.md
├── AGENTS.md
├── EduDataGen.sln
└── src/
    ├── EduDataGen.DAL/
    │   ├── Workspace/           # Workspace directory initializer & disk paths
    │   ├── Repositories/        # Blueprint & Preset JSON file repositories
    │   ├── Exporters/           # Dual-table CSV exporters (CsvHelper with UTF-8 BOM)
    │   └── Models/              # File system DTOs and workspace metadata
    ├── EduDataGen.Engine/
    │   ├── Abstractions/       # Engine service interfaces
    │   ├── Domain/              # Core domain entities (Blueprint, Cohort, Vitals, Condition)
    │   ├── Pipeline/            # 3-tier stochastic pathogenesis execution engine
    │   ├── Math/                # Attenuated math accumulators & Box-Muller Gaussian PRNG
    │   ├── Clamping/            # Physiological hard/soft boundaries clampers
    │   └── Traps/               # Pedagogical anomaly injector (outliers, typos, missing)
    ├── EduDataGen.ConnectedService/
    │   ├── Gemini/              # Gemini REST API HttpClient & JSON Schema compiler
    │   └── Options/             # API Key & LLM endpoint settings
    ├── EduDataGen.WebAPI/
    │   ├── Controllers/         # BlueprintsController, DatasetsController, TrapsController, AssetsController
    │   ├── wwwroot/             # Static HTML/CSS/JS Manual Tester Page (index.html)
    │   └── Program.cs           # Dependency Injection & Middleware pipeline setup
    └── EduDataGen.Tests/
        ├── EngineTests/         # PRNG determinism, 3-tier pipeline, & math formula tests
        ├── DalTests/            # CSV export formatting & workspace disk I/O tests
        └── WebApiTests/         # Integration tests for Web API endpoints
```

---

## 4. Component Details & Layer Responsibilities

### 4.1 `EduDataGen.DAL` (Data Access Layer)
- **Workspace Disk Management**: Automatically initializes `./EduDataGen_Workspace/` with required subfolders:
  - `blueprints/`: User-created and Gemini-compiled JSON blueprint files.
  - `presets/`: Built-in official teaching blueprints (`medical-triage.json`, `youth-soccer.json`, `pediatric-vitals.json`).
  - `datasets/`: Dual-table CSV output files (`*_features.csv` and `*_ground_truth.csv`).
  - `configs/`: Local settings (`settings.json`).
- **CSV Serialization**: Uses `CsvHelper` adhering strictly to **RFC 4180** standards with **UTF-8 with BOM** encoding to ensure seamless compatibility with Excel, CODAP, and Google Colab.

### 4.2 `EduDataGen.Engine` (Mathematical Execution Engine & Business Logic)
- **Determinism**: Unified PRNG seeded via `System.Random(Seed)` or `MathNet.Numerics`.
- **3-Tier Stochastic Pathogenesis Pipeline**:
  1. *Tier 1 (Cohort Roll)*: Roll $R_1 \in [0, 1)$. If $R_1 < \text{HealthyRatio}$, marked pure healthy; bypasses conditions.
  2. *Tier 2 (Phenotypic Roll)*: For non-healthy samples, roll $R_2$ against `OccurrenceProbability` for each condition.
  3. *Tier 3 (Severity Sampling)*: Sample severity factor $S \in [\text{MinFactor}, \text{MaxFactor}]$.
- **Non-Linear Attenuated Accumulator**:
  $$\Delta_{\text{total}} = \Delta_{(1)} + \sum_{i=2}^{N} \left( \Delta_{(i)} \cdot \gamma^{i-1} \right) \quad (\gamma = 0.35)$$
- **Physiological Clamping**: Clamps values within $[\text{HardMin}, \text{HardMax}]$.
- **Pedagogical Anomaly Injector**: Injects missing values, extreme outliers (e.g., body temp $99.0^\circ\text{C}$), and inconsistent typos exclusively into the Features table.

### 4.3 `EduDataGen.ConnectedService` (Gemini API Integration)
- Connects to Google Gemini API (via HttpClient / Structured Outputs JSON Schema).
- Accepts user natural language prompts and compiles structured `SimulationBlueprint` JSON files with verified metric units, baseline statistics, and medical condition deltas.

### 4.4 `EduDataGen.WebAPI` (REST API & Static Web Host)
Exposes clean RESTful endpoints:
- `POST /api/blueprints/generate-ai`: Calls ConnectedService to compile blueprint via Gemini API.
- `GET /api/blueprints`: Lists all blueprints (custom + presets).
- `POST /api/datasets/render`: Renders dual-table CSV datasets from a specified blueprint and seed.
- `POST /api/datasets/inject-traps`: Secondary injection of pedagogical dirty data into a Features table.
- `GET /api/assets/datasets`: Browse and preview existing datasets (Features vs. Ground Truth comparison).

### 4.5 Static HTML Manual Tester UI (`wwwroot/index.html`)
- A single-page, zero-dependency HTML5/CSS3/JS Web UI hosted directly by ASP.NET Core.
- Provides interactive forms and tabular data views for developers and teachers to:
  1. Submit natural language scenario prompts to AI blueprint builder.
  2. Select blueprints and trigger deterministic dual-table rendering.
  3. Preview side-by-side table rows (Features vs. Ground Truth).
  4. Perform test trap injections and inspect output CSV files.

---

## 5. Security & Data Integrity Guarantees

1. **Ground Truth Integrity**: Anomaly injection algorithms MUST NEVER corrupt `*_ground_truth.csv`. Ground Truth remains the absolute answer key.
2. **Metric Unit Enforcement**: Engine and ConnectedService enforce公制 (Metric) units across all endpoints.
3. **Local Disk Isolation**: All file writes are strictly sandboxed inside `./EduDataGen_Workspace/`. Path traversal attacks are validated and blocked in `EduDataGen.DAL`.
