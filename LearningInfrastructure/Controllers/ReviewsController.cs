using System;
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
    public class ReviewsController : Controller
    {
        private readonly LearningMvcContext _context;

        public ReviewsController(LearningMvcContext context)
        {
            _context = context;
        }

        // GET: Reviews
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

            var reviewByCourse = _context.Reviews
                                         .Where(b => b.CourseId == id)
                                         .Include(b => b.Course);
            return View(await reviewByCourse.ToListAsync());
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(l => l.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // GET: Reviews/Create
        public IActionResult Create(int courseId)
        {
            ViewBag.CourseId = courseId;
            ViewBag.CourseName = _context.Courses.Where(c => c.Id == courseId).FirstOrDefault().Name;
            return View();
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int courseId, [Bind("Id,Name,Info")] Review review)
        {
            review.CourseId = courseId;

            _context.Add(review);
            await _context.SaveChangesAsync();


            return RedirectToAction("Index", "Reviews", new { id = courseId, name = _context.Courses.Where(c => c.Id == courseId).FirstOrDefault().Name });
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = _context.Reviews
      .Include(l => l.Course)
      .FirstOrDefault(l => l.Id == id);
            if (review == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", review.CourseId);
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Info,CourseId,Id")] Review review)
        {
            if (id != review.Id)
            {
                return NotFound();
            }

            
                try
                {
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                
               
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", review.CourseId);
            return RedirectToAction("Index", new { id = review.CourseId });
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                int courseId = review.CourseId;
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Reviews", new { id = courseId });
            }

            return RedirectToAction("Index", "Courses");
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
}
