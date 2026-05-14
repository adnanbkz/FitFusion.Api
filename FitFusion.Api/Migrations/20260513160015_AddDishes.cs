using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitFusion.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDishes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dishes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 96, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    SuitableSlots = table.Column<string>(type: "TEXT", maxLength: 96, nullable: false),
                    DefaultPortionG = table.Column<float>(type: "REAL", nullable: false),
                    KcalPer100g = table.Column<float>(type: "REAL", nullable: false),
                    ProteinPer100g = table.Column<float>(type: "REAL", nullable: false),
                    CarbsPer100g = table.Column<float>(type: "REAL", nullable: false),
                    FatsPer100g = table.Column<float>(type: "REAL", nullable: false),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dishes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_Name",
                table: "Dishes",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dishes");
        }
    }
}
