namespace MITT_QueueA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTitleRequirements : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Questions", "Title", c => c.String(nullable: false, maxLength: 150));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Questions", "Title", c => c.String());
        }
    }
}
