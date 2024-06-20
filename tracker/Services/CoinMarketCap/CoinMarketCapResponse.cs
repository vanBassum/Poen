namespace Tracker.Services.CoinMarketCap
{
    public class CoinMarketCapResponse
    {
        public Status? Status { get; set; }
        public Dictionary<string, CoinData>? Data { get; set; }

        public override string ToString() => $"{Status}";
    }
}
