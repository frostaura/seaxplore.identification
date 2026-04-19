# API and Plugin Model

## Public API surface

- `GET /api/search?q=...&topK=...` returns ranked semantic matches
- `GET /api/species` returns the full species catalog
- `GET /api/species/{id}` returns a single species
- `GET /api/species/categories` returns public category metadata

## Admin API surface

- `POST /api/admin/login` issues a JWT for admin users
- `GET/POST/PUT/DELETE /api/admin/species` manages species
- `GET/POST/PUT/DELETE /api/admin/attributes` manages dynamic attributes
- `GET/POST/PUT/DELETE /api/admin/categories` manages categories

All admin endpoints except login require bearer authentication.

## Plugin contract

The search subsystem depends on:

- `IPlugin` for common plugin metadata
- `IEmbeddingPlugin` for text-to-vector generation
- `ISearchService` for ranking and response shaping

`Program.cs` currently registers `TfIdfEmbeddingPlugin` as the active embedding provider. Any replacement plugin must preserve deterministic vector dimensionality and be safe to call during startup seeding and admin-triggered reindexing.
