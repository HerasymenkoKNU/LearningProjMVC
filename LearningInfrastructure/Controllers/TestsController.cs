﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LearningDomain.Model;
using LearningInfrastructure;

namespace LearningInfrastructure.Controllers
{
    public class TestsController : Controller
    {
        private readonly LearningMvcContext _context;

        public TestsController(LearningMvcContext context)
        {
            _context = context;
        }

        // GET: Tests
        public async Task<IActionResult> Index(int? id, string? name)
        {
            if (id == null)
                return RedirectToAction("Index", "Courses");

            // Если name не передан, получаем курс из БД
            if (string.IsNullOrEmpty(name))
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                    return NotFound();
                name = course.Name;
            }

            ViewBag.CourseId = id;
            ViewBag.CourseName = name;

            var testByCourse = _context.Tests
                                         .Where(b => b.CourseId == id)
                                         .Include(b => b.Course);
            return View(await testByCourse.ToListAsync());
        }

        // GET: Tests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .Include(l => l.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // GET: Tests/Create
        public IActionResult Create(int courseId)
        {
            ViewBag.CourseId = courseId;
            ViewBag.CourseName = _context.Courses.Where(c => c.Id == courseId).FirstOrDefault().Name;
            return View();
        }

        // POST: Tests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int courseId,[Bind("Id,Name,Info,FormUrl")] Test test)
        {
            test.CourseId = courseId;

            _context.Add(test);
            await _context.SaveChangesAsync();


            return RedirectToAction("Index", "Tests", new { id = courseId, name = _context.Courses.Where(c => c.Id == courseId).FirstOrDefault().Name });
        }

        // GET: Tests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = _context.Tests
       .Include(l => l.Course)
       .FirstOrDefault(l => l.Id == id);
            if (test == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", test.CourseId);
            return View(test);
        }

        // POST: Tests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Info,FormUrl,CourseId,Id")] Test test)
        {
            if (id != test.Id)
            {
                return NotFound();
            }

            
        
                try
                {
                    _context.Update(test);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestExists(test.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                
               
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", test.CourseId);
            return RedirectToAction("Index", new { id = test.CourseId });
        }

        // GET: Tests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .Include(l => l.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // POST: Tests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var test = await _context.Tests.FindAsync(id);
            if (test != null)
            {
                int courseId = test.CourseId;
                _context.Tests.Remove(test);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Tests", new { id = courseId });
            }

            return RedirectToAction("Index", "Courses");
        }

        private bool TestExists(int id)
        {
            return _context.Tests.Any(e => e.Id == id);
        }
    }
}
