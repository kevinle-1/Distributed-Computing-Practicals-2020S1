// Filename: JobServerInterface.cs
// Project:  DC Assignment (COMP3008)
// Purpose:  Interface for blockchain server - functions it must implement
// Author:   Kevin Le (19472960)
//
// Date:     25/05/2020

using System.Collections.Generic;
using System.ServiceModel;

namespace dc_p8_wallet.Blockchain
{
    [ServiceContract]
    public interface BlockchainServerInterface
    {
        [OperationContract]
        List<Block> GetCurrentBlockchain();
        [OperationContract]
        Block GetCurrentBlock(); //End block
        [OperationContract]
        void RecieveNewTransaction(uint walletIDfrom, uint walletIDto, float amount); //What details does transaction have? 
        [OperationContract]
        int GetNumBlocks();
    }
}