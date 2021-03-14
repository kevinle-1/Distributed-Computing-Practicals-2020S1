// Filename: MineController.cs
// Project:  DC Assignment (COMP3008)
// Purpose:  API controller for the Miner - Handles new submitted transactions 
// Author:   Kevin Le (19472960)
//
// Date:     26/05/2020

using dc_p7_miner.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Http;

namespace dc_p7_miner.Controllers
{
    public delegate void ProcessTransactionDelegate(); 

    public class MineController : ApiController
    {
        private string BlockchainURL = "https://localhost:44341"; //URL of the blockchain web service 
        private static Queue<Transaction> transactions = new Queue<Transaction>();

        private static Boolean firstStart = true; 

        /// <summary>
        /// Create a new transaction and adds it to the list for ProcessTransactions to process.
        /// If process transactions hasn't been started yet (First time mine controller is called). Then start it. 
        /// </summary>
        /// <param name="walletIDfrom">Unsigned integer ID of wallet for amount to be transfered from</param>
        /// <param name="walletIDto">Unsigned integer ID of wallet for amount to be transfered to</param>
        /// <param name="amount">Float amount to be transferred</param>
        [Route("api/Miner/Transaction/{walletIDfrom}/{walletIDto}/{amount}")]
        [HttpPost]
        public void Transaction(uint walletIDfrom, uint walletIDto, float amount)
        {
            if(firstStart)
            {
                ProcessTransactionDelegate processDelegate = ProcessTransactions;
                processDelegate.BeginInvoke(null, null);

                firstStart = false; 
            }

            Transaction t = new Transaction();

            t.walletIDfrom = walletIDfrom;
            t.walletIDto = walletIDto; 
            t.amount = amount;

            t.processed = false; 

            transactions.Enqueue(t);
        }

        /// <summary>
        /// Keep checking the transactions list for any transactions, and process it by building a block 
        /// and calling GenerateHash() to calculate hash. 
        /// </summary>
        private void ProcessTransactions()
        {
            RestClient client = new RestClient(BlockchainURL);

            while (true)
            {
                try
                {
                    if(transactions.Count > 0)
                    {
                        Transaction t = transactions.Dequeue();

                        if (!t.processed)
                        {
                            System.Diagnostics.Debug.WriteLine("[Miner] Processing transaction for: " + t.ToString());

                            uint walletIDfrom = t.walletIDfrom;
                            uint walletIDto = t.walletIDto;
                            float amount = t.amount;

                            if (amount > 0 && walletIDto >= 0 && walletIDfrom >= 0) //If all values are non negative 
                            {
                                System.Diagnostics.Debug.WriteLine("[Miner] Values non negative");

                                string walletFromBalResponse = client.Get(new RestRequest("api/Blockchain/Balance/" + walletIDfrom)).Content; //Get the current balance of the wallet the money is to be sent from
                                float walletFromBal = float.Parse(walletFromBalResponse);

                                System.Diagnostics.Debug.WriteLine("[Miner] From Balance: " + walletFromBal);

                                if (walletFromBal >= amount) //If the wallet from has sufficient funds to make the transfer 
                                {
                                    System.Diagnostics.Debug.WriteLine("[Miner] Sufficient Funds");

                                    Block appendBlock = new Block(); //Create a new block 

                                    string endblockResponse = client.Get(new RestRequest("api/Blockchain/Endblock")).Content; //Get the end block (for id, hash, and offset) 
                                    Block endBlock = JsonConvert.DeserializeObject<Block>(endblockResponse);

                                    appendBlock.blockID = endBlock.blockID + 1; //Increment end block ID by 1 
                                    appendBlock.walletIDfrom = walletIDfrom; //Set the ID of the wallet to be sent from 
                                    appendBlock.walletIDto = walletIDto; //Set the ID of the wallet to be sent to 
                                    appendBlock.amount = amount; //Set amount of transfer 
                                    appendBlock.offset = 0;
                                    appendBlock.prevHash = endBlock.hash; //Set previous hash to the hash of the current end block in the chain 

                                    appendBlock.hash = "";

                                    appendBlock = GenerateHash(appendBlock); //Generate the hash based on the concatenated data so far. Set to the block hash 

                                    RestRequest request = new RestRequest("api/Blockchain/Addblock");
                                    request.RequestFormat = DataFormat.Json;
                                    request.AddJsonBody(appendBlock); //Send Block object as JSON 

                                    client.Post(request); //POST request to add block 

                                    t.processed = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[Miner] Queue Empty");
                    }
                }
                catch(InvalidOperationException e)
                {
                    System.Diagnostics.Debug.WriteLine("[Miner] Error: " + e.ToString());
                    System.Diagnostics.Debug.WriteLine("[Miner] Queue Empty");
                }

                Thread.Sleep(10000); 
            }
        }

        /// <summary>
        /// Generates a new hash by brute force. If no hash begins with 12345, increment offset by 1 and check again. 
        /// </summary>
        /// <param name="b"></param>
        /// <returns>Block with hash</returns>
        public static Block GenerateHash(Block b)
        {
            Boolean hashGenerated = false;
            SHA256 sha256 = SHA256.Create();

            Block outB = b;

            while (!hashGenerated)
            {
                try
                {
                    //If hash doesn't start with 12345 or exists keep attempting to generate 
                    while (!outB.hash.StartsWith("12345") || outB.hash == "")
                    {
                        outB.offset = outB.offset + 1;

                        //Convert SHA256 to ulong, then to string  
                        string hash = (BitConverter.ToUInt64(sha256.ComputeHash(Encoding.UTF8.GetBytes(outB.ToHashString())), 0)).ToString();

                        outB.hash = hash;
                    }

                    hashGenerated = true;
                }
                catch (ArgumentOutOfRangeException)
                {
                    System.Diagnostics.Debug.WriteLine("Out of string bounds");
                }
            }

            return outB;
        }
    }
}