using PrecisionReporters.MediaService.AppConfigurations.Sections;

namespace PrecisionReporters.MediaService.AppConfigurations
{
    public class AppConfiguration
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public Swagger Swagger { get; set; }
        public AzureMediaServiceConfiguration AzureMediaServiceConfiguration { get; set; }
    }
}
