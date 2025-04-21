using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearningDomain.Model;
using LearningInfrastructure;
using LearningInfrastructure.Services;  // Для доступа к IDataPortServiceFactory

namespace LearningInfrastructure.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly LearningMvcContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDataPortServiceFactory<Course> _dataPortServiceFactory;

        public CoursesController(LearningMvcContext context,
                                 UserManager<ApplicationUser> userManager,
                                 IDataPortServiceFactory<Course> dataPortServiceFactory)
        {
            _context = context;
            _userManager = userManager;
            _dataPortServiceFactory = dataPortServiceFactory;
        }

        // GET: Courses
        // Для учителя отображаем только его курсы; для студентов – все курсы (с поиском)
        public async Task<IActionResult> Index(string searchString)
        {
            IQueryable<Course> courses = _context.Courses;

            if (User.IsInRole("Teacher"))
            {
                // Получаем текущего учителя
                string userId = _userManager.GetUserId(User);
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdentityId == userId);
                if (teacher != null)
                {
                    // Получаем список идентификаторов курсов, связанных с этим учителем через TeachersCourses
                    var teacherCourseIds = await _context.TeachersCourses
                        .Where(tc => tc.TeacherId == teacher.Id)
                        .Select(tc => tc.CourseId)
                        .ToListAsync();
                    courses = courses.Where(c => teacherCourseIds.Contains(c.Id));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    courses = courses.Where(c => c.Name.Contains(searchString));
                }
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
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses.FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
                return NotFound();

            return View(course);
        }

        [Authorize]
        public async Task<IActionResult> Lessons(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses.FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
                return NotFound();

            return RedirectToAction("Index", "Lessons", new { id = course.Id, name = course.Name });
        }

        [Authorize]
        public async Task<IActionResult> Reviews(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses.FirstOrDefaultAsync(s => s.Id == id);
            if (course == null)
                return NotFound();

            return RedirectToAction("Index", "Reviews", new { id = course.Id, name = course.Name });
        }

        [Authorize]
        public async Task<IActionResult> Tests(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses.FirstOrDefaultAsync(s => s.Id == id);
            if (course == null)
                return NotFound();

            return RedirectToAction("Index", "Tests", new { id = course.Id, name = course.Name });
        }

        // GET: Courses/Create (для учителя)
        [Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create (для учителя)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create([Bind("Name,Info,Id")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();

                // После создания курса, связываем его с текущим учителем через TeachersCourse
                string userId = _userManager.GetUserId(User);
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdentityId == userId);
                if (teacher != null)
                {
                    var teacherCourse = new TeachersCourse
                    {
                        CourseId = course.Id,
                        TeacherId = teacher.Id
                    };
                    _context.TeachersCourses.Add(teacherCourse);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: Courses/Edit/5 (для учителя)
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            string userId = _userManager.GetUserId(User);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdentityId == userId);
            if (teacher == null)
                return NotFound("Учитель не знайдений.");

            bool isTeacherCourse = await _context.TeachersCourses.AnyAsync(tc => tc.TeacherId == teacher.Id && tc.CourseId == id);
            if (!isTeacherCourse)
                return Forbid("Ви не маєте доступу до редагування цього курсу.");

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();
            return View(course);
        }

        // POST: Courses/Edit/5 (для учителя)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Info,Id")] Course course)
        {
            if (id != course.Id)
                return NotFound();

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
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: Courses/Delete/5 (для учителя)
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses.FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
                return NotFound();

            return View(course);
        }

        // POST: Courses/Delete/5 (для учителя)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses
                .Include(c => c.StudentsCourses)
                    .ThenInclude(sc => sc.Certificates)
                .Include(c => c.TeachersCourses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course != null)
            {
                foreach (var sc in course.StudentsCourses)
                {
                    var certs = await _context.Certificates.Where(x => x.StudentCoursesId == sc.Id).ToListAsync();
                    _context.Certificates.RemoveRange(certs);
                }
                _context.StudentsCourses.RemoveRange(course.StudentsCourses);
                _context.TeachersCourses.RemoveRange(course.TeachersCourses);
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }

        // ==========================
        // Методы импорта и экспорта
        // Доступны только для учителя
        // GET: Courses/Import
        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public IActionResult Import()
        {
            return View();
        }

        // POST: Courses/Import
        [HttpPost, ActionName("Import")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ImportAsync()
        {
            var file = Request.Form.Files["fileExcel"];
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Оберіть файл для завантаження");
                return View();
            }

            // 1) Собственно импорт курсов
            string contentType = file.ContentType;
            var importService = _dataPortServiceFactory.GetImportService(contentType);
            using (var stream = file.OpenReadStream())
            {
                await importService.ImportFromStreamAsync(stream, HttpContext.RequestAborted);
            }

            // 2) Определяем текущего преподавателя
            string userId = _userManager.GetUserId(User);
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.IdentityId == userId);
            if (teacher == null)
            {
                // должно быть всегда, но на всякий случай
                TempData["ErrorMessage"] = "Не знайдений поточний викладач.";
                return RedirectToAction(nameof(Index));
            }

            // 3) Повторно открываем файл, чтобы получить список импортированных имён
            var importedNames = new List<string>();
            using (var wb = new ClosedXML.Excel.XLWorkbook(file.OpenReadStream()))
            {
                var ws = wb.Worksheets.First();
                foreach (var row in ws.RowsUsed().Skip(1))
                {
                    var name = row.Cell(1).GetString().Trim();
                    if (!string.IsNullOrEmpty(name))
                        importedNames.Add(name);
                }
            }

            // 4) Для каждого импортированного курса создаём связь с этим учителем
            var importedCourses = await _context.Courses
                .Where(c => importedNames.Contains(c.Name))
                .ToListAsync();
            foreach (var c in importedCourses)
            {
                bool linked = await _context.TeachersCourses
                    .AnyAsync(tc => tc.TeacherId == teacher.Id && tc.CourseId == c.Id);
                if (!linked)
                {
                    _context.TeachersCourses.Add(new TeachersCourse
                    {
                        CourseId = c.Id,
                        TeacherId = teacher.Id
                    });
                }
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Курси успішно імпортовані!";
            return RedirectToAction(nameof(Index));
        }
        // GET: Courses/Export
        [HttpGet]
        [Authorize(Roles = "Teacher")]
        [ActionName("Export")]
        public async Task<IActionResult> ExportAsync(string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            var exportService = _dataPortServiceFactory.GetExportService(contentType);
            var memoryStream = new MemoryStream();
            await exportService.WriteToAsync(memoryStream, HttpContext.RequestAborted);
            memoryStream.Position = 0;
            return new FileStreamResult(memoryStream, contentType)
            {
                FileDownloadName = $"courses_{DateTime.UtcNow:yyyyMMdd}.xlsx"
            };
        }

    }
}
