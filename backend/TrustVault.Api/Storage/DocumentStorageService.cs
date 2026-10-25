
namespace TrustVault.Api.Services;

public class DocumentStorageService
{
    private readonly string _storagePath;

    public DocumentStorageService(IWebHostEnvironment environment)
    {
        _storagePath = Path.Combine(
            environment.ContentRootPath,
            "Storage");

        Directory.CreateDirectory(_storagePath);
    }

    public async Task<(string StoredFileName, long SizeBytes)> SaveAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        var storedFileName = $"{Guid.NewGuid():N}.bin";
        var filePath = Path.Combine(_storagePath, storedFileName);

        await using var output = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await fileStream.CopyToAsync(output, cancellationToken);

        return (storedFileName, output.Length);
    }

    public string GetPath(string storedFileName)
    {
        if (string.IsNullOrWhiteSpace(storedFileName) ||
            Path.GetFileName(storedFileName) != storedFileName)
        {
            throw new ArgumentException("Invalid stored file name.");
        }

        return Path.Combine(_storagePath, storedFileName);
    }

    public bool Delete(string storedFileName)
    {
        var filePath = GetPath(storedFileName);

        if (!File.Exists(filePath))
        {
            return false;
        }

        File.Delete(filePath);
        return true;
    }
}
