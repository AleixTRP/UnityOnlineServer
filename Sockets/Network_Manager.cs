using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public class Network_Manager
{
    private TcpListener serverListener;
    private List<Client> clients;
    private Mutex clientListMutex;
    private int lastTimePing;
    private List<Client> disconnenctClients;

    public Network_Manager()
    {
        //Inicio lista de clientes
        this.clients = new List<Client>();

        //Configuro el listener (Ip acceso + puerto)
        this.serverListener = new TcpListener(IPAddress.Any, 6543);

        //Instancio el mutex
        this.clientListMutex = new Mutex();  
        
        this.lastTimePing = Environment.TickCount;

        this.disconnenctClients = new List<Client>();   

    }

    private void RecievePing(Client client)
    {
        client.SetWaitingPing(false);
    }

    public void CheckConnection()
    {
        if(Environment.TickCount - this.lastTimePing > 5000)
        {
            clientListMutex.WaitOne();
            foreach(Client client in this.clients) 
            { 
                if(client.GetWaitingPing() == true)
                {
                    disconnenctClients.Add(client);
                }
                else
                {
                    SendPing(client);
                }
            
            }
            this.lastTimePing = Environment.TickCount;
            clientListMutex.ReleaseMutex();
        }
    }

    public void Start_Network_Service()
    {
        try
        {
            //Inicio de servicios de red
            this.serverListener.Start();

            //Inicio la escucha
            StartListening();
        }
        catch (Exception ex)
        {
            //Saco error si falla
            Console.WriteLine(ex.ToString());
        }

    }

    private void StartListening()
    {
        //Escucho
        Console.WriteLine("Esperando nueva conexion");

        //Acepta conexiones TCP
        this.serverListener.BeginAcceptTcpClient(AcceptConnection, this.serverListener);
    }

    private void AcceptConnection(IAsyncResult ar)
    {
        Console.WriteLine("Recibo una conexion");

        //Almacenamos conexion
        TcpListener listener = (TcpListener)ar.AsyncState;
        clientListMutex.WaitOne();
        this.clients.Add(new Client(listener.EndAcceptTcpClient(ar)));
        clientListMutex.ReleaseMutex();

        //Vuelvo a escuchar
        StartListening();
    }
    private void SendPing(Client client)
    {
        try
        {
            StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());
            writer.WriteLine("Ping");
            writer.Flush(); 
            client.SetWaitingPing(true);
        }catch (Exception ex)
        {
            Console.WriteLine("Error al enviar ping al usuario;");
        }
    }

    private void Login(string nick, string password)
    {
        Console.WriteLine("Peticion de: " + nick + " usando pass:" + password);
    }

    public void DisconnectClients()
    {
        clientListMutex.WaitOne();
        foreach (Client client in this.disconnenctClients) 
        {
            Console.Write("Desconectando usuarios");
            client.GetTcpClient().Close();
            this.clients.Remove(client);
        }
        this.disconnenctClients.Clear();
        clientListMutex.ReleaseMutex();
    }

    private void ManageData(Client client, string data)
    {
        string[] parameters = data.Split('/');

        switch(parameters[0])
        {
            case "0":
                Login(parameters[1], parameters[2]);
                break;
            case "1":
                RecievePing(client);
                break;
            
        }
    }

    public void CheckMessage()
    {
        clientListMutex.WaitOne();

        foreach(Client client in clients) 
        {
            NetworkStream netStream = client.GetTcpClient().GetStream();
            if(netStream.DataAvailable)
            {
                StreamReader reader = new StreamReader(netStream,true);
                string data = reader.ReadLine();

                if(data != null)
                {
                    ManageData(client, data);
                }
            }
        
        }
        clientListMutex.ReleaseMutex();
    }
}