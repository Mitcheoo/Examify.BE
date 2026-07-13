using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVocabularyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VocabularyWords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Word = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Meaning = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Pronunciation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Example = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PartOfSpeech = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Level = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "B1"),
                    Topic = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AudioUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VietnameseExample = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyWords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyProgress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VocabularyWordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsMastered = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReviewCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextReviewAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StreakCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CorrectCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IncorrectCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocabularyProgress_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VocabularyProgress_VocabularyWords_VocabularyWordId",
                        column: x => x.VocabularyWordId,
                        principalTable: "VocabularyWords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyProgress_IsMastered",
                table: "VocabularyProgress",
                column: "IsMastered");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyProgress_LastReviewedAt",
                table: "VocabularyProgress",
                column: "LastReviewedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyProgress_NextReviewAt",
                table: "VocabularyProgress",
                column: "NextReviewAt");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyProgress_UserId",
                table: "VocabularyProgress",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyProgress_UserId_VocabularyWordId",
                table: "VocabularyProgress",
                columns: new[] { "UserId", "VocabularyWordId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyProgress_VocabularyWordId",
                table: "VocabularyProgress",
                column: "VocabularyWordId");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyWords_CreatedAt",
                table: "VocabularyWords",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyWords_Level",
                table: "VocabularyWords",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyWords_Topic",
                table: "VocabularyWords",
                column: "Topic");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyWords_Word",
                table: "VocabularyWords",
                column: "Word",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VocabularyProgress");

            migrationBuilder.DropTable(
                name: "VocabularyWords");
        }
    }
}
