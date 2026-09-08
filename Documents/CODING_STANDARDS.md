# EduDataGen - Coding Standards & Engineering Guidelines

## 1. Overview

This document defines the C# .NET coding conventions, architectural guidelines, package governance policies, and testing standards for the **EduDataGen** project. All developers and AI agents MUST adhere to these practices.

---

## 2. C# Language & Framework Standards

- **Target Framework**: .NET 10 (`net10.0`).
- **Language Version**: C# 12 / C# 13 with all modern language features enabled:
  - File-scoped namespaces (`namespace EduDataGen.Engine.Domain;`).
  - Nullable reference types (`<Nullable>enable</Nullable>`).
  - Treat warnings as errors in release builds (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).
  - Primary constructors and expression-bodied members where appropriate.

### Naming Conventions
- **PascalCase**: Classes, Interfaces (`IBlueprintRepository`), Methods, Properties, Public Fields, Namespaces, Enum values.
- **camelCase**: Method parameters, local variables, private fields (prefixed with underscore, e.g., `_mathEngine`).
- **Interfaces**: Always prefix with capital `I` (e.g., `IEnginePipeline`, `ICsvExporter`).
- **Async Methods**: Always suffix with `Async` (e.g., `RenderDatasetAsync`).

---

## 3. Layer Separation & Dependency Rules

1. **Strict Dependency Flow**:
   - `WebAPI` -> `Engine`, `ConnectedService`, `DAL`
   - `Engine` -> `DAL` (for models/workspace)
   - `ConnectedService` -> `Engine` (for domain models)
   - `DAL` -> No dependency on higher layers.
2. **DTO Mapping**: Domain entities in `EduDataGen.Engine` MUST NOT be directly exposed in REST API response bodies if internal fields or logic differ. Use explicit API Request/Response DTOs in `EduDataGen.WebAPI`.
3. **Dependency Injection**: Use ASP.NET Core native `IServiceCollection` extension methods (e.g., `services.AddEduDataGenEngine()`). Prefer constructor injection via primary constructors or readonly fields.

---

## 4. Package Governance Policy

To maintain maintainability, security, and minimal footprint, **Microsoft native libraries are preferred by default**.

### Pre-Approved Package List
- **Microsoft Libraries**:
  - `Microsoft.AspNetCore.OpenApi`
  - `Swashbuckle.AspNetCore`
  - `Microsoft.Extensions.Configuration`
  - `Microsoft.Extensions.DependencyInjection`
  - `Microsoft.Extensions.Logging`
  - `System.Text.Json`
- **Pre-Approved Third-Party Libraries**:
  - `CsvHelper` (v33+)
  - `MathNet.Numerics` (v5+)
  - `xUnit`
  - `FluentAssertions`

### New Package Request Rule
Before adding any new 3rd-party NuGet package not listed above:
1. Explain why a Microsoft native library or existing approved library is insufficient.
2. Request approval from the project owner.

---

## 5. Exception Handling & Logging

1. **Domain Exceptions**: Define custom exception classes in `EduDataGen.Engine.Exceptions` (e.g., `InvalidBlueprintException`, `ClampingRangeException`).
2. **Web API Exception Middleware**: Use ASP.NET Core `UseExceptionHandler` or custom middleware to map domain exceptions into standardized `ProblemDetails` (RFC 7807) responses.
3. **Zero Information Leaks**: Never expose raw stack traces, DB connection strings, or API keys in REST API error responses.
4. **Structured Logging**: Use `ILogger<T>` with message templates:
   ```csharp
   _logger.LogInformation("Rendered dataset for Scenario {Scenario} with Seed {Seed}", scenario, seed);
   ```

---

## 6. Testing Standards (xUnit & FluentAssertions)

1. **Framework**: `xUnit` with `FluentAssertions` for clear assertions.
2. **Pattern**: Structure unit tests using the **Arrange-Act-Assert (AAA)** pattern:
   ```csharp
   [Fact]
   public void CalculateDelta_WithAttenuation_ShouldDiminishSecondaryDeltas()
   {
       // Arrange
       var accumulator = new AttenuatedAccumulator(gamma: 0.35);
       double[] deltas = [10.0, 5.0, 2.0];

       // Act
       double totalDelta = accumulator.Accumulate(deltas);

       // Assert
       totalDelta.Should().BeApproximately(12.22, 0.01);
   }
   ```
3. **Determinism Verification**: Tests MUST assert that given identical `Seed` values, `EduDataGen.Engine` outputs identical row values across repeated runs.
