using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PodMD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceTPTInheritance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create Sources table first
            migrationBuilder.CreateTable(
                name: "Sources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Server = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KeyVersion = table.Column<int>(type: "int", nullable: false),
                    Instructions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseFormat = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Step 2: Migrate existing KubeCluster data to Sources table before dropping columns
            migrationBuilder.Sql(@"
                INSERT INTO Sources (Id, Type, Name, Server, KeyVersion, Instructions, ResponseFormat, CreatedAt, UpdatedAt)
                SELECT Id, 'Kubernetes', Name, Server, KeyVersion, Instructions, ResponseFormat, CreatedAt, UpdatedAt
                FROM KubeClusters;
            ");

            // Step 2: Drop the index on Name in KubeClusters
            migrationBuilder.DropIndex(
                name: "IX_KubeClusters_Name",
                table: "KubeClusters");

            // Step 3: Drop the columns that moved to Sources table
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "KubeClusters");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "KubeClusters");

            migrationBuilder.DropColumn(
                name: "KeyVersion",
                table: "KubeClusters");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "KubeClusters");

            migrationBuilder.DropColumn(
                name: "ResponseFormat",
                table: "KubeClusters");

            migrationBuilder.DropColumn(
                name: "Server",
                table: "KubeClusters");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "KubeClusters");

            // Step 4: Create JenkinsServers table
            migrationBuilder.CreateTable(
                name: "JenkinsServers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApiTokenEnc = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JenkinsServers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JenkinsServers_Sources_Id",
                        column: x => x.Id,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Step 5: Add foreign key constraints for TPT inheritance
            migrationBuilder.AddForeignKey(
                name: "FK_KubeClusters_Sources_Id",
                table: "KubeClusters",
                column: "Id",
                principalTable: "Sources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KubeClusters_Sources_Id",
                table: "KubeClusters");

            migrationBuilder.DropTable(
                name: "JenkinsServers");

            migrationBuilder.DropTable(
                name: "Sources");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "KubeClusters",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "KubeClusters",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "KeyVersion",
                table: "KubeClusters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "KubeClusters",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ResponseFormat",
                table: "KubeClusters",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Server",
                table: "KubeClusters",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "KubeClusters",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_KubeClusters_Name",
                table: "KubeClusters",
                column: "Name",
                unique: true);
        }
    }
}
