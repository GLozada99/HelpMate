using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeBoardMembershipAllowOnlyOneRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Roles",
                table: "BoardMemberships");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "BoardMemberships",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "BoardMemberships");

            migrationBuilder.AddColumn<string[]>(
                name: "Roles",
                table: "BoardMemberships",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }
    }
}
