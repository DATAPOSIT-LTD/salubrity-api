using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Salubrity.Infrastructure.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Salubrity.Api"));

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString); // ← ✅ Change here

            var mockMediator = new NoMediator();

            return new AppDbContext(optionsBuilder.Options, mockMediator);
        }

        private class NoMediator : IMediator
        {
            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
                => Task.FromResult(default(TResponse)!);

            public Task<object?> Send(object request, CancellationToken cancellationToken = default)
                => Task.FromResult<object?>(null);

            public Task Publish(object notification, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
                where TNotification : INotification
                => Task.CompletedTask;

            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
                => AsyncEnumerable.Empty<TResponse>();

            public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
                => AsyncEnumerable.Empty<object?>();
        }
    }
}
