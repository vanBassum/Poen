
namespace Poen.Config
{
    public class ApplicationConfig
    {
        public List<WalletSettings> Wallets { get; set; } = new List<WalletSettings>();
        public int ScanInterval { get; set; } = 300;
    }

}


