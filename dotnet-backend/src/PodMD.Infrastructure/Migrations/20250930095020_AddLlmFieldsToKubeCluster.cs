using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PodMD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLlmFieldsToKubeCluster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "KubeClusters",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ResponseFormat",
                table: "KubeClusters",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "KubeClusters");

            migrationBuilder.DropColumn(
                name: "ResponseFormat",
                table: "KubeClusters");
        }
    }
}
