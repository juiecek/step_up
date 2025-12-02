using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace step_up.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "ScheduleDanceStyles");

            migrationBuilder.AddColumn<int>(
                name: "DanceStyleLevelId",
                table: "ScheduleDanceStyles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DanceStyleLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DanceStyleId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanceStyleLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanceStyleLevels_DanceStyles_DanceStyleId",
                        column: x => x.DanceStyleId,
                        principalTable: "DanceStyles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDanceStyles_DanceStyleLevelId",
                table: "ScheduleDanceStyles",
                column: "DanceStyleLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_DanceStyleLevels_DanceStyleId",
                table: "DanceStyleLevels",
                column: "DanceStyleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleDanceStyles_DanceStyleLevels_DanceStyleLevelId",
                table: "ScheduleDanceStyles",
                column: "DanceStyleLevelId",
                principalTable: "DanceStyleLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleDanceStyles_DanceStyleLevels_DanceStyleLevelId",
                table: "ScheduleDanceStyles");

            migrationBuilder.DropTable(
                name: "DanceStyleLevels");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleDanceStyles_DanceStyleLevelId",
                table: "ScheduleDanceStyles");

            migrationBuilder.DropColumn(
                name: "DanceStyleLevelId",
                table: "ScheduleDanceStyles");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "ScheduleDanceStyles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
