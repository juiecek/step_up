using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace step_up.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Attended",
                table: "Registration",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DanceStyles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "DanceStyles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DanceStyleReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DanceStyleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanceStyleReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanceStyleReviews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanceStyleReviews_DanceStyles_DanceStyleId",
                        column: x => x.DanceStyleId,
                        principalTable: "DanceStyles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DanceStyleReviews_Schedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstructorReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InstructorId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstructorReviews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstructorReviews_Instructor_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Instructor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanceStyleReviews_DanceStyleId",
                table: "DanceStyleReviews",
                column: "DanceStyleId");

            migrationBuilder.CreateIndex(
                name: "IX_DanceStyleReviews_ScheduleId",
                table: "DanceStyleReviews",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_DanceStyleReviews_UserId",
                table: "DanceStyleReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorReviews_InstructorId",
                table: "InstructorReviews",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorReviews_UserId",
                table: "InstructorReviews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanceStyleReviews");

            migrationBuilder.DropTable(
                name: "InstructorReviews");

            migrationBuilder.DropColumn(
                name: "Attended",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "DanceStyles");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "DanceStyles");
        }
    }
}
