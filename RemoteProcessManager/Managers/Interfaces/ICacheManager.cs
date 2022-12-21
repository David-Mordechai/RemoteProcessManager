using RemoteProcessManager.Models;

namespace RemoteProcessManager.Managers.Interfaces;

public interface ICacheManager
{
    void Save(string fileName, RemoteProcessModel content);
    RemoteProcessModel? Get(string fileName);
    void Delete(string fileName);
}