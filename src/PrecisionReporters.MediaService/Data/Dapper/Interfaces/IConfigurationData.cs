using FluentResults;
using PrecisionReporters.MediaService.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrecisionReporters.MediaService.Data.Dapper.Interfaces
{
    public interface IConfigurationData
    {
        Task<IEnumerable<Transcription>> GetTranscriptionsAsync(Guid depositionId);
        Task<int> SaveTranscriptionsAsync(Transcription transcript);
    }
}
