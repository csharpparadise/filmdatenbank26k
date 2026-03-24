# FilmDatenBank

A modern film database management application built with .NET 8 and Blazor Server, designed for organizing and cataloging personal film collections with poster artwork, metadata, and disc type tracking.

## Features

- **Film Management** – Add, edit, and delete films with comprehensive metadata (title, year, disc type, internal number, notes)
- **Search & Filtering** – Paginated search with 20 items per page
- **Genre Organization** – Categorize films by genre with many-to-many relationships
- **Poster Artwork** – Automatic poster fetching from TMDB API
- **Disc Type Support** – Track films by format: DVD, Blu-ray, 4K, VHS
- **User Authentication** – Cookie-based auth with role management
- **Responsive UI** – Warm, accessible light theme with dark sidebar navigation (Parchemin design system)
- **Cloud Ready** – Deployed as Docker container on Azure Web App for Containers

## Technology Stack

- **Framework:** .NET 8 with Blazor Server
- **Database:** SQLite with Entity Framework Core 8
- **UI Components:** Bootstrap + custom Blazor components
- **Styling:** CSS variables, scoped component styles, Google Fonts (Playfair Display + DM Sans)
- **API Integration:** TMDB (metadata & poster retrieval)
- **Deployment:** Docker container on Azure Web App

## Project Structure

```
FilmDatenBank/
├── Components/
│   ├── Pages/
│   │   ├── Filme.razor              # Main film list & search
│   │   ├── FilmHinzufuegen.razor    # Add new film
│   │   ├── FilmBearbeiten.razor     # Edit existing film
│   │   └── Login.razor              # Authentication
│   ├── FilmFormular.razor           # Shared form (Add & Edit)
│   ├── DeleteConfirmation.razor     # Reusable delete modal
│   └── Toast.razor                  # Notification system
├── Data/
│   ├── Models/
│   │   └── Film.cs
│   ├── AppDbContext.cs
│   └── Migrations/
├── Services/
│   ├── IFilmService.cs
│   ├── FilmService.cs
│   ├── TmdbService.cs               # TMDB API client
│   └── ToastService.cs              # Notifications
├── Helpers/
│   └── FilmHelpers.cs               # Disc type & MIME type utilities
├── wwwroot/
│   └── app.css                      # Global CSS variables & styling
├── appsettings.json
├── appsettings.Production.json
├── Dockerfile
└── Program.cs
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQLite (included with .NET)
- (Optional) TMDB API key for poster fetching

### Installation

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd fdb-agentic
   ```

2. **Configure TMDB API (optional):**
   - Update `appsettings.json` with your TMDB API key
   ```json
   {
     "Tmdb": {
       "ApiKey": "your-api-key-here"
     }
   }
   ```

3. **Build the project:**
   ```bash
   dotnet build FilmDatenBank/FilmDatenBank.csproj
   ```

## Development

### Hot Reload (Recommended)
```bash
dotnet watch run --project FilmDatenBank/FilmDatenBank.csproj
```

The application will start on `http://localhost:5000`

**Default Login Credentials:**
- Username: `admin`
- Password: `changeme`

(Auto-created on first run if no users exist)

### Build for Production
```bash
dotnet publish -c Release -o /app/publish FilmDatenBank/FilmDatenBank.csproj
```

### Database Migrations

The database schema is automatically migrated on startup. To add a new migration:

```bash
dotnet ef migrations add <MigrationName> --project FilmDatenBank/FilmDatenBank.csproj
```

## Database

### Design

- **Dev Environment:** `<ContentRoot>/filmdatenbank.db` (local SQLite file)
- **Production (Azure):** `$HOME/data/filmdatenbank.db` (Azure persistent storage)
- **Auto-Migration:** Schema is automatically updated on application startup via `db.Database.MigrateAsync()`

### Key Features

- Unique index on `InternNummer` (nullable, filtered index)
- Case-insensitive collation (NOCASE)
- Many-to-many relationship: `Film ↔ Genre` via `tFilmGenre` join table

