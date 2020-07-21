using Microsoft.EntityFrameworkCore.Migrations;

namespace ComplexUnitOfWork.Migrations
{
    public partial class AddedLocaleReferenceToTheSample : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocaleId",
                table: "Samples",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Samples_LocaleId",
                table: "Samples",
                column: "LocaleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Samples_Locales_LocaleId",
                table: "Samples",
                column: "LocaleId",
                principalTable: "Locales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Samples_Locales_LocaleId",
                table: "Samples");

            migrationBuilder.DropIndex(
                name: "IX_Samples_LocaleId",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "LocaleId",
                table: "Samples");
        }
    }
}
