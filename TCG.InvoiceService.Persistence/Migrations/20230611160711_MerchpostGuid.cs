using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCG.InvoiceService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MerchpostGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "MerchPostId",
                table: "Orders",
                type: "char(36)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MerchPostId",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)");
        }
    }
}
