using System.Collections.Generic;
using System.Threading.Tasks;
using VaultChanger.Models;

namespace VaultChanger.Repositories
{
    public interface IVaultRepository
    {
        Task<Folder> QueryFolder(string vaultNamespace, string mount, string path = "/");
        
        Task WriteSecretInPath(string vaultNamespace, string mount, string path, string key, SecretValue secretValue);
        
        Task DeleteSecretInPath(string vaultNamespace, string mount, string path, string key);

        Task<string> EncryptSecret(SecretValue value);

        Task<SecretValue> DecryptSecret(string cipherText);
    }
}