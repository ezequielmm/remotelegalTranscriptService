using System;

namespace PrecisionReporters.MediaService.Data.Models
{
    public class Transcription : BaseEntity<Transcription>
    {
        public string Text { get; set; }
        public Guid UserId { get; set; }
        public Guid DepositionId { get; set; }
        public DateTime TranscriptDateTime { get; set; }
        public int Duration { get; set; }
        public double Confidence { get; set; }
        public bool PostProcessed { get; set; }
    }
}
