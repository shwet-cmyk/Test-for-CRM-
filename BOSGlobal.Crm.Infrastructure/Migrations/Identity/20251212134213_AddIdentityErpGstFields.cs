using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOSGlobal.Crm.Infrastructure.Migrations.Identity
{
    /// <inheritdoc />
    public partial class AddIdentityErpGstFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop only if it exists (prevents crash on fresh DBs)
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[IdentityRole]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[IdentityRole];
END
");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GstNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErpId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GstNumber",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            // Recreate only if missing (prevents crash on rollbacks)
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[IdentityRole]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[IdentityRole] (
        [Id] nvarchar(450) NULL,
        CONSTRAINT [FK_IdentityRole_AspNetRoles_Id] FOREIGN KEY ([Id])
            REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END
");
        }
    }
}
