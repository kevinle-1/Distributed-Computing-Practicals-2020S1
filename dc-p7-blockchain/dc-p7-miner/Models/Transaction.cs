using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dc_p7_miner.Models
{
    public class Transaction
    {
        public uint walletIDfrom;
        public uint walletIDto;
        public float amount;

        public Boolean processed;

        public override string ToString()
        {
            return walletIDfrom.ToString() + " -> " + walletIDto.ToString() + " for: " + amount.ToString();
        }
    }
}