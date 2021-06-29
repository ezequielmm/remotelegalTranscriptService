using FluentResults;
using PrecisionReporters.MediaService.Data.Dapper.Interfaces;
using PrecisionReporters.MediaService.Data.Models;
using PrecisionReporters.MediaService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrecisionReporters.MediaService.Services
{
    public class TranscriptionService : ITranscriptionService
    {
        private readonly IConfigurationData _configurationData;

        public TranscriptionService(IConfigurationData configurationData) 
        {
            _configurationData = configurationData;
        }

        public async Task<List<Transcription>> GetTranscriptionsByDepositionId(Guid depositionId)
        {
            var transcripts = await _configurationData.GetTranscriptionsAsync(depositionId);
            return transcripts.ToList();
        }
    }
}
