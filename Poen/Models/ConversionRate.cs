﻿namespace Poen.Models
{
    public class ConversionRate
    {
        public required Token FromToken { get; set; }
        public required Token ToToken { get; set; }
        public decimal? Price { get; set; }
        public DateTime LastUpdated { get; set; }

        public override string ToString() => $"({FromToken} - {ToToken}): {Price}";

    }
}
