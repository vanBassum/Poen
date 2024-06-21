
namespace Poen.Config
{
    public class WalletSettings
    {
        public string? Address { get; set; }
        public List<TokenSettings> Tokens { get; set; } = new List<TokenSettings>();
    }

}


