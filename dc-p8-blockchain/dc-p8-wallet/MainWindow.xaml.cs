using dc_p8_wallet.Blockchain;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Windows;

namespace dc_p8_wallet
{
    public delegate void StartMinerDelegate();
    public delegate void StartBlockchainServerDelegate();

    public delegate void SetAddressDelegate(string address); //Shows IP:PORT the client is using 
    public delegate void SetAddOutputEntry(string output); //Allows another thread to add text to the output TextBox

    public delegate void UpdateBlockCount(); //Allows another thread to make call to update block count 

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ClientListURL = "https://localhost:44322/"; 

        private Boolean IsClosed = false; //If the client is closed 
        private Boolean BlockchainServerStarted = false; //If the blockchain server has been started 

        private string BlockchainServerIP = "127.0.0.1"; //Default to 127.0.0.1 as it is only running on local machine right now 
        private uint BlockchainServerPort = 8100; //Start at port 8100, and check from here (incremented to check available ports) 

        public MainWindow()
        {
            StartBlockchainServerDelegate blockchainServer = BlockchainServer;
            StartMinerDelegate miner = Miner;

            blockchainServer.BeginInvoke(null, null);
            miner.BeginInvoke(null, null); 

            InitializeComponent();

            AddOutputEntry("Started");
        }

        public void Miner()
        {
            NetTcpBinding tcp = new NetTcpBinding();

            //Process Transactions 

            while (!IsClosed)
            {
                try
                {
                    //Process any transactions 

                    Queue<Transaction.Transaction> transactions = Transaction.Transactions.GetTransactions();

                    Dispatcher.Invoke(new SetAddOutputEntry(AddOutputEntry), new Object[] { "Processing Transactions..." });

                    if (transactions.Count > 0)
                    {
                        Transaction.Transaction t = transactions.Dequeue();

                        Boolean validTransaction = false;

                        uint walletIDfrom = t.walletIDfrom;
                        uint walletIDto = t.walletIDto;

                        float amount = t.amount;

                        if (!t.processed)
                        {
                            if (amount > 0 && walletIDto >= 0 && walletIDfrom >= 0) //If all values are non negative 
                            {
                                float walletFromBal = Blockchain.Blockchain.GetBalance(walletIDfrom);

                                if (walletFromBal != -1.0 && walletFromBal >= amount) //If the wallet from exists and has sufficient funds to make the transfer 
                                {
                                    Block appendBlock = new Block(); //Create a new block 

                                    Block endBlock = Blockchain.Blockchain.GetEndBlock();

                                    appendBlock.blockID = endBlock.blockID + 1; //Increment end block ID by 1 
                                    appendBlock.walletIDfrom = walletIDfrom; //Set the ID of the wallet to be sent from 
                                    appendBlock.walletIDto = walletIDto; //Set the ID of the wallet to be sent to 
                                    appendBlock.amount = amount; //Set amount of transfer 
                                    appendBlock.offset = 0;
                                    appendBlock.prevHash = endBlock.hash; //Set previous hash to the hash of the current end block in the chain 

                                    appendBlock.hash = "";

                                    Dispatcher.Invoke(new SetAddOutputEntry(AddOutputEntry), new Object[] { "Generating Hash..." });

                                    appendBlock = GenerateHash(appendBlock); //Generate the hash based on the concatenated data so far. Set to the block hash

                                    Blockchain.Blockchain.AddBlock(appendBlock);
                                    Transaction.Transactions.MarkTransactionProcessed(t);

                                    Dispatcher.Invoke(new SetAddOutputEntry(AddOutputEntry), new Object[] { "Transaction: " + t.ToString() + " Processed" });
                                    Logger.Log("Transaction: " + t.ToString() + " Processed");

                                    validTransaction = true;
                                }
                            }

                            if (!validTransaction)
                            {
                                Logger.Error("Invalid Transaction. " + t.ToString() + ". Could not Mine");
                            }
                        }
                        else
                        {
                            Logger.Error("Transaction " + t.ToString() + " Already Processed");
                        }
                    }
                    else
                    {
                        Logger.Error("Queue empty");
                    }

                    //Check if have popular hash 

                    Logger.Log("Getting clients");
                    Dispatcher.Invoke(new SetAddOutputEntry(AddOutputEntry), new Object[] { "Getting Clients" });

                    RestClient client = new RestClient(ClientListURL);
                    RestRequest request = new RestRequest("api/Client/Clients");
                    IRestResponse list = client.Get(request); //GET request for list of clients 

                    List<Client> clients = JsonConvert.DeserializeObject<List<Client>>(list.Content); //Deserialize JSON to List<Client>
                    Dictionary<string, int> endblockDictionary = new Dictionary<string, int>();

                    if (clients != null)
                    {
                        foreach (Client c in clients)
                        {
                            Logger.Log("Connecting to client " + c.ToString());
                            Dispatcher.Invoke(new SetAddOutputEntry(AddOutputEntry), new Object[] { "Connecting to client " + c.ToString() });

                            string clientURL = "net.tcp://" + c.ToString() + "/BlockchainServer"; //Build client URL 

                            ChannelFactory<BlockchainServerInterface> blockchainFactory = new ChannelFactory<BlockchainServerInterface>(tcp, clientURL);
                            BlockchainServerInterface clientBlockchainServer = blockchainFactory.CreateChannel(); //Create channel 

                            String hash = clientBlockchainServer.GetCurrentBlock().hash;
                            Logger.Log("Client current end block hash " + hash);

                            if (endblockDictionary.ContainsKey(hash))
                            {
                                endblockDictionary[hash] += 1;
                            }
                            else
                            {
                                endblockDictionary.Add(hash, 1);
                            }
                        }

                        int blockCount = 0;
                        string popularHash = "";

                        foreach (KeyValuePair<string, int> entry in endblockDictionary) //Count to check most popular block
                        {
                            Logger.Log("End block hash frequencies: " + entry.Key + " > " + entry.Value);

                            if (entry.Value > blockCount)
                            {
                                blockCount = entry.Value;
                                popularHash = entry.Key;
                            }
                        }

                        Dispatcher.Invoke(new SetAddOutputEntry(AddOutputEntry), new Object[] { "Popular Hash: " + popularHash });

                        if (!(Blockchain.Blockchain.GetEndBlock().hash == popularHash)) //If Current Blockchain doesn't have the most popular end block
                        {
                            Dispatcher.Invoke(new SetAddOutputEntry(AddOutputEntry), new Object[] { "Current Blockchain doesn't have popular hash." });
                            Dispatcher.Invoke(new SetAddOutputEntry(AddOutputEntry), new Object[] { "Replacing Blockchain..." });

                            //Replace the chain with chain that has most popular block 

                            foreach (Client c in clients)
                            {
                                string clientURL = "net.tcp://" + c.ToString() + "/BlockchainServer"; //Build client URL 

                                ChannelFactory<BlockchainServerInterface> blockchainFactory = new ChannelFactory<BlockchainServerInterface>(tcp, clientURL);
                                BlockchainServerInterface clientBlockchainServer = blockchainFactory.CreateChannel(); //Create channel 

                                if (clientBlockchainServer.GetCurrentBlock().hash == popularHash) 
                                {
                                    Blockchain.Blockchain.UpdateChain(clientBlockchainServer.GetCurrentBlockchain());
                                    break;
                                }
                            }
                        }
                    }
                }
                catch(EndpointNotFoundException)
                {
                    Logger.Error("Error: Connection could not be made to client.");
                    Dispatcher.Invoke(new SetAddOutputEntry(AddOutputEntry), new Object[] { "Error: Connection could not be made to client." });
                }
                catch(Exception)
                {
                    Logger.Error("Error: Could not process transactions.");
                    Dispatcher.Invoke(new SetAddOutputEntry(AddOutputEntry), new Object[] { "Error: Could not process transactions." });
                }


                Dispatcher.Invoke(new UpdateBlockCount(UpdateBlocks));
                Thread.Sleep(5000); //Dont want things going supersonic yet 
            }
        }

