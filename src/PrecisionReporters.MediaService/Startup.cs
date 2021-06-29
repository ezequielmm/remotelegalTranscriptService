using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrecisionReporters.MediaService.AppConfigurations;
using PrecisionReporters.MediaService.AppConfigurations.Sections;
using PrecisionReporters.MediaService.Data.Dapper;
using PrecisionReporters.MediaService.Data.Dapper.Interfaces;
using PrecisionReporters.MediaService.Services;
using PrecisionReporters.MediaService.Services.Interfaces;

namespace PrecisionReporters.MediaService
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddUserSecrets("6c3d9886-0df6-47d9-948c-01dce9eab1c4")
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var appConfiguration = Configuration.GetApplicationConfig();            

            // Repositories
            services.AddTransient<IConfigurationData>(o => new ConfigurationData(appConfiguration.ConnectionStrings.MySqlConnection));

            // Services
            services.AddTransient<ITranscriptionService, TranscriptionService>();
            services.AddTransient<IAzureTranscriptService, AzureTranscriptService>();
            services.Configure<AzureMediaServiceConfiguration>(x =>
            {
                x.AadClientId = appConfiguration.AzureMediaServiceConfiguration.AadClientId;
                x.AadSecret = appConfiguration.AzureMediaServiceConfiguration.AadSecret;
                x.AadTenantId = appConfiguration.AzureMediaServiceConfiguration.AadTenantId;
                x.AccountName = appConfiguration.AzureMediaServiceConfiguration.AccountName;
                x.ArmAadAudience = appConfiguration.AzureMediaServiceConfiguration.ArmAadAudience;
                x.ArmEndpoint = appConfiguration.AzureMediaServiceConfiguration.ArmEndpoint;
                x.ResourceGroup = appConfiguration.AzureMediaServiceConfiguration.ResourceGroup;
                x.SubscriptionId = appConfiguration.AzureMediaServiceConfiguration.SubscriptionId;
            });

            // Register the Swagger generator, defining our Swagger documents
            services.AddSwaggerGen();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Media Service V1");
            });


            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
