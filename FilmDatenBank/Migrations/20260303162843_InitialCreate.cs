using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmDatenBank.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tAblage",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ort = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Bemerkung = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Typ = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tAblage", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "tgenre",
                columns: table => new
                {
                    gid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    gname = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tgenre", x => x.gid);
                });

            migrationBuilder.CreateTable(
                name: "tfilme",
                columns: table => new
                {
                    fid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    fname = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    faufnahme = table.Column<DateTime>(type: "TEXT", nullable: false),
                    fbemerkung = table.Column<string>(type: "TEXT", nullable: true),
                    fbewertung = table.Column<int>(type: "INTEGER", nullable: false),
                    fcausleih = table.Column<int>(type: "INTEGER", nullable: false),
                    frent = table.Column<bool>(type: "INTEGER", nullable: false),
                    flock = table.Column<bool>(type: "INTEGER", nullable: false),
                    fmark = table.Column<bool>(type: "INTEGER", nullable: false),
                    fASIN = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    fFilmNummer = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    fAblage = table.Column<int>(type: "INTEGER", nullable: true),
                    fTrailer = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    fThumbnail = table.Column<byte[]>(type: "BLOB", nullable: true),
                    fDiscType = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tfilme", x => x.fid);
                    table.ForeignKey(
                        name: "FK_tfilme_tAblage_fAblage",
                        column: x => x.fAblage,
                        principalTable: "tAblage",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "tFilmGenre",
                columns: table => new
                {
                    Film = table.Column<int>(type: "INTEGER", nullable: false),
                    Genre = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tFilmGenre", x => new { x.Film, x.Genre });
                    table.ForeignKey(
                        name: "FK_tFilmGenre_tfilme_Film",
                        column: x => x.Film,
                        principalTable: "tfilme",
                        principalColumn: "fid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tFilmGenre_tgenre_Genre",
                        column: x => x.Genre,
                        principalTable: "tgenre",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tfilme_fAblage",
                table: "tfilme",
                column: "fAblage");

            migrationBuilder.CreateIndex(
                name: "IX_tFilmGenre_Genre",
                table: "tFilmGenre",
                column: "Genre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tFilmGenre");

            migrationBuilder.DropTable(
                name: "tfilme");

            migrationBuilder.DropTable(
                name: "tgenre");

            migrationBuilder.DropTable(
                name: "tAblage");
        }
    }
}
