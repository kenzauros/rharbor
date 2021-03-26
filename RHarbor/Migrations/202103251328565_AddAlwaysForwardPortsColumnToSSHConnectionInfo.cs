namespace kenzauros.RHarbor.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAlwaysForwardPortsColumnToSSHConnectionInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ssh_connection_infos", "AlwaysForwardPorts", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ssh_connection_infos", "AlwaysForwardPorts");
        }
    }
}
