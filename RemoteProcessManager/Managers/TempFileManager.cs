using System.Text.Json;
using RemoteProcessManager.Managers.Interfaces;

namespace RemoteProcessManager.Managers;

internal class TempFileManager : ICacheManager
{
    private readonly ILogger<TempFileManager> _logger;

    public TempFileManager(ILogger<TempFileManager> logger)
    {
        _logger = logger;
    }
    
    public void Save<T>(string fileName, T content)
    {
        var tempFilePath = Path.GetTempPath();
        var tempFile = $"{tempFilePath}tmp_{fileName}.tmp";
        try
        {
            var jsonContent = JsonSerializer.Serialize(content);
            using var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
            using var streamWriter = new StreamWriter(fileStream);
            streamWriter.WriteLine(jsonContent);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to save to temp file, File: {FileName}", tempFile);
        }
    }

    public T? Get<T>(string fileName)
    {
        var tempFilePath = Path.GetTempPath();
        var tempFile = $"{tempFilePath}tmp_{fileName}.tmp";
        if (File.Exists(tempFile) is false) return default;
        using var fs = File.Open(tempFile, FileMode.Open);
        try
        {
            using var reader = new StreamReader(fs);
            var content = reader.ReadToEnd();
            var processModel = JsonSerializer.Deserialize<T>(content)!;
            return processModel;
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
            File.Delete(tempFilePath);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to delete temp file, File: {FileName}", tempFile);
        }
    }
}