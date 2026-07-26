# Improvement Plan — InlayHtmlTemplate

## Overview

Plan de mejoras priorizado para InlayHtmlTemplate. Cada fase implementa un conjunto de cambios autónomo, ordenado por impacto/esfuerzo.

---

## Phase 1: Infraestructura y DX (Prioridad Baja)

Duración estimada: ~1 día.   
Objetivo: estandarizar configuración, reducir duplicación en csproj, mejorar experiencia de contribución y documentar cambios.

### 1.1 `Directory.Build.props` centralizado

**Problema**: 8 `.csproj` repiten propiedades idénticas. Los 3 de `src/` tienen bloques NuGet casi iguales. Los 3 de `tests/` tienen los mismos PackageReferences.

**Archivos a crear**:

`/Directory.Build.props` (raíz):
```xml
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <Authors>InlayHtmlTemplate Contributors</Authors>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryType>git</RepositoryType>
    <RepositoryUrl>https://github.com/bgurmendi/InlayHtmlTemplate.git</RepositoryUrl>
  </PropertyGroup>
</Project>
```

`/src/Directory.Build.props`:
```xml
<Project>
  <PropertyGroup>
    <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
  </PropertyGroup>
</Project>
```

**Archivos a modificar** (eliminar propiedades ahora heredadas):

| Archivo | Propiedades a eliminar |
|---------|----------------------|
| `src/InlayHtmlTemplate/InlayHtmlTemplate.csproj` | Nullable, ImplicitUsings, Authors, License, RepositoryType, Url, GenerateDoc, IncludeSymbols, SymbolFormat |
| `src/InlayHtmlTemplate.DaisyUI/InlayHtmlTemplate.DaisyUI.csproj` | Ídem |
| `src/InlayHtmlTemplate.Components/InlayHtmlTemplate.Components.csproj` | Ídem |
| `tests/InlayHtmlTemplate.Tests/InlayHtmlTemplate.Tests.csproj` | Nullable, ImplicitUsings |
| `tests/InlayHtmlTemplate.DaisyUI.Tests/InlayHtmlTemplate.DaisyUI.Tests.csproj` | Ídem |
| `tests/InlayHtmlTemplate.Components.Tests/InlayHtmlTemplate.Components.Tests.csproj` | Ídem |
| `samples/WebApp/WebApp.csproj` | Nullable, ImplicitUsings |
| `samples/DaisyShowcase/DaisyShowcase.csproj` | Ídem |

### 1.2 `.editorconfig`

**Problema**: No hay reglas de estilo. Los analyzers usan defaults.

**Crear**: `/.editorconfig`
- `indent_style = space`, `indent_size = 4`
- `charset = utf-8-bom`
- `trim_trailing_whitespace = true`
- `insert_final_newline = true`
- `dotnet_naming_rule` para tipos/métodos/propiedades `PascalCase`
- `dotnet_naming_rule` para parámetros/locales `camelCase`
- `dotnet_naming_rule` para campos privados `_camelCase`
- `dotnet_diagnostic.CS1591.severity = none` (evitar NoWarn manual en DaisyUI/Components)
- `dotnet_analyzer_diagnostic.category-Security.severity = warning`

### 1.3 `dotnet format` en CI

**Problema**: No se verifica estilo automáticamente.

**Modificar**: `/.github/workflows/ci.yml` — añadir paso después de build:
```yaml
- name: Format check
  run: dotnet format InlayHtmlTemplate.sln --verify-no-changes
```

### 1.4 `CHANGELOG.md`

**Crear**: `/CHANGELOG.md` con releases extraídas de `git log`:
- v0.1.2 — fix UrlAttribute encoding, AGENTS.md
- v0.1.1 — Components + DaisyUI, boolean attributes, release automation
- v0.1.0 — release inicial

### 1.5 PR Template

**Crear**: `/.github/PULL_REQUEST_TEMPLATE.md`
- Resumen del cambio
- Tipo (fix/feat/docs/refactor)
- Checklist (tests pasan, warnings, CHANGELOG actualizado)
- Breaking changes

### 1.6 Guía de migración Razor → Inlay

**Crear**: `/docs/migration-guide.md`
- Tabla de equivalencias Razor ↔ Inlay
- Ejemplo paso a paso de migración de _Layout.cshtml a función Layout()
- Eliminar dependencia de _ViewImports y _ViewStart

### 1.7 Global usings recomendados

**Modificar**: `/docs/getting-started.md` — añadir sección:
```csharp
global using InlayHtmlTemplate;
global using static InlayHtmlTemplate.Inlay;
```

---

## Phase 2: Refactorización del Core (Prioridad Alta)

Duración estimada: ~2-3 días.   
Objetivo: reducir complejidad ciclomática, eliminar duplicación, mejorar mantenibilidad.

