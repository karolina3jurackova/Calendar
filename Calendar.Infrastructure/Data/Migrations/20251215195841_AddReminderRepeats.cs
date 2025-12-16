using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderRepeats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RepeatCountLeft",
                table: "Reminders",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatEveryMinutes",
                table: "Reminders",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepeatCountLeft",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "RepeatEveryMinutes",
                table: "Reminders");
        }
    }
}
