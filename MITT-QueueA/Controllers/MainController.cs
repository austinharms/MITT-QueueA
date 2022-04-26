﻿using System;
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

            Question question = await db.Questions.FindAsync(answer.QuestionId);
            if (question == null)
                return HttpNotFound();

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
            return View("Details", question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddComment([Bind(Include = "Content,AnswerId")] Comment comment)
        {
            //if (!Request.IsAuthenticated) return RedirectToAction("Login", "Account");
            //if (ModelState.IsValid)
            //{
            //    string id = User.Identity.GetUserId();
            //    answer.UserId = id;
            //    answer.AcceptedAnswer = false;
            //    answer.DateAnswered = DateTime.Now;
            //    answer.IsQuestion = false;
            //    db.Answers.Add(answer);
            //    await db.SaveChangesAsync();
            //    return RedirectToAction("Details", new { id = answer.QuestionId });
            //}

            //ViewBag.Answer = answer;
            Question question = await db.Questions.FindAsync(0);
            if (question == null)
                return HttpNotFound();

            return View("Details", question);
        }
    }
}