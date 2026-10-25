
using Microsoft.AspNetCore.Http;
using TrustVault.Api.Domain;
using TrustVault.Api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustVault.Api.Data;
using Microsoft.AspNetCore.Identity;
using TrustVault.Api.Models;

namespace TrustVault.Api.Endpoints;

public static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(
        this IEndpointRouteBuilder app)
    {
        var documents = app.MapGroup("/api/documents")
            .RequireAuthorization();

        documents.MapGet("/", async (
            ClaimsPrincipal user,
            TrustVaultDbContext dbContext) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var ownedDocuments = await dbContext.Documents
                .AsNoTracking()
                .Where(document => document.OwnerId == userId)
                .OrderByDescending(document => document.UploadedAt)
                .Select(document => new
                {
                    document.Id,
                    document.FileName,
                    document.Category,
                    document.ContentType,
                    document.SizeBytes,
                    document.UploadedAt
                })
                .ToListAsync();

            return Results.Ok(ownedDocuments);

});
documents.MapGet("/shared", async (
    ClaimsPrincipal user,
    TrustVaultDbContext dbContext) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrWhiteSpace(userId))
        return Results.Unauthorized();

    var sharedDocuments = await (
        from permission in dbContext.DocumentPermissions.AsNoTracking()
        join document in dbContext.Documents.AsNoTracking()
            on permission.DocumentId equals document.Id
        where permission.UserId == userId
              && permission.CanRead
              && document.OwnerId != userId
        orderby document.UploadedAt descending
        select new
        {
            document.Id,
            document.FileName,
            document.Category,
            document.ContentType,
            document.SizeBytes,
            document.UploadedAt
        })
        .ToListAsync();

    return Results.Ok(sharedDocuments);
});
        
        documents.MapPost("/upload", async (
            IFormFile file,
            string category,
            ClaimsPrincipal user,
            TrustVaultDbContext dbContext,
            DocumentStorageService storage,
            AuditService audit,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            if (file is null || file.Length == 0)
            {
                return Results.BadRequest("Please select a non-empty file.");
            }

            const long maxFileSize = 10 * 1024 * 1024; // 10 MB

            if (file.Length > maxFileSize)
            {
                return Results.BadRequest("Maximum file size is 10 MB.");
            }

            if (string.IsNullOrWhiteSpace(category) || category.Length > 100)
            {
                return Results.BadRequest("Provide a category of up to 100 characters.");
            }

            var originalName = Path.GetFileName(file.FileName);

            if (string.IsNullOrWhiteSpace(originalName) || originalName.Length > 255)
            {
                return Results.BadRequest("Invalid file name.");
            }

            string storedFileName;
            long actualSize;

            try
            {
                await using var input = file.OpenReadStream();
                var saved = await storage.SaveAsync(input, cancellationToken);
                storedFileName = saved.StoredFileName;
                actualSize = saved.SizeBytes;
            }
            catch (OperationCanceledException)
            {
                return Results.StatusCode(499);
            }

            if (actualSize > maxFileSize)
            {
                storage.Delete(storedFileName);
                return Results.BadRequest("Maximum file size is 10 MB.");
            }

            var document = new Document
            {
                OwnerId = userId,
                FileName = originalName,
                StoredFileName = storedFileName,
                Category = category.Trim(),
                ContentType = "application/octet-stream",
                SizeBytes = actualSize,
                UploadedAt = DateTimeOffset.UtcNow
            };

            try
            {
                dbContext.Documents.Add(document);
                await dbContext.SaveChangesAsync(cancellationToken);

                await audit.LogAsync(
                    userId,
                    "Upload",
                    document.Id,
                    "Document uploaded.",
                    true);

                return Results.Created(
                    $"/api/documents/{document.Id}",
                    new
                    {
                        document.Id,
                        document.FileName,
                        document.Category,
                        document.SizeBytes,
                        document.UploadedAt
                    });
            }
            catch
            {
                storage.Delete(storedFileName);
                throw;
            }
        }).DisableAntiforgery();
        documents.MapGet("/{id:guid}/download", async (
    Guid id,
    ClaimsPrincipal user,
    TrustVaultDbContext dbContext,
    DocumentStorageService storage,
    AuditService audit) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrWhiteSpace(userId))
        return Results.Unauthorized();

    var document = await dbContext.Documents
    .AsNoTracking()
    .FirstOrDefaultAsync(d => d.Id == id);

if (document is null)
{
    await audit.LogAsync(
        userId, "DownloadDenied", id,
        "Document not found.", false);

    return Results.NotFound();
}

