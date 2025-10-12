namespace Salubrity.Application.Interfaces.Repositories.DB_Dump
{
    public interface IDatabaseDumpRepository
    {
        Task<string> CreateDumpAsync();
    }
}
