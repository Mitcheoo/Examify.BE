using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnswerJsonToSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnswerJson",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerJson",
                table: "Submissions");
        }
    }
}
