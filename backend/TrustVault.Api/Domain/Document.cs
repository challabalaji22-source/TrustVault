namespace TrustVault.Api.Domain;

public class Document
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string OwnerId { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTimeOffset UploadedAt { get; set; }
        = DateTimeOffset.UtcNow;
}