using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearningDomain.Model;
using LearningInfrastructure;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LearningInfrastructure.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly LearningMvcContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewsController(LearningMvcContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reviews?courseId=5&courseName=...
        public async Task<IActionResult> Index(int? id, string? name)
        {
            if (id == null)
                return RedirectToAction("Index", "Courses");

            if (string.IsNullOrEmpty(name))
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                    return NotFound();
                name = course.Name;
            }

            ViewBag.CourseId = id;
            ViewBag.CourseName = name;

            var reviews = await _context.Reviews
                                        .Where(r => r.CourseId == id)
                                        .Include(r => r.Course)
                                        .ToListAsync();
            return View(reviews);
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            var review = await _context.Reviews
                .Include(r => r.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (review == null)
                return NotFound();
            return View(review);
        }

        // GET: Reviews/Create
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create(int courseId)
        {
            // Проверяем, что заявка студента подтверждена, иначе запрещаем создание
            string userId = _userManager.GetUserId(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.IdentityId == userId);
            if (student != null)
            {
                var application = await _context.StudentsCourses
                    .FirstOrDefaultAsync(sc => sc.CourseId == courseId && sc.StudentId == student.Id);
                if (application == null || application.Status != "Принято" && application.Status != "Пройдено")
                {
                    TempData["AccessMessage"] = "Створення відгуків доступне лише після підтвердження заявки.";
                    return RedirectToAction("AccessDenied", "Courses");
                }
            }
            ViewBag.CourseId = courseId;
            ViewBag.CourseName = (await _context.Courses.FindAsync(courseId))?.Name;
            return View();
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create(int courseId, [Bind("Id,Name,Info")] Review review)
        {
            // Проверяем заявку так же, как в GET
            string userId = _userManager.GetUserId(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.IdentityId == userId);
            if (student != null)
            {
                var application = await _context.StudentsCourses
                    .FirstOrDefaultAsync(sc => sc.CourseId == courseId && sc.StudentId == student.Id);
                if (application == null || application.Status != "Принято")
                {
                    TempData["AccessMessage"] = "Створення відгуків доступне лише після підтвердження заявки.";
                    return RedirectToAction("AccessDenied", "Courses");
                }
            }
            review.CourseId = courseId;
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { id = courseId });
        }

        // GET: Reviews/Edit/5 (для учителя)
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
            var review = _context.Reviews
                .Include(r => r.Course)
                .FirstOrDefault(r => r.Id == id);
            if (review == null)
                return NotFound();
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", review.CourseId);
            return View(review);
        }

        // POST: Reviews/Edit/5 (для учителя)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Info,CourseId,Id")] Review review)
        {
            if (id != review.Id)
                return NotFound();

            try
            {
                _context.Update(review);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(review.Id))
                    return NotFound();
                else
                    throw;
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", review.CourseId);
            return RedirectToAction("Index", new { id = review.CourseId });
        }

        // GET: Reviews/Delete/5 (для учителя)
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();
            var review = await _context.Reviews
                .Include(r => r.Course)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
                return NotFound();
            return View(review);
        }

        // POST: Reviews/Delete/5 (для учителя)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                int courseId = review.CourseId;
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { id = courseId });
            }
            return RedirectToAction("Index", "Courses");
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
}
