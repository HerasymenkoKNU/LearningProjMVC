using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using LearningInfrastructure;            // пространство имён вашего проекта
using LearningDomain.Model;             // если в этом неймспейсе лежат сущности

namespace LearningInfrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartsController : ControllerBase
    {
        private readonly LearningMvcContext _context;

        public ChartsController(LearningMvcContext context)
        {
            _context = context;
        }

        // Вспомогательная модель ответа, которую будем сериализовать в JSON
        private record CountResponseItem(string Label, int Count);

        /// <summary>
        /// Круговая диаграмма: Распределение студентов по курсам
        /// </summary>
        [HttpGet("studentsByCourse")]
        public async Task<IActionResult> GetStudentsByCourse(CancellationToken cancellationToken)
        {
            // Группируем по названию курса и считаем количество студентов (записей) в связующей таблице
            var responseItems = await _context.StudentsCourses
                .Include(sc => sc.Course)
                .GroupBy(sc => sc.Course.Name)            // группировка по имени курса
                .Select(g => new CountResponseItem(g.Key, g.Count()))
                .ToListAsync(cancellationToken);

            return new JsonResult(responseItems);
        }

        /// <summary>
        /// Столбчатая диаграмма: Количество сертификатов у каждого студента
        /// </summary>
        [HttpGet("certificatesByStudent")]
        public async Task<IActionResult> GetCertificatesByStudent(CancellationToken cancellationToken)
        {
            // Предполагается, что:
            //   Certificate.StudentCoursesId -> навигационное свойство -> StudentCourses
            //   StudentCourses.StudentId     -> навигационное свойство -> Student
            //   Student.Name                 -> имя студента
            var responseItems = await _context.Certificates
                .Include(c => c.StudentCourses)
                    .ThenInclude(sc => sc.Student)
                .GroupBy(c => c.StudentCourses.Student.Name)
                .Select(g => new CountResponseItem(g.Key, g.Count()))
                .ToListAsync(cancellationToken);

            return new JsonResult(responseItems);
        }
    }
}
