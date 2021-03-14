using System;
using System.ServiceModel;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            string clientURL = "net.tcp://127.0.0.1:8100/BlockchainServer";

            NetTcpBinding tcp = new NetTcpBinding();
            ChannelFactory<BlockchainServerInterface> blockchainFactory = new ChannelFactory<BlockchainServerInterface>(tcp, clientURL);
            BlockchainServerInterface clientBlockchainServer = blockchainFactory.CreateChannel(); //Create channel 

            int num = clientBlockchainServer.GetNumBlocks(); //Attempt to get last block in blockchain, EndpointNotFoundException thrown here if unable to, indicating it cannot be connected to. 

            Console.WriteLine(num);
            Console.WriteLine("UP");
            Console.ReadLine();
        }
    }
}