var isOwner = document.OwnerId == userId;

var hasReadPermission = await dbContext.DocumentPermissions
    .AsNoTracking()
    .AnyAsync(p =>
        p.DocumentId == id &&
        p.UserId == userId &&
        p.CanRead);

if (!isOwner && !hasReadPermission)
{
    await audit.LogAsync(
        userId, "DownloadDenied", id,
        "User does not have read permission.", false);

    return Results.NotFound();
}
    var path = storage.GetPath(document.StoredFileName);

    if (!File.Exists(path))
        return Results.NotFound("Stored file not found.");

    await audit.LogAsync(
        userId, "Download", id, "Document downloaded.", true);

    return Results.File(
        path,
        "application/octet-stream",
        document.FileName);
});
documents.MapPost("/{id:guid}/permissions", async (
    Guid id,
    string email,
    bool canRead,
    bool canEdit,
    ClaimsPrincipal user,
    TrustVaultDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    AuditService audit) =>
{
    var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrWhiteSpace(ownerId))
        return Results.Unauthorized();

    var document = await dbContext.Documents
        .FirstOrDefaultAsync(d => d.Id == id && d.OwnerId == ownerId);

    if (document is null)
        return Results.NotFound("Document not found or you are not its owner.");

    var targetUser = await userManager.FindByEmailAsync(email);

    if (targetUser is null)
        return Results.BadRequest("Target user was not found.");

    if (targetUser.Id == ownerId)
        return Results.BadRequest("You already own this document.");

    if (canEdit)
        canRead = true;

    var permission = await dbContext.DocumentPermissions
        .FirstOrDefaultAsync(p => p.DocumentId == id && p.UserId == targetUser.Id);

    if (permission is null)
    {
        permission = new DocumentPermission
        {
            DocumentId = id,
            UserId = targetUser.Id,
            GrantedByUserId = ownerId
        };

        dbContext.DocumentPermissions.Add(permission);
    }

    permission.CanRead = canRead;
    permission.CanEdit = canEdit;
    permission.GrantedAt = DateTimeOffset.UtcNow;
    permission.GrantedByUserId = ownerId;

    await dbContext.SaveChangesAsync();

    await audit.LogAsync(
        ownerId,
        "PermissionGranted",
        id,
        $"Permission updated for user {targetUser.Id}.",
        true);

    return Results.Ok(new
    {
        documentId = id,
        email = targetUser.Email,
        permission.CanRead,
        permission.CanEdit
    });
});
documents.MapDelete("/{id:guid}/permissions", async (
    Guid id,
    string email,
    ClaimsPrincipal user,
    TrustVaultDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    AuditService audit) =>
{
    var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrWhiteSpace(ownerId))
        return Results.Unauthorized();

    var document = await dbContext.Documents
        .FirstOrDefaultAsync(d => d.Id == id && d.OwnerId == ownerId);

    if (document is null)
        return Results.NotFound("Document not found or you are not its owner.");

    var targetUser = await userManager.FindByEmailAsync(email);

    if (targetUser is null)
        return Results.NotFound("User not found.");

    var permission = await dbContext.DocumentPermissions
        .FirstOrDefaultAsync(p => p.DocumentId == id && p.UserId == targetUser.Id);

    if (permission is null)
        return Results.NotFound("Permission not found.");

    dbContext.DocumentPermissions.Remove(permission);
    await dbContext.SaveChangesAsync();

    await audit.LogAsync(
        ownerId,
        "PermissionRevoked",
        id,
        $"Permission revoked for user {targetUser.Id}.",
        true);

    return Results.Ok(new
    {
        documentId = id,
        email = targetUser.Email,
        message = "Permission revoked."
    });
});
documents.MapGet("/audit", async (
    ClaimsPrincipal user,
    TrustVaultDbContext dbContext) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrWhiteSpace(userId))
        return Results.Unauthorized();

    var auditEvents = await (
        from audit in dbContext.AuditEvents.AsNoTracking()
        join document in dbContext.Documents.AsNoTracking()
            on audit.DocumentId equals (Guid?)document.Id
        where document.OwnerId == userId
        orderby audit.OccurredAt descending
        select new
        {
            audit.Id,
            audit.UserId,
            audit.Action,
            audit.DocumentId,
            document.FileName,
            audit.Details,
            audit.Succeeded,
            audit.OccurredAt
        })
        .Take(200)
        .ToListAsync();

    return Results.Ok(auditEvents);
});
        return app;
    }
}
