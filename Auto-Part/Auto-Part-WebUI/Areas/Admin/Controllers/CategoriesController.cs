﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Auto_Part_WebUI.Models.DataContexts;
using Auto_Part_WebUI.Models.Entities;

namespace Auto_Part_WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ECoPartDbContext db;

        public CategoriesController(ECoPartDbContext db)
        {
            this.db = db;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            var eCoPartDbContext = db.Categories
                .Where(c => c.DeletedById == null)
                .Include(c => c.Brand).Include(c => c.Parent);
            return View(await eCoPartDbContext.ToListAsync());
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await db.Categories
                .Include(c => c.Brand)
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(m => m.Id == id && m.DeletedById == null);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            var data = db.Categories
                .Where(c => c.DeletedById == null)
                .Select(c => new
                {
                    Id = c.Id,
                    Name = c.ParentId == null ? c.Name : $"- {c.Name}"
                })
                .ToList();
            ViewData["BrandId"] = new SelectList(db.Brands.Where(b => b.DeletedById == null), "Id", "Name");
            ViewData["ParentId"] = new SelectList(data, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,BrandId,ParentId,Id,CreatedById,CreatedDate,DeletedById,DeletedDate")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Add(category);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var data = db.Categories
                .Where(c => c.DeletedById == null)
                .Select(c => new
                {
                    Id = c.Id,
                    Name = c.ParentId == null ? c.Name : $"- {c.Name}"
                })
                .ToList();
            ViewData["BrandId"] = new SelectList(db.Brands.Where(b => b.DeletedById == null), "Id", "Name", category.BrandId);
            ViewData["ParentId"] = new SelectList(data, "Id", "Name", category.ParentId);
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await db.Categories
                .FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewData["BrandId"] = new SelectList(db.Brands.Where(c => c.DeletedById == null), "Id", "Name", category.BrandId);
            ViewData["ParentId"] = new SelectList(db.Categories.Where(c => c.DeletedById == null && c.Parent.Id != category.ParentId), "Id", "Name", category.ParentId);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,BrandId,ParentId,Id,CreatedById,CreatedDate,DeletedById,DeletedDate")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Update(category);
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(db.Brands.Where(c => c.DeletedById == null), "Id", "Name", category.BrandId);
            ViewData["ParentId"] = new SelectList(db.Categories.Where(c => c.DeletedById == null), "Id", "Name", category.ParentId);
            return View(category);
        }

        [HttpPost]
        public IActionResult Delete([FromRoute] int id)
        {
            var entity = db.Categories.FirstOrDefault(b => b.Id == id);
            if (entity == null)
            {
                return Json(new
                {
                    error = true,
                    message = "Movcud deyil"
                });
            }
            if (entity.ParentId == null)
            {
                return Json(new
                {
                    error = true,
                    message = "Ana Kateqoriyanı Silmək Olmaz!"
                });
            }
            entity.DeletedById = 1; //todo
            entity.DeletedDate = DateTime.UtcNow.AddHours(4);
            db.SaveChanges();
            return Json(new
            {
                error = false,
                message = "Ugurla silindi"
            });
        }

        private bool CategoryExists(int id)
        {
            return db.Categories.Any(e => e.Id == id);
        }
    }
}