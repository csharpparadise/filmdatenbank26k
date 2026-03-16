using FilmDatenBank.Components;
using FilmDatenBank.Data;
using FilmDatenBank.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

string dbPath;
if (builder.Environment.IsDevelopment())
{
    dbPath = Path.Combine(builder.Environment.ContentRootPath, "filmdatenbank.db");
}
else
{
    // Azure App Service: HOME=/home (persistent, writable)
    // Fly.io: use /data (volume mount)
    var home = Environment.GetEnvironmentVariable("HOME");
    if (home != null && Directory.Exists(Path.Combine(home, "site")))
    {
        // Azure App Service detected (HOME/site exists)
        var dataDir = Path.Combine(home, "data");
        Directory.CreateDirectory(dataDir);
        dbPath = Path.Combine(dataDir, "filmdatenbank.db");
    }
    else if (Directory.Exists("/data"))
    {
        dbPath = "/data/filmdatenbank.db";
    }
    else
    {
        dbPath = Path.Combine(builder.Environment.ContentRootPath, "filmdatenbank.db");
    }
}

Console.WriteLine("DB Path: " + dbPath);

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<IFilmService, FilmService>();
builder.Services.AddScoped<ToastService>();

builder.Services.AddHttpClient("tmdb", client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
});
builder.Services.AddScoped<ITmdbService, TmdbService>();

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
