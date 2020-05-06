using kenzauros.RHarbor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace kenzauros.RHarbor.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using var context = new AppDbContext();
            var db = context.Database;
            using var command = db.GetDbConnection().CreateCommand();
            command.CommandText = @"SELECT COUNT(*) from sqlite_master WHERE type='table' AND name='ssh_connection_infos';";
            db.OpenConnection();
            var result = Convert.ToInt32(command.ExecuteScalar());
            if (result > 0) return;

            migrationBuilder.CreateTable(
                name: "ssh_connection_infos",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    Host = table.Column<string>(nullable: false),
                    Port = table.Column<int>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    SaveUsername = table.Column<bool>(nullable: false),
                    Password = table.Column<string>(nullable: true),
                    SavePassword = table.Column<bool>(nullable: false),
                    ShowInJumpList = table.Column<bool>(nullable: true),
                    PrivateKeyFilePath = table.Column<string>(nullable: true),
                    ExpectedFingerPrint = table.Column<string>(nullable: true),
                    KeepAliveEnabled = table.Column<bool>(nullable: false),
                    KeepAliveInterval = table.Column<int>(nullable: false),
                    RequiredConnectionId = table.Column<long>(nullable: true),
                    PortForwardings = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ssh_connection_infos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ssh_connection_infos_ssh_connection_infos_RequiredConnectionId",
                        column: x => x.RequiredConnectionId,
                        principalTable: "ssh_connection_infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rdp_connection_infos",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    Host = table.Column<string>(nullable: false),
                    Port = table.Column<int>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    SaveUsername = table.Column<bool>(nullable: false),
                    Password = table.Column<string>(nullable: true),
                    SavePassword = table.Column<bool>(nullable: false),
                    ShowInJumpList = table.Column<bool>(nullable: true),
                    FullScreen = table.Column<bool>(nullable: false),
                    DesktopWidth = table.Column<int>(nullable: true),
                    DesktopHeight = table.Column<int>(nullable: true),
                    Admin = table.Column<bool>(nullable: false),
                    Settings = table.Column<string>(nullable: true),
                    RequiredConnectionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rdp_connection_infos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rdp_connection_infos_ssh_connection_infos_RequiredConnectionId",
                        column: x => x.RequiredConnectionId,
                        principalTable: "ssh_connection_infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rdp_connection_infos_RequiredConnectionId",
                table: "rdp_connection_infos",
                column: "RequiredConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ssh_connection_infos_RequiredConnectionId",
                table: "ssh_connection_infos",
                column: "RequiredConnectionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rdp_connection_infos");

            migrationBuilder.DropTable(
                name: "ssh_connection_infos");
        }
    }
}
