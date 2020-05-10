namespace kenzauros.RHarbor.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddGroupName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.rdp_connection_infos", "GroupName", c => c.String(maxLength: 2147483647));
            AddColumn("dbo.ssh_connection_infos", "GroupName", c => c.String(maxLength: 2147483647));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ssh_connection_infos", "GroupName");
            DropColumn("dbo.rdp_connection_infos", "GroupName");
        }
    }
}
