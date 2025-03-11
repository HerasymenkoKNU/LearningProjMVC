using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LearningDomain.Model;
using LearningInfrastructure;
using Microsoft.AspNetCore.Authorization;

namespace LearningInfrastructure.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly LearningMvcContext _context;

       
        public CoursesController(LearningMvcContext context) 
        {
            _context = context;
        }

    
        [Authorize]
        public async Task<IActionResult> Index(string searchString)
        {
            
            var courses = _context.Courses.AsQueryable();

           
            if (!string.IsNullOrEmpty(searchString))
            {
             
                courses = courses.Where(c => c.Name.Contains(searchString));
            }

            var courseList = await courses.ToListAsync();

           
            if (!string.IsNullOrEmpty(searchString) && courseList.Count == 0)
            {
              
                ViewBag.NotFoundMessage = $"Не вдалось знайти курс с ім'ям «{searchString}».";

           
                courseList = await _context.Courses.ToListAsync();
            }

            return View(courseList);
        }
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            var message = TempData["AccessMessage"] as string ?? "Доступ заборонено.";
            return View((object)message);
        }

        // GET: Courses/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }
        [Authorize]
        public async Task<IActionResult> Lessons(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return RedirectToAction("Index", "Lessons", new { id = course.Id, name = course.Name });
        }
        [Authorize]
        public async Task<IActionResult> Reviews(int? id) // id — это CourseId
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(s => s.Id == id);
            if (course == null)
            {
                return NotFound();
            }
            return RedirectToAction("Index", "Reviews", new { id = course.Id, name = course.Name });

        }
        [Authorize]
        public async Task<IActionResult> Tests(int? id) // id — это CourseId
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(s => s.Id == id);
            if (course == null)
            {
                return NotFound();
            }
            return RedirectToAction("Index", "Tests", new { id = course.Id, name = course.Name });

        }
        // GET: Courses/Create
        [Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create([Bind("Name,Info,Id")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: Courses/Edit/5
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Info,Id")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
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
            return View(course);
        }

        // GET: Courses/Delete/5
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
