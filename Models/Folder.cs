using System.Collections.Generic;

namespace VaultChanger.Models
{
    public class Folder
    {
        public string Path { get; set; }

        public List<Folder> Children { get; set; }
        
        public List<Secret> Secrets { get; set; }
    }
}