using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VaultChanger.Models;

public enum RequestType
{
    Upsert,
    Delete
}

public class Request
{
    public string VaultNamespace { get; init; }

    public string MountPoint { get; init; }

    public string Path { get; init; }

    public string Key { get; init; }

    public string Value { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public RequestType Type { get; init; }
}