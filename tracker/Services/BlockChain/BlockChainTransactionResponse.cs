namespace Tracker.Services.BinanceSmartChain
{
    public class BlockChainTransactionResponse
    {
        public string? status { get; set; }
        public string? message { get; set; }
        public List<BlockChainTransaction> result { get; set; } = new List<BlockChainTransaction>();

        public override string ToString()
        {
            return $"{status} {message}";
        }
    }
}