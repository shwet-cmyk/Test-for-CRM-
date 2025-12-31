using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOSGlobal.Crm.Infrastructure.Migrations
{
    public partial class AddDashboardEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WidgetLibraries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    GraphType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DataSource = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DefaultConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultFiltersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WidgetLibraries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WidgetDataCaches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WidgetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    FiltersHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CachedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WidgetDataCaches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDashboardConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    WidgetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TitleOverride = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LayoutJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FiltersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDashboardConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDashboardConfigs_UserId",
                table: "UserDashboardConfigs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WidgetDataCaches_WidgetId",
                table: "WidgetDataCaches",
                column: "WidgetId");

            migrationBuilder.CreateIndex(
                name: "IX_WidgetLibraries_Key",
                table: "WidgetLibraries",
                column: "Key",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDashboardConfigs");

            migrationBuilder.DropTable(
                name: "WidgetDataCaches");

            migrationBuilder.DropTable(
                name: "WidgetLibraries");
        }
    }
}
