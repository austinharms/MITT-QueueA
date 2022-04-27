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

namespace MITT_QueueA.Controllers
{
    public class MainController : Controller
    {
        private const int PAGESIZE = 10;
        private ApplicationDbContext db = new ApplicationDbContext();

        public async Task<ActionResult> Index(int? page = 1)
        {
            int questionCount = await db.Questions.CountAsync();
            ViewBag.PageCount = (int)Math.Ceiling((double)questionCount / PAGESIZE);
            int pg = page ?? 1;
            if (pg < 1 || pg > ViewBag.PageCount) pg = 1;
            return View(await db.Questions.OrderBy(q => q.DateAsked).Skip((pg - 1) * PAGESIZE).Take(PAGESIZE).ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Question question = await db.Questions.FindAsync(id);
            if (question == null)
                return HttpNotFound();
            question.Answers = question.Answers.Select(a => { a.Rating = a.UserVotes.Sum(v => v.IsUpvote ? 1 : -1); return a; }).OrderByDescending(a => a.AcceptedAnswer).ThenByDescending(a => a.Rating).ThenByDescending(a => a.DateAnswered).ToList();
            ViewBag.UserId = User.Identity.GetUserId();
            return View(question);
        }

        // GET: Questions/Create
        public ActionResult CreateQuestion()
        {
            if (!Request.IsAuthenticated) return RedirectToAction("Login", "Account");
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateQuestion(CreateQuestion question)
        {
            if (!Request.IsAuthenticated) return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                string id = User.Identity.GetUserId();
                //ApplicationUser user = await db.Users.FirstAsync(u => u.Id == id);
                Question q = new Question { DateAsked = DateTime.Now, UserId = id, Title = question.Title };
                db.Questions.Add(q);
                db.Answers.Add(new Answer { QuestionId = q.Id, IsQuestion = true, AcceptedAnswer = false, Content = question.Content, DateAnswered = DateTime.Now, UserId = id });
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAnswer([Bind(Include = "Content,QuestionId")] Answer answer)
        {
            if (!Request.IsAuthenticated)
                return RedirectToAction("Login", "Account");

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
        public async Task<ActionResult> AddComment([Bind(Include = "Message,AnswerId,QuestionId")] Comment comment)
        {
            if (!Request.IsAuthenticated) return RedirectToAction("Login", "Account");
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
        public async Task<ActionResult> UpvoteAnswer(int answerId)
        {
            if (!Request.IsAuthenticated) return RedirectToAction("Login", "Account");
            Answer answer = await db.Answers.FindAsync(answerId);
            if (answer == null)
                return HttpNotFound();

            string userId = User.Identity.GetUserId();
            if (userId != answer.UserId)
            {
                AnswerVote vote = await db.AnswerVotes.FirstOrDefaultAsync(v => v.AnswerId == answerId && v.UserId == userId);
                if (vote == null)
                {
                    vote = new AnswerVote { UserId = userId, AnswerId = answerId, IsUpvote = true };
                    db.AnswerVotes.Add(vote);
                }
                else
                {
                    if (vote.IsUpvote)
                    {
                        db.AnswerVotes.Remove(vote);
                    }
                    else
                    {
                        vote.IsUpvote = true;
                    }
                }

                await db.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = answer.QuestionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownvoteAnswer(int answerId)
        {
            if (!Request.IsAuthenticated) return RedirectToAction("Login", "Account");
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
                }
                else
                {
                    if (!vote.IsUpvote)
                    {
                        db.AnswerVotes.Remove(vote);
                    }
                    else
                    {
                        vote.IsUpvote = false;
                    }
                }

                await db.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = answer.QuestionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAnswerCorrect(int answerId)
        {
            if (!Request.IsAuthenticated) return RedirectToAction("Login", "Account");
            Answer answer = await db.Answers.FindAsync(answerId);
            if (answer == null)
                return HttpNotFound();

            Question question = await db.Questions.FindAsync(answer.QuestionId);
            if (question == null)
                return HttpNotFound();

            string userId = User.Identity.GetUserId();
            if (question.UserId != userId)
                return RedirectToAction("Details", new { id = answer.QuestionId });

            answer.AcceptedAnswer = !answer.AcceptedAnswer;
            await db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = answer.QuestionId });
        }
    }
}