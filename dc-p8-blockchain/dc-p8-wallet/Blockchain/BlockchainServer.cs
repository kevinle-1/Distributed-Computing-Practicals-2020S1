using dc_p8_wallet.Transaction;
using System.Collections.Generic;

namespace dc_p8_wallet.Blockchain
{
    class BlockchainServer : BlockchainServerInterface
    {
        public List<Block> GetCurrentBlockchain()
        {
            return Blockchain.GetChain(); 
        }

        public Block GetCurrentBlock()
        {
            return Blockchain.GetEndBlock(); 
        }

        public void RecieveNewTransaction(uint walletIDfrom, uint walletIDto, float amount)
        {
            Transactions.AddTransaction(walletIDfrom, walletIDto, amount);
        }

        public int GetNumBlocks()
        {
            return Blockchain.GetNumBlocks(); 
        }
    }
}
