using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public class Network_Manager
{
    private Database_Manager dbManager;

    private TcpListener serverListener;
    private List<Client> clients;
    private Mutex clientListMutex;
    private int lastTimePing;
    private List<Client> disconnectClients;

    public Network_Manager()
    {

        dbManager = new Database_Manager();

        //Inicio lista de clientes
        this.clients = new List<Client>();

        //Configuro el listener (Ip acceso + puerto)
        this.serverListener = new TcpListener(IPAddress.Any, 6543);

        //Instancia el mutex
        this.clientListMutex = new Mutex();

        this.lastTimePing = Environment.TickCount;

        this.disconnectClients = new List<Client>();
    }

    private void ReceivePing(Client client)
    {
        client.SetWaitingPing(false);
    }

    public void CheckConnection()
    {
        if (Environment.TickCount - this.lastTimePing > 10000)
        {
            clientListMutex.WaitOne();
            foreach (Client client in this.clients)
            {
                if (client.GetWaitingPing() == true)
                {
                    disconnectClients.Add(client);
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
        }
        catch (Exception ex) { Console.WriteLine("Error al enviar ping al usuario"); }
    }

    public void Register(string nick, string password, string race, StreamWriter writer)
    {
        bool registerSuccess = dbManager.Register(nick, password, race);
        if (registerSuccess)
        {
            Console.WriteLine("Registro completado");
            writer.WriteLine("true");
            writer.Flush();
        }
        else
        {
            writer.WriteLine("false");
            writer.Flush();
        }
    }

    public void Login(string nick, string password, StreamWriter writer)
    {
        string loginResult = dbManager.Login(nick, password);
        if (loginResult.StartsWith("true"))
        {
            Console.WriteLine("Login successful");
            writer.WriteLine(loginResult);
            writer.Flush();
        }
        else
        {
            Console.WriteLine("Login failed");
            writer.WriteLine("false");
            writer.Flush();
        }
    }


    public void DisconnectClients()
    {
        clientListMutex.WaitOne();
        foreach (Client client in this.disconnectClients)
        {
            Console.Write("Desconectando usuarios");
            client.GetTcpClient().Close();
            this.clients.Remove(client);
        }
        this.disconnectClients.Clear();
        clientListMutex.ReleaseMutex();
    }

    private void ManageData(Client client, string data)
    {
        string[] parameters = data.Split('/');

        switch (parameters[0])
        {
            case "LOGIN":
                Login(parameters[1], parameters[2], new StreamWriter(client.GetTcpClient().GetStream()));
                break;
            case "REGISTER":
                Register(parameters[1], parameters[2], parameters[3], new StreamWriter(client.GetTcpClient().GetStream()));
                break;
        }
    }


    public void CheckMessage()
    {
        clientListMutex.WaitOne();
        foreach (Client client in this.clients)
        {
            NetworkStream netStream = client.GetTcpClient().GetStream();
            if (netStream.DataAvailable)
            {
                StreamReader reader = new StreamReader(netStream, true);
                string data = reader.ReadLine();

                if (data != null)
                {
                    ManageData(client, data);
                }
            }
        }
        clientListMutex.ReleaseMutex();
    }
}
