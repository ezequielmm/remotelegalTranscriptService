using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using FluentResults;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Rest;
using PrecisionReporters.MediaService.AppConfigurations.Sections;
using PrecisionReporters.MediaService.Services.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PrecisionReporters.MediaService.Services
{
    public class AzureTranscriptService : IAzureTranscriptService
    {
        private readonly AzureMediaServiceConfiguration _azureConfiguration;
        private readonly ILogger<AzureTranscriptService> _logger;
        private readonly string _filePath;

        public AzureTranscriptService(IOptions<AzureMediaServiceConfiguration> azureConfiguration, ILogger<AzureTranscriptService> logger, IHostEnvironment env) 
        {
            _azureConfiguration = azureConfiguration.Value;
            _logger = logger;
            _filePath = env.ContentRootPath;
        }

        public async Task<Result> GetAudioTranscript()
        {
            try
            {
                var client = await CreateMediaServicesClientAsync();
                var compositionName = "CR.mp4";

                // Set the polling interval for long running operations to 2 seconds.
                // The default value is 30 seconds for the .NET client SDK
                client.LongRunningOperationRetryTimeout = 2;                

                // Creating a unique suffix so that we don't have name collisions with existing assets                
                string jobName = $"job-{compositionName}";
                string outputAssetName = $"output-{compositionName}";
                string inputAssetName = $"input-{compositionName}";

                // Create a new input Asset and upload the specified local media file into it.
                Asset inputAsset = await CreateInputAssetAsync(client,
                    _azureConfiguration.ResourceGroup,
                    _azureConfiguration.AccountName,
                    inputAssetName, Path.Combine(_filePath, @"TemporalAudios\AudioTest.mka")
                    );

                // Output from the encoding Job must be written to an Asset, so let's create one
                Asset outputAsset = await CreateOutputAssetAsync(client, _azureConfiguration.ResourceGroup, _azureConfiguration.AccountName, outputAssetName);

                // Then we use the PresetOverride property of the JobOutput to pass in the override values to use on this single Job
                // without the need to create a completely separate and new Transform with another langauge code or Mode setting. 
                // This can save a lot of complexity in your AMS account and reduce the number of Transforms used.
                JobOutput jobOutput = new JobOutputAsset()
                {
                    AssetName = outputAsset.Name
                };

                var job = await SubmitJobAsync(client, _azureConfiguration.ResourceGroup, _azureConfiguration.AccountName, "AudioTranscription", jobName, inputAssetName, jobOutput);

                return Result.Ok();
            }
            catch (Exception exception)
            {
                return Result.Fail($"{exception}");
            }
        }

        public async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync()
        {
            ServiceClientCredentials credentials;

            credentials = await GetCredentialsAsync();

            return new AzureMediaServicesClient(credentials)
            {
                SubscriptionId = _azureConfiguration.SubscriptionId,
            };
        }

        private async Task<ServiceClientCredentials> GetCredentialsAsync()
        {
            var scopes = new[] { _azureConfiguration.ArmAadAudience + "/.default" };

            var app = ConfidentialClientApplicationBuilder.Create(_azureConfiguration.AadClientId)
                .WithClientSecret(_azureConfiguration.AadSecret)
                .WithAuthority(AzureCloudInstance.AzurePublic, _azureConfiguration.AadTenantId)
                .Build();

            var authResult = await app.AcquireTokenForClient(scopes)
                                                     .ExecuteAsync()
                                                     .ConfigureAwait(false);

            return new TokenCredentials(authResult.AccessToken);
        }

        /// <summary>
        /// Creates a new input Asset and uploads the specified local media file into it.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="assetName">The asset name.</param>
        /// <param name="fileToUpload">The file you want to upload into the asset.</param>
        /// <returns></returns>
        private async Task<Asset> CreateInputAssetAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string assetName,
            string fileToUpload)
        {

            // Call Media Services API to create an Asset.
            var asset = await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, assetName, new Asset());

            // Use Media Services API to get back a response that contains
            // SAS URL for the Asset container into which to upload blobs.
            // That is where you would specify read-write permissions 
            // and the expiration time for the SAS URL.
            var response = await client.Assets.ListContainerSasAsync(
                resourceGroupName,
                accountName,
                assetName,
                permissions: AssetContainerPermission.ReadWrite,
                expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());

            var sasUri = new Uri(response.AssetContainerSasUrls.First());

            // Use Storage API to get a reference to the Asset container
            // that was created by calling Asset's CreateOrUpdate method.
            var blobClientOptions = new BlobClientOptions
            {
                Transport = new HttpClientTransport(new HttpClient { Timeout = Timeout.InfiniteTimeSpan }),
                Retry = { NetworkTimeout = Timeout.InfiniteTimeSpan }
            };

            BlobContainerClient container = new BlobContainerClient(blobContainerUri: sasUri, options: blobClientOptions);
            BlobClient blob = container.GetBlobClient("NameOfTheFile"); //TODO: Create a unique container per deposition to store assets

            // Use Storage API to upload the file into the container in storage.           
            await blob.UploadAsync(fileToUpload);
            return asset;
        }

        /// <summary>
        /// Creates an output asset. The output from the encoding Job must be written to an Asset.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="assetName">The output asset name.</param>
        /// <returns></returns>
        private async Task<Asset> CreateOutputAssetAsync(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string assetName)
        {
            // Create a new asset, we are not validating if the asset exist or not in Azure.
            var outputAsset = new Asset();
            return await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, assetName, outputAsset);
        }

        /// <summary>
        /// Submits a request to Media Services to apply the specified Transform to a given input video.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="transformName">The name of the transform.</param>
        /// <param name="jobName">The (unique) name of the job.</param>
        /// <param name="inputAssetName"></param>
        /// <param name="outputAssetName">The (unique) name of the  output asset that will store the result of the encoding job. </param>
        // <SubmitJob>
        private async Task<Job> SubmitJobAsync(IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string transformName,
            string jobName,
            string inputAssetName,
            JobOutput jobOutput)
        {
            JobInput jobInput = new JobInputAsset(assetName: inputAssetName);

            JobOutput[] jobOutputs =
            {
                jobOutput
            };

            Job job;
            try
            {
                job = await client.Jobs.CreateAsync(
                         resourceGroupName,
                         accountName,
                         transformName,
                         jobName,
                         new Job
                         {
                             Input = jobInput,
                             Outputs = jobOutputs,
                         });
            }
            catch (Exception exception)
            {
                if (exception.GetBaseException() is ApiErrorException apiException)
                {
                    _logger.LogError(
                        $"Media Service API call failed with error code '{apiException.Body.Error.Code}' and message '{apiException.Body.Error.Message}'.");
                }
                throw exception;
            }

            return job;
        }
    }
}
