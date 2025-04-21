// Services/IImportService.cs
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LearningDomain.Model;

namespace LearningInfrastructure.Services
{
    public interface IImportService<TEntity>
        where TEntity : Entity
    {
        Task ImportFromStreamAsync(Stream stream, CancellationToken cancellationToken);
    }
}