using FilmDatenBank.Data.Models;

namespace FilmDatenBank.Services;

public interface ITmdbService
{
    Task<TmdbFilmInfo> GetFilmInfoAsync(string tmdbId);
}
