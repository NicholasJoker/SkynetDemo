using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Script.Game;
using TMPro;
using UnityEngine;
using Script.Net;
using Script.Proto;
using UnityEngine.UI;
using UnityWebSocket;
public class Launch : MonoBehaviour
{
    [SerializeField] 
    public string ip = "ws://192.168.1.160";
        //"ws://192.168.1.87";
        //"ws://127.0.0.1";
    [SerializeField]
    public int port = 8886;
    private TMP_InputField account;
    private TMP_InputField password;
    private Button loginBtn;
    bool isRunning = true;
    public GameManager gameManager;
    #region unity 生命周期

    private void Awake()
    {
        
    }

    void Start()
    {
        Debug.Log("Start launch!!!");
        TWebSocket.Instance.socket.OnOpen += SocketOnOpen;
        TWebSocket.Instance.socket.OnMessage += SocketOnMessage;
        TWebSocket.Instance.socket.OnError += SocketOnError;
        TWebSocket.Instance.socket.OnClose += SocketOnClose;
        TWebSocket.Instance.socket.OnError += SocketOnError;
        account =  GameObject.Find("Login/Account").GetComponent<TMP_InputField>();
        password = GameObject.Find("Login/Password").GetComponent<TMP_InputField>();
        loginBtn = GameObject.Find("Login/LoginBtn").GetComponent<Button>();
        Debug.Log(gameManager);
        if (account != null)
        {
            var username = PlayerPrefs.GetString("username");
            // Debug.Log(userName);
            if (!string.IsNullOrEmpty(username))
            {
                account.text = username;
            }
            account.onValueChanged.AddListener(OnAccountValueChange);
        }
        if (password != null)
        {
            var pwd = PlayerPrefs.GetString("password");
            // Debug.Log(pwd);
            if (!string.IsNullOrEmpty(pwd))
            {
                this.password.text = pwd;
            }
            password.onValueChanged.AddListener(OnPwdValueChange);
        }
        if (loginBtn!=null)
        {
            loginBtn.onClick.AddListener(OnLogin);
        }
    }
    public void OnAccountValueChange(string account)
    {
        Debug.Log($"OnAccountValueChange{account}");
        CheckLogin(this.account.text, this.password.text);
    }
    public void OnPwdValueChange(string password)
    {
        Debug.Log($"OnAccountValueChange{password}");
        CheckLogin(this.account.text, this.password.text);
    }

    public void OnLogin()
    {
        Debug.Log("OnLogin");
        LoginRequest loginRequest = new LoginRequest()
        {
            opCode = OperationCode.Login,
            username = account.text,
            password = password.text,
        };
        TWebSocket.Instance.Send(loginRequest);
    }
    public void CheckLogin(string account, string password)
    {
        if (account.Length>2&&account.Length<12&&password.Length>5&&password.Length<9)
        {
            loginBtn.enabled = true;
        }
    }
    public void ReciveData(string msg)
    {
        string  reciveData = msg.Substring(2, msg.Length - 2);
        Debug.Log($"reciveData:{reciveData}");
    }
    void Update()
    {
        if (isDisConnect)
        {
            reconnectTime -= Time.deltaTime;
            if (reconnectTime < 0.1f)
            {
                reconnectTime = 1;
                TWebSocket.Instance.socket.ConnectAsync();
            }
        }
    }
    #endregion

    #region 服务器的消息
    private void SocketOnOpen(object sender, OpenEventArgs e)
    {
        isDisConnect = false;
        Debug.Log("Launch---SocketOnOpen--------{add}");
    }

    private void SocketOnMessage(object sender, MessageEventArgs e)
    {
        if (e.IsBinary)
        {
            Debug.Log(e.Data);
        }
        else if (e.IsText)
        {
            Debug.Log($"e.IsText--------{e.Data}");
            var jsonString1 = JObject.Parse(e.Data);
            Debug.Log(jsonString1);
            
            //登录
            var opStr = jsonString1["reciveCode"].ToString();
            ReciveCode key;
            ReciveCode.TryParse(opStr, out key);
            // var jsonString = "{ \"reciveCode\": \"0\", \"playerId\": 899020, \"lobbyIp\": \"127.0.0.1\", \"roomConfig\": [{\"roomId\": \"10001\", \"roomName\": \"普通房\" }, { \"roomId\": \"10002\", \"roomName\": \"高手房\"}]}";
            LoginResponse loginResponse = JsonConvert.DeserializeObject<LoginResponse>(jsonString1.ToString());
            Debug.Log(loginResponse.reciveCode);
            PlayerPrefs.SetString("username", account.text);
            PlayerPrefs.SetString("password", password.text);
            if (loginResponse != null&&loginResponse.reciveCode==0)
            {
                CloseLogin(loginResponse);
            }
        }
    }
        
    private void SocketOnClose(object sender, CloseEventArgs e)
    {
        if (e.StatusCode == CloseStatusCode.Abnormal)
        {
            isDisConnect = true;
        }
        Debug.Log(string.Format("Closed: StatusCode: {0}, Reason: {1}", e.StatusCode, e.Reason));
    }

    public bool isDisConnect = false;
    public float reconnectTime = 1.0f;
    private void SocketOnError(object sender, ErrorEventArgs e)
    {
        Debug.Log(string.Format("Error: {0}", e.Message));
    }
    #endregion

    #region 界面逻辑处理
    /// <summary>
    /// 关闭界面
    /// </summary>
    private void CloseLogin(LoginResponse loginResponse)
    {
        // TWebSocket.Instance.socket.OnMessage -= SocketOnMessage;
        // TWebSocket.Instance.socket.OnError -= SocketOnError;
        // TWebSocket.Instance.socket.OnClose -= SocketOnClose;
        // TWebSocket.Instance.socket.OnError -= SocketOnError;
        // webSocket.socket.CloseAsync();
        if (loginBtn!=null)
        {
            loginBtn.transform.parent.gameObject.SetActive(false);
        }
        if (gameManager!=null)
        {
            gameManager.InitGameManager(loginResponse);
            gameManager.gameObject.SetActive(true);
        }
    }
    #endregion
}
