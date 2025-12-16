using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reminders_EventId",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "ChannelEmail",
                table: "Reminders");

            migrationBuilder.RenameColumn(
                name: "ChannelLocal",
                table: "Reminders",
                newName: "Channel");

            migrationBuilder.RenameColumn(
                name: "AtUtc",
                table: "Reminders",
                newName: "AbsoluteUtc");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "Reminders",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FireAtUtc",
                table: "Reminders",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsSent",
                table: "Reminders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_EventId_FireAtUtc",
                table: "Reminders",
                columns: new[] { "EventId", "FireAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_IsSent_FireAtUtc",
                table: "Reminders",
                columns: new[] { "IsSent", "FireAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reminders_EventId_FireAtUtc",
                table: "Reminders");

            migrationBuilder.DropIndex(
                name: "IX_Reminders_IsSent_FireAtUtc",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "FireAtUtc",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "IsSent",
                table: "Reminders");

            migrationBuilder.RenameColumn(
                name: "Channel",
                table: "Reminders",
                newName: "ChannelLocal");

            migrationBuilder.RenameColumn(
                name: "AbsoluteUtc",
                table: "Reminders",
                newName: "AtUtc");

            migrationBuilder.AddColumn<bool>(
                name: "ChannelEmail",
                table: "Reminders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_EventId",
                table: "Reminders",
                column: "EventId");
        }
    }
}
