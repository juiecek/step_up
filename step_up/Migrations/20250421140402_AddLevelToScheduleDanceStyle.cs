using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace step_up.Migrations
{
    /// <inheritdoc />
    public partial class AddLevelToScheduleDanceStyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleDanceStyles_DanceStyleLevels_DanceStyleLevelId",
                table: "ScheduleDanceStyles");

            migrationBuilder.DropTable(
                name: "DanceStyleLevels");

            migrationBuilder.RenameColumn(
                name: "DanceStyleLevelId",
                table: "ScheduleDanceStyles",
                newName: "LevelId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleDanceStyles_DanceStyleLevelId",
                table: "ScheduleDanceStyles",
                newName: "IX_ScheduleDanceStyles_LevelId");

            migrationBuilder.CreateTable(
                name: "Levels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Levels", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleDanceStyles_Levels_LevelId",
                table: "ScheduleDanceStyles",
                column: "LevelId",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleDanceStyles_Levels_LevelId",
                table: "ScheduleDanceStyles");

            migrationBuilder.DropTable(
                name: "Levels");

            migrationBuilder.RenameColumn(
                name: "LevelId",
                table: "ScheduleDanceStyles",
                newName: "DanceStyleLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleDanceStyles_LevelId",
                table: "ScheduleDanceStyles",
                newName: "IX_ScheduleDanceStyles_DanceStyleLevelId");

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
    }
}
