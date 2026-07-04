# InlayHtmlTemplate — AGENTS.md

Proyecto de templates HTML contextuales para ASP.NET Core con escape automático anti-XSS vía interpolación de strings de C#.

Ver [README.md](README.md) para documentación completa, ejemplos y guías.

## MCPs disponibles (¡USARLOS SIEMPRE!)

| MCP | Propósito |
|-----|-----------|
| `local-rag` | RAG local sobre el códigobase |
| `lsp` | Language server (csharp-ls) para navegación de código |
| `chrome-devtools` | Depuración en navegador |
| `playwright` | Tests de integración y E2E en navegador |
| `git` | Operaciones git (historial, diff, blame) |
| `sharplens` | Análisis y refactorización de C# |
| `context7` | Contexto adicional |
| `daisyui-docs` | Documentación de DaisyUI (componentes, clases) |

**Importante:** Siempre que sea posible, usa estos MCPs en lugar de herramientas genéricas — proporcionan contexto más preciso y acciones específicas del proyecto.
