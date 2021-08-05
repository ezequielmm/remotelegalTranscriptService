using Newtonsoft.Json.Converters;
using PrecisionReporters.MediaService.Data.Enums;
using System;
using System.Text.Json.Serialization;

namespace PrecisionReporters.MediaService.Data.Models
{
    public class DepositionEvent
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public EventType EventType { get; set; }
        public Guid UserId { get; set; }
        public string Details { get; set; }
    }
}
