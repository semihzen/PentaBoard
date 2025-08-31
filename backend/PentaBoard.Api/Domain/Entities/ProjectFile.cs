using System;

namespace PentaBoard.Api.Domain.Entities
{
    public class ProjectFile
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // ilişkiler
        public Guid ProjectId { get; set; }      
        public Guid UploadedById { get; set; }   

        // meta
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = "application/pdf";
        public long SizeBytes { get; set; }
        public string StoragePath { get; set; } = null!;

        // audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
