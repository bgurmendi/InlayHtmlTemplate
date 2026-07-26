# Proposed Improvements — InlayHtmlTemplate

This document captures the full set of improvement opportunities identified during the project analysis, organized by priority.

---

## Code Quality Baseline

| Metric | Result |
|--------|--------|
| Compiler errors | 0 |
| Compiler warnings | 0 |
| Analyzer warnings | 0 |
| Naming violations | 0 |
| Async-hygiene issues | 0 |
| Unused code (core) | 0 |
| God object candidates | 0 |
| Test count | ~140 |
| Mutation testing | Stryker (high=80, low=60, break=50) |

---

## 1. Core Refactoring (Priority: High)

| Area | File | Lines | Cyclomatic | Nesting | Cognitive |
|------|------|-------|-----------|---------|-----------|
| `HtmlContextAnalyzer.ProcessChar()` | `SimpleHtmlTemplate.cs:111` | 49 | 13 | 7 | 23 |
| `TemplatePlan.Analyze()` | `TemplatePlan.cs:30` | 60 | 13 | 5 | 23 |
| `TemplatePlan.TrimBooleanAttribute()` | `TemplatePlan.cs:92` | 28 | 10 | 1 | 9 |
| `TemplatePlan.RenderTo()` | `TemplatePlan.cs:122` | 23 | 6 | 4 | 12 |
| `SimpleHtmlTemplate.WriteWithContext()` | `SimpleHtmlTemplate.cs:27` | 32 | 9 | 2 | 7 |

### 1.1 `ProcessChar` — Extract state machine
The method mixes 5 logical states (outside tag, inside tag, collecting attribute name, entering quoted value, inside quoted value) in a single monolithic if/else. Extract into separate methods (`ProcessOutsideTag`, `ProcessInsideTag`, `ProcessQuotedValue`) or use an enum-based state machine with a `switch` expression.

### 1.2 `Analyze` — Split into sub-methods
The 60-line loop scans argument slots, tracks HTML context, and handles boolean attribute trimming simultaneously. Extract `ScanNextSlot(format, ref i, ...)` and `HandlePostBooleanAttribute(format, ...)`.

### 1.3 `TrimBooleanAttribute` — Simplify with spans
Five sequential reverse scans (quote → whitespace → `=` → whitespace → name → whitespace). Use `ReadOnlySpan<char>.Trim()` and single-pass forward scan instead.

### 1.4 `InlayTemplate` — Remove async duplication
`ExecuteResultAsync` and `ExecuteAsync` are ~90% identical (both buffer to `StringBuilder` via `StringWriter`, then `Response.WriteAsync`). Extract a common `RenderToString()` method.

### 1.5 `RenderTo` — Simplify loop
The main render loop mixes boolean attribute rendering with `WriteWithContext` dispatch. Separate the boolean case and the generic case.

### 1.6 `DeferredHtml.ToString()` — CA1822 candidate
The project health report flags one `CA1822` (member can be made static). However, `ToString()` is an override, so the warning likely refers to a different member — verify and fix.

---

## 2. Security (Priority: High)

### 2.1 Event handler attributes (`on*`) unprotected
**File**: `SimpleHtmlTemplate.cs` (`HtmlContextAnalyzer` + `WriteWithContext`)

**Issue**: Attributes like `onclick`, `onmouseover`, `onload`, `onerror` etc. are treated as generic `Attribute` context. While HTML encoding prevents tag breakout (`<`, `>`), a value like `javascript:alert(1)` inside an event handler is still dangerous.

**Proposal**:
1. Detect `on*` attribute names in `HtmlContextAnalyzer`
2. Add `HtmlContext.EventHandler` to the enum
3. In `WriteWithContext`, strip or block `javascript:` and other dangerous URL schemes in event handler contexts

### 2.2 `HtmlContext.Script` defined but never implemented
**File**: `SimpleHtmlTemplate.cs:67-79`

**Issue**: The enum has a `Script` value but it's never assigned by the analyzer.

**Proposal**: Either:
- Detect `<script>` tags and apply script-context escaping (escape `</script>`, `<!--`, etc.), or
- Detect `<script>` and skip escaping entirely (trust the content), but at least track the context correctly to avoid false positives from other logic.

### 2.3 CSS/style attribute context missing
**File**: `SimpleHtmlTemplate.cs`

**Issue**: `style` attributes are treated as generic `Attribute` context.

**Proposal**: Detect `style` attribute and apply CSS-context escaping:
- Escape CSS special characters (`url()`, `expression()`, `javascript:`)
- Block `url(javascript:...)` patterns

### 2.4 URL space encoding fragility
**File**: `SimpleHtmlTemplate.cs:53-54`

**Issue**: The current approach encodes first (producing `&lt;` etc.) then replaces ` ` with `%20`. If the encoder output could theoretically contain a space in an entity, this would be incorrect as a secondary mutation.

**Proposal**: Build the encoded output character by character, escaping `<>&\"'` with HTML entities and encoding spaces as `%20` in a single pass, or use a `StringBuilder`-based approach.

