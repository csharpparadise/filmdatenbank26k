using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FilmDatenBank.Data.Models;

namespace FilmDatenBank.Services;

public class TmdbService(IHttpClientFactory httpClientFactory, IConfiguration config) : ITmdbService
{
    private readonly string _apiKey = config["Tmdb:ApiKey"] ?? string.Empty;

    private HttpClient CreateClient()
    {
        var client = httpClientFactory.CreateClient("tmdb");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);
        return client;
    }

    public async Task<TmdbFilmInfo> GetFilmInfoAsync(string tmdbId)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("TMDB API-Key ist nicht konfiguriert (Tmdb:ApiKey in appsettings.json).");

        if (!int.TryParse(tmdbId, out var movieId))
            throw new ArgumentException($"Ungültige TMDB-ID: '{tmdbId}'");

        var http = CreateClient();

        var movieTask   = http.GetFromJsonAsync<TmdbMovieResponse>(
            $"movie/{movieId}?language=de-DE");
        var creditsTask = http.GetFromJsonAsync<TmdbCreditsResponse>(
            $"movie/{movieId}/credits?language=de-DE");

        await Task.WhenAll(movieTask, creditsTask);

        var movie   = movieTask.Result  ?? throw new Exception("Keine Filmdaten von TMDB erhalten.");
        var credits = creditsTask.Result ?? throw new Exception("Keine Credits von TMDB erhalten.");

        var director = credits.Crew
            .Where(c => c.Job == "Director")
            .Select(c => c.Name)
            .FirstOrDefault() ?? string.Empty;

        var hauptdarsteller = credits.Cast
            .OrderBy(c => c.Order)
            .Take(5)
            .Select(c => c.Name)
            .ToList();

        return new TmdbFilmInfo
        {
            Budget          = movie.Budget,
            Revenue         = movie.Revenue,
            Runtime         = movie.Runtime,
            Director        = director,
            Hauptdarsteller = hauptdarsteller,
        };
    }

    // ── interne Deserialisierungs-Records ────────────────────────────────────

    private record TmdbMovieResponse(
        [property: JsonPropertyName("budget")]  long Budget,
        [property: JsonPropertyName("revenue")] long Revenue,
        [property: JsonPropertyName("runtime")] int  Runtime);

    private record TmdbCreditsResponse(
        [property: JsonPropertyName("cast")] List<TmdbCastMember> Cast,
        [property: JsonPropertyName("crew")] List<TmdbCrewMember> Crew);

    private record TmdbCastMember(
        [property: JsonPropertyName("name")]  string Name,
        [property: JsonPropertyName("order")] int    Order);

    private record TmdbCrewMember(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("job")]  string Job);
}
