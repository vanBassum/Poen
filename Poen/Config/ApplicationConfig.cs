﻿
namespace Poen.Config
{
    public class ApplicationConfig
    {
        public string? Wallet { get; set; }
        public List<string> TokenBlacklist { get; set; } = new List<string>();
        public int ScanInterval { get; set; } = 300;
    }
}


