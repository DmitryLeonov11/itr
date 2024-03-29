﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using itr.Infrastructure;
using itr.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace itr.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    [Area("Admin")]
    public class ArticlesController : Controller
    {
        private readonly itrContext context;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ArticlesController(itrContext context, IWebHostEnvironment webHostEnvironment)
        {
            this.context = context;
            this.webHostEnvironment = webHostEnvironment;
        }

        // GET /admin/articles
        public async Task<IActionResult> Index(int p = 1)
        {
            int pageSize = 6;
            var articles = context.Articles.OrderByDescending(x => x.Id)
                                            .Include(x => x.Category)
                                            .Skip((p - 1) * pageSize)
                                            .Take(pageSize);
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)context.Articles.Count() / pageSize);
            return View(await articles.ToListAsync());
        }

        // GET /admin/articles/details/5
        public async Task<IActionResult> Details(int id)
        {
            Article article = await context.Articles.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // GET /admin/articles/create
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(context.Categories.OrderBy(x => x.Sorting), "Id", "Name");

            return View();
        }

        // POST /admin/articles/create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Article article)
        {
            ViewBag.CategoryId = new SelectList(context.Categories.OrderBy(x => x.Sorting), "Id", "Name");

            if (ModelState.IsValid)
            {
                article.Slug = article.Name.ToLower().Replace(" ", "-");

                var slug = await context.Articles.FirstOrDefaultAsync(x => x.Slug == article.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "The article already exists.");
                    return View(article);
                }

                string imageName = "noimage.png";
                if (article.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(webHostEnvironment.WebRootPath, "media/articles");
                    imageName = Guid.NewGuid().ToString() + "_" + article.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await article.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                }

                article.Image = imageName;

                context.Add(article);
                await context.SaveChangesAsync();

                TempData["Success"] = "The article has been added!";

                return RedirectToAction("Index");
            }

            return View(article);
        }

        // GET /admin/articles/edit/5
        public async Task<IActionResult> Edit(int id)
        {
            Article article = await context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            ViewBag.CategoryId = new SelectList(context.Categories.OrderBy(x => x.Sorting), "Id", "Name", article.CategoryId);

            return View(article);
        }

        // POST /admin/articles/edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Article article)
        {
            ViewBag.CategoryId = new SelectList(context.Categories.OrderBy(x => x.Sorting), "Id", "Name", article.CategoryId);

            if (ModelState.IsValid)
            {
                article.Slug = article.Name.ToLower().Replace(" ", "-");

                var slug = await context.Articles.Where(x => x.Id != id).FirstOrDefaultAsync(x => x.Slug == article.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "The article already exists.");
                    return View(article);
                }

                if (article.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(webHostEnvironment.WebRootPath, "media/articles");

                    if (!string.Equals(article.Image, "noimage.png"))
                    {
                        string oldImagePath = Path.Combine(uploadsDir, article.Image);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string imageName = Guid.NewGuid().ToString() + "_" + article.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await article.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    article.Image = imageName;
                }

                context.Update(article);
                await context.SaveChangesAsync();

                TempData["Success"] = "The article has been edited!";

                return RedirectToAction("Index");
            }

            return View(article);
        }

        // GET /admin/articles/delete/5
        public async Task<IActionResult> Delete(int id)
        {
            Article article = await context.Articles.FindAsync(id);

            if (article == null)
            {
                TempData["Error"] = "The article does not exist!";
            }
            else
            {
                if (!string.Equals(article.Image, "noimage.png"))
                {
                    string uploadsDir = Path.Combine(webHostEnvironment.WebRootPath, "media/articles");
                    string oldImagePath = Path.Combine(uploadsDir, article.Image);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                context.Articles.Remove(article);
                await context.SaveChangesAsync();

                TempData["Success"] = "The article has been deleted!";
            }

            return RedirectToAction("Index");
        }
    }
}