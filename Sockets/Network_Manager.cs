using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public class NetworkManager
{
    private DatabaseManager dbManager;

    private TcpListener serverListener;
    private List<Client> clients;
    private Mutex clientListMutex;
    private int lastTimePing;
    private List<Client> disconnectClients;

    public NetworkManager()
    {

        dbManager = new DatabaseManager();

        // Initialize client list
        this.clients = new List<Client>();

        // Configure listener (Access IP + Port)
        this.serverListener = new TcpListener(IPAddress.Any, 6543);

        // Instantiate mutex
        this.clientListMutex = new Mutex();

        this.lastTimePing = Environment.TickCount;

        this.disconnectClients = new List<Client>();
    }

    // Check if client connections are made
    public void CheckConnection()
    {
        if (Environment.TickCount - this.lastTimePing > 10000)
        {
            clientListMutex.WaitOne();
            foreach (Client client in this.clients)
            {
                if (client.GetAwaitingResponse() == true)
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

    // Start the network service
    public void StartNetworkService()
    {
        try
        {
            // Start network services
            this.serverListener.Start();

            // Start listening
            StartListening();
        }
        catch (Exception ex)
        {
            // Display error if fails
            Console.WriteLine(ex.ToString());
        }

    }

    // Listen for new connections
    private void StartListening()
    {
        // Listen
        Console.WriteLine("Waiting for new connection");

        // Accept TCP connections
        this.serverListener.BeginAcceptTcpClient(AcceptConnection, this.serverListener);
    }

    // Accept new connections
    private void AcceptConnection(IAsyncResult ar)
    {
        Console.WriteLine("Received a connection");

        // Store connection
        TcpListener listener = (TcpListener)ar.AsyncState;
        clientListMutex.WaitOne();
        this.clients.Add(new Client(listener.EndAcceptTcpClient(ar)));
        clientListMutex.ReleaseMutex();
        // Listen again
        StartListening();
    }

    // Send ping to a client
    private void SendPing(Client client)
    {
        try
        {
            StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());
            writer.WriteLine("Ping");
            writer.Flush();
            client.SetAwaitingResponse(true);
        }
        catch (Exception ex) { Console.WriteLine("Error sending ping to user"); }
    }

    // Register new user
    public void Register(string nick, string password, string race, StreamWriter writer)
    {
        bool registerSuccess = dbManager.Register(nick, password, race);
        if (registerSuccess)
        {
            Console.WriteLine("Registration completed");
            writer.WriteLine("true");
            writer.Flush();
        }
        else
        {
            writer.WriteLine("false");
            writer.Flush();
        }
    }

    // Login
    public void Login(string nick, string password, StreamWriter writer)
    {
        string loginResult = dbManager.Login(nick, password);
        if (loginResult.StartsWith("true"))
        {
            Console.WriteLine("Login completed");
            writer.WriteLine(loginResult);
            writer.Flush();
        }
        else
        {
            Console.WriteLine("Failed to log in");
            writer.WriteLine("false");
            writer.Flush();
        }
    }

    // Disconnect clients
    public void DisconnectClients()
    {
        clientListMutex.WaitOne();
        foreach (Client client in this.disconnectClients)
        {
            Console.Write("Disconnecting users");
            client.GetTcpClient().Close();
            this.clients.Remove(client);
        }
        this.disconnectClients.Clear();
        clientListMutex.ReleaseMutex();
    }

    // Handle data received from clients
    private void ManageData(Client client, string data)
    {
        // Split data into parameters
        string[] parameters = data.Split('/');

        switch (parameters[0])
        {
            // If the action is LOGIN then call login function
            case "LOGIN":
                Login(parameters[1], parameters[2], new StreamWriter(client.GetTcpClient().GetStream()));
                break;
            // If the action is REGISTER then call register function
            case "REGISTER":
                Register(parameters[1], parameters[2], parameters[3], new StreamWriter(client.GetTcpClient().GetStream()));
                break;
        }
    }

    // Check messages received from clients
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