### SQL Server → SQLite Migration

For migrating from SQL Server:

```bash
cd SqlMigration
dotnet run
```

Customize the source connection string and column mappings in `SqlMigration/Program.cs` before running.

## Authentication

- **Scheme:** Cookie-based (`fdb_auth`)
- **Protected Routes:** All routes except `/login` require authentication
- **Endpoints:**
  - `POST /api/auth/login` – Login
  - `POST /api/auth/logout` – Logout

## UI & Design System (Parchemin Theme)

### Color Palette

- **Background:** Warm cream (`#F2EDE3`)
- **Accent:** Deep burgundy (`#7B2C34`)
- **Sidebar:** Dark (`#1A1D26`) – hardcoded, not themeable
- **Navigation Icon:** Warm gold (`#C89A5A`)

### Typography

- **Headings:** Playfair Display (serif)
- **Body:** DM Sans (sans-serif)
- Both loaded via Google Fonts

### Components

- **Film Cards:** Horizontal layout with poster border
- **Disc Type Badges:** DVD, Blu-ray, 4K, VHS with light-theme-appropriate colors
- **Status Pills:** Color-coded for availability, condition
- **Pagination:** Ellipsis-style with smart page navigation
- **Delete Modal:** Backdrop-blur overlay with confirmation

## Deployment

### Docker Deployment on Azure

The application is containerized and deployed to Azure Web App for Containers:

1. **Container Configuration:**
   - Listens on port 8080 (`ASPNETCORE_URLS=http://+:8080`)
   - No HTTPS redirection (TLS terminated by Azure proxy)

2. **Build & Deploy:**
   ```bash
   docker build -t filmdatenbank:latest .
   ```

3. **Database Persistence:**
   - SQLite database stored at `/home/data/filmdatenbank.db`
   - Azure persistent storage volume mounted at `/home/data`

### Critical Constraints

- **EF Core Version:** Must be pinned to `8.x` – NuGet auto-resolves to `10.x` which is incompatible (pin: `8.0.13`)
- **DbContext Injection:** Use `IDbContextFactory<AppDbContext>`, never inject `AppDbContext` directly (Blazor Server concurrency requirement)
- **HTTPS:** Do not enable `UseHttpsRedirection()` – Azure proxy handles TLS termination

## Services

### FilmService

Core service for film data operations:
- **Search:** `SuchePagedAsync(FilmSucheParameter)` → `PagedResult<Film>` (20 items/page)
- **CRUD Operations:** Create, read, update, delete films
- **Genre Management:** Associate/manage genres
- **Storage Organization:** Track Ablagen (storage locations)

All methods create a fresh `AppDbContext` via `IDbContextFactory` and dispose immediately.

### TmdbService

Queries The Movie Database (TMDB) API:
- Poster artwork retrieval
- Metadata enrichment
- Trailer information

### ToastService

Lightweight async notification service for user feedback. Avoid `Task.Run` or nested `InvokeAsync` within it.

## Architecture Notes

### Data Flow

```
Components/Pages/*.razor
    ↓
IFilmService (Dependency Injection)
    ↓
FilmService
    ↓
IDbContextFactory<AppDbContext>
    ↓
Fresh AppDbContext per operation
    ↓
SQLite
```

### Important Patterns

- **Scoped DbContext:** Never reuse a context across service methods; create fresh context per operation
- **Component Reuse:** FilmFormular.razor used by both Add and Edit pages
- **Modal Management:** DeleteConfirmation.razor provides reusable confirmation dialog
- **Error Handling:** ToastService provides user-friendly notifications
- **Helpers:** FilmHelpers.cs contains presentation logic (disc type styling, MIME types)

## Contributing

Contributions are welcome! Please ensure:
- Changes follow the existing code style and architecture patterns
- Database changes are added as EF Core migrations
- Component styles use scoped `.razor.css` files where possible
- Global changes coordinate with the Parchemin design system

## License

[Your License Here]

---

**Maintained by:** FilmDatenBank Community
**Last Updated:** 2026-03-24
