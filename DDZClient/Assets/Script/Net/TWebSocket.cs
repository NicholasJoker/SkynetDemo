using System.Threading;
using Script.Proto;
using UnityEngine;
using UnityWebSocket;
using WebSocket = UnityWebSocket.WebSocket;
namespace Script.Net
{
    public class TWebSocket
    {
        private CancellationToken token;
        public  IWebSocket socket;
        private string addrass = "ws://192.168.1.160:8886";
        private int sendCount;
        private static int receiveCount;
        private static TWebSocket instance;
        public static TWebSocket Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TWebSocket();
                    instance.socket = new WebSocket("ws://192.168.1.160:8886");
                    instance.socket.OnOpen += SocketOnOpen;
                    instance.socket.OnMessage += SocketOnMessage;
                    instance.socket.OnClose += SocketOnClose;
                    instance.socket.OnError += SocketOnError;
                    instance.socket.ConnectAsync();
                }
                return instance;
            }
        }
        public string Address
        {
            get { return addrass; }
            set { addrass = value; }
        }
        public TWebSocket()
        {
            socket = new WebSocket(this.addrass);
            socket.OnOpen += SocketOnOpen;
            socket.OnMessage += SocketOnMessage;
            socket.OnClose += SocketOnClose;
            socket.OnError += SocketOnError;
            socket.ConnectAsync();
            Debug.Log("Connecting...");
        }
        private static void SocketOnOpen(object sender, OpenEventArgs e)
        {
            Debug.Log("SocketOnOpen--------{add}");
        }

        private static void SocketOnMessage(object sender, MessageEventArgs e)
        {
            if (e.IsBinary)
            {
                Debug.Log(e.Data);
            }
            else if (e.IsText)
            {
                Debug.Log($"e.IsText--------{e.Data}");
            }
            receiveCount += 1;
        }
        
        private static void SocketOnClose(object sender, CloseEventArgs e)
        {
            Debug.Log(string.Format("Closed: StatusCode: {0}, Reason: {1}", e.StatusCode, e.Reason));
        }
        
        private static void SocketOnError(object sender, ErrorEventArgs e)
        {
            Debug.Log(string.Format("Error: {0}", e.Message));
        }
        public void Send(IMessage message)
        {
            Debug.Log($"Send--message---{message}");
            if(message == null)return;
            
            var content =  JsonUtility.ToJson(message);
            Debug.Log("Sending...:"+content);
            Instance.socket.SendAsync(content);
        }
    }
}