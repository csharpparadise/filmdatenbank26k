namespace FilmDatenBank.Data.Models;

public class TmdbFilmInfo
{
    public long Budget { get; set; }
    public long Revenue { get; set; }
    public int Runtime { get; set; }
    public string Director { get; set; } = string.Empty;
    public List<string> Hauptdarsteller { get; set; } = [];
}
