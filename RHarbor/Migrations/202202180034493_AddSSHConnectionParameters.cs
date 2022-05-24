namespace kenzauros.RHarbor.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSSHConnectionParameters : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ssh_connection_parameters",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ConnectionId = c.Long(nullable: false),
                        Key = c.String(nullable: false, maxLength: 2147483647),
                        Value = c.String(maxLength: 2147483647),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ssh_connection_infos", t => t.ConnectionId, cascadeDelete: true)
                .Index(t => t.ConnectionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ssh_connection_parameters", "ConnectionId", "dbo.ssh_connection_infos");
            DropIndex("dbo.ssh_connection_parameters", new[] { "ConnectionId" });
            DropTable("dbo.ssh_connection_parameters");
        }
    }
}