---

## 3. Performance (Priority: Medium)

### 3.1 Cache `javascript:` check in UrlAttribute

**File**: `SimpleHtmlTemplate.cs:47`

**Issue**: `value.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)` runs on every render for every UrlAttribute slot. This can be hoisted to `TemplatePlan.Analyze()` — the analyzer already knows which attributes are URL attributes and whether the literal prefix contains `javascript:`.

### 3.2 `StringBuilder` pooling in async pipeline

**File**: `InlayTemplate.cs:32-36`

**Issue**: `ExecuteResultAsync` and `ExecuteAsync` allocate a new `StringBuilder` and `StringWriter` on every request.

**Proposal**: Use `StringBuilderPool` (from `Microsoft.Extensions.ObjectPool`) or reuse `StringBuilder` via `ZString`'s pool. Alternatively, render directly to a pooled `char[]` buffer.

### 3.3 `IHtmlContent` early dispatch

**File**: `SimpleHtmlTemplate.cs:32`

**Issue**: `arg is IHtmlContent` check inside `WriteWithContext` is fine, but it's called for every non-boolean arg during render. If the argument type is known at cache-analysis time (e.g., it's an `IHtmlContent` from `Inlay.If`, `Inlay.Each`), this could be pre-dispatched.

### 3.4 `ToList()` materialization in `Inlay.Each`

**File**: `Inlay.cs:68, 103`

**Issue**: The `empty` overloads materialize the enumerable to `IReadOnlyList<T>` to check `Count == 0`. For large collections this is fine (one allocation). For streaming scenarios, consider `IBufferWriter<T>` or `IEnumerator`-based empty detection with fallback.

---

## 4. Components & DaisyUI (Priority: Medium)

### 4.1 Unused fluent methods in `Field`

**File**: `src/InlayHtmlTemplate.Components/Field.cs:22-24`

**Issue**: `SetId()`, `SetError()`, `SetVariant()` are public but never called anywhere in the codebase (including samples and tests).

**Proposal**: Either:
- Remove them (they add surface area without value), or
- Integrate them into the builder pattern and add usage + tests.

### 4.2 Grid CSS regenerated on every call

**File**: `src/InlayHtmlTemplate.Components/Grid.cs`

**Issue**: The `<style>` block containing responsive CSS rules is generated from scratch each time `Render()` is called.

**Proposal**: Cache the generated CSS per breakpoint configuration. Use a `ConcurrentDictionary` keyed by a hash of breakpoint settings.

### 4.3 DaisyUI variant documentation gaps

**Files**: `src/InlayHtmlTemplate.DaisyUI/Variants.cs`, `Daisy.Input.cs`

**Issue**: `InputVariant`, `InputSize`, and other variant enums exist but are not documented in `docs/`.

**Proposal**: Add API documentation entries for variant enums and their effects.

### 4.4 Self-hosted/CDN alternative for DaisyUI + Tailwind

**File**: `src/InlayHtmlTemplate.DaisyUI/DaisyLayout.cs`

**Issue**: `DaisyLayout` loads DaisyUI and Tailwind from CDN. No `Subresource Integrity` attributes are used, and there is no self-hosted fallback.

**Proposal**: Add optional SRI hashes, or provide an overload that accepts a custom stylesheet path.

### 4.5 Edge case tests for DaisyUI

**Files**: `tests/InlayHtmlTemplate.DaisyUI.Tests/`

**Issue**: 56 tests cover the main happy paths but miss:
- Ghost variants combined with other modifiers
- Disabled + variant combinations
- All 9 toast positions
- Responsive layout collapse behavior
- Empty/null content in all components

---

## 5. Testing (Priority: Medium)

### 5.1 Direct `DeferredHtml` tests

**Issue**: `DeferredHtml.WriteTo()` and `ToString()` are public but have no direct unit tests — they are only exercised indirectly through `Inlay.Each`.

**Proposal**: Add dedicated tests calling `WriteTo` and `ToString` directly with controlled `Action<TextWriter, HtmlEncoder>`.

### 5.2 ASP.NET Core integration tests

**Issue**: `InlayTemplate` implements `IActionResult` / `IResult`, but there are no tests using `WebApplicationFactory` to verify the HTTP response pipeline end-to-end.

**Proposal**: Add a test project (or extend existing) with:
- MVC controller returning `InlayTemplate`
- Minimal API endpoint returning `InlayTemplate` (as `IResult`)
- Assertions on response body, content type, and XSS safety

### 5.3 Benchmark project

**Issue**: Performance claims in the README (zero-copy, cached analysis, deferred rendering) are not backed by published benchmarks.

**Proposal**: Create a `/benchmarks/` project using `BenchmarkDotNet` comparing:
- InlayHtmlTemplate vs Razor vs string concatenation
- Cached vs cold rendering
- Memory allocation (GC pressure) per render

### 5.4 Property-based testing

**Issue**: The escaping logic is tested with specific examples but not with randomized inputs.

