namespace MITT_QueueA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveQuestionVote : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.QuestionVotes", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.QuestionVotes", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.QuestionVotes", new[] { "QuestionId" });
            DropIndex("dbo.QuestionVotes", new[] { "UserId" });
            DropTable("dbo.QuestionVotes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.QuestionVotes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuestionId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        IsUpvote = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.QuestionVotes", "UserId");
            CreateIndex("dbo.QuestionVotes", "QuestionId");
            AddForeignKey("dbo.QuestionVotes", "UserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.QuestionVotes", "QuestionId", "dbo.Questions", "Id", cascadeDelete: true);
        }
    }
}
