namespace Poen.Models
{
    public class TokenBalance
    { 
        public required Token Token { get; set; }
        public required decimal Balance { get; set; }
        public decimal AdjustedBalance => Balance / (decimal)Math.Pow(10, Token.DecimalPlace);
        public override string ToString() => $"{Token.Symbol} {AdjustedBalance}";

    }

}
