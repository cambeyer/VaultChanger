using System.Threading.Tasks;
using VaultChanger.Models;

namespace VaultChanger.Repositories;

public interface IRequestsRepository
{
    Task<Request> GetRequest(string vaultNamespace, string mount, string path, string key);

    Task<Request> CreateRequest(string vaultNamespace, string mount, string path, string key, RequestType type,
        SecretValue secretValue);

    Task<bool> RejectRequest(string vaultNamespace, string mount, string path, string key);

    Task<bool> ApplyRequest(string vaultNamespace, string mount, string path, string key);
}