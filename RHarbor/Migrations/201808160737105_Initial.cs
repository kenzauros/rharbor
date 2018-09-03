namespace kenzauros.RHarbor.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.rdp_connection_infos",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 2147483647),
                        Host = c.String(nullable: false, maxLength: 2147483647),
                        Port = c.Int(nullable: false),
                        Username = c.String(maxLength: 2147483647),
                        Password = c.String(maxLength: 2147483647),
                        SaveUsername = c.Boolean(nullable: false),
                        SavePassword = c.Boolean(nullable: false),
                        FullScreen = c.Boolean(nullable: false),
                        DesktopWidth = c.Int(),
                        DesktopHeight = c.Int(),
                        Admin = c.Boolean(nullable: false),
                        Settings = c.String(maxLength: 2147483647),
                        RequiredConnectionId = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ssh_connection_infos", t => t.RequiredConnectionId)
                .Index(t => t.RequiredConnectionId);
            
            CreateTable(
                "dbo.ssh_connection_infos",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 2147483647),
                        Host = c.String(nullable: false, maxLength: 2147483647),
                        Port = c.Int(nullable: false),
                        Username = c.String(maxLength: 2147483647),
                        SaveUsername = c.Boolean(nullable: false),
                        Password = c.String(maxLength: 2147483647),
                        SavePassword = c.Boolean(nullable: false),
                        PrivateKeyFilePath = c.String(maxLength: 2147483647),
                        ExpectedFingerPrint = c.String(maxLength: 2147483647),
                        KeepAliveEnabled = c.Boolean(nullable: false),
                        KeepAliveInterval = c.Int(nullable: false),
                        RequiredConnectionId = c.Long(),
                        PortForwardings = c.String(maxLength: 2147483647),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ssh_connection_infos", t => t.RequiredConnectionId)
                .Index(t => t.RequiredConnectionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.rdp_connection_infos", "RequiredConnectionId", "dbo.ssh_connection_infos");
            DropForeignKey("dbo.ssh_connection_infos", "RequiredConnectionId", "dbo.ssh_connection_infos");
            DropIndex("dbo.ssh_connection_infos", new[] { "RequiredConnectionId" });
            DropIndex("dbo.rdp_connection_infos", new[] { "RequiredConnectionId" });
            DropTable("dbo.ssh_connection_infos");
            DropTable("dbo.rdp_connection_infos");
        }
    }
}
