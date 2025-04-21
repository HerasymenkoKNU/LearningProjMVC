using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LearningDomain.Model;

namespace LearningInfrastructure.Services
{
    public interface IExportService<TEntity>
        where TEntity : Entity
    {
        Task WriteToAsync(Stream stream, CancellationToken cancellationToken);
    }
}
