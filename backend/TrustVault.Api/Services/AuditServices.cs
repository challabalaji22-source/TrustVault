
using TrustVault.Api.Data;
using TrustVault.Api.Domain;

namespace TrustVault.Api.Services;

public class AuditService
{
    private readonly TrustVaultDbContext _dbContext;

    public AuditService(TrustVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(
        string? userId,
        string action,
        Guid? documentId,
        string details,
        bool succeeded)
    {
        var auditEvent = new AuditEvent
        {
            UserId = userId,
            Action = action,
            DocumentId = documentId,
            Details = details,
            Succeeded = succeeded,
            OccurredAt = DateTimeOffset.UtcNow
        };

        _dbContext.AuditEvents.Add(auditEvent);
        await _dbContext.SaveChangesAsync();
    }
}
