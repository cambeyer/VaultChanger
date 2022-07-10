using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VaultChanger.Extensions;
using VaultChanger.Models;
using VaultChanger.Models.Configuration;
using VaultSharp;
using VaultSharp.Core;
using VaultSharp.V1.SecretsEngines.Transit;

namespace VaultChanger.Repositories
{
    public class VaultRepository : IVaultRepository
    {
        private readonly IVaultClient _vaultClient;
        private readonly VaultConfig _vaultConfig;
        
        public VaultRepository(IVaultClient vaultClient, IOptions<VaultConfig> vaultConfig)
        {
            _vaultClient = vaultClient;
            _vaultConfig = vaultConfig.Value;
        }

        public async Task<Folder> QueryFolder(string vaultNamespace, string mount, string path = "/")
        {
            path = WebUtility.UrlDecode(path);
            try
            {
                var paths = (await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretPathsAsync(
                        path: VaultExtensions.GeneratePath(vaultNamespace, path), mountPoint: mount)).Data
                    .Keys
                    .ToList();
                var secrets = paths.Where(subPath => !subPath.EndsWith("/")).ToList();
                var children = paths.Except(secrets);
                return new Folder
                {
                    Children = (await Task.WhenAll(children
                        .Select(async folder => await QueryFolder(vaultNamespace, mount, $"{path}{folder}")))).ToList(),
                    Path = path,
                    Secrets = (await Task.WhenAll(secrets.Select(async secret =>
                    {
                        var secretPath = $"{path}{secret}";
                        return new Secret
                        {
                            Path = secretPath,
                            Keys = (await GetSecretForPath(vaultNamespace, mount, secretPath)).Keys.ToList()
                        };
                    }))).ToList()
                };
            }
            catch (VaultApiException e)
            {
                if (e.HttpStatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }
            }

            return null;
        }

        private async Task<IDictionary<string, object>> GetSecretForPath(string vaultNamespace, string mount, string path)
        {
            return (await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                path: VaultExtensions.GeneratePath(vaultNamespace, path), mountPoint: mount)).Data.Data;
        }

        public async Task<string> EncryptSecret(SecretValue value)
        {
            return (await _vaultClient.V1.Secrets.Transit.EncryptAsync(keyName: _vaultConfig.TransitKeyName, 
                new EncryptRequestOptions
                {
                    Base64EncodedPlainText = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)))
                })).Data.CipherText;
        }

        public async Task<SecretValue> DecryptSecret(string cipherText)
        {
            return JsonConvert.DeserializeObject<SecretValue>(Encoding.UTF8.GetString(Convert.FromBase64String(
                (await _vaultClient.V1.Secrets.Transit.DecryptAsync(keyName: _vaultConfig.TransitKeyName,
                    new DecryptRequestOptions
                    {
                        CipherText = cipherText
                    })).Data.Base64EncodedPlainText)));
        }

        public async Task WriteSecretInPath(string vaultNamespace, string mount, string path, string key, SecretValue secretValue)
        {
            path = VaultExtensions.GeneratePath(vaultNamespace, path);
            IDictionary<string, object> currentSecret = new Dictionary<string, object>();
            try
            {
                currentSecret = (await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                    path: path, mountPoint: mount)).Data.Data;
            }
            catch (VaultApiException exception)
            {
                if (exception.HttpStatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }
            }
            
            if (!currentSecret.TryGetValue(key, out var currentValue) || !currentValue.Equals(secretValue.Value)) {
                currentSecret[key] = secretValue.Value;

                await _vaultClient.V1.Secrets.KeyValue.V2.WriteSecretAsync(path: path, mountPoint: mount,
                    data: currentSecret);
            }
        }
        
        public async Task DeleteSecretInPath(string vaultNamespace, string mount, string path, string key)
        {
            path = VaultExtensions.GeneratePath(vaultNamespace, path);
            
            var currentSecret = await GetSecretForPath(vaultNamespace, mount, path);
            if (currentSecret.Remove(key))
            {
                if (currentSecret.Keys.Count == 0)
                {
                    await _vaultClient.V1.Secrets.KeyValue.V2.DeleteMetadataAsync(path: path, mountPoint: mount);
                }
                else
                {
                    await _vaultClient.V1.Secrets.KeyValue.V2.WriteSecretAsync(path: path, mountPoint: mount,
                        data: currentSecret);
                }
            }
        }
    }
}