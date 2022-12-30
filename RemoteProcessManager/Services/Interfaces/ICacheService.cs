namespace RemoteProcessManager.Services.Interfaces;

public interface ICacheService<T>
{
    void Save(string name, T content);
    T? Get(string name);
    void Delete(string name);
}