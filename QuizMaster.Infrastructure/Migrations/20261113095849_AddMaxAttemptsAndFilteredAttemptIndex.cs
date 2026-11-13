using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizMaster.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxAttemptsAndFilteredAttemptIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_QuizId_UserId",
                table: "QuizAttempts");

            migrationBuilder.AddColumn<int>(
                name: "MaxAttempts",
                table: "Quizzes",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Quizzes_MaxAttempts",
                table: "Quizzes",
                sql: "[MaxAttempts] >= 1");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizId",
                table: "QuizAttempts",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizId_UserId",
                table: "QuizAttempts",
                columns: new[] { "QuizId", "UserId" },
                unique: true,
                filter: "[Status] = 'InProgress'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Quizzes_MaxAttempts",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_QuizId",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_QuizId_UserId",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "MaxAttempts",
                table: "Quizzes");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizId_UserId",
                table: "QuizAttempts",
                columns: new[] { "QuizId", "UserId" },
                unique: true);
        }
    }
}
