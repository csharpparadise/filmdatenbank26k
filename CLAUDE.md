# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Development (hot reload)
dotnet watch run --project FilmDatenBank/FilmDatenBank.csproj

# Build
dotnet build FilmDatenBank/FilmDatenBank.csproj

# Publish (production)
dotnet publish -c Release -o /app/publish FilmDatenBank/FilmDatenBank.csproj

# Add a new EF Core migration
dotnet ef migrations add <MigrationName> --project FilmDatenBank/FilmDatenBank.csproj

# One-time SQL Server → SQLite migration tool
cd SqlMigration && dotnet run
```

App runs on `http://localhost:5000` in development. Default login: `admin / changeme` (auto-created on first run if no users exist).

There are no automated tests.

## Architecture

**Stack:** .NET 8 + Blazor Server + EF Core 8 + SQLite, deployed as a Docker container on Azure Web App for Containers.

### Critical Constraints

- **EF Core packages must be pinned to `8.x`** — NuGet auto-resolves to `10.x` which is incompatible. Current pin: `8.0.13`.
- **Use `IDbContextFactory<AppDbContext>`**, never inject `AppDbContext` directly — Blazor Server's component lifecycle requires scoped factories to avoid concurrency issues.
- **No `UseHttpsRedirection`** — TLS is terminated by the Azure proxy.

### Data Flow

`Components/Pages/*.razor` → `IFilmService` (via DI) → `FilmService` → `IDbContextFactory` → fresh `AppDbContext` per operation → SQLite

Every `FilmService` method calls `dbFactory.CreateDbContextAsync()` and disposes the context immediately after. Never reuse a context across methods.

### Key Services

- **`FilmService`** — all CRUD for Films, Genres, Ablagen. Search is done via `SuchePagedAsync(FilmSucheParameter)` returning `PagedResult<Film>` (20 items/page).
- **`TmdbService`** — queries TMDB API for posters, trailers, metadata. API key in `appsettings.json`.
- **`ToastService`** — lightweight async notification service. Avoid `Task.Run`/nested `InvokeAsync` inside it.

### Authentication

Cookie-based auth with scheme `"fdb_auth"`. All routes except `/login` require authentication (fallback policy). Login/logout handled by minimal API endpoints at `POST /api/auth/login` and `POST /api/auth/logout`.

### Database

- **Dev:** `<ContentRoot>/filmdatenbank.db`
- **Prod (Azure):** `$HOME/data/filmdatenbank.db` (Azure persistent storage mounted at `/home/data`)
- Schema is auto-migrated via `db.Database.MigrateAsync()` at startup.
- Unique index on `InternNummer` (nullable, filtered). Many-to-many `Film ↔ Genre` via `tFilmGenre` join table.

### UI / Theming (Parchemin theme)

The design system uses a **light** warm cream/burgundy palette with a **dark sidebar**:

- `wwwroot/app.css` — global CSS variables (`--bg`, `--accent`, `--border`, etc.). Light theme: `#F2EDE3` bg, `#7B2C34` accent.
- `NavMenu.razor.css` — **hardcoded colors only** (not CSS vars) because the sidebar stays dark regardless of theme.
- Fonts: Playfair Display (headings) + DM Sans (body), loaded via Google Fonts in `App.razor`.
- Disc type badges use scoped CSS classes `disc-dvd / bluray / 4k / vhs` with light-bg-appropriate colors.
- Component-level styles use `.razor.css` scoped files (e.g., `Filme.razor.css`, `FilmDetails.razor.css`).

### Shared Components

- **`FilmFormular.razor`** — reused by both `FilmHinzufuegen.razor` and `FilmBearbeiten.razor`.
- **`DeleteConfirmation.razor`** — reusable delete modal with backdrop-blur overlay.
- **`Toast.razor`** — renders notifications from `ToastService`.

### Helpers

`Helpers/FilmHelpers.cs` contains static methods: `MimeType()`, `DiscTypeClass()`, `DiscTypeLabel()`. Add disc-type presentation logic here, not in components.

## AI Agents

A `ux-designer` agent is configured at `.claude/agents/ux-designer.md` — use it for UI/UX design tasks (accessibility, layout, visual hierarchy).
