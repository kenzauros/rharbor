namespace kenzauros.RHarbor.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExternalProgramDefinitions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.external_program_definitions",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 2147483647),
                        ExePath = c.String(maxLength: 2147483647),
                        Arguments = c.String(maxLength: 2147483647),
                        CopyToClipboard = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.external_program_definitions");
        }
    }
}
