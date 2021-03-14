// Filename: MainWindow.xaml.cs
// Project:  DC Assignment (COMP3008)
// Purpose:  Transaction client to send transactions or view blockchain status 
// Author:   Kevin Le (19472960)
//
// Date:     24/05/2020

using RestSharp;
using System.Windows;

namespace dc_p7_transaction
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string MinerURL = "https://localhost:44303/";  
        private string BlockchainURL = "https://localhost:44341/"; 

        /// <summary>
        /// Initialize the window, and set "Number of Blocks: " field to the current blocks in the blockchain. 
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            UpdateBlocks();
        }

        /// <summary>
        /// "Check" Button click handler, gets the balance of a given wallet from the Blockchain given Wallet ID 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CheckBalanceClick(object sender, RoutedEventArgs e)
        {
            RestClient client = new RestClient(BlockchainURL); 

            string walletCheckStr = WalletNumberCheck.Text.ToString();
            uint walletCheck;

            if (uint.TryParse(walletCheckStr, out walletCheck)) //Check if valid ID is provided 
            {
                IRestResponse response = await client.ExecuteAsync(new RestRequest("api/Blockchain/Balance/" + walletCheck.ToString())); //GET balance for wallet ID 
                string bal = response.Content.ToString();

                if (bal != "-1.0") //-1.0 float indicates wallet doesn't exist 
                {
                    BalanceText.Text = bal; //Show balance  
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
        /// "Refresh" button click event handler. Calls UpdateBlocks() to update the field showing number of blocks in the blockchain. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            UpdateBlocks(); 
        }

        /// <summary>
        /// Update the field showing number of blocks in the blockchain by calling Blockchain web service. 
        /// </summary>
        private async void UpdateBlocks()
        {
            RestClient client = new RestClient(BlockchainURL);
            IRestResponse response = await client.ExecuteAsync(new RestRequest("api/Blockchain/Current"));
            string numBlocks = response.Content.ToString(); 

            NumBlocksText.Text = numBlocks; //Set number of blocks 
        }

        /// <summary>
        /// "Send" button clicked, create a new transaction and send it to the Miner web service to process 
        ///  Given wallet ID from, wallet ID to, and amount. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SendClick(object sender, RoutedEventArgs e)
        {
            string walletFromStr = WalletFrom.Text.ToString(); //Get all input fields 
            string walletToStr = WalletTo.Text.ToString();
            string walletAmountStr = Amount.Text.ToString();

            uint walletFrom, walletTo;
            float amount;  

            if (uint.TryParse(walletFromStr, out walletFrom) && uint.TryParse(walletToStr, out walletTo)) //Check wallet ID validity 
            {
                if(float.TryParse(walletAmountStr, out amount)) //Check amount validity (float) 
                {
                    RestClient client = new RestClient(MinerURL);
                    await client.ExecutePostAsync(new RestRequest("api/Miner/Transaction/" + walletFrom.ToString() + "/" + walletTo.ToString() + "/" + amount.ToString()));
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

            UpdateBlocks();
        }
    }
}
