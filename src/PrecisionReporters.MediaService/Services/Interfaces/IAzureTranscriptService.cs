using FluentResults;
using System.Threading.Tasks;

namespace PrecisionReporters.MediaService.Services.Interfaces
{
    public interface IAzureTranscriptService
    {
        Task<Result> GetAudioTranscript();
    }
}
