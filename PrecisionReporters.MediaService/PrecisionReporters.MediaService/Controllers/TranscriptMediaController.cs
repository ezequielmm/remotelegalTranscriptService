using FluentResults;
using Microsoft.AspNetCore.Mvc;
using PrecisionReporters.MediaService.Data.Models;
using PrecisionReporters.MediaService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrecisionReporters.MediaService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TranscriptMediaController : Controller
    {
        private readonly ITranscriptionService _transcriptMediaService;
        private readonly IAzureTranscriptService _azureTranscriptService;

        public TranscriptMediaController(ITranscriptionService transcriptMediaService, IAzureTranscriptService azureTranscriptService)
        {
            _transcriptMediaService = transcriptMediaService;
            _azureTranscriptService = azureTranscriptService;
        }
        /// <summary>
        /// Gets a Transcriptions based on depostionId
        /// </summary>
        /// <param name="depositionId"></param>
        /// <returns>Participant transcriptions for an entire deposition</returns>
        [HttpGet("{depositionId}")]
        public async Task<ActionResult<List<Transcription>>> GetTranscriptions(Guid depositionId) 
        {
            return await _transcriptMediaService.GetTranscriptionsByDepositionId(depositionId);
        }

        [HttpGet("audioTranscript")]
        public async Task<Result> GetAudioTranscript()
        {
            return await _azureTranscriptService.GetAudioTranscript();
        }
    }
}
