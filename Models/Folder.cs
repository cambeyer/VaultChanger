using System.Collections.Generic;

namespace VaultChanger.Models;

public class Folder
{
    public string Path { get; init; }

    public List<Folder> Children { get; init; }

    public List<Secret> Secrets { get; init; }
}