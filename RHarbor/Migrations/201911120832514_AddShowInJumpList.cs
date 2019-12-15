namespace kenzauros.RHarbor.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddShowInJumpList : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.rdp_connection_infos", "ShowInJumpList", c => c.Boolean(defaultValue: false));
            AddColumn("dbo.ssh_connection_infos", "ShowInJumpList", c => c.Boolean(defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ssh_connection_infos", "ShowInJumpList");
            DropColumn("dbo.rdp_connection_infos", "ShowInJumpList");
        }
    }
}
