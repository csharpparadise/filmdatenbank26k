using System.Security.Claims;
using FilmDatenBank.Components;
using FilmDatenBank.Data;
using FilmDatenBank.Data.Models;
using FilmDatenBank.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

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
    // Azure App Service: /home/data (persistent storage)
    var home = Environment.GetEnvironmentVariable("HOME") ?? "/home";
    var dataDir = Path.Combine(home, "data");
    Directory.CreateDirectory(dataDir);
    dbPath = Path.Combine(dataDir, "filmdatenbank.db");
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

builder.Services.AddAuthentication("fdb_auth")
    .AddCookie("fdb_auth", options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.Name = "fdb_auth";
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Auto-migrate on startup + seed default admin
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.MigrateAsync();

    if (!db.Users.Any())
    {
        var hasher = new PasswordHasher<AppUser>();
        var user = new AppUser { Username = cfg["Auth:DefaultAdminUsername"]! };
        user.PasswordHash = hasher.HashPassword(user, cfg["Auth:DefaultAdminPassword"]!);
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPost("/api/auth/login", async (
    HttpContext ctx,
    IDbContextFactory<AppDbContext> dbFactory,
    [FromForm] string username,
    [FromForm] string password,
    [FromForm] string? returnUrl) =>
{
    await using var db = await dbFactory.CreateDbContextAsync();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
    if (user is null)
        return Results.Redirect($"/login?error=1&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");

    var hasher = new PasswordHasher<AppUser>();
    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
    if (result == PasswordVerificationResult.Failed)
        return Results.Redirect($"/login?error=1&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, user.Username),
        new(ClaimTypes.NameIdentifier, user.Id.ToString())
    };
    var identity = new ClaimsIdentity(claims, "fdb_auth");
    await ctx.SignInAsync("fdb_auth", new ClaimsPrincipal(identity));
    return Results.Redirect(returnUrl is { Length: > 0 } r && r.StartsWith('/') ? r : "/");
}).DisableAntiforgery();

app.MapPost("/api/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync("fdb_auth");
    return Results.Redirect("/login");
}).DisableAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
