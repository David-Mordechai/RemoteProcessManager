namespace RemoteProcessManager.Managers.Interfaces;

public interface ICacheManager
{
    void Save<T>(string fileName, T content);
    T? Get<T>(string fileName);
    void Delete(string fileName);
}