namespace PingPong.API.Domain
{
    public sealed class FileMetaData
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;
        public string OriginalFileName { get; private set; }
        public string StoredFileName { get; private set; }
        public string FilePath { get; private set; }
        public string ContentType { get; private set; }
        public long Size { get; private set; }
        public DateTime UploadedAt { get; private set; }

        public FileMetaData(
            Guid userId,
            string originalFileName,
            string storedFileName,
            string filePath,
            string contentType,
            long size)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            OriginalFileName = originalFileName;
            StoredFileName = storedFileName;
            FilePath = filePath;
            ContentType = contentType;
            Size = size;
            UploadedAt = DateTime.UtcNow;
        }
    }
}