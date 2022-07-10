using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VaultChanger.Models;
using VaultChanger.Repositories;

namespace VaultChanger.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly IRequestsRepository _requestsRepository;
        private const string StandardPath = "{vaultNamespace}/{mount}/{path}/{key}";

        public RequestsController(IRequestsRepository requestsRepository)
        {
            _requestsRepository = requestsRepository;
        }

        [HttpGet(StandardPath, Name = "GetRequest")]
        public async Task<IActionResult> GetRequest(string vaultNamespace, string mount, string path, string key)
        {
            var request = await _requestsRepository.GetRequest(vaultNamespace, mount, path, key);
            return request != null ? Ok(request) : NotFound();
        }

        [HttpPost(StandardPath)]
        public async Task<IActionResult> CreateRequest(string vaultNamespace, string mount, string path, string key, [FromBody] SecretValue secretValue)
        {
            return CreatedAtRoute(nameof(GetRequest), new { vaultNamespace, mount, path, key },
                await _requestsRepository.CreateRequest(vaultNamespace, mount, path, key, RequestType.Upsert, secretValue));
        }
        
        [HttpDelete(StandardPath)]
        public async Task<IActionResult> CreateRequest(string vaultNamespace, string mount, string path, string key)
        {
            return CreatedAtRoute(nameof(GetRequest), new { vaultNamespace, mount, path, key },
                await _requestsRepository.CreateRequest(vaultNamespace, mount, path, key, RequestType.Delete, null));
        }
        
        [HttpPost($"{StandardPath}/reject")]
        public async Task<IActionResult> RejectRequest(string vaultNamespace, string mount, string path, string key)
        {
            return await _requestsRepository.RejectRequest(vaultNamespace, mount, path, key) ? Ok() : NotFound();
        }
        
        [HttpPost($"{StandardPath}/apply")]
        public async Task<IActionResult> ApplyRequest(string vaultNamespace, string mount, string path, string key)
        {
            return await _requestsRepository.ApplyRequest(vaultNamespace, mount, path, key) ? Ok() : NotFound();
        }
    }
}