using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Server
{
    static void Main(string[] args)
    {
        // Server runs while it's not turned off
        bool isServerOn = true;

        // Instantiate network services
        NetworkManager networkService = new NetworkManager();

        // Start services
        StartServices();

        while (isServerOn)
        {
            networkService.CheckConnection();
            networkService.CheckMessage();
            networkService.DisconnectClients();
        }

        // Function to start server services
        void StartServices()
        {
            networkService.StartNetworkService();
        }
    }
}
