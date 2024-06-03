using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Server
{
    static void Main(string[] args)
    {
        //Mientras no sea off el servidor rula
        bool bServerOn = true;

        //Instancia los servicios de red
        Network_Manager Network_Service = new Network_Manager();

        //Inicio de servicios
        StartServices();
        while (bServerOn)
        {
            Network_Service.CheckConnection();
            Network_Service.CheckMessage();
            Network_Service.DisconnectClients();
        }

        //Funcion que inicia los servicios del servidor
        void StartServices()
        {
            Network_Service.Start_Network_Service();
        }

    }
}
