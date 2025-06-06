using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace System_Music.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMigrations_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayCount",
                table: "Playlists",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TrackId",
                table: "ListenHistories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AlbumId",
                table: "ListenHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArtistId",
                table: "ListenHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityType",
                table: "ListenHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaylistId",
                table: "ListenHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayCount",
                table: "Artists",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayCount",
                table: "Albums",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListenHistories_AlbumId",
                table: "ListenHistories",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_ListenHistories_ArtistId",
                table: "ListenHistories",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ListenHistories_PlaylistId",
                table: "ListenHistories",
                column: "PlaylistId");

            migrationBuilder.AddForeignKey(
                name: "FK_ListenHistories_Albums_AlbumId",
                table: "ListenHistories",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "AlbumId");

            migrationBuilder.AddForeignKey(
                name: "FK_ListenHistories_Artists_ArtistId",
                table: "ListenHistories",
                column: "ArtistId",
                principalTable: "Artists",
                principalColumn: "ArtistId");

            migrationBuilder.AddForeignKey(
                name: "FK_ListenHistories_Playlists_PlaylistId",
                table: "ListenHistories",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "PlaylistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListenHistories_Albums_AlbumId",
                table: "ListenHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_ListenHistories_Artists_ArtistId",
                table: "ListenHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_ListenHistories_Playlists_PlaylistId",
                table: "ListenHistories");

            migrationBuilder.DropIndex(
                name: "IX_ListenHistories_AlbumId",
                table: "ListenHistories");

            migrationBuilder.DropIndex(
                name: "IX_ListenHistories_ArtistId",
                table: "ListenHistories");

            migrationBuilder.DropIndex(
                name: "IX_ListenHistories_PlaylistId",
                table: "ListenHistories");

            migrationBuilder.DropColumn(
                name: "PlayCount",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "AlbumId",
                table: "ListenHistories");

            migrationBuilder.DropColumn(
                name: "ArtistId",
                table: "ListenHistories");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "ListenHistories");

            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "ListenHistories");

            migrationBuilder.DropColumn(
                name: "PlayCount",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "PlayCount",
                table: "Albums");

            migrationBuilder.AlterColumn<int>(
                name: "TrackId",
                table: "ListenHistories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
