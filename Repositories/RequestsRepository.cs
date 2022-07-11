using System.Data;
using System.Threading.Tasks;
using VaultChanger.Data;
using VaultChanger.Models;

namespace VaultChanger.Repositories;

public class RequestsRepository : IRequestsRepository
{
    private readonly VaultContext _vaultContext;
    private readonly IVaultRepository _vaultRepository;

    public RequestsRepository(IVaultRepository vaultRepository, VaultContext vaultContext)
    {
        _vaultRepository = vaultRepository;
        _vaultContext = vaultContext;
    }

    public async Task<Request> GetRequest(string vaultNamespace, string mount, string path, string key)
    {
        return await _vaultContext.Requests.FindAsync(vaultNamespace, mount, path, key);
    }

    public async Task<Request> CreateRequest(string vaultNamespace, string mount, string path, string key,
        RequestType type, SecretValue secretValue)
    {
        var request = new Request
        {
            VaultNamespace = vaultNamespace,
            MountPoint = mount,
            Path = path,
            Key = key,
            Value = type == RequestType.Upsert ? await _vaultRepository.EncryptSecret(secretValue) : string.Empty,
            Type = type
        };

        _vaultContext.Requests.Add(request);

        await _vaultContext.SaveChangesAsync();

        return request;
    }

    public async Task<bool> RejectRequest(string vaultNamespace, string mount, string path, string key)
    {
        var request = await GetRequest(vaultNamespace, mount, path, key);
        if (request == null) return false;

        _vaultContext.Requests.Remove(request);
        await _vaultContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ApplyRequest(string vaultNamespace, string mount, string path, string key)
    {
        var request = await GetRequest(vaultNamespace, mount, path, key);
        if (request == null) return false;

        switch (request.Type)
        {
            case RequestType.Upsert:
                await _vaultRepository.WriteSecretInPath(vaultNamespace, mount, path, key,
                    await _vaultRepository.DecryptSecret(request.Value));
                break;
            case RequestType.Delete:
                await _vaultRepository.DeleteSecretInPath(vaultNamespace, mount, path, key);
                break;
            default:
                throw new ConstraintException("enum value provided is not supported");
        }

        _vaultContext.Requests.Remove(request);
        await _vaultContext.SaveChangesAsync();

        return true;
    }
}