using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kaleido.Modules.Services.Grpc.Products.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ProductPriceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductPrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "varchar(36)", nullable: false),
                    Units = table.Column<long>(type: "bigint", nullable: false),
                    Nanos = table.Column<int>(type: "int", nullable: false),
                    CurrencyKey = table.Column<string>(type: "varchar(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPrices", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPrices");
        }
    }
}
