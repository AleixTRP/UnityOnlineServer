using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

class Client
{
    private TcpClient tcp;
    private string nick;
    private bool waitingPing;

    public Client(TcpClient tcp)
    {
        this.tcp = tcp;
        this.nick = "Guest";
        this.waitingPing = false;
    }

    public bool GetWaitingPing()
    {
        return this.waitingPing;
    }

    public void SetWaitingPing(bool waitingPing)
    {
        this.waitingPing = waitingPing;
    }

    public TcpClient GetTcpClient()
    {
        return this.tcp;
    }
}
