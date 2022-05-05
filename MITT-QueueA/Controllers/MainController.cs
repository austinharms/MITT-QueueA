using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MITT_QueueA.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MITT_QueueA.Controllers
{
    public class MainController : Controller
    {
        private const int PAGESIZE = 10;
        private ApplicationDbContext db = new ApplicationDbContext();

        [AllowAnonymous]
        public async Task<ActionResult> Index(int page = 1, int sort = 0)
        {
            int questionCount = await db.Questions.CountAsync();
            ViewBag.PageCount = (int)Math.Ceiling((double)questionCount / PAGESIZE);
            ViewBag.Sort = sort;
            if (page < 1 || page > ViewBag.PageCount) page = 1;
            ViewBag.Page = page;
            if (sort == 1)
            {
                return View(await db.Questions.OrderByDescending(q => q.Answers.Count()).Skip((page - 1) * PAGESIZE).Take(PAGESIZE).ToListAsync());
            }
            else
            {
                return View(await db.Questions.OrderBy(q => q.DateAsked).Skip((page - 1) * PAGESIZE).Take(PAGESIZE).ToListAsync());
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> Tags(int id = -1, int page = 1, int sort = 0)
        {
            if (id == -1)
                return HttpNotFound();

            Tag t = await db.Tags.FindAsync(id);
            if (t == null)
                return HttpNotFound();

            int questionCount = t.Questions.Count();
            ViewBag.PageCount = (int)Math.Ceiling((double)questionCount / PAGESIZE);
            ViewBag.Tag = t;
            ViewBag.Sort = sort;
            if (page < 1 || page > ViewBag.PageCount) page = 1;
            ViewBag.Page = page;
            if (sort == 1)
            {
                return View(t.Questions.OrderByDescending(q => q.Answers.Count()).Skip((page - 1) * PAGESIZE).Take(PAGESIZE).ToArray());
            }
            else
            {
                return View(t.Questions.OrderBy(q => q.DateAsked).Skip((page - 1) * PAGESIZE).Take(PAGESIZE).ToArray());
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Question question = await db.Questions.FindAsync(id);
            if (question == null)
                return HttpNotFound();

            string userId = User.Identity.GetUserId();
            question.Answers = question.Answers.Select(a =>
            {
                a.Rating = a.UserVotes.Sum(v => v.IsUpvote ? 1 : -1);
                AnswerVote vote = a.UserVotes.FirstOrDefault(v => v.UserId == userId);
                if (vote != null)
                {
                    if (vote.IsUpvote)
                    {
                        a.UserUpvote = true;
                    }
                    else
                    {
                        a.UserDownvote = true;
                    }
                }

                return a;
            }).OrderByDescending(a => a.AcceptedAnswer).ThenByDescending(a => a.Rating).ThenByDescending(a => a.DateAnswered).ToList();
            ViewBag.UserId = User.Identity.GetUserId();
            return View(question);
        }

        [Authorize]
        public async Task<ActionResult> CreateQuestion()
        {
            ViewBag.Tags = String.Join(", ", await db.Tags.Select(t => t.Name).ToListAsync());
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> CreateQuestion(CreateQuestion question)
        {
            if (ModelState.IsValid)
            {
                string id = User.Identity.GetUserId();
                //ApplicationUser user = await db.Users.FirstAsync(u => u.Id == id);
                Question q = new Question { DateAsked = DateTime.Now, UserId = id, Title = question.Title };
                db.Questions.Add(q);
                db.Answers.Add(new Answer { QuestionId = q.Id, IsQuestion = true, AcceptedAnswer = false, Content = question.Content, DateAnswered = DateTime.Now, UserId = id });
                foreach (string tagString in question.Tags)
                {
                    Tag tag = await db.Tags.FirstOrDefaultAsync(t => t.Name == tagString);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagString };
                        db.Tags.Add(tag);
                    }

                    q.Tags.Add(tag);
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Tags = String.Join(", ", await db.Tags.Select(t => t.Name).ToListAsync());
            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> AddAnswer([Bind(Include = "Content,QuestionId")] Answer answer)
        {

            if (ModelState.IsValid)
            {
                string id = User.Identity.GetUserId();
                answer.UserId = id;
                answer.AcceptedAnswer = false;
                answer.DateAnswered = DateTime.Now;
                answer.IsQuestion = false;
                db.Answers.Add(answer);
                await db.SaveChangesAsync();

                // needed to clear form?
                return RedirectToAction("Details", new { id = answer.QuestionId });
            }

            ViewBag.Answer = answer;
            return RedirectToAction("Details", new { id = answer.QuestionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> AddComment([Bind(Include = "Message,AnswerId,QuestionId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                string id = User.Identity.GetUserId();
                comment.UserId = id;
                comment.DateAdded = DateTime.Now;
                db.Comments.Add(comment);
                await db.SaveChangesAsync();
                return RedirectToAction("Details", new { id = comment.QuestionId });
            }

            ViewBag.Comment = comment;
            return RedirectToAction("Details", new { id = comment.QuestionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> UpvoteAnswer(int answerId)
        {
            Answer answer = await db.Answers.FindAsync(answerId);
            if (answer == null)
                return HttpNotFound();

            string userId = User.Identity.GetUserId();
            if (userId != answer.UserId)
            {
                AnswerVote vote = await db.AnswerVotes.FirstOrDefaultAsync(v => v.AnswerId == answerId && v.UserId == userId);
                if (vote == null)
                {
                    answer.User.Reputation += 5;
                    vote = new AnswerVote { UserId = userId, AnswerId = answerId, IsUpvote = true };
                    db.AnswerVotes.Add(vote);
                    if (answer.User.Reputation >= 100)
                    {
                        IdentityRole goldRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Gold");
                        if (goldRole != null && !answer.User.IdentityRoles.Contains(goldRole))
                            answer.User.IdentityRoles.Add(goldRole);
                    }
                }
                else
                {
                    if (vote.IsUpvote)
                    {
                        db.AnswerVotes.Remove(vote);
                        answer.User.Reputation -= 5;
                    }
                    else
                    {
                        vote.IsUpvote = true;
                        answer.User.Reputation += 10;
                        if (answer.User.Reputation >= 100)
                        {
                            IdentityRole goldRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Gold");
                            if (goldRole != null && answer.User.Roles.FirstOrDefault(r => r.RoleId == goldRole.Id) == null)
                                answer.User.Roles.Add(new IdentityUserRole { UserId = answer.UserId, RoleId = goldRole.Id });
                        }
                    }
                }

                await db.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = answer.QuestionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> DownvoteAnswer(int answerId)
        {
            Answer answer = await db.Answers.FindAsync(answerId);
            if (answer == null)
                return HttpNotFound();

            string userId = User.Identity.GetUserId();
            if (userId != answer.UserId)
            {
                AnswerVote vote = await db.AnswerVotes.FirstOrDefaultAsync(v => v.AnswerId == answerId && v.UserId == userId);
                if (vote == null)
                {
                    vote = new AnswerVote { UserId = userId, AnswerId = answerId, IsUpvote = false };
                    db.AnswerVotes.Add(vote);
                    answer.User.Reputation -= 5;
                }
                else
                {
                    if (!vote.IsUpvote)
                    {
                        db.AnswerVotes.Remove(vote);
                        answer.User.Reputation += 5;
                        if (answer.User.Reputation >= 100)
                        {
                            IdentityRole goldRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Gold");
                            if (goldRole != null && answer.User.Roles.FirstOrDefault(r => r.RoleId == goldRole.Id) == null)
                                answer.User.Roles.Add(new IdentityUserRole { UserId = answer.UserId, RoleId = goldRole.Id });
                        }
                    }
                    else
                    {
                        vote.IsUpvote = false;
                        answer.User.Reputation -= 10;
                    }
                }

                await db.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = answer.QuestionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> MarkAnswerCorrect(int answerId)
        {
            Answer answer = await db.Answers.FindAsync(answerId);
            if (answer == null)
                return HttpNotFound();

            Question question = await db.Questions.FindAsync(answer.QuestionId);
            if (question == null)
                return HttpNotFound();

            string userId = User.Identity.GetUserId();
            if (question.UserId != userId)
                return RedirectToAction("Details", new { id = answer.QuestionId });

            if (answer.AcceptedAnswer)
            {
                answer.AcceptedAnswer = false;
            }
            else
            {
                Answer acceptedAnswer = await db.Answers.FirstOrDefaultAsync(a => a.AcceptedAnswer && a.QuestionId == answer.QuestionId);
                if (acceptedAnswer != null)
                    acceptedAnswer.AcceptedAnswer = false;

                answer.AcceptedAnswer = true;
            }

            await db.SaveChangesAsync();
            return RedirectToAction("Details", new { id = answer.QuestionId });
        }
    }
}