using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearningDomain.Model;
using LearningInfrastructure;

namespace LearningInfrastructure.Controllers
{
    public class CertificatesController : Controller
    {
        private readonly LearningMvcContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CertificatesController(LearningMvcContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

      
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ApproveCompletion(int id)
        {
            // id - это Id записи StudentsCourses
            var application = await _context.StudentsCourses
                .Include(sc => sc.Course)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (application == null)
                return NotFound("Заявка не знайдена.");

            if (application.Status != "Ожидает завершения")
            {
                return BadRequest("Статус заявки має бути 'Ожидает завершения'.");
            }

           
            application.Status = "Пройдено";
            _context.Update(application);
            await _context.SaveChangesAsync();

      
            var certificate = new Certificate
            {
                StudentCoursesId = application.Id,
                Name = $"Сертифікат курсу: {application.Course.Name}",
                Info = "Автоматичне генерування"
            
            };
            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

         
            return RedirectToAction("Index", "StudentsCourses");
        }

   
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyCertificates()
        {
            string userId = _userManager.GetUserId(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.IdentityId == userId);
            if (student == null)
                return NotFound("Студент не знайдений.");

            var certificates = await _context.Certificates
                .Include(c => c.StudentCourses)
                    .ThenInclude(sc => sc.Course)
                .Where(c => c.StudentCourses.StudentId == student.Id)
                .ToListAsync();

            return View(certificates);
        }

      
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Details(int id)
        {
            var certificate = await _context.Certificates
                .Include(c => c.StudentCourses)
                    .ThenInclude(sc => sc.Course)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (certificate == null)
                return NotFound();

            return View(certificate);
        }
    }
}
