using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StripeAPITest.Data
{
    public class TransactionDetails
    {
        public string User { get; set; }
        public long Amount { get; set; }
        public string Description { get; set; }
        //[JsonIgnore]
        //public string TokenId { get; set; }
    }
}
