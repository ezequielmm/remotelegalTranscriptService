using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrecisionReporters.MediaService.AppConfigurations
{
    public static class ConfigurationExtensions
    {
        public static AppConfiguration GetApplicationConfig(this IConfiguration configuration)
        {
            var appConfig = new AppConfiguration();
            configuration.GetSection("AppConfiguration").Bind(appConfig);
            return appConfig;
        }
    }
}
