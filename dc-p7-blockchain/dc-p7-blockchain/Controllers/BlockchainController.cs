// Filename: BlockchainController.cs
// Project:  DC Assignment (COMP3008)
// Purpose:  Controller for Blockchain API 
// Author:   Kevin Le (19472960)
//
// Date:     24/05/2020

using dc_p7_blockchain.Models;
using System.Collections.Generic;
using System.Web.Http;

namespace dc_p7_blockchain.Controllers
{
    public class BlockchainController : ApiController
    {
 
        /// <summary>
        /// Get current state of blockchain, returns the number of blocks in the blockchain 
        /// </summary>
        /// <returns>Integer Number</returns>
        [Route("api/Blockchain/Current")]
        [HttpGet]
        public int GetCurrentState()
        {
            return Blockchain.GetNumBlocks(); 
        }   
        
        /// <summary>
        /// Returns the list of blocks in the blockchain 
        /// </summary>
        /// <returns>List of blocks</returns>
        [Route("api/Blockchain/Chain")]
        [HttpGet]
        public List<Block> GetChain()
        {
            return Blockchain.GetChain(); 
        }
                
        /// <summary>
        /// Get the balance of a given wallet ID 
        /// </summary>
        /// <param name="walletIDfrom">Number of Wallet ID</param>
        /// <returns>Float number of calculated balance</returns>
        [Route("api/Blockchain/Balance/{walletIDfrom}")]
        [HttpGet]
        public float GetBalance(uint walletIDfrom)
        {
            return Blockchain.GetBalance(walletIDfrom); 
        }      
        
        /// <summary>
        /// Returns the block at the end of the blockchain 
        /// </summary>
        /// <returns></returns>
        [Route("api/Blockchain/Endblock")]
        [HttpGet]
        public Block GetLastBlock()
        {
            return Blockchain.GetEndBlock(); 
        }    
        
        /// <summary>
        /// Adds a block onto the end of the blockchain, given the Block object. 
        /// </summary>
        /// <param name="b">Block object</param>
        [Route("api/Blockchain/Addblock")]
        [HttpPost]
        public void AddBlock(Block b)
        {
            Blockchain.AddBlock(b); 
        }
    }
}