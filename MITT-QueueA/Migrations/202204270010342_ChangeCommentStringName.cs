namespace MITT_QueueA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeCommentStringName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comments", "Message", c => c.String(maxLength: 500));
            DropColumn("dbo.Comments", "Content");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Comments", "Content", c => c.String(maxLength: 500));
            DropColumn("dbo.Comments", "Message");
        }
    }
}
