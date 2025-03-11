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
using Microsoft.AspNetCore.Identity;

namespace LearningInfrastructure.Controllers
{
    [Authorize]
    public class LessonsController : Controller
    {
        private readonly LearningMvcContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LessonsController(LearningMvcContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Lessons?courseId=5&courseName=...
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

            // Если пользователь студент, проверяем его заявку на курс
            if (User.IsInRole("Student"))
            {
                string userId = _userManager.GetUserId(User);
                var student = await _context.Students.FirstOrDefaultAsync(s => s.IdentityId == userId);
                if (student == null)
                {
                    TempData["AccessMessage"] = "Студент не знайдений.";
                    return RedirectToAction("AccessDenied", "Courses");
                }
                var application = await _context.StudentsCourses
                    .FirstOrDefaultAsync(sc => sc.CourseId == id && sc.StudentId == student.Id);
                if (application == null || application.Status != "Принято" && application.Status != "Пройдено")
                {
                    TempData["AccessMessage"] = "Доступ до уроків можливий лише після підтвердження заявки.";
                    return RedirectToAction("AccessDenied", "Courses");
                }
            }

            var lessons = await _context.Lessons
                                         .Where(l => l.CourseId == id)
                                         .Include(l => l.Course)
                                         .ToListAsync();
            return View(lessons);
        }


        // GET: Lessons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View(lesson);
        }

        // GET: Lessons/Create
        [Authorize(Roles = "Teacher")]
        public IActionResult Create(int courseId)
        {
         ViewBag.CourseId = courseId;
            ViewBag.CourseName = _context.Courses.Where(c => c.Id == courseId).FirstOrDefault().Name;
            return View();
        }

        // POST: Lessons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(int courseId,[Bind("Id,Name,Info,VideoUrl,DocxUrl")] Lesson lesson)
        {
            lesson.CourseId = courseId;
            
                _context.Add(lesson);
                await _context.SaveChangesAsync();
              
            
            return RedirectToAction("Index", "Lessons", new { id = courseId, name = _context.Courses.Where(c=>c.Id==courseId).FirstOrDefault().Name });
        }

        // GET: Lessons/Edit/5
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = _context.Lessons
       .Include(l => l.Course) 
       .FirstOrDefault(l => l.Id == id);
            if (lesson == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", lesson.CourseId);
            return View(lesson);
        }

        // POST: Lessons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Info,VideoUrl,DocxUrl,CourseId,Id")] Lesson lesson)
        {
            if (id != lesson.Id)
            {
                return NotFound();
            }

            
                try
                {
                    _context.Update(lesson);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LessonExists(lesson.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                
               
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", lesson.CourseId);
            return RedirectToAction("Index", new { id = lesson.CourseId});
        }

        // GET: Lessons/Delete/5
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson != null)
            {
                int courseId = lesson.CourseId;
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Lessons", new { id = courseId });
            }

            return RedirectToAction("Index", "Courses");
        }

        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(e => e.Id == id);
        }
    }
}
