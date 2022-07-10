using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VaultChanger.Extensions;
using VaultChanger.Models;
using VaultChanger.Repositories;

namespace VaultChanger.Controllers;

[ApiController]
[Route("[controller]")]
public class VaultController : ControllerBase
{
    private readonly IVaultRepository _vaultRepository;

    public VaultController(IVaultRepository vaultRepository)
    {
        _vaultRepository = vaultRepository;
    }

    [HttpGet("everything/{vaultNamespace}/{mount}/{path}")]
    public async Task<IActionResult> GetEverythingForPath(string vaultNamespace, string mount, string path)
    {
        var result = await _vaultRepository.QueryFolder(vaultNamespace, mount, path);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("folders/{vaultNamespace}/{mount}/{path}")]
    public async Task<IActionResult> GetFoldersForPath(string vaultNamespace, string mount, string path)
    {
        var result = await _vaultRepository.QueryFolder(vaultNamespace, mount, path);
        return result != null
            ? Ok(result.Flatten(
                folder => new List<string> { folder.Path },
                folder => folder.Children))
            : NotFound();
    }

    [HttpGet("secrets/{vaultNamespace}/{mount}/{path}")]
    public async Task<IActionResult> GetSecretsForPath(string vaultNamespace, string mount, string path)
    {
        var result = await _vaultRepository.QueryFolder(vaultNamespace, mount, path);
        return result != null
            ? Ok(result.Flatten(
                folder => folder.Secrets.SelectMany(secret => secret.Keys.Select(key => $"{secret.Path}/{key}")),
                folder => folder.Children))
            : NotFound();
    }

    [HttpPost("secrets/{vaultNamespace}/{mount}/{path}/{key}")]
    public async Task<IActionResult> WriteSecretInPath(string vaultNamespace, string mount, string path, string key,
        [FromBody] SecretValue secretValue)
    {
        await _vaultRepository.WriteSecretInPath(vaultNamespace, mount, path, key, secretValue);
        return Ok();
    }
}