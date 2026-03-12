using FilmDatenBank.Components;
using FilmDatenBank.Data;
using FilmDatenBank.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var dbPath = builder.Environment.IsDevelopment()
    ? Path.Combine(builder.Environment.ContentRootPath, "filmdatenbank.db")
    : "/data/filmdatenbank.db";

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
