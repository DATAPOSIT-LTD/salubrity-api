namespace Salubrity.Application.DTOs.HomepageOverview
{
    public class OverviewDTO
    {
        public class OverviewStatsDto
        {
            public string Name { get; set; } = default!;
            public double Value { get; set; }
        }

        public class ServiceUptakeDto
        {
            public string Service { get; set; } = default!;
            public int Uptake { get; set; }
        }

        public class HomepageOverviewDto
        {
            public List<OverviewStatsDto> OverviewStats { get; set; } = [];
            public List<ServiceUptakeDto> ServiceUptake { get; set; } = [];
        }
    }
}
