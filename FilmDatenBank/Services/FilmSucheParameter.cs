namespace FilmDatenBank.Services;

public record FilmSucheParameter
{
    public string? Suchbegriff { get; init; }
    public string SortFeld { get; init; } = "Nummer";
    public bool SortAufsteigend { get; init; } = true;
    public int Seite { get; init; } = 1;
    public int ProSeite { get; init; } = 20;

    // Filter (A2)
    public List<int>? GenreIds { get; init; }
    public List<string>? DiscTypes { get; init; }
    public int? BewertungMin { get; init; }
    public int? BewertungMax { get; init; }
    public bool? IstAusgeliehen { get; init; }
    public bool? IstMarkiert { get; init; }
    public bool? IstGesperrt { get; init; }
}

public record PagedResult<T>(List<T> Items, int GesamtAnzahl, int ProSeite)
{
    public int GesamtSeiten => Math.Max(1, (int)Math.Ceiling(GesamtAnzahl / (double)ProSeite));
}
