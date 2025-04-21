// Services/CourseImportService.cs
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using LearningDomain.Model;
using Microsoft.EntityFrameworkCore;

namespace LearningInfrastructure.Services
{
    public class CourseImportService : IImportService<Course>
    {
        private readonly LearningMvcContext _context;
        public CourseImportService(LearningMvcContext context)
            => _context = context;

        public async Task ImportFromStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Stream is not readable", nameof(stream));

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            // Предполагаем, что первая строка — заголовок
            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                var name = row.Cell(1).GetString().Trim();
                var info = row.Cell(2).GetString().Trim();

                if (string.IsNullOrEmpty(name))
                    continue; // или бросить ошибку

                // Проверим, есть ли уже курс с таким именем
                var existing = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

                if (existing == null)
                {
                    // новый
                    var course = new Course
                    {
                        Name = name,
                        Info = info
                    };
                    _context.Courses.Add(course);
                }
                else
                {
                    // можем обновить инфо, если нужно
                    existing.Info = info;
                    _context.Courses.Update(existing);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
