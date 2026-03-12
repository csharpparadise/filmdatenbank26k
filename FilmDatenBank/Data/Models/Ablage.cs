using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmDatenBank.Data.Models;

[Table("tAblage")]
public class Ablage
{
    [Column("ID")]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    [Column("Ort")]
    public string Ort { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("Bemerkung")]
    public string? Bemerkung { get; set; }

    [Required, MaxLength(20)]
    [Column("Typ")]
    public string Typ { get; set; } = string.Empty;

    public List<Film> Filme { get; set; } = [];
}
