using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace test_2.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicalReportFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TechnicalReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "TechnicalReports",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "TechnicalReports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "TechnicalReports");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "TechnicalReports");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "TechnicalReports");
        }
    }
}
