using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSessionAnswerIndexWithFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ TẠO INDEX MỚI VỚI FILTER
            migrationBuilder.CreateIndex(
                name: "IX_SessionAnswers_SessionId_QuestionId",
                table: "SessionAnswers",
                columns: new[] { "SessionId", "QuestionId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ XÓA INDEX KHI ROLLBACK
            migrationBuilder.DropIndex(
                name: "IX_SessionAnswers_SessionId_QuestionId",
                table: "SessionAnswers");
        }
    }
}