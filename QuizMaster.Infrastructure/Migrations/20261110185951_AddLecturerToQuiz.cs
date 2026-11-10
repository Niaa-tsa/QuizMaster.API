using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizMaster.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLecturerToQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LecturerId",
                table: "Quizzes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_LecturerId",
                table: "Quizzes",
                column: "LecturerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Users_LecturerId",
                table: "Quizzes",
                column: "LecturerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_LecturerId",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_LecturerId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "LecturerId",
                table: "Quizzes");
        }
    }
}
