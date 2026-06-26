using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPROTOFULLTEST : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FullTestId",
                table: "FullTestSessions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullTestId",
                table: "FullTestSessions");
        }
    }
}
