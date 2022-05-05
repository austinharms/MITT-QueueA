namespace MITT_QueueA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUserRep : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "Reputation");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Reputation", c => c.Int(nullable: false));
        }
    }
}