### 2.1 State machine `HtmlContextAnalyzer.ProcessChar()` (CC=13, nesting=7)

**Archivo**: `src/InlayHtmlTemplate/SimpleHtmlTemplate.cs:111`

**Problema**: Método monolítico de 49 líneas con 7 niveles de anidamiento. Difícil de seguir y modificar.

**Propuesta**: Extraer estados en métodos separados o usar un switch exhaustivo:

```csharp
public void ProcessChar(char c)
{
    if (!_inTag) { ProcessOutsideTag(c); return; }
    ProcessInsideTag(c);
}

private void ProcessOutsideTag(char c)
{
    if (c == '<') _inTag = true;
}

private void ProcessInsideTag(char c)
{
    if (_quoteChar != '\0') { ProcessQuoted(c); return; }
    switch (c)
    {
        case '>': ExitTag(); break;
        case '\'' or '"': EnterQuote(c); break;
        case '=': EnterAttribute(); break;
        case var ws when char.IsWhiteSpace(ws): ResetAttribute(); break;
        case var letter when char.IsLetter(letter): BuildAttributeName(letter); break;
    }
}
```

### 2.2 Duplicación `InlayTemplate.ExecuteResultAsync` / `ExecuteAsync` (~90% igual)

**Archivo**: `src/InlayHtmlTemplate/InlayTemplate.cs:28-51`

**Problema**: Dos métodos casi idénticos que bufferizan a StringBuilder y escriben async.

**Propuesta**: Extraer helper común:

```csharp
private string RenderToString()
{
    var sb = new StringBuilder();
    using (var writer = new StringWriter(sb))
        WriteTo(writer, HtmlEncoder.Default);
    return sb.ToString();
}
```

Ambos métodos async llaman a `RenderToString()` + `response.WriteAsync(...)`.

### 2.3 `TemplatePlan.Analyze()` (CC=13, nesting=5)

**Archivo**: `src/InlayHtmlTemplate/TemplatePlan.cs:30`

**Problema**: Bucle único de 60 líneas que mezcla scanning de slots, análisis de contexto, trimming de boolean attributes.

**Propuesta**: Extraer `ScanArgSlot()` y `HandleBooleanAttributeTrailing()`:

```csharp
private static void ScanArgSlot(string format, ref int i, ...) { ... }
private static void HandlePostBooleanAttribute(string format, ...) { ... }
```

### 2.4 `TemplatePlan.TrimBooleanAttribute()` (CC=10)

**Archivo**: `src/InlayHtmlTemplate/TemplatePlan.cs:92`

**Problema**: Lógica de trimming con 5 bucles/pases hacia atrás.

**Propuesta**: Usar `ReadOnlySpan<char>` y `MemoryExtensions` para simplificar:

```csharp
var span = format.AsSpan(litStart, litLength);
// encontrar '=', trim whitespace, todo en un solo pase hacia adelante
```

---

## Phase 3: Seguridad (Prioridad Alta)

Duración estimada: ~1-2 días.   
Objetivo: cubrir vectores XSS no protegidos actualmente.

### 3.1 Event handlers (`on*`) sin protección específica

**Archivo**: `src/InlayHtmlTemplate/SimpleHtmlTemplate.cs` (HtmlContextAnalyzer + WriteWithContext)

**Problema**: Atributos como `onclick`, `onmouseover` se tratan como `Attribute` genérico. Aunque HTML-encodea `<>&\"'`, un `javascript:` sigue siendo peligroso.

**Propuesta**: 
1. Añadir detección de atributos `on*` en `HtmlContextAnalyzer`
2. Añadir `HtmlContext.EventHandler` al enum
3. En `WriteWithContext`: si contexto `EventHandler`, bloquear `javascript:` y cualquier contenido que no sean caracteres seguros

### 3.2 `HtmlContext.Script` definido pero no implementado

**Archivo**: `src/InlayHtmlTemplate/SimpleHtmlTemplate.cs:67-79`

**Problema**: El enum `HtmlContext` tiene `Script` pero nunca se asigna.

**Propuesta**: 
1. Detectar `<script>` en `ProcessChar`
2. Implementar escaping Script: escapar `</script>`, `<!--`, `<script`, `-->`
3. O alternativamente, ignorar contenido dentro de `<script>` (no escapar nada, pero al menos trackear el contexto correctamente)

### 3.3 Contexto CSS/style no detectado

**Archivo**: `src/InlayHtmlTemplate/SimpleHtmlTemplate.cs`

**Problema**: Atributos `style` se tratan como `Attribute` genérico.

**Propuesta**: 
1. Detectar `style` como contexto CSS
2. Aplicar escaping CSS (escapar `url()`, `expression()`...)

