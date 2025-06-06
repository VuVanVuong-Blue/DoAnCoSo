using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace System_Music.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMigrations_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    EncodeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Thumbnail = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ThumbnailM = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    ArtistsNames = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Link = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Mp4_480 = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Mp4_720 = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Mp4_1080 = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Hls = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.EncodeId);
                });

            migrationBuilder.CreateTable(
                name: "VideoArtists",
                columns: table => new
                {
                    VideoEncodeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ArtistId = table.Column<int>(type: "int", nullable: false),
                    VideoArtistId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoArtists", x => new { x.VideoEncodeId, x.ArtistId });
                    table.ForeignKey(
                        name: "FK_VideoArtists_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "ArtistId");
                    table.ForeignKey(
                        name: "FK_VideoArtists_Videos_VideoEncodeId",
                        column: x => x.VideoEncodeId,
                        principalTable: "Videos",
                        principalColumn: "EncodeId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoArtists_ArtistId",
                table: "VideoArtists",
                column: "ArtistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoArtists");

            migrationBuilder.DropTable(
                name: "Videos");
        }
    }
}
