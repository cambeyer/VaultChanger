namespace VaultChanger.Models.Configuration
{
    public class VaultConfig
    {
        public static string Vault = "Vault";
        
        public string Url { get; set; }

        public string TokenPath { get; set; }
        
        public string TransitKeyName { get; set; }
        
        public string TransitMountPoint { get; set; }
    }
}