using System.Collections.Generic;

namespace VaultChanger.Models;

public class Secret
{
    public string Path { get; init; }

    public List<string> Keys { get; init; }
}