namespace Tracker.Models
{
    public class Token
    {
        public required string Name { get; set; }
        public required string Symbol { get; set; }
        public required string Contract { get; set; }

        public override string ToString() => $"{Symbol}";

        public static bool operator ==(Token? left, Token? right) => left?.Equals(right) ?? false;
        public static bool operator !=(Token? left, Token? right) => !(left == right);

        public override bool Equals(object? obj)
        {
            if (obj is Token token)
            {
                return string.Equals(Symbol, token.Symbol, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Symbol?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;
        }
    }
}
