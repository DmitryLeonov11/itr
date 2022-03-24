﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using itr.Infrastructure;
using itr.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace itr.Controllers
{
    public class PagesController : Controller
    {
        private readonly itrContext context;

        public PagesController(itrContext context)
        {
            this.context = context;
        }

        // GET / or /slug
        public async Task<IActionResult> Page(string slug)
        {
            if (slug == null)
            {
                return View(await context.Pages.Where(x => x.Slug == "home").FirstOrDefaultAsync());
            }

            Page page = await context.Pages.Where(x => x.Slug == slug).FirstOrDefaultAsync();

            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }
    }
}