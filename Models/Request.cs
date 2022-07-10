using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VaultChanger.Models
{
    public enum RequestType
    {
        Upsert,
        Delete
    }

    public class Request
    {
        public string VaultNamespace { get; set; }
        
        public string MountPoint { get; set; }
        
        public string Path { get; set; }
        
        public string Key { get; set; }
        
        public string Value { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public RequestType Type { get; set; }
    }
}