**Proposal**: Use FsCheck or `AutoFixture` to generate random strings with XSS payloads and verify:
- Output is always safe (no unescaped `<`, `>`, `"`, `&`)
- `javascript:` is always blocked in URL contexts

### 5.5 Raise Stryker threshold

**Issue**: Current thresholds: high=80, low=60, break=50. The project already has good coverage.

**Proposal**: Raise to high=85, low=70, break=60 to enforce higher quality over time.

---

## 6. Documentation & Developer Experience (Priority: Low)

### 6.1 Razor → Inlay migration guide

**Issue**: Developers coming from Razor have no reference for translating their knowledge.

**Proposal**: Create `docs/migration-guide.md` with:
- Equivalence table (Razor ↔ Inlay)
- Step-by-step migration of `_Layout.cshtml`, `_ViewImports`, partials
- Comparison of `@section` vs function slot parameters

### 6.2 `CHANGELOG.md`

**Issue**: No change history for NuGet consumers to track what changed between versions.

**Proposal**: Create `/CHANGELOG.md` documenting all releases:
- v0.1.2: UrlAttribute encoding fix, AGENTS.md
- v0.1.1: DaisyUI + Components, boolean attributes, release tooling
- v0.1.0: Initial release

### 6.3 PR template

**Issue**: No standardized pull request template for contributors.

**Proposal**: Create `/.github/PULL_REQUEST_TEMPLATE.md` with sections for summary, change type, checklist, and breaking changes.

### 6.4 Performance benchmarks in README

**Issue**: The README makes strong performance claims but shows no data.

**Proposal**: After implementing Phase 5.3, add a benchmark comparison table to the README's Performance section.

### 6.5 Recommended global usings

**Issue**: New users don't know what using directives to add.

**Proposal**: Add a section to `docs/getting-started.md` showing recommended `global using` statements:
```csharp
global using InlayHtmlTemplate;
global using static InlayHtmlTemplate.Inlay;
```

---

## 7. Infrastructure (Priority: Low)

### 7.1 `.editorconfig`

**Issue**: No project-wide style rules. Analyzers use default settings.

**Proposal**: Create `/.editorconfig` with:
- Indentation: spaces, size 4
- Naming: PascalCase for types/methods/properties, camelCase for parameters/locals, `_camelCase` for private fields
- `dotnet_diagnostic.CS1591.severity = none` (suppress missing doc warnings)
- `dotnet_analyzer_diagnostic.category-Security.severity = warning`

### 7.2 `dotnet format` in CI

**Issue**: Code style violations are not detected automatically.

**Proposal**: Add `dotnet format InlayHtmlTemplate.sln --verify-no-changes` to the CI workflow after the build step.

### 7.3 Centralized `Directory.Build.props`

**Issue**: 8 `.csproj` files repeat the same properties:
- `Nullable`, `ImplicitUsings` (all 8 files)
- `Authors`, `License`, `RepositoryUrl`, `RepositoryType` (3 src files)
- `GenerateDocumentationFile`, `IncludeSymbols`, `SymbolPackageFormat` (3 src files)
- Same xunit packages + versions (3 test files)

**Proposal**: Create two `Directory.Build.props` files:
- `/Directory.Build.props` (root): shared properties across all projects
- `/src/Directory.Build.props`: NuGet packaging properties for source projects

---

## Priority Summary

| Priority | Area | Effort | Impact |
|----------|------|--------|--------|
| **High** | Core refactor (state machine, duplication) | 2-3 days | Maintainability |
| **High** | Security (event handlers, script, CSS, URL) | 1-2 days | Security posture |
| **Medium** | Performance caches + StringBuilder pooling | 1 day | Throughput & GC |
| **Medium** | Testing (DeferredHtml, integration, benchmarks, property-based) | 2 days | Release confidence |
| **Medium** | Components & DaisyUI (cleanup, cache, tests, docs) | 1 day | API quality |
| **Low** | Documentation & DX (migration guide, CHANGELOG, PR template) | 0.5 days | Contributor experience |
| **Low** | Infrastructure (.editorconfig, CI format check, Directory.Build.props) | 0.5 days | Consistency |

---

## Phased Implementation Roadmap

```
Phase 1 (Low)     → Infrastructure & DX     → .editorconfig, Directory.Build.props, 
                      CHANGELOG, PR template, migration guide, format in CI
Phase 2 (High)    → Core Refactoring        → ProcessChar state machine, async dedup,
                      Analyze/Trim split
Phase 3 (High)    → Security                → Event handlers, Script/CSS contexts,
                      URL encoding robustness
Phase 4 (Medium)  → Performance             → UrlAttribute cache, StringBuilder pool
Phase 5 (Medium)  → Testing                 → DeferredHtml tests, integration tests,
                      benchmarks, property-based, Stryker threshold
Phase 6 (Medium)  → Components & DaisyUI   → Field cleanup, Grid CSS cache,
                      edge case tests, SRI
```

Each phase is self-contained and independently reviewable.
