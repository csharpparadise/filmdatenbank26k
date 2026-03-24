using FilmDatenBank.Data;
using FilmDatenBank.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FilmDatenBank.Services;

public class FilmService(IDbContextFactory<AppDbContext> dbFactory) : IFilmService
{
    public async Task<List<Film>> AlleFilmeAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Filme
            .Include(f => f.Genres)
            .Include(f => f.Ablage)
            .OrderBy(f => f.FilmNummer)
            .ThenBy(f => f.Titel)
            .ToListAsync();
    }

    public async Task<List<Film>> SucheAsync(string suchbegriff)
    {
        if (string.IsNullOrWhiteSpace(suchbegriff))
            return await AlleFilmeAsync();

        var term = suchbegriff.ToLower().Trim();

        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Filme
            .Include(f => f.Genres)
            .Include(f => f.Ablage)
            .Where(f =>
                f.Titel.ToLower().Contains(term) ||
                f.FilmNummer.ToLower().Contains(term) ||
                f.Genres.Any(g => g.Name.ToLower().Contains(term)))
            .OrderBy(f => f.FilmNummer)
            .ThenBy(f => f.Titel)
            .ToListAsync();
    }

    public async Task<List<AutocompleteVorschlag>> AutocompleteAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < 3)
            return [];

        var t = term.ToLower().Trim();
        await using var db = await dbFactory.CreateDbContextAsync();

        var filme = await db.Filme
            .Where(f => f.Titel.ToLower().Contains(t))
            .OrderBy(f => f.Titel)
            .Select(f => new { f.Id, f.Titel })
            .Take(6)
            .ToListAsync();

        var genres = await db.Genres
            .Where(g => g.Name.ToLower().Contains(t))
            .OrderBy(g => g.Name)
            .Select(g => g.Name)
            .Distinct()
            .Take(3)
            .ToListAsync();

        return [
            .. filme.Select(f    => new AutocompleteVorschlag(f.Titel, "Film", f.Id)),
            .. genres.Select(name => new AutocompleteVorschlag(name,   "Genre")),
        ];
    }

    public async Task<PagedResult<Film>> SuchePagedAsync(FilmSucheParameter p)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        IQueryable<Film> query = db.Filme;

        // Suchbegriff
        if (!string.IsNullOrWhiteSpace(p.Suchbegriff))
        {
            var term = p.Suchbegriff.ToLower().Trim();
            query = query.Where(f =>
                f.Titel.ToLower().Contains(term) ||
                f.FilmNummer.ToLower().Contains(term) ||
                f.Genres.Any(g => g.Name.ToLower().Contains(term)));
        }

        // Filter: Genres
        if (p.GenreIds is { Count: > 0 })
            query = query.Where(f => f.Genres.Any(g => p.GenreIds.Contains(g.Id)));

        // Filter: DiscTypes
        if (p.DiscTypes is { Count: > 0 })
            query = query.Where(f => p.DiscTypes.Contains(f.DiscType));

        // Filter: Bewertung
        if (p.BewertungMin.HasValue)
            query = query.Where(f => f.Bewertung >= p.BewertungMin.Value);
        if (p.BewertungMax.HasValue)
            query = query.Where(f => f.Bewertung <= p.BewertungMax.Value);

        // Filter: Status-Flags
        if (p.IstAusgeliehen.HasValue)
            query = query.Where(f => f.IstAusgeliehen == p.IstAusgeliehen.Value);
        if (p.IstMarkiert.HasValue)
            query = query.Where(f => f.IstMarkiert == p.IstMarkiert.Value);
        if (p.IstGesperrt.HasValue)
            query = query.Where(f => f.IstGesperrt == p.IstGesperrt.Value);

        var total = await query.CountAsync();

        // Sortierung
        IOrderedQueryable<Film> sorted = p.SortFeld switch
        {
            "Titel" => p.SortAufsteigend ? query.OrderBy(f => f.Titel) : query.OrderByDescending(f => f.Titel),
            "Datum" => p.SortAufsteigend ? query.OrderBy(f => f.Aufnahme) : query.OrderByDescending(f => f.Aufnahme),
            "Bewertung" => p.SortAufsteigend ? query.OrderBy(f => f.Bewertung) : query.OrderByDescending(f => f.Bewertung),
            _ => p.SortAufsteigend
                ? query.OrderBy(f => f.FilmNummer).ThenBy(f => f.Titel)
                : query.OrderByDescending(f => f.FilmNummer).ThenByDescending(f => f.Titel),
        };

        var items = await sorted
            .Skip((p.Seite - 1) * p.ProSeite)
            .Take(p.ProSeite)
            .Include(f => f.Genres)
            .Include(f => f.Ablage)
            .ToListAsync();

        return new PagedResult<Film>(items, total, p.ProSeite);
    }

    public async Task<Film?> FilmNachIdAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Filme
            .Include(f => f.Genres)
            .Include(f => f.Ablage)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task HinzufuegenAsync(Film film)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var genreIds = film.Genres.Select(g => g.Id).ToList();
        film.Genres.Clear();
        db.Filme.Add(film);
        if (genreIds.Count > 0)
        {
            var genres = await db.Genres.Where(g => genreIds.Contains(g.Id)).ToListAsync();
            film.Genres.AddRange(genres);
        }
        await db.SaveChangesAsync();
    }

    public async Task AktualisierenAsync(Film film)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var existing = await db.Filme
            .Include(f => f.Genres)
            .FirstAsync(f => f.Id == film.Id);

        db.Entry(existing).CurrentValues.SetValues(film);

        existing.Genres.Clear();
        var genreIds = film.Genres.Select(g => g.Id).ToList();
        if (genreIds.Count > 0)
        {
            var genres = await db.Genres.Where(g => genreIds.Contains(g.Id)).ToListAsync();
            existing.Genres.AddRange(genres);
        }
        await db.SaveChangesAsync();
    }

    public async Task LoeschenAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var film = await db.Filme.FindAsync(id);
        if (film is not null)
        {
            db.Filme.Remove(film);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<Ablage>> AlleAblageAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Ablagen.OrderBy(a => a.Ort).ToListAsync();
    }

    public async Task AblageHinzufuegenAsync(Ablage ablage)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Ablagen.Add(ablage);
        await db.SaveChangesAsync();
    }

    public async Task AblageAktualisierenAsync(Ablage ablage)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Ablagen.Update(ablage);
        await db.SaveChangesAsync();
    }

    public async Task AblageLoeschenAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var ablage = await db.Ablagen.FindAsync(id);
        if (ablage is not null)
        {
            db.Ablagen.Remove(ablage);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<Genre>> AlleGenreAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Genres.OrderBy(g => g.Name).ToListAsync();
    }

    public async Task GenreHinzufuegenAsync(Genre genre)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Genres.Add(genre);
        await db.SaveChangesAsync();
    }

    public async Task GenreAktualisierenAsync(Genre genre)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Genres.Update(genre);
        await db.SaveChangesAsync();
    }

    public async Task GenreLoeschenAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var genre = await db.Genres.FindAsync(id);
        if (genre is not null)
        {
            db.Genres.Remove(genre);
            await db.SaveChangesAsync();
        }
    }
}
