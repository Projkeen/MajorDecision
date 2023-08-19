using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MajorDecision.Web.Migrations
{
    /// <inheritdoc />
    public partial class addDateOfQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfQuestion",
                table: "Decisions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfQuestion",
                table: "Decisions");
        }
    }
}
