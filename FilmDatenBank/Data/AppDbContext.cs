using FilmDatenBank.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FilmDatenBank.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Film> Filme => Set<Film>();
    public DbSet<Ablage> Ablagen => Set<Ablage>();
    public DbSet<Genre> Genres => Set<Genre>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Film>(entity =>
        {
            entity.HasOne(f => f.Ablage)
                  .WithMany(a => a.Filme)
                  .HasForeignKey(f => f.AblageId);

            entity.HasMany(f => f.Genres)
                  .WithMany(g => g.Filme)
                  .UsingEntity<Dictionary<string, object>>(
                      "tFilmGenre",
                      j => j.HasOne<Genre>().WithMany().HasForeignKey("Genre"),
                      j => j.HasOne<Film>().WithMany().HasForeignKey("Film"));
        });
    }
}
