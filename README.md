<p align="center">
  <img src="https://github.com/user-attachments/assets/2aa3817b-f4a4-4dc1-92bf-a44147e7b45b" alt="OceanCare" width="700" />
</p>

<h1 align="center">🐠 OceanCare</h1>
<h3 align="center">Semantic Search Engine for Marine Life</h3>

---

[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-blue.svg)](https://react.dev/)
[![Redux](https://img.shields.io/badge/Redux-Toolkit-764ABC.svg)](https://redux-toolkit.js.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5-blue.svg)](https://www.typescriptlang.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## What is OceanCare?

OceanCare is a full-stack semantic search engine for marine life — corals, fish, jellyfish, sea turtles, and more. Describe what you're looking for in plain natural language and OceanCare finds the closest semantic matches in real time.

### Example queries

- *"a red coral with branching antler-like tips"*
- *"a small colorful fish with orange and white stripes"*
- *"a bioluminescent creature that glows in the dark ocean"*
- *"a large sea turtle with a beautifully patterned shell"*

Results refine as you type — narrowing from broad descriptions to exact matches.

---

## Features

- 🔍 **Semantic Search** — Natural language queries matched via cosine-similarity over TF-IDF embeddings (pluggable for OpenAI/Ollama)
- ⚡ **Real-time results** — Search results update as you type with 400ms debounce
- 🎴 **Rich species cards** — Images, similarity scores, category tags, and dynamic attributes
- 🔌 **Plugin architecture** — Swap the embedding model without changing application code
- 🔐 **Admin panel** — JWT-authenticated management for species, attributes, and categories
- 🏷️ **Dynamic attributes** — Every marine species attribute is configurable (Color, Pattern, Depth, Bioluminescent, etc.)
- 🐳 **Docker-ready** — Full stack runs with a single `docker-compose up`

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | React 18, Vite, Redux Toolkit, TypeScript |
| Backend | .NET 10, ASP.NET Core Web API |
| Database | SQLite via Entity Framework Core |
| Auth | JWT Bearer tokens |
| Embeddings | TF-IDF cosine similarity (pluggable) |
| Deployment | Docker Compose |

---

## Quick Start

### Option 1 — Docker Compose (recommended)

```bash
docker-compose up --build
```

- Frontend: http://localhost:3000
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

### Option 2 — Local development

**API:**
```bash
cd src/api/OceanCare.Api
dotnet run
# API runs at http://localhost:5103
```

**Frontend:**
```bash
cd src/web/oceancare-web
npm install
npm run dev
# UI runs at http://localhost:5173
```

---

## Default Admin Credentials

| Field | Value |
|---|---|
| Username | `admin` |
| Password | `OceanCare2024!` |

> ⚠️ Change the JWT key and admin password before deploying to production.

---

## Seed Data

OceanCare ships with **16 real marine species** across 4 categories:

| Category | Species |
|---|---|
| 🪸 Coral | Brain Coral, Staghorn Coral, Red Sea Whip Coral, Blue Coral, Mushroom Coral |
| 🐠 Fish | Clownfish, Mandarin Fish, Lionfish, Blue Tang, Bioluminescent Dragonfish, Parrotfish |
| 🪼 Jellyfish | Moon Jellyfish, Pacific Sea Nettle, Comb Jelly |
| 🐢 Sea Turtle | Green Sea Turtle, Hawksbill Sea Turtle |

Each species has **8 dynamic attributes**: Color, Pattern, Size, Depth Range, Habitat, Diet, Bioluminescent, Conservation Status.

---

## Plugin Architecture

Swap the embedding engine by implementing `IEmbeddingPlugin`:

```csharp
public class MyEmbeddingPlugin : IEmbeddingPlugin
{
    public string Name => "My Custom Embeddings";
    public string Version => "1.0.0";
    public string Description => "Uses my preferred embedding model.";
    public int EmbeddingDimension => 1536;

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        // Call OpenAI, Ollama, HuggingFace, etc.
    }
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddSingleton<IEmbeddingPlugin, MyEmbeddingPlugin>();
```

---

## Screenshots

### Search Interface
![Search](https://github.com/user-attachments/assets/4a0bf128-2274-434e-bf43-bf15afe8078b)

### Admin Panel
![Admin](https://github.com/user-attachments/assets/448fae3c-939b-4e34-b9d4-16af0ed91621)

---

## Project Structure

```
src/
  api/OceanCare.Api/
    Domain/
      Models/          ← Species, Category, MarineAttribute, SearchEmbedding, Admin
      Interfaces/      ← IPlugin, IEmbeddingPlugin, ISearchService
    Infrastructure/
      Data/            ← EF Core DbContext + DataSeeder
      Plugins/         ← TfIdfEmbeddingPlugin (built-in)
    Application/
      Services/        ← SearchService (cosine similarity)
    Controllers/       ← Search, Species, Admin
    Models/DTOs/       ← Request/response DTOs
  web/oceancare-web/
    src/
      components/
        atoms/         ← AttributeBadge
        molecules/     ← SpeciesCard
        organisms/     ← SearchBar, SpeciesForm, AttributeManager
      pages/           ← SearchPage, LoginPage, AdminPage
      store/           ← Redux slices (search, auth)
      services/        ← Axios API client
      types/           ← TypeScript interfaces
docker-compose.yml
```

---

<p align="center"><i>Powered by Gaia · Built with ❤️ for the ocean</i></p>
