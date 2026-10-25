namespace TrustVault.Api.Domain;

public class DocumentPermission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DocumentId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public bool CanRead { get; set; } = true;

    public bool CanEdit { get; set; } = false;

    public DateTimeOffset GrantedAt { get; set; }
        = DateTimeOffset.UtcNow;

    public string GrantedByUserId { get; set; } = string.Empty;
}