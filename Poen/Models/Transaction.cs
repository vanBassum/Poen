using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Poen.Models
{
    public class Transaction
    {
        public DateTime TimeStamp { get; set; }
        public required string From { get; set; }
        public required string To { get; set; }
        public required Token Token { get; set; }
        public decimal Value { get; set; }
        public override string ToString() => $"{Token}";
    }
}
