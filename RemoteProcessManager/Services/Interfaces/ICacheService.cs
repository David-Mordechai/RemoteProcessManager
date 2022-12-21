namespace RemoteProcessManager.Services.Interfaces;

public interface ICacheService<T>
{
    void Save(string fileName, T content);
    T? Get(string fileName);
    void Delete(string fileName);
}