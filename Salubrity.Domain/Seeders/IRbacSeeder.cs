using System.Threading;
using System.Threading.Tasks;

namespace Salubrity.Domain.Seeders;


public interface IRbacSeeder
{
    Task SeedDefaultRolesAsync(CancellationToken cancellationToken = default);
}
