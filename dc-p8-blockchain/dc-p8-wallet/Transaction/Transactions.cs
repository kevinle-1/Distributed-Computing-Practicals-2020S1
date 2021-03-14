// Filename: Transactions.cs
// Project:  DC Assignment (COMP3008)
// Purpose:  Manage transactions 
// Author:   Kevin Le (19472960)
//
// Date:     25/05/2020

using System.Collections.Generic;

namespace dc_p8_wallet.Transaction
{
    class Transactions
    {
        private static Queue<Transaction> transactions = new Queue<Transaction>();

        /// <summary>
        /// Add a new transaction to the transaction list 
        /// </summary>
        /// <param name="walletIDfrom"></param>
        /// <param name="walletIDto"></param>
        /// <param name="amount"></param>
        public static void AddTransaction(uint walletIDfrom, uint walletIDto, float amount)
        {
            Transaction t = new Transaction();
            t.walletIDfrom = walletIDfrom;
            t.walletIDto = walletIDto;
            t.amount = amount;

            t.processed = false;

            transactions.Enqueue(t); 
        }

        /// <summary>
        /// Return list of transactions - Used by miner thread 
        /// </summary>
        /// <returns></returns>
        public static Queue<Transaction> GetTransactions()
        {
            return transactions; 
        }

        public static void MarkTransactionProcessed(Transaction t)
        {
            foreach(Transaction tr in transactions)
            {
                if(tr == t)
                {
                    tr.processed = true; 
                }
            }
        }
    }
}
