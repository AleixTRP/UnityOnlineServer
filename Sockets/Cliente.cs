using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

class Client
{
    private TcpClient tcpClient;
    private string nickname;
    private bool awaitingResponse;

    public Client(TcpClient tcpClient)
    {
        this.tcpClient = tcpClient;
        this.nickname = "Guest";
        this.awaitingResponse = false;
    }

    public bool GetAwaitingResponse()
    {
        return this.awaitingResponse;
    }

    public void SetAwaitingResponse(bool awaitingResponse)
    {
        this.awaitingResponse = awaitingResponse;
    }

    public TcpClient GetTcpClient()
    {
        return this.tcpClient;
    }
}
