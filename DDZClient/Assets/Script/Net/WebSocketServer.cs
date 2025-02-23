using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
public class WebSocketServer
{
    private TcpListener listener;
    private Thread listenThread;
    private const int BufferSize = 1024;
    private bool listening = false;
    public WebSocketServer(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);
        listenThread = new Thread(new ThreadStart(ListenForClients));
        listenThread.Start();
    }

    private void ListenForClients()
    {
        listener.Start();

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
            clientThread.Start(client);
        }
    }

    private void HandleClientComm(object client)
    {
        TcpClient tcpClient = (TcpClient)client;
        NetworkStream clientStream = tcpClient.GetStream();

        byte[] message = new byte[BufferSize];
        int bytesRead;

        while (true)
        {
            bytesRead = 0;

            try
            {
                bytesRead = clientStream.Read(message, 0, BufferSize);
            }
            catch
            {
                break;
            }

            if (bytesRead == 0)
            {
                break;
            }

            string data = Encoding.UTF8.GetString(message, 0, bytesRead);
            Console.WriteLine("Received: " + data);

            byte[] response = Encoding.UTF8.GetBytes("Hello world!");
            clientStream.Write(response, 0, response.Length);
            clientStream.Flush();
        }

        tcpClient.Close();
    }
}
