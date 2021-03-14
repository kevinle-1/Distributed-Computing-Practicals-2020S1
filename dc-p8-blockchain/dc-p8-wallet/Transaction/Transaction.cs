// Filename: Transaction.cs
// Project:  DC Assignment (COMP3008)
// Purpose:  Object representation of a Transaction 
// Author:   Kevin Le (19472960)
//
// Date:     25/05/2020

using System;

namespace dc_p8_wallet.Transaction
{
    class Transaction
    {
        public uint walletIDfrom; 
        public uint walletIDto; 
        public float amount;

        public Boolean processed;

        /// <summary>
        /// Returns string representation of transaction 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return walletIDfrom.ToString() + " -> " + walletIDto.ToString() + ": $" + amount.ToString(); 
        }
    }
}
