namespace Poen.Models
{
    public class Portofolio
    { 
        public required string Address { get; set; }
        public List<TokenBalance> Balances { get; set; } = new List<TokenBalance>();
        public override string ToString() => $"{Address}";
    }

}
