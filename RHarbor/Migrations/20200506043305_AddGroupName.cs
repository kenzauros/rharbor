using kenzauros.RHarbor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace kenzauros.RHarbor.Migrations
{
    public partial class AddGroupName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using var context = new AppDbContext();
            var db = context.Database;
            using var command = db.GetDbConnection().CreateCommand();
            command.CommandText = @"PRAGMA table_info('ssh_connection_infos');";
            db.OpenConnection();
            using var reader = command.ExecuteReader();
            while ( reader.Read())
            {
                if (reader.GetString(1) == "GroupName") return;
            }

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "ssh_connection_infos",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "rdp_connection_infos",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "ssh_connection_infos");

            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "rdp_connection_infos");
        }
    }
}
