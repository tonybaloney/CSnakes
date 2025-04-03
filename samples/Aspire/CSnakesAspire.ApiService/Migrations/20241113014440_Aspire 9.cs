using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSnakesAspire.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Aspire9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Wind",
                table: "WeatherRecords",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "TemperatureMinC",
                table: "WeatherRecords",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "TemperatureMaxC",
                table: "WeatherRecords",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "Precipitation",
                table: "WeatherRecords",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Wind",
                table: "WeatherRecords",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<int>(
                name: "TemperatureMinC",
                table: "WeatherRecords",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<int>(
                name: "TemperatureMaxC",
                table: "WeatherRecords",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<int>(
                name: "Precipitation",
                table: "WeatherRecords",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }
    }
}
