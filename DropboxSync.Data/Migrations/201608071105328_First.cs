namespace DropboxSync.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class First : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DropboxRefs",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DropboxId = c.String(maxLength: 50),
                        ExactId = c.String(maxLength: 50),
                        DropboxModifiedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Settings",
                c => new
                    {
                        Key = c.String(nullable: false, maxLength: 50),
                        value = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.Key);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Settings");
            DropTable("dbo.DropboxRefs");
        }
    }
}
