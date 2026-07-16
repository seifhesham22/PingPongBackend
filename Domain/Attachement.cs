namespace PingPong.API.Domain
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public string OriginalName { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public long Size { get; set; }
        public Guid UploaderId { get; set; }
        public string StoragePath { get; set; } = null!;
        // Deferred confirmation,uploaded files start as unconfirmed and will be cleaned up by a background job if never linked to a message later
        public bool IsConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? MessageId { get; set; }
        public Message? Message { get; set; }
    }
}