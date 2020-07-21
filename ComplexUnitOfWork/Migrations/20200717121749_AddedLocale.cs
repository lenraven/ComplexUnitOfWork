using Microsoft.EntityFrameworkCore.Migrations;

namespace ComplexUnitOfWork.Migrations
{
    public partial class AddedLocale : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locales",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locales", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Locales",
                column: "Id",
                value: "en-GB");

            migrationBuilder.InsertData(
                table: "Locales",
                column: "Id",
                value: "hu-HU");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Locales");
        }
    }
}
