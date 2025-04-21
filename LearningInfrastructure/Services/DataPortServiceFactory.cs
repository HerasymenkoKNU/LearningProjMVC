// Services/CourseDataPortServiceFactory.cs
using System;
using LearningDomain.Model;

namespace LearningInfrastructure.Services
{
    public class DataPortServiceFactory
        : IDataPortServiceFactory<Course>
    {
        private readonly LearningMvcContext _context;
        public DataPortServiceFactory(LearningMvcContext context)
            => _context = context;

        public IImportService<Course> GetImportService(string contentType)
        {
            if (contentType
                == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                return new CourseImportService(_context);
            throw new NotSupportedException($"Import for {contentType} not supported.");
        }

        public IExportService<Course> GetExportService(string contentType)
        {
            if (contentType
                == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                return new CourseExportService(_context);
            throw new NotSupportedException($"Export for {contentType} not supported.");
        }
    }
}
