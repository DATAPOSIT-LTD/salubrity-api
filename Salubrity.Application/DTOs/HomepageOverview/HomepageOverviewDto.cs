namespace Salubrity.Application.DTOs.HomepageOverview
{
    public class HomepageOverviewDto
    {
        public List<OverviewStatsDto> OverviewStats { get; set; } = [];
        public List<ServiceUptakeDto> ServiceUptake { get; set; } = [];
    }
}
