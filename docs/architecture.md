# Architecture

## System overview

OceanCare is a full-stack application composed of:

- a **.NET 10 ASP.NET Core API** in `/src/api/OceanCare.Api`
- a **React + Vite frontend** in `/src/web/oceancare-web`
- a **SQLite database** managed through Entity Framework Core
- **Docker Compose** orchestration from `/home/runner/work/seaxplore.identification/seaxplore.identification/docker-compose.yml`

## Runtime flow

1. The frontend calls the API for public species lookup and admin operations.
2. The API loads species, categories, attributes, and stored embeddings from SQLite.
3. Search requests are delegated to the configured `IEmbeddingPlugin`.
4. The search service ranks stored species embeddings against the query embedding.
5. Admin mutations update relational data and regenerate the corresponding embedding.

## Major backend layers

- **Controllers** expose HTTP endpoints
- **Application services** hold search-specific orchestration
- **Domain models and interfaces** define data contracts and plugin seams
- **Infrastructure** contains EF Core persistence, seed data, and embedding plugins

## Operational baseline

- Local development can run the API and web app independently
- Docker Compose provides a full-stack local environment
- CI must validate backend build and tests, frontend lint/build, and compose startup smoke coverage