        /// <summary>
        /// Blockchain server thread. Starts the blockchain server for other clients to connect to 
        /// </summary>
        public void BlockchainServer()
        {
            Logger.Log("Blockchain Server Starting...");
            Dispatcher.Invoke(new SetAddOutputEntry(AddOutputEntry), new Object[] { "Blockchain Server Starting..." });

            ServiceHost host = new ServiceHost(typeof(BlockchainServer));

            while(!BlockchainServerStarted) //Keep trying to start it till it has started. 
            {
                string port = BlockchainServerPort.ToString(); //Get the port to attempt to start the server from 

                try
                {
                    NetTcpBinding tcp = new NetTcpBinding();

                    Logger.Log("Using Address: net.tcp://" + BlockchainServerIP + ":" + port + "/BlockchainServer");

                    host.AddServiceEndpoint(typeof(BlockchainServerInterface), tcp, "net.tcp://" + BlockchainServerIP + ":" + port + "/BlockchainServer"); //Build URI 

                    host.Open(); //Start Blockchain server - AddressAlreadyInUseException exception thrown here if another client is already using 
                    BlockchainServerStarted = true; //Stop the while !start loop 

                    RegisterBlockchain(); //Register with the Client web service it's Blockchain server
                }
                catch (AddressAlreadyInUseException) //Client already using the port 
                {
                    BlockchainServerPort++; //Increment port 

                    Logger.Error("Address already in use, attempting with next port: " + BlockchainServerPort.ToString());
                    Logger.Error("Using Address: net.tcp://" + BlockchainServerIP + ":" + port + "/BlockchainServer");

                    host = new ServiceHost(typeof(BlockchainServer)); //Reset host and try again until has started
                }
            }

            Logger.Log("Blockchain Server Started");

            while (!IsClosed) { } //Keep Blockchain server running 

            Logger.Log("Blockchain Server Closing");
            host.Close();
        }

