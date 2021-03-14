// Filename: Database.cs
// Project:  DC Assignment (COMP3008)
// Purpose:  Mock database to hold a list of clients 
// Author:   Kevin Le (19472960)
//
// Date:     24/05/2020

using System.Collections.Generic;

namespace dc_p8_blockchain.Models
{
    public class Database
    {
        private static List<Client> clients = new List<Client>();

        /// <summary>
        /// Return the list of clients 
        /// </summary>
        /// <returns>List of clients</returns>
        public static List<Client> GetClients()
        {
            return clients;
        }

        /// <summary>
        /// Add a client to the list of clients 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public static void AddClient(string ip, string port)
        {
            Client c = new Client(); //Create new client 

            if (!clients.Contains(c)) //If client doesn't already exist 
            {
                c.ip = ip;
                c.port = port;

                clients.Add(c);
            }
        }

        /// <summary>
        /// Remove a client from the list of clients 
        /// </summary>
        /// <param name="c"></param>
        public static void RemoveClient(Client c)
        {
            clients.Remove(c);
        }
    }
}