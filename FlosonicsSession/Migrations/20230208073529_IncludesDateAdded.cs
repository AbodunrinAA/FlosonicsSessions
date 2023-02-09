using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlosonicsSession.Migrations
{
    /// <inheritdoc />
    public partial class IncludesDateAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DateAdded",
                table: "Sessions",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ETag",
                table: "Sessions",
                column: "ETag",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_ETag",
                table: "Sessions");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DateAdded",
                table: "Sessions",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "GETDATE()");
        }
    }
}
