using System;
using UnityEngine;
public interface BaseSocket
{
    public void OnAccept(IAsyncResult ar) {}
    public void OnConnect(IAsyncResult ar);
    public void OnReceive(IAsyncResult ar);
    //默认应该是 byte
    public void OnSend(string msg);
    public void OnSend(byte[] msg);
}
