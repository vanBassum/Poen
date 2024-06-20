namespace Tracker.Services.CoinMarketCap
{
    public class CoinData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Symbol { get; set; }
        public string? Slug { get; set; }
        public int? IsActive { get; set; }
        public object? IsFiat { get; set; }
        public long CirculatingSupply { get; set; }
        public long TotalSupply { get; set; }
        public long MaxSupply { get; set; }
        public DateTime DateAdded { get; set; }
        public int NumMarketPairs { get; set; }
        public int CmcRank { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<string>? Tags { get; set; }
        public object? Platform { get; set; }
        public object? SelfReportedCirculatingSupply { get; set; }
        public object? SelfReportedMarketCap { get; set; }
        public Dictionary<string, Quote>? Quote { get; set; }

        public override string ToString() => $"{Symbol}";
    }
}
