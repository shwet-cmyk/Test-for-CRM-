using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOSGlobal.Crm.Infrastructure.Migrations
{
    public partial class AddAttendanceEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdminUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EmployeeUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceOverrides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ScopeValue = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    MinActiveHours = table.Column<double>(type: "float", nullable: false),
                    MinMeetingsPerDay = table.Column<int>(type: "int", nullable: false),
                    MinTasksPerDay = table.Column<int>(type: "int", nullable: false),
                    HybridAllowed = table.Column<bool>(type: "bit", nullable: false),
                    RequireGeoValidation = table.Column<bool>(type: "bit", nullable: false),
                    GeoRadiusMeters = table.Column<int>(type: "int", nullable: false),
                    NightShiftRollover = table.Column<bool>(type: "bit", nullable: false),
                    ShiftAlias = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeShifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ShiftAlias = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    GraceMinutes = table.Column<int>(type: "int", nullable: false),
                    WeeklyOffPattern = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    NightShiftRollover = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeShifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeoMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LocationType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    RadiusMeters = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PunchType = table.Column<int>(type: "int", nullable: false),
                    PunchUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ShiftAlias = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ShiftStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    ShiftEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    GraceMinutes = table.Column<int>(type: "int", nullable: true),
                    MeetingsLogged = table.Column<int>(type: "int", nullable: true),
                    TasksLogged = table.Column<int>(type: "int", nullable: true),
                    ActiveHours = table.Column<double>(type: "float", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LocationLabel = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FlaggedForReview = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLogs_UserId",
                table: "AttendanceLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceOverrides_AttendanceDate",
                table: "AttendanceOverrides",
                column: "AttendanceDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShifts_UserId",
                table: "EmployeeShifts",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceLogs");

            migrationBuilder.DropTable(
                name: "AttendanceOverrides");

            migrationBuilder.DropTable(
                name: "AttendanceRules");

            migrationBuilder.DropTable(
                name: "EmployeeShifts");

            migrationBuilder.DropTable(
                name: "GeoMappings");
        }
    }
}
