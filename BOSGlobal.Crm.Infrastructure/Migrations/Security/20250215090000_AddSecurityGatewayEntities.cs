using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOSGlobal.Crm.Infrastructure.Migrations
{
    public partial class AddSecurityGatewayEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaptchaVerificationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TokenHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Score = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemoteIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ProviderResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaptchaVerificationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoginAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Device = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtpChallenges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Destination = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Salt = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false),
                    LastSentUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SendCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    MetaJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpChallenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleRights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleMasterId = table.Column<int>(type: "int", nullable: false),
                    PermissionKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsAllowed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleRights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleRights_RoleMasters_RoleMasterId",
                        column: x => x.RoleMasterId,
                        principalTable: "RoleMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoleMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleMasterId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoleMappings_RoleMasters_RoleMasterId",
                        column: x => x.RoleMasterId,
                        principalTable: "RoleMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtpAudits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OtpChallengeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ProviderName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RemoteIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpAudits_OtpChallenges_OtpChallengeId",
                        column: x => x.OtpChallengeId,
                        principalTable: "OtpChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaptchaVerificationLogs_CreatedUtc",
                table: "CaptchaVerificationLogs",
                column: "CreatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAudits_UserId",
                table: "LoginAudits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpAudits_CorrelationId",
                table: "OtpAudits",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpAudits_OtpChallengeId",
                table: "OtpAudits",
                column: "OtpChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpAudits_TimestampUtc",
                table: "OtpAudits",
                column: "TimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OtpChallenges_CorrelationId",
                table: "OtpChallenges",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OtpChallenges_Destination",
                table: "OtpChallenges",
                column: "Destination");

            migrationBuilder.CreateIndex(
                name: "IX_RoleRights_RoleMasterId",
                table: "RoleRights",
                column: "RoleMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleMappings_RoleMasterId",
                table: "UserRoleMappings",
                column: "RoleMasterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaptchaVerificationLogs");

            migrationBuilder.DropTable(
                name: "LoginAudits");

            migrationBuilder.DropTable(
                name: "OtpAudits");

            migrationBuilder.DropTable(
                name: "RoleRights");

            migrationBuilder.DropTable(
                name: "UserRoleMappings");

            migrationBuilder.DropTable(
                name: "OtpChallenges");

            migrationBuilder.DropTable(
                name: "RoleMasters");
        }
    }
}
