using dc_p8_blockchain.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace dc_p8_blockchain.Controllers
{
    public class ClientController : ApiController
    {
        //Determine if it is the start is the first start of the client 
        private Boolean firstStart = true;

        /// <summary>
        /// Allow a client to register by providing their IP and PORT 
        /// </summary>
        /// <param name="ip">String IP</param>
        /// <param name="port">String Port</param>
        [Route("api/Client/Register/{ip}/{port}")]
        [HttpPost]
        public void Register(string ip, string port)
        {
            if (Database.GetClients().Count == 0 && firstStart) //If this is the first client, start monitoring 
            {
                Monitor.StartMonitor(); //StartMonitor()'s job is to monitor and remove clients if they go offline -> Only want to call it once to start the loop 
                firstStart = false;
            }

            Database.AddClient(ip, port);
        }

        /// <summary>
        /// Get a list of clients that are registered
        /// </summary>
        /// <returns>Returns list of clients represented by client objects</returns>
        [Route("api/Client/Clients")]
        [HttpGet]
        public List<Client> Get()
        {
            return Database.GetClients();
        }
    }
}