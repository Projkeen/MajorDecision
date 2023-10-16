using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MajorDecision.Web.Migrations
{
    /// <inheritdoc />
    public partial class addFKForUserDecisionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Decisions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Decisions_ApplicationUserId",
                table: "Decisions",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Decisions_AspNetUsers_ApplicationUserId",
                table: "Decisions",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decisions_AspNetUsers_ApplicationUserId",
                table: "Decisions");

            migrationBuilder.DropIndex(
                name: "IX_Decisions_ApplicationUserId",
                table: "Decisions");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Decisions");
        }
    }
}
