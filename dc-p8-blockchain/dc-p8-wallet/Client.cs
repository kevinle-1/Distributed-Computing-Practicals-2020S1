// Filename: Client.cs
// Project:  DC Assignment (COMP3008)
// Purpose:  Object representing a Client (their blockchain server) 
//           It is assumed a client is uniquely identifiable by their IP and Port
// Author:   Kevin Le (19472960)
//
// Date:     25/05/2020

namespace dc_p8_wallet
{
    public class Client
    {
        public string ip;
        public string port;

        /// <summary>
        /// Returns string representation of client 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ip + ":" + port;
        }
    }
}