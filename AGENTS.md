---
name: edudatagen-agent
description: Skills, development guidelines, and reference index for AI agents working on EduDataGen synthetic data generation engine.
license: MIT
compatibility: Requires .NET 8.0 SDK or .NET 9.0 SDK
metadata:
  project: EduDataGen
  version: "1.0"
---

# EduDataGen Agent Skills & Developer Guide

This document serves as the primary Agent Skill reference and navigation index for AI agents (and human developers) working on the EduDataGen project.

---

## 1. Environment & Setup Reference

### Virtual Machine / Dev Environment Setup
The environment setup script and configuration are managed via standard .NET CLI scripts.
- **SDK**: .NET 8.0 or .NET 9.0 LTS
- **Build Command**: `dotnet build`
- **Test Command**: `dotnet test`
- **Run Web API**: `dotnet run --project src/EduDataGen.WebAPI`

---

## 2. Solution Architecture & Project Structure

EduDataGen is structured into 5 decoupled projects:

1. **`EduDataGen.DAL`**: Data Access Layer for local workspace disk I/O, Blueprint JSON persistence, and Dual-Table CSV serialization/reading.
2. **`EduDataGen.Engine`**: Mathematical execution engine, PRNG deterministic sampling, 3-tier stochastic pathogenesis pipeline, non-linear attenuated accumulator, physiological clamping, and pedagogical dirty data injector.
3. **`EduDataGen.ConnectedService`**: External API integrations, specifically Google Gemini API structured outputs client for LLM-based Blueprint compilation.
4. **`EduDataGen.WebAPI`**: ASP.NET Core REST API exposing controllers for blueprints, dataset generation, dirty data traps, asset management, and hosting the static `index.html` manual tester page.
5. **`EduDataGen.Tests`**: xUnit & FluentAssertions test suite covering math engine formulas, DAL CSV formatting, and Web API endpoints.

For detailed architecture specification, see [`Documents/APPROVED_ARCHITECTURE.md`](Documents/APPROVED_ARCHITECTURE.md).

---

## 3. Approved Package Governance Policy

To maintain security, determinism, and minimal dependency bloat, **only Microsoft-provided libraries or pre-approved packages** may be added to the project.

### Approved Packages
- **Microsoft / ASP.NET Core Native**:
  - `Microsoft.AspNetCore.OpenApi`
  - `Swashbuckle.AspNetCore` (OpenAPI / Swagger UI)
  - `Microsoft.Extensions.Configuration`
  - `Microsoft.Extensions.DependencyInjection`
  - `Microsoft.Extensions.Logging`
  - `System.Text.Json`
- **Third-Party Packages (Pre-Approved)**:
  - `CsvHelper` (v33+) — strictly RFC 4180 + UTF-8 with BOM
  - `MathNet.Numerics` (v5+) — Box-Muller / MersenneTwister sampling
  - `xUnit` & `FluentAssertions` — Testing framework

*Rule*: Any new third-party package MUST be proposed and explicitly approved before adding to `.csproj` files.

For full coding standards and package policies, see [`Documents/CODING_STANDARDS.md`](Documents/CODING_STANDARDS.md).

---

## 4. Progressive Disclosure & Document Index

When executing specific coding tasks, refer to the following documents for on-demand detailed instructions:

| Domain / Task | Reference Document | Key Content |
| :--- | :--- | :--- |
| **System Architecture & Solution Design** | [`Documents/APPROVED_ARCHITECTURE.md`](Documents/APPROVED_ARCHITECTURE.md) | 5-layer decomposition, REST API schemas, Static HTML client design |
| **Coding Standards & Testing Rules** | [`Documents/CODING_STANDARDS.md`](Documents/CODING_STANDARDS.md) | C# conventions, error handling, package governance, xUnit specs |
| **Project Roadmap & Feature Tracker** | [`Documents/PROJECT_PLAN.md`](Documents/PROJECT_PLAN.md) | Living tracker across Phase 1, Phase 2, and Phase 3 |
| **Original Requirements Spec** | [`Documents/EduDataGen (Synthetic Data Engine) 需求与架构规范文档.md`](Documents/EduDataGen (Synthetic Data Engine) 需求与架构规范文档.md) | Domain background, math engine formulas, JSON blueprint schema |

---

## 5. Critical Development Directives

1. **Language**: All code comments, public APIs, and technical documentation MUST be written in **English**.
2. **Metric System Mandate**: All physical/physiological indicators in blueprints and generated data MUST strictly use the **Metric system** (°C, kg, cm, km/h).
3. **Dual-Table Preservation**: Dirty data traps (outliers, typos, missing values) MUST ONLY be injected into `*_features.csv` (Features Table). `*_ground_truth.csv` MUST NEVER be corrupted.
4. **Determinism**: All random operations in `EduDataGen.Engine` MUST rely on seeded PRNG (`System.Random(Seed)` or `MathNet.Numerics`) to guarantee 100% reproducible data generation.
