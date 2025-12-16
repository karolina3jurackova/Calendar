using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventShares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GroupKey",
                table: "Shares",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateTable(
                name: "EventShares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SharedWithUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SharedWithGroupId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Access = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventShares_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShareGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShareGroupMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AddedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareGroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShareGroupMembers_ShareGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "ShareGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventShares_EventId_SharedWithGroupId",
                table: "EventShares",
                columns: new[] { "EventId", "SharedWithGroupId" },
                unique: true,
                filter: "SharedWithGroupId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventShares_EventId_SharedWithUserId",
                table: "EventShares",
                columns: new[] { "EventId", "SharedWithUserId" },
                unique: true,
                filter: "SharedWithUserId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShareGroupMembers_GroupId_UserId",
                table: "ShareGroupMembers",
                columns: new[] { "GroupId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShareGroups_OwnerId_Name",
                table: "ShareGroups",
                columns: new[] { "OwnerId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventShares");

            migrationBuilder.DropTable(
                name: "ShareGroupMembers");

            migrationBuilder.DropTable(
                name: "ShareGroups");

            migrationBuilder.AlterColumn<int>(
                name: "GroupKey",
                table: "Shares",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
