using PrecisionReporters.MediaService.AppConfigurations.Sections;

namespace PrecisionReporters.MediaService.AppConfigurations
{
    public interface IAppConfiguration
    {
        ConnectionStrings ConnectionStrings { get; set; }
        Swagger Swagger { get; set; }
    }
}
