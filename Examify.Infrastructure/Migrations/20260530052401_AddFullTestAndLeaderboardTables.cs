using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTestAndLeaderboardTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentPart",
                table: "FullTestSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ListeningExerciseId",
                table: "FullTestSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ListeningTimeSpent",
                table: "FullTestSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ReadingExerciseId",
                table: "FullTestSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReadingTimeSpent",
                table: "FullTestSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SpeakingExerciseId",
                table: "FullTestSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpeakingTimeSpent",
                table: "FullTestSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "FullTestSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "WritingExerciseId",
                table: "FullTestSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WritingTimeSpent",
                table: "FullTestSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FullTestSessions_ListeningExerciseId",
                table: "FullTestSessions",
                column: "ListeningExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_FullTestSessions_ReadingExerciseId",
                table: "FullTestSessions",
                column: "ReadingExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_FullTestSessions_SpeakingExerciseId",
                table: "FullTestSessions",
                column: "SpeakingExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_FullTestSessions_WritingExerciseId",
                table: "FullTestSessions",
                column: "WritingExerciseId");

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Exercises_ListeningExerciseId",
                table: "FullTestSessions",
                column: "ListeningExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Exercises_ReadingExerciseId",
                table: "FullTestSessions",
                column: "ReadingExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Exercises_SpeakingExerciseId",
                table: "FullTestSessions",
                column: "SpeakingExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Exercises_WritingExerciseId",
                table: "FullTestSessions",
                column: "WritingExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FullTestSessions_Exercises_ListeningExerciseId",
                table: "FullTestSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_FullTestSessions_Exercises_ReadingExerciseId",
                table: "FullTestSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_FullTestSessions_Exercises_SpeakingExerciseId",
                table: "FullTestSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_FullTestSessions_Exercises_WritingExerciseId",
                table: "FullTestSessions");

            migrationBuilder.DropIndex(
                name: "IX_FullTestSessions_ListeningExerciseId",
                table: "FullTestSessions");

            migrationBuilder.DropIndex(
                name: "IX_FullTestSessions_ReadingExerciseId",
                table: "FullTestSessions");

            migrationBuilder.DropIndex(
                name: "IX_FullTestSessions_SpeakingExerciseId",
                table: "FullTestSessions");

            migrationBuilder.DropIndex(
                name: "IX_FullTestSessions_WritingExerciseId",
                table: "FullTestSessions");

            migrationBuilder.DropColumn(
                name: "CurrentPart",
                table: "FullTestSessions");

            migrationBuilder.DropColumn(
                name: "ListeningExerciseId",
                table: "FullTestSessions");

            migrationBuilder.DropColumn(
                name: "ListeningTimeSpent",
                table: "FullTestSessions");

            migrationBuilder.DropColumn(
                name: "ReadingExerciseId",
                table: "FullTestSessions");

            migrationBuilder.DropColumn(
                name: "ReadingTimeSpent",
                table: "FullTestSessions");

            migrationBuilder.DropColumn(
                name: "SpeakingExerciseId",
                table: "FullTestSessions");

            migrationBuilder.DropColumn(
                name: "SpeakingTimeSpent",
                table: "FullTestSessions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "FullTestSessions");

            migrationBuilder.DropColumn(
                name: "WritingExerciseId",
                table: "FullTestSessions");

            migrationBuilder.DropColumn(
                name: "WritingTimeSpent",
                table: "FullTestSessions");
        }
    }
}
