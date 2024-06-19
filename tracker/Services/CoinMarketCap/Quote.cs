namespace Tracker.Services.CoinMarketCap
{
    public class Quote
    {
        public decimal? Price { get; set; }
        public decimal Volume24h { get; set; }
        public decimal VolumeChange24h { get; set; }
        public decimal PercentChange1h { get; set; }
        public decimal PercentChange24h { get; set; }
        public decimal PercentChange7d { get; set; }
        public decimal PercentChange30d { get; set; }
        public decimal MarketCap { get; set; }
        public int MarketCapDominance { get; set; }
        public decimal FullyDilutedMarketCap { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
