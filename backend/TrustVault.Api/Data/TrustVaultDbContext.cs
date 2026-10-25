using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrustVault.Api.Models;
using TrustVault.Api.Domain;

namespace TrustVault.Api.Data;

public class TrustVaultDbContext : IdentityDbContext<ApplicationUser>
{
    public TrustVaultDbContext(
        DbContextOptions<TrustVaultDbContext> options)
        : base(options)
    {
    }
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentPermission> DocumentPermissions => Set<DocumentPermission>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
}