### 3.4 URL encoding de espacios frágil

**Archivo**: `src/InlayHtmlTemplate/SimpleHtmlTemplate.cs:53-54`

**Problema**: `encoder.Encode()` codifica `<>&\"'` pero no espacios. Luego se busca `' '` y se reemplaza con `%20`. Si `encoder.Encode()` genera `&lt;` que contiene espacio en otro encoding... edge case.

**Propuesta**: Normalizar: primero `encoder.Encode(value)` → luego `Replace(" ", "%20")`. Mejor aún: escanear una vez con StringBuilder.

---

## Phase 4: Rendimiento (Prioridad Media)

Duración estimada: ~1 día.   
Objetivo: optimizar hotspots de rendimiento identificados.

### 4.1 Cache del check javascript: en UrlAttribute

**Archivo**: `src/InlayHtmlTemplate/SimpleHtmlTemplate.cs:47`

**Propuesta**: Mover el flag `isJavaScriptUrl` a `TemplatePlan` (analizado una vez en cache, no en cada render).

### 4.2 StringBuilderPool en InlayTemplate async

**Archivo**: `src/InlayHtmlTemplate/InlayTemplate.cs:32-36`

**Propuesta**: Usar `StringBuilderPool` o reutilizar `StringBuilder` vía `ZString` para reducir allocaciones en el pipeline async.

### 4.3 Cache de `ToList()` en `Inlay.Each`

**Archivo**: `src/InlayHtmlTemplate/Inlay.cs:68,103`

**Propuesta**: Ya se convierte a `IReadOnlyList<T>`. Considerar `IBufferWriter<T>` para streams muy grandes.

---

## Phase 5: Testing (Prioridad Media)

Duración estimada: ~2 días.   
Objetivo: cerrar gaps de cobertura, añadir tests de integración y benchmarks.

### 5.1 Tests directos para `DeferredHtml`

**Problema**: `DeferredHtml.ToString()` y `WriteTo()` son públicos pero no tienen tests directos (solo se ejercitan a través de `Inlay.Each`).

### 5.2 Tests de integración ASP.NET Core

Usar `WebApplicationFactory` para probar `IActionResult` / `IResult` real en pipeline MVC y minimal APIs.

### 5.3 Benchmark project with BenchmarkDotNet

Verificar claims de rendimiento: zero-copy, cached analysis, deferred rendering.

### 5.4 Property-based testing

Usar FsCheck o `AutoFixture` para probar escaping con entradas aleatorias maliciosas.

### 5.5 Stryker mutation threshold

Subir de high=80 a high=85-90.

---

## Phase 6: Components y DaisyUI (Prioridad Media)

Duración estimada: ~1 día.   
Objetivo: limpiar API, mejorar testing y documentación.

### 6.1 Métodos no usados en `Field`

`Field.SetId()`, `SetError()`, `SetVariant()` — eliminarlos o integrarlos realmente.

### 6.2 Cache del CSS generado por Grid

El `<style>` de Grid se regenera en cada llamada. Cachear por breakpoint configurado.

### 6.3 Tests de edge cases en DaisyUI

- Variantes ghost, estados disabled combinados, layouts responsivos.

### 6.4 Documentación de variantes DaisyUI

Faltan docs de `InputVariant`, `InputSize`, etc. a nivel de API.

### 6.5 SRI para CDN DaisyUI/Tailwind

Ofrecer `Subresource Integrity` hashes para los CDN usados en `DaisyLayout.cs`.

---

## Resumen de archivos por fase

| Fase | Crear | Modificar |
|------|-------|-----------|
| **P1** Infraestructura | `Directory.Build.props` (raíz), `src/Directory.Build.props`, `.editorconfig`, `CHANGELOG.md`, `.github/PULL_REQUEST_TEMPLATE.md`, `docs/migration-guide.md` | 8 csproj, `ci.yml`, `docs/getting-started.md` |
| **P2** Refactor Core | — | `SimpleHtmlTemplate.cs`, `InlayTemplate.cs`, `TemplatePlan.cs` |
| **P3** Seguridad | — | `SimpleHtmlTemplate.cs` (HtmlContextAnalyzer + WriteWithContext), `TemplatePlan.cs` |
| **P4** Rendimiento | — | `SimpleHtmlTemplate.cs`, `InlayTemplate.cs`, `Inlay.cs` |
| **P5** Testing | `Benchmarks/`, tests varios | tests existentes, `stryker-config.json` |
| **P6** Components | — | `Field.cs`, `Grid.cs`, tests DaisyUI |

---

## Orden de implementación sugerido

```
P1 → P2 → P3 → P4 → P5 → P6
     └── P2 y P3 pueden solaparse (mismos archivos)
```

Cada fase es autónoma y revisable independientemente.
