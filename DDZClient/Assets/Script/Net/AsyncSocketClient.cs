using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class AsyncSocketClient : BaseSocket
{
    private Socket socket;
    private byte[] readBuff = new byte[4096];
    private Action<string> msgCallback ;

    public AsyncSocketClient(string ip,int port,Action<string> msgCallback)
    {
        this.msgCallback = msgCallback;
        socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), 8888);
        socket.BeginConnect(endPoint,OnConnect,socket);
    }
    ~AsyncSocketClient()
    {
        socket.Close();
    }
    public void OnConnect(IAsyncResult ar)
    {
        try
        {
            socket.EndConnect(ar);
            Debug.Log($"Socket connected to {socket.RemoteEndPoint.ToString()}");
            OnBeginReceive();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            Console.WriteLine(e);
            throw;
        }
    }
    public void OnReceive(IAsyncResult ar)
    {
        Socket workingSocket = ar.AsyncState as Socket;
        int count = workingSocket.EndReceive(ar);
        //不一定是string
        string msg = Encoding.UTF8.GetString(readBuff,0,count);
        msgCallback(msg);
        Debug.Log($"Socket received:{msg.Length}");
    }

    private void OnBeginReceive()
    {
        socket.BeginReceive(readBuff,0,readBuff.Length,SocketFlags.None,OnReceive,socket);
    }
    public void OnSend(string msg)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(msg);
        socket.Send(buffer,buffer.Length,SocketFlags.None);
    }

    public void OnSend(byte[] buffer)
    {
        socket.Send(buffer,buffer.Length,SocketFlags.None);
    }
}
