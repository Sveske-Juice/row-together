using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

[System.Serializable]
public class InputUDPPacket
{
    char nw;
    char ne;
    char se;
    char sw;

    public override string ToString()
    {
        return $"NW: {nw}, NE: {ne}, SE: {se}, SW: {sw}";
    }
}

public class BoatNetworkInput : MonoBehaviour
{
    private Thread receiveThread;

    private UdpClient client;

    [SerializeField]
    private int port;

    public string lastReceivedUDPPacket = "";
    public string allReceivedUDPPackets = ""; // clean up this from time to time!

    public void Start()
    {
        init();
    }

    // OnGUI
    void OnGUI()
    {
        string ipv4 = NetworkManager.singleton.networkAddress;
        Rect rectObj = new Rect(40, 10, 200, 400);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(
            rectObj,
            $"IP: {ipv4}",
            style
        );
    }

    private void init()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);

                string text = Encoding.UTF8.GetString(data);


                InputUDPPacket packet = ParseInput(text);
                lastReceivedUDPPacket = packet.ToString();

                allReceivedUDPPackets = allReceivedUDPPackets + packet.ToString();
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    public string getLatestUDPPacket()
    {
        allReceivedUDPPackets = "";
        return lastReceivedUDPPacket;
    }

    private InputUDPPacket ParseInput(string inputmap)
    {
        try
        {
            InputUDPPacket packet = JsonUtility.FromJson<InputUDPPacket>(inputmap);
            return packet;
        }
        catch (Exception e)
        {
            Debug.LogError($"Invalid input map packet from network controller: {inputmap}");
            throw e;
        }
        return null;
    }
}
