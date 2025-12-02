using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace step_up.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInstructorDanceStyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstructorDanceStyles");

            migrationBuilder.AddColumn<int>(
                name: "DanceStyleId",
                table: "Instructor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Instructor_DanceStyleId",
                table: "Instructor",
                column: "DanceStyleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Instructor_DanceStyles_DanceStyleId",
                table: "Instructor",
                column: "DanceStyleId",
                principalTable: "DanceStyles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Instructor_DanceStyles_DanceStyleId",
                table: "Instructor");

            migrationBuilder.DropIndex(
                name: "IX_Instructor_DanceStyleId",
                table: "Instructor");

            migrationBuilder.DropColumn(
                name: "DanceStyleId",
                table: "Instructor");

            migrationBuilder.CreateTable(
                name: "InstructorDanceStyles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DanceStyleId = table.Column<int>(type: "int", nullable: false),
                    InstructorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorDanceStyles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstructorDanceStyles_DanceStyles_DanceStyleId",
                        column: x => x.DanceStyleId,
                        principalTable: "DanceStyles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstructorDanceStyles_Instructor_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Instructor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstructorDanceStyles_DanceStyleId",
                table: "InstructorDanceStyles",
                column: "DanceStyleId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorDanceStyles_InstructorId",
                table: "InstructorDanceStyles",
                column: "InstructorId");
        }
    }
}