        /// <summary>
        /// Monitor when the UI has been closed 
        /// 
        /// Based on answer at: https://stackoverflow.com/a/36400810/11378789
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Logger.Log("[Closing Program]");
            IsClosed = true;
        }

        /// <summary>
        /// Add the client to the Clients list in the client web service 
        /// </summary>
        private void RegisterBlockchain()
        {
            Logger.Log("Registering with Client Web Service: " + BlockchainServerIP + ":" + BlockchainServerPort);

            RestClient client = new RestClient(ClientListURL);
            RestRequest request = new RestRequest("api/Client/Register/" + BlockchainServerIP + "/" + BlockchainServerPort);
            client.Post(request); //POST request to add

            Dispatcher.Invoke(new SetAddressDelegate(SetAddress), new Object[] { BlockchainServerIP + ":" + BlockchainServerPort });
        }

        /// <summary>
        /// Display the IP:PORT the client is currently using
        /// </summary>
        /// <param name="address"></param>
        private void SetAddress(string address)
        {
            Connection.Text = address;
        }

        /// <summary>
        /// Append text to the output TextBox in client - Adds on new line 
        /// Used as a log to show the user. 
        /// </summary>
        /// <param name="output">Text to add </param>
        private void AddOutputEntry(string output)
        {
            Output.AppendText(output);
            Output.AppendText(Environment.NewLine);
        }

        /// <summary>
        /// Update the field showing number of blocks in the blockchain by calling Blockchain web service. 
        /// </summary>
        private void UpdateBlocks()
        {
            int numBlocks = Blockchain.Blockchain.GetNumBlocks();
            NumBlocksText.Text = numBlocks.ToString(); //Set number of blocks 
        }

        /// <summary>
        /// Calls hash function in blockchain 
        /// Generates a new hash by brute force. If no hash begins with 12345, increment offset by 1 and check again. 
        /// </summary>
        /// <param name="b"></param>
        /// <returns>Block with hash</returns>
        public Block GenerateHash(Block b)
        {
            Dispatcher.Invoke(new SetAddOutputEntry(AddOutputEntry), new Object[] { "Generating hash..." });
            return Blockchain.Blockchain.GenerateHash(b);
        }

        private void CheckBalanceClick(object sender, RoutedEventArgs e)
        {
            string walletCheckStr = WalletNumberCheck.Text.ToString();
            uint walletCheck;

            if (uint.TryParse(walletCheckStr, out walletCheck)) //Check if valid ID is provided 
            {
                float bal = Blockchain.Blockchain.GetBalance(walletCheck); 

                if (bal != -1.0) //-1.0 float indicates wallet doesn't exist 
                {
                    BalanceText.Text = bal.ToString(); //Show balance  
                }
                else
                {
                    MessageBox.Show("Error: Wallet does not exist");
                }
            }
            else
            {
                MessageBox.Show("Error: Invalid wallet ID");
            }
        }

        /// <summary>
        /// "Send" button click handler, gets the list of clients and validates the input 
        /// If valid input creates connection to each client and submits the transaction. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("Getting clients");

            string walletFromStr = WalletFrom.Text.ToString(); //Get all input fields 
            string walletToStr = WalletTo.Text.ToString();
            string walletAmountStr = Amount.Text.ToString();

            uint walletFrom, walletTo;
            float amount;

            NetTcpBinding tcp = new NetTcpBinding();

            RestClient client = new RestClient(ClientListURL);
            RestRequest request = new RestRequest("api/Client/Clients");
            IRestResponse list = client.Get(request); //GET request for list of clients 

            List<Client> clients = JsonConvert.DeserializeObject<List<Client>>(list.Content); //Deserialize JSON to List<Client>

            if (uint.TryParse(walletFromStr, out walletFrom) && uint.TryParse(walletToStr, out walletTo)) //Check wallet ID validity 
            {
                if (float.TryParse(walletAmountStr, out amount)) //Check amount validity (float) 
                {

                    foreach (Client c in clients) //Send new transaction to all clients 
                    {
                        try
                        {
                            string clientURL = "net.tcp://" + c.ToString() + "/BlockchainServer"; //Build client URL 

                            ChannelFactory<BlockchainServerInterface> blockchainFactory = new ChannelFactory<BlockchainServerInterface>(tcp, clientURL);
                            BlockchainServerInterface clientBlockchainServer = blockchainFactory.CreateChannel(); //Create channel 

                            clientBlockchainServer.RecieveNewTransaction(walletFrom, walletTo, amount);
                        }
                        catch(EndpointNotFoundException ex)
                        {
                            Logger.Error("Error: Connection could not be made to client.");
                            Dispatcher.Invoke(new SetAddOutputEntry(AddOutputEntry), new Object[] { "Error: Connection could not be made to client." });
                        }                        
                        catch(Exception ex)
                        {
                            Logger.Error("Error: Could not submit transaction to client " + c.ToString() + " Due to: " + ex.ToString());
                            Dispatcher.Invoke(new SetAddOutputEntry(AddOutputEntry), new Object[] { "Error: Could not submit transaction to client " + c.ToString() });
                        }
                    }
                } 
                else
                {
                    MessageBox.Show("Error: Invalid Amount");
                }
            }
            else
            {
                MessageBox.Show("Error: Invalid Wallet ID(s)");
            }
        }
    }
}
