namespace MITT_QueueA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStringLimitsOnAnswer : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Answers", "Content", c => c.String(maxLength: 2000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Answers", "Content", c => c.String());
        }
    }
}
