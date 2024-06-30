//using System;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using UnityEngine;

//public class UDPClient : MonoBehaviour
//{
//    public string serverIP = "172.16.0.228";
//    public int serverPort = 11001;

//    private UdpClient client;

//    void Start()
//    {
//        client = new UdpClient();
//    }

//    public void Send(float x, float y)
//    {
//        string message = (x + "/" + y + "/" + "0/-90/0/170/50");
//        Debug.Log(message);
//        byte[] data = Encoding.ASCII.GetBytes(message);
//        client.Send(data, data.Length, serverIP, serverPort);
//    }

//    private void OnApplicationQuit()
//    {
//        client.Close();
//        Debug.Log("ソケットを閉じる");
//    }
//}
