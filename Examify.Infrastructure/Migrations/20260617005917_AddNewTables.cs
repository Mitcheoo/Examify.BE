using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResultJson",
                table: "Submissions",
                newName: "AnswerJson");

            migrationBuilder.AddColumn<string>(
                name: "Explanation",
                table: "SubmissionDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Explanation",
                table: "SubmissionDetails");

            migrationBuilder.RenameColumn(
                name: "AnswerJson",
                table: "Submissions",
                newName: "ResultJson");
        }
    }
}
