using System.Text.Json;
using RemoteProcessManager.Managers.Interfaces;
using RemoteProcessManager.Models;

namespace RemoteProcessManager.Managers;

internal class TempFileManager : ICacheManager
{
    private readonly ILogger<TempFileManager> _logger;
    public RemoteProcessModel? CachedRemoteProcessModel { get; private set; }

    public TempFileManager(ILogger<TempFileManager> logger)
    {
        _logger = logger;
    }
    
    public void Save(string fileName, RemoteProcessModel content)
    {
        var tempFilePath = Path.GetTempPath();
        var tempFile = $"{tempFilePath}tmp_{fileName}.tmp";
        try
        {
            var jsonContent = JsonSerializer.Serialize(content);
            using var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
            using var streamWriter = new StreamWriter(fileStream);
            streamWriter.WriteLine(jsonContent);
            CachedRemoteProcessModel = content;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to save to temp file, File: {FileName}", tempFile);
        }
    }

    public RemoteProcessModel? Get(string fileName)
    {
        if(CachedRemoteProcessModel is not null) return CachedRemoteProcessModel;

        var tempFilePath = Path.GetTempPath();
        var tempFile = $"{tempFilePath}tmp_{fileName}.tmp";
        if (File.Exists(tempFile) is false) return default;
        using var fs = File.Open(tempFile, FileMode.Open);
        try
        {
            using var reader = new StreamReader(fs);
            var content = reader.ReadToEnd();
            var processModel = JsonSerializer.Deserialize<RemoteProcessModel>(content)!;
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
            CachedRemoteProcessModel = null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to delete temp file, File: {FileName}", tempFile);
        }
    }
}