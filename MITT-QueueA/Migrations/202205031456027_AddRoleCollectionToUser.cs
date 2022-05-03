namespace MITT_QueueA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRoleCollectionToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetRoles", "ApplicationUser_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.AspNetRoles", "ApplicationUser_Id");
            AddForeignKey("dbo.AspNetRoles", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetRoles", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", new[] { "ApplicationUser_Id" });
            DropColumn("dbo.AspNetRoles", "ApplicationUser_Id");
        }
    }
}
