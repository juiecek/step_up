using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace step_up.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLevelFromDanceStyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "DanceStyles");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "ScheduleDanceStyles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "ScheduleDanceStyles");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "DanceStyles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
