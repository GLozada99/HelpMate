using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketCommentEntitya : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Boards_BoardId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Users_AssignedId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Users_CreatedById",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Users_ReporterId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketComment_Ticket_TicketId",
                table: "TicketComment");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketComment_Users_UserId",
                table: "TicketComment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketComment",
                table: "TicketComment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ticket",
                table: "Ticket");

            migrationBuilder.RenameTable(
                name: "TicketComment",
                newName: "TicketComments");

            migrationBuilder.RenameTable(
                name: "Ticket",
                newName: "Tickets");

            migrationBuilder.RenameIndex(
                name: "IX_TicketComment_UserId",
                table: "TicketComments",
                newName: "IX_TicketComments_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketComment_TicketId",
                table: "TicketComments",
                newName: "IX_TicketComments_TicketId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_ReporterId",
                table: "Tickets",
                newName: "IX_Tickets_ReporterId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_CreatedById",
                table: "Tickets",
                newName: "IX_Tickets_CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_BoardId",
                table: "Tickets",
                newName: "IX_Tickets_BoardId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_AssignedId",
                table: "Tickets",
                newName: "IX_Tickets_AssignedId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketComments",
                table: "TicketComments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tickets",
                table: "Tickets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComments_Tickets_TicketId",
                table: "TicketComments",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComments_Users_UserId",
                table: "TicketComments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Boards_BoardId",
                table: "Tickets",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_AssignedId",
                table: "Tickets",
                column: "AssignedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_CreatedById",
                table: "Tickets",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_ReporterId",
                table: "Tickets",
                column: "ReporterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketComments_Tickets_TicketId",
                table: "TicketComments");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketComments_Users_UserId",
                table: "TicketComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Boards_BoardId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_AssignedId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_CreatedById",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_ReporterId",
                table: "Tickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tickets",
                table: "Tickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketComments",
                table: "TicketComments");

            migrationBuilder.RenameTable(
                name: "Tickets",
                newName: "Ticket");

            migrationBuilder.RenameTable(
                name: "TicketComments",
                newName: "TicketComment");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_ReporterId",
                table: "Ticket",
                newName: "IX_Ticket_ReporterId");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_CreatedById",
                table: "Ticket",
                newName: "IX_Ticket_CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_BoardId",
                table: "Ticket",
                newName: "IX_Ticket_BoardId");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_AssignedId",
                table: "Ticket",
                newName: "IX_Ticket_AssignedId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketComments_UserId",
                table: "TicketComment",
                newName: "IX_TicketComment_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketComments_TicketId",
                table: "TicketComment",
                newName: "IX_TicketComment_TicketId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ticket",
                table: "Ticket",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketComment",
                table: "TicketComment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Boards_BoardId",
                table: "Ticket",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Users_AssignedId",
                table: "Ticket",
                column: "AssignedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Users_CreatedById",
                table: "Ticket",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Users_ReporterId",
                table: "Ticket",
                column: "ReporterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComment_Ticket_TicketId",
                table: "TicketComment",
                column: "TicketId",
                principalTable: "Ticket",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComment_Users_UserId",
                table: "TicketComment",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
