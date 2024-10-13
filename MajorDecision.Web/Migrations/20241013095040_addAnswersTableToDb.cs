using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MajorDecision.Web.Migrations
{
    /// <inheritdoc />
    public partial class addAnswersTableToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Answers",
                columns: new[] { "Id", "Answer" },
                values: new object[,]
                {
                    { 1, "May be" },
                    { 2, "More likely" },
                    { 3, "Ask this question tomorrow" },
                    { 4, "What do you think?" },
                    { 5, "I can not answer" },
                    { 6, "..." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers");
        }
    }
}
