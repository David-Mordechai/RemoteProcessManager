using System.Text.Json;
using RemoteProcessManager.Services.Interfaces;

namespace RemoteProcessManager.Services;

internal class TempFileService<T> : ICacheService<T>
{
    private readonly ILogger<TempFileService<T>> _logger;
    private T? _cachedObject;

    public TempFileService(ILogger<TempFileService<T>> logger)
    {
        _logger = logger;
    }

    public void Save(string fileName, T content)
    {
        var tempFilePath = Path.GetTempPath();
        var tempFile = $"{tempFilePath}tmp_{fileName}.tmp";
        try
        {
            var jsonContent = JsonSerializer.Serialize(content);
            using var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
            using var streamWriter = new StreamWriter(fileStream);
            streamWriter.WriteLine(jsonContent);
            _cachedObject = content;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to save to temp file, File: {FileName}", tempFile);
        }
    }

    public T? Get(string fileName)
    {
        if (_cachedObject is not null) return _cachedObject;

        var tempFilePath = Path.GetTempPath();
        var tempFile = $"{tempFilePath}tmp_{fileName}.tmp";
        if (File.Exists(tempFile) is false) return default;
        using var fs = File.Open(tempFile, FileMode.Open);
        try
        {
            using var reader = new StreamReader(fs);
            var content = reader.ReadToEnd();
            var obj = JsonSerializer.Deserialize<T>(content)!;
            _cachedObject = obj;
            return obj;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to read from temp file, File: {FileName}", tempFile);
            return default;
        }
    }

    public void Delete(string fileName)
    {
        var tempFilePath = Path.GetTempPath();
        var tempFile = $"{tempFilePath}tmp_{fileName}.tmp";
        if (File.Exists(tempFile) is false) return;
        try
        {
            File.Delete(tempFile);
            _cachedObject = default;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to delete temp file, File: {FileName}", tempFile);
        }
    }
}