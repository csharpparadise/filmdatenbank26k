using FilmDatenBank.Data.Models;

namespace FilmDatenBank.Services;

/// <summary>Ein Autocomplete-Vorschlag mit Typ-Unterscheidung (Film / Genre).</summary>
public record AutocompleteVorschlag(string Text, string Typ, int? FilmId = null);

public interface IFilmService
{
    Task<List<Film>> AlleFilmeAsync();
    Task<List<Film>> SucheAsync(string suchbegriff);
    Task<PagedResult<Film>> SuchePagedAsync(FilmSucheParameter parameter);
    Task<List<AutocompleteVorschlag>> AutocompleteAsync(string term);
    Task<Film?> FilmNachIdAsync(int id);
    Task HinzufuegenAsync(Film film);
    Task AktualisierenAsync(Film film);
    Task LoeschenAsync(int id);
    Task<List<Ablage>> AlleAblageAsync();
    Task AblageHinzufuegenAsync(Ablage ablage);
    Task AblageAktualisierenAsync(Ablage ablage);
    Task AblageLoeschenAsync(int id);
    Task<List<Genre>> AlleGenreAsync();
    Task GenreHinzufuegenAsync(Genre genre);
    Task GenreAktualisierenAsync(Genre genre);
    Task GenreLoeschenAsync(int id);
}
