/*
 * SQL Server → SQLite Migration
 *
 * Workflow:
 *   1. Start the FilmDatenBank app once locally so EF Core creates the SQLite schema.
 *   2. Adjust the SQL_CONN_STR constant below.
 *   3. Run: dotnet run
 *   4. Upload the resulting filmdatenbank.db to Fly.io:
 *        fly ssh sftp shell → put filmdatenbank.db /data/filmdatenbank.db
 *   5. fly machine restart
 */

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

// ── Configuration ────────────────────────────────────────────────────────────
const string SQL_CONN_STR = "Server=XPS8940\\sqlexpress;Database=csharp-paradise_movies;Integrated Security=SSPI";
const string SQLITE_PATH  = "../FilmDatenBank/filmdatenbank.db";

await using var sqlConn    = new SqlConnection(SQL_CONN_STR);
await using var sqliteConn = new SqliteConnection($"Data Source={SQLITE_PATH}");
await sqliteConn.OpenAsync();

// ── tAblage ───────────────────────────────────────────────────────────────────
Console.WriteLine("Migriere tAblage...");
var ablagen = (await sqlConn.QueryAsync("SELECT ID, Ort, Bemerkung, Typ FROM dbo.tAblage")).AsList();
Console.WriteLine($"  {ablagen.Count} Einträge gefunden.");
int insertedAblage = 0;
foreach (var row in ablagen)
{
    var cmd = sqliteConn.CreateCommand();
    cmd.CommandText = """
        INSERT OR IGNORE INTO tAblage (ID, Ort, Bemerkung, Typ)
        VALUES ($id, $ort, $bemerkung, $typ)
        """;
    cmd.Parameters.AddWithValue("$id",        (object?)row.ID        ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$ort",        (object?)row.Ort       ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$bemerkung",  (object?)row.Bemerkung ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$typ",        (object?)row.Typ       ?? DBNull.Value);
    if (await cmd.ExecuteNonQueryAsync() > 0) insertedAblage++;
}
Console.WriteLine($"  Eingefügt: {insertedAblage}");

// ── tgenre ────────────────────────────────────────────────────────────────────
Console.WriteLine("Migriere tgenre...");
var genres = (await sqlConn.QueryAsync("SELECT gid, gname FROM dbo.tgenre")).AsList();
Console.WriteLine($"  {genres.Count} Einträge gefunden.");
int insertedGenre = 0;
foreach (var row in genres)
{
    var cmd = sqliteConn.CreateCommand();
    cmd.CommandText = """
        INSERT OR IGNORE INTO tgenre (gid, gname)
        VALUES ($gid, $gname)
        """;
    cmd.Parameters.AddWithValue("$gid",   (object?)row.gid   ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$gname", (object?)row.gname ?? DBNull.Value);
    if (await cmd.ExecuteNonQueryAsync() > 0) insertedGenre++;
}
Console.WriteLine($"  Eingefügt: {insertedGenre}");

// ── tfilme ────────────────────────────────────────────────────────────────────
Console.WriteLine("Migriere tfilme...");
var filme = (await sqlConn.QueryAsync("""
    SELECT fid, fname, faufnahme, fbemerkung, fbewertung, fcausleih,
           frent, flock, fmark, fASIN, fFilmNummer, fAblage,
           fTrailer, fThumbnail, fDiscType
    FROM dbo.tfilme
    """)).AsList();
Console.WriteLine($"  {filme.Count} Einträge gefunden.");
int insertedFilme = 0, skippedFilme = 0;
foreach (var row in filme)
{
    var cmd = sqliteConn.CreateCommand();
    cmd.CommandText = """
        INSERT OR IGNORE INTO tfilme
            (fid, fname, faufnahme, fbemerkung, fbewertung, fcausleih,
             frent, flock, fmark, fASIN, fFilmNummer, fAblage,
             fTrailer, fThumbnail, fDiscType)
        VALUES
            ($fid, $fname, $faufnahme, $fbemerkung, $fbewertung, $fcausleih,
             $frent, $flock, $fmark, $fASIN, $fFilmNummer, $fAblage,
             $fTrailer, $fThumbnail, $fDiscType)
        """;
    cmd.Parameters.AddWithValue("$fid",         (object?)row.fid         ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$fname",        (object?)row.fname       ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$faufnahme",    ((DateTime?)row.faufnahme)?.ToString("o") ?? (object)DBNull.Value);
    cmd.Parameters.AddWithValue("$fbemerkung",   (object?)row.fbemerkung  ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$fbewertung",   (object?)row.fbewertung  ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$fcausleih",    (object?)row.fcausleih   ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$frent",        (object?)row.frent       ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$flock",        (object?)row.flock       ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$fmark",        (object?)row.fmark       ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$fASIN",        (object?)row.fASIN       ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$fFilmNummer",  (object?)row.fFilmNummer ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$fAblage",      (object?)row.fAblage     ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$fTrailer",     (object?)row.fTrailer    ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$fThumbnail",   (object?)row.fThumbnail  ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$fDiscType",    (object?)row.fDiscType   ?? DBNull.Value);
    int affected = await cmd.ExecuteNonQueryAsync();
    if (affected > 0) insertedFilme++; else skippedFilme++;
}
Console.WriteLine($"  Eingefügt: {insertedFilme}, Übersprungen (Duplikate): {skippedFilme}");

// ── tFilmGenre ────────────────────────────────────────────────────────────────
Console.WriteLine("Migriere tFilmGenre...");
var filmGenres = (await sqlConn.QueryAsync("SELECT Film, Genre FROM dbo.tFilmGenre")).AsList();
Console.WriteLine($"  {filmGenres.Count} Einträge gefunden.");
int insertedFG = 0;
foreach (var row in filmGenres)
{
    var cmd = sqliteConn.CreateCommand();
    cmd.CommandText = """
        INSERT OR IGNORE INTO tFilmGenre (Film, Genre)
        VALUES ($film, $genre)
        """;
    cmd.Parameters.AddWithValue("$film",  (object?)row.Film  ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$genre", (object?)row.Genre ?? DBNull.Value);
    if (await cmd.ExecuteNonQueryAsync() > 0) insertedFG++;
}
Console.WriteLine($"  Eingefügt: {insertedFG}");

Console.WriteLine("\nMigration abgeschlossen.");
