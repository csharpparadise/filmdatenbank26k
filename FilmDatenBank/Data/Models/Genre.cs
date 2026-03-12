using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmDatenBank.Data.Models;

[Table("tgenre")]
public class Genre
{
    [Column("gid")]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    [Column("gname")]
    public string Name { get; set; } = string.Empty;

    public List<Film> Filme { get; set; } = [];
}
