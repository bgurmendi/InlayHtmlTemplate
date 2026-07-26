# Changelog

## v0.1.2 (2026-06-19)

- UrlAttribute encoding fix — space → %20 conversion
- AGENTS.md for AI-assisted development
- Improved enum naming in Variants (DaisyUI)
- Stryker mutation testing config

## v0.1.1 (2026-06-18)

- DaisyUI component library (Alert, Badge, Button, Card, Check, Footer, FormControl, Hero, Input, Modal, Navbar, Stats, Table, Toast)
- Layout components (DaisyLayout, DaisySidebarLayout)
- Components library (Field, Grid)
- Boolean attribute support (disabled, checked, selected, etc.)
- `Inlay.Each` with empty/fallback overloads
- Release tooling (`cmd/release_new_version.sh`)

## v0.1.0 (2026-06-17)

- Initial release
- Context-aware HTML escaping (Content, Attribute, UrlAttribute, BooleanAttribute)
- `Inlay.Template`, `Inlay.If`, `Inlay.Css`, `Inlay.Raw`
- `InlayTemplate` implements `IHtmlContent`, `IActionResult`, `IResult`
- Template analysis caching via `ConcurrentDictionary`
