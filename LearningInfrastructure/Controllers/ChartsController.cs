using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using LearningInfrastructure;           
using LearningDomain.Model;             

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

     
        private record CountResponseItem(string Label, int Count);

      
        [HttpGet("studentsByCourse")]
        public async Task<IActionResult> GetStudentsByCourse(CancellationToken cancellationToken)
        {
   
            var responseItems = await _context.StudentsCourses
                .Include(sc => sc.Course)
                .GroupBy(sc => sc.Course.Name)            
                .Select(g => new CountResponseItem(g.Key, g.Count()))
                .ToListAsync(cancellationToken);

            return new JsonResult(responseItems);
        }

      
        [HttpGet("certificatesByStudent")]
        public async Task<IActionResult> GetCertificatesByStudent(CancellationToken cancellationToken)
        {
      
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
