using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrecisionReporters.MediaService.AppConfigurations.Sections
{
    public class AzureMediaServiceConfiguration
    {
        public const string SectionName = "AzureMediaServiceConfiguration";
        public string AadClientId { get; set; }
        public string AadSecret { get; set; }
        public string AadTenantId { get; set; }
        public string AccountName { get; set; }
        public string ArmAadAudience { get; set; }
        public string ArmEndpoint { get; set; }
        public string ResourceGroup { get; set; }
        public string SubscriptionId { get; set; }
    }
}
