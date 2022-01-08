namespace kenzauros.RHarbor.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSortOrderColumnToExternalProgramDefinition : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.external_program_definitions", "SortOrder", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.external_program_definitions", "SortOrder");
        }
    }
}
