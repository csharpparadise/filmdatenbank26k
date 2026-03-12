using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmDatenBank.Data.Models;

[Table("tfilme")]
public class Film
{
    [Column("fid")]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    [Column("fname")]
    public string Titel { get; set; } = string.Empty;

    [Column("faufnahme")]
    public DateTime Aufnahme { get; set; } = DateTime.Now;

    [Column("fbemerkung")]
    public string? Bemerkung { get; set; }

    [Column("fbewertung")]
    public int Bewertung { get; set; }

    [Column("fcausleih")]
    public int CounterAusleihen { get; set; }

    [Column("frent")]
    public bool IstAusgeliehen { get; set; }

    [Column("flock")]
    public bool IstGesperrt { get; set; }

    [Column("fmark")]
    public bool IstMarkiert { get; set; }

    [MaxLength(50)]
    [Column("fASIN")]
    public string? TmdbId { get; set; }

    [Required, MaxLength(10)]
    [Column("fFilmNummer")]
    public string FilmNummer { get; set; } = string.Empty;

    [Column("fAblage")]
    public int? AblageId { get; set; }

    [MaxLength(200)]
    [Column("fTrailer")]
    public string? Trailer { get; set; }

    [Column("fThumbnail")]
    public byte[]? Thumbnail { get; set; }

    [Required, MaxLength(10)]
    [Column("fDiscType")]
    public string DiscType { get; set; } = "DVD";

    public Ablage? Ablage { get; set; }
    public List<Genre> Genres { get; set; } = [];

    /// <summary>Zusammengesetzte Anzeigenummer: YY-####-###</summary>
    public string AnzeigeNummer =>
        $"{Aufnahme:yy}-{FilmNummer.PadLeft(4, '0')}-{Id:D3}";
}
