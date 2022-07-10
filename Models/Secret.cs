using System.Collections.Generic;

namespace VaultChanger.Models;

public class Secret
{
    public string Path { get; set; }

    public List<string> Keys { get; set; }
}