using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaRocha.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImdbRatingToMovie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImdbRating",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImdbRating",
                table: "Movies");
        }
    }
}
