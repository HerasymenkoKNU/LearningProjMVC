// Services/CourseExportService.cs
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using LearningDomain.Model;
using Microsoft.EntityFrameworkCore;

namespace LearningInfrastructure.Services
{
    public class CourseExportService : IExportService<Course>
    {
        private readonly LearningMvcContext _context;
        public CourseExportService(LearningMvcContext context)
            => _context = context;

        public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Courses");

            // Заголовки
            ws.Cell(1, 1).Value = "Name";
            ws.Cell(1, 2).Value = "Info";
            ws.Row(1).Style.Font.Bold = true;

            // Данные
            var courses = await _context.Courses.ToListAsync(cancellationToken);
            for (int i = 0; i < courses.Count; i++)
            {
                ws.Cell(i + 2, 1).Value = courses[i].Name;
                ws.Cell(i + 2, 2).Value = courses[i].Info;
            }

            workbook.SaveAs(stream);
        }
    }
}
