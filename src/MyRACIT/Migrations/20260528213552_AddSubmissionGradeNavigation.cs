using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyRACIT.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionGradeNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubmissionId1",
                table: "Grades",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Assignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Grades_SubmissionId1",
                table: "Grades",
                column: "SubmissionId1",
                unique: true,
                filter: "[SubmissionId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Submissions_SubmissionId1",
                table: "Grades",
                column: "SubmissionId1",
                principalTable: "Submissions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Submissions_SubmissionId1",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Grades_SubmissionId1",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "SubmissionId1",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Assignments");
        }
    }
}
