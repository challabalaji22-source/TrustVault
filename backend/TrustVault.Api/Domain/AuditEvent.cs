namespace TrustVault.Api.Domain;

public class AuditEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? UserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public Guid? DocumentId { get; set; }

    public string Details { get; set; } = string.Empty;

    public bool Succeeded { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
        = DateTimeOffset.UtcNow;
}