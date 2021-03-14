// Filename: Monitor.cs
// Project:  DC Assignment (COMP3008)
// Purpose:  Logic to monitor clients from clients list in a loop and remove them if they go offline  
// Author:   Kevin Le (19472960)
//
// Date:     24/05/2020

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;

namespace dc_p8_blockchain.Models
{
    public static class Monitor
    {
        private delegate void ClientMonitor();

        /// <summary>
        /// Called once by the Client Controller to start the monitoring thread 
        /// </summary>
        public static void StartMonitor()
        {
            ClientMonitor monitor = CheckAlive;
            monitor.BeginInvoke(null, null); //Start monitoring thread 
        }

        /// <summary>
        /// Retrieves the list of clients and checks each client for connectivity in a loop. 
        /// Removes clients if they cannot be connected to (When EndpointNotFoundException is thrown)
        /// </summary>
        private static void CheckAlive()
        {
            Debug.WriteLine("[Monitor]: Started");

            while (true) //Loops forever until terminated 
            {
                List<Client> clients = Database.GetClients(); //Get the list of clients 

                foreach (Client c in clients)
                {
                    string clientURL = "net.tcp://" + c.ToString() + "/BlockchainServer";

                    try
                    {
                        Debug.WriteLine("[Monitor]: Checking if client: " + c.ToString() + " is up.");

                        NetTcpBinding tcp = new NetTcpBinding();
                        ChannelFactory<BlockchainServerInterface> blockchainFactory = new ChannelFactory<BlockchainServerInterface>(tcp, clientURL);
                        BlockchainServerInterface clientBlockchainServer = blockchainFactory.CreateChannel(); //Create channel 

                        clientBlockchainServer.GetNumBlocks(); //Attempt to get the blockchain, EndpointNotFoundException thrown here if unable to, indicating it cannot be connected to. 

                        Debug.WriteLine("[Monitor]: Client: " + c.ToString() + " is up.");
                    }
                    catch (EndpointNotFoundException)
                    {
                        //Client is offline 
                        Debug.WriteLine("[Monitor]: Client: " + c.ToString() + " is offline. Removing.");
                        Database.RemoveClient(c);

                        break; //Exit the for loop and retrieve the list again -> Cannot continue for loop as list has changed. 
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("[Monitor]: Error " + e.ToString());
                    }
                }

                Thread.Sleep(10000);
            }
        }
    }
}