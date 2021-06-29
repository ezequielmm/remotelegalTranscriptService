using System;

namespace PrecisionReporters.MediaService.Data.Models
{
    public abstract class BaseEntity<T>
    {
        public Guid Id { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
