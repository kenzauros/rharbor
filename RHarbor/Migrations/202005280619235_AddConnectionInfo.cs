namespace kenzauros.RHarbor.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddConnectionInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ssh_connection_infos", "ConnectionTimeout", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ssh_connection_infos", "ConnectionTimeout");
        }
    }
}
