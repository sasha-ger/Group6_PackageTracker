using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackageTracker.Accessors.Migrations
{
    /// <inheritdoc />
    public partial class DeliverySimulationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentPackageId",
                table: "Drones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationDepotId",
                table: "Drones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedArrivalTime",
                table: "Drones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PackageStatusEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    DepotId = table.Column<int>(type: "int", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageStatusEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageStatusEvents_Depots_DepotId",
                        column: x => x.DepotId,
                        principalTable: "Depots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageStatusEvents_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Drones_CurrentPackageId",
                table: "Drones",
                column: "CurrentPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Drones_DestinationDepotId",
                table: "Drones",
                column: "DestinationDepotId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageStatusEvents_DepotId",
                table: "PackageStatusEvents",
                column: "DepotId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageStatusEvents_PackageId",
                table: "PackageStatusEvents",
                column: "PackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drones_Depots_DestinationDepotId",
                table: "Drones",
                column: "DestinationDepotId",
                principalTable: "Depots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Drones_Packages_CurrentPackageId",
                table: "Drones",
                column: "CurrentPackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drones_Depots_DestinationDepotId",
                table: "Drones");

            migrationBuilder.DropForeignKey(
                name: "FK_Drones_Packages_CurrentPackageId",
                table: "Drones");

            migrationBuilder.DropTable(
                name: "PackageStatusEvents");

            migrationBuilder.DropIndex(
                name: "IX_Drones_CurrentPackageId",
                table: "Drones");

            migrationBuilder.DropIndex(
                name: "IX_Drones_DestinationDepotId",
                table: "Drones");

            migrationBuilder.DropColumn(
                name: "CurrentPackageId",
                table: "Drones");

            migrationBuilder.DropColumn(
                name: "DestinationDepotId",
                table: "Drones");

            migrationBuilder.DropColumn(
                name: "EstimatedArrivalTime",
                table: "Drones");
        }
    }
}
