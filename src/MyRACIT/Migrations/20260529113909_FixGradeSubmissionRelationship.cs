using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyRACIT.Migrations
{
    /// <inheritdoc />
    public partial class FixGradeSubmissionRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_StudentProfiles_StudentId",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Submissions_SubmissionId",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Submissions_SubmissionId1",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Grades_SubmissionId",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Grades_SubmissionId1",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "SubmissionId1",
                table: "Grades");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Grades",
                newName: "GradedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Grades_StudentId",
                table: "Grades",
                newName: "IX_Grades_GradedBy");

            migrationBuilder.AlterColumn<string>(
                name: "Feedback",
                table: "Grades",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Assignments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Assignments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Assignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Grades_SubmissionId",
                table: "Grades",
                column: "SubmissionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Submissions_SubmissionId",
                table: "Grades",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_TeacherProfiles_GradedBy",
                table: "Grades",
                column: "GradedBy",
                principalTable: "TeacherProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Submissions_SubmissionId",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK_Grades_TeacherProfiles_GradedBy",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Grades_SubmissionId",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Assignments");

            migrationBuilder.RenameColumn(
                name: "GradedBy",
                table: "Grades",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Grades_GradedBy",
                table: "Grades",
                newName: "IX_Grades_StudentId");

            migrationBuilder.AlterColumn<string>(
                name: "Feedback",
                table: "Grades",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubmissionId1",
                table: "Grades",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Grades_SubmissionId",
                table: "Grades",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_SubmissionId1",
                table: "Grades",
                column: "SubmissionId1",
                unique: true,
                filter: "[SubmissionId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_StudentProfiles_StudentId",
                table: "Grades",
                column: "StudentId",
                principalTable: "StudentProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Submissions_SubmissionId",
                table: "Grades",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Submissions_SubmissionId1",
                table: "Grades",
                column: "SubmissionId1",
                principalTable: "Submissions",
                principalColumn: "Id");
        }
    }
}
