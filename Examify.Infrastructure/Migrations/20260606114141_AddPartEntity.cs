using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropForeignKey(
                name: "FK_FullTestSessions_Submissions_ListeningSubmissionId",
                table: "FullTestSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_FullTestSessions_Submissions_ReadingSubmissionId",
                table: "FullTestSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_FullTestSessions_Submissions_SpeakingSubmissionId",
                table: "FullTestSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_FullTestSessions_Submissions_WritingSubmissionId",
                table: "FullTestSessions");

            migrationBuilder.AddColumn<Guid>(
                name: "ListeningExerciseId",
                table: "Exercises",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReadingExerciseId",
                table: "Exercises",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SpeakingExerciseId",
                table: "Exercises",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WritingExerciseId",
                table: "Exercises",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_ListeningExerciseId",
                table: "Exercises",
                column: "ListeningExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_ReadingExerciseId",
                table: "Exercises",
                column: "ReadingExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_SpeakingExerciseId",
                table: "Exercises",
                column: "SpeakingExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_WritingExerciseId",
                table: "Exercises",
                column: "WritingExerciseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Exercises_ListeningExerciseId",
                table: "Exercises",
                column: "ListeningExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Exercises_ReadingExerciseId",
                table: "Exercises",
                column: "ReadingExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Exercises_SpeakingExerciseId",
                table: "Exercises",
                column: "SpeakingExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Exercises_WritingExerciseId",
                table: "Exercises",
                column: "WritingExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Exercises_ListeningExerciseId",
                table: "FullTestSessions",
                column: "ListeningExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Exercises_ReadingExerciseId",
                table: "FullTestSessions",
                column: "ReadingExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Exercises_SpeakingExerciseId",
                table: "FullTestSessions",
                column: "SpeakingExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Exercises_WritingExerciseId",
                table: "FullTestSessions",
                column: "WritingExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Submissions_ListeningSubmissionId",
                table: "FullTestSessions",
                column: "ListeningSubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Submissions_ReadingSubmissionId",
                table: "FullTestSessions",
                column: "ReadingSubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Submissions_SpeakingSubmissionId",
                table: "FullTestSessions",
                column: "SpeakingSubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Submissions_WritingSubmissionId",
                table: "FullTestSessions",
                column: "WritingSubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Exercises_ListeningExerciseId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Exercises_ReadingExerciseId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Exercises_SpeakingExerciseId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Exercises_WritingExerciseId",
                table: "Exercises");

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

            migrationBuilder.DropForeignKey(
                name: "FK_FullTestSessions_Submissions_ListeningSubmissionId",
                table: "FullTestSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_FullTestSessions_Submissions_ReadingSubmissionId",
                table: "FullTestSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_FullTestSessions_Submissions_SpeakingSubmissionId",
                table: "FullTestSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_FullTestSessions_Submissions_WritingSubmissionId",
                table: "FullTestSessions");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_ListeningExerciseId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_ReadingExerciseId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_SpeakingExerciseId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_WritingExerciseId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "ListeningExerciseId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "ReadingExerciseId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SpeakingExerciseId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "WritingExerciseId",
                table: "Exercises");

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

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Submissions_ListeningSubmissionId",
                table: "FullTestSessions",
                column: "ListeningSubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Submissions_ReadingSubmissionId",
                table: "FullTestSessions",
                column: "ReadingSubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Submissions_SpeakingSubmissionId",
                table: "FullTestSessions",
                column: "SpeakingSubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FullTestSessions_Submissions_WritingSubmissionId",
                table: "FullTestSessions",
                column: "WritingSubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id");
        }
    }
}
