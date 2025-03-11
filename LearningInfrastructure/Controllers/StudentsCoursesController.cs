using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearningDomain.Model;
using LearningInfrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace LearningInfrastructure.Controllers
{
    [Authorize]
    public class StudentsCoursesController : Controller
    {
        private readonly LearningMvcContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentsCoursesController(LearningMvcContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===== Для студентов =====

        // GET: StudentsCourses/Create?courseId=5
        [Authorize(Roles = "Student")]
        public IActionResult Create(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        // POST: StudentsCourses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(int courseId)
        {
            
            string userId = _userManager.GetUserId(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.IdentityId == userId);
            if (student == null)
            {
                return NotFound("Студента не знайдено");
            }

            
            var existingApplication = await _context.StudentsCourses
                .FirstOrDefaultAsync(sc => sc.CourseId == courseId && sc.StudentId == student.Id);
            if (existingApplication != null)
            {
                ModelState.AddModelError("", "Ви вже подавали заявку на цей курс");
                ViewBag.CourseId = courseId;
                return View();
            }

   
            var application = new StudentsCourse
            {
                CourseId = courseId,
                StudentId = student.Id,
                Status = "Ожидает"
            };

            _context.StudentsCourses.Add(application);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }

        
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyCourses()
        {
            string userId = _userManager.GetUserId(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.IdentityId == userId);
            if (student == null)
                return NotFound("Студент не знайдений.");

           
            var approvedApplications = await _context.StudentsCourses
                .Where(sc => sc.StudentId == student.Id
                          && (sc.Status == "Принято" || sc.Status == "Пройдено"))
                .Include(sc => sc.Course)
                .ToListAsync();

            return View(approvedApplications);
        }


   
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Index()
        {
            
            var applications = await _context.StudentsCourses
                .Include(sc => sc.Course)
                .Include(sc => sc.Student)
                .Where(sc => sc.Status == "Ожидает" || sc.Status == "Ожидает завершения")
                .ToListAsync();
            return View(applications);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> RequestCompletion(int id)
        {
          
            var application = await _context.StudentsCourses.FindAsync(id);
            if (application == null)
                return NotFound("Заявка не знайдена.");

            if (application.Status != "Принято")
            {
                return BadRequest("Неможливо запросити завершення, якщо заявка не 'Принято'.");
            }

            
            application.Status = "Ожидает завершения";
            _context.Update(application);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyCourses");
        }
        
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Approve(int id)
        {
            var application = await _context.StudentsCourses.FindAsync(id);
            if (application == null)
                return NotFound();

            application.Status = "Принято";
            _context.Update(application);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Reject(int id)
        {
            var application = await _context.StudentsCourses.FindAsync(id);
            if (application == null)
                return NotFound();

            application.Status = "Отклонено";
            _context.Update(application);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
