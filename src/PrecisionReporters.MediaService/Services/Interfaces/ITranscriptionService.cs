using FluentResults;
using PrecisionReporters.MediaService.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrecisionReporters.MediaService.Services.Interfaces
{
    public interface ITranscriptionService
    {
        Task<List<Transcription>> GetTranscriptionsByDepositionId(Guid depositionId);
        Task<List<TranscriptionItem>> ParseStream(); //Stream stream, Encoding encoding
    }
}
