using System;
using System.Text;
using Google.Protobuf;
using Lprotobuf;
using TMPro;
 using UnityEngine;
 using UnityEngine.UI;
 public class SocketTest : MonoBehaviour
{
    TextMeshProUGUI receiveText;
    AsyncSocketClient socket;
    readonly string host = "127.0.0.1";
    readonly int port = 8888;
    private Button sendButton;
    bool isRunning = true;
    // private int[,,] intThreeArray = { { {1, 2, 3} }, { {4, 5, 6} }, { { 7, 8, 9 }}};
    // List<int> intList = new List<int>();
    private void OnEnable()
    {
        Application.runInBackground = true;
        sendButton = GameObject.Find("Canvas/Begin").GetComponent<Button>();
        receiveText = GameObject.Find("Canvas/Begin/Text").GetComponent<TextMeshProUGUI>();
        // foreach (var VARIABLE in intThreeArray)
        // {
        //     Debug.Log(VARIABLE);
        //     
        //     intList.Add(VARIABLE);
        // }
    }
    async void Start()
    {
        // Application.runInBackground = true;
        //测试连接
        CreateClient();
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(TestSend);
        }
        // _connector = new ZenConnector();
        // bool connect = _connector.Connect("127.0.0.1", port);
        // Debug.Log($"connect success---{connect}");
        // await LoopUpdateAsync();
    }
    private void CreateClient() { // 创建客户端
        socket = new AsyncSocketClient(host,port,ReciveData);
    }

    public void TestSend()
    {
        //protobuf 测试
         Userdata userdata = new Userdata()
         {
             A = 123,
             B = 234,
             C=123,
             D = new Nest(){
                 A = true,
                 B = "我想润"
             },
             E = { 0,1,3 },
             F = { "加麻大","阿迈利卡" }
         };
         byte[] msg =userdata.ToByteArray();
         
         // //增加两个字节 表示size
         ushort len = (ushort)msg.Length;
         byte[] sendMsg = new byte[msg.Length+2];
         //低位
         sendMsg[0] = (byte)(len&0xFF);
         //高位
         sendMsg[1] = (byte)(len >> 8);
         sendMsg[2] = 0;
         Array.Copy(msg,0,sendMsg,3,msg.Length);
         Debug.Log(sendMsg.Length);
         socket.OnSend(sendMsg);
         // bool sendSuccessful =  _connector.Send("socketServer",msg,msg.Length);
         // Debug.Log("-------:"+ sendSuccessful);

    }

    /*async private void Update()
    {
        if (_connector != null)
        {
            _connector.Update(ReciveData);
        }
    }*/

    // async UniTask LoopUpdateAsync()
    // {
    //     while (isRunning)
    //     {
    //         if (_connector!=null)
    //         {
    //             _connector.Update(ReciveData);
    //             await UniTask.Delay(10);
    //         }
    //     }
    // }
    public void ReciveData(byte[]msg,int size)
    {
        string  reciveData = Encoding.UTF8.GetString(msg, 0, size);
        Debug.Log($"reciveData:{reciveData}");
    }
    public void ReciveData(string msg)
    {
        string  reciveData = msg.Substring(2, msg.Length - 2);
        Debug.Log($"reciveData:{reciveData}");
    }
}
