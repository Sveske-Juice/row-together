using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InputUDPPacket
{
    public string nw = "0";
    public string ne = "0";
    public string se = "0";
    public string sw = "0";

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

    bool inputThisFrame = false;

    public string lastReceivedUDPPacket = "";
    public string allReceivedUDPPackets = ""; // clean up this from time to time!

    public UnityEvent OnNWClick;
    public UnityEvent OnNEClick;
    public UnityEvent OnSEClick;
    public UnityEvent OnSWClick;

    public InputUDPPacket input = new();

    public void Start()
    {
        init();
    }

    // OnGUI
    //void OnGUI()
    //{
    //    string ipv4 = IPManager.GetIP(ADDRESSFAM.IPv4);
    //    Rect rectObj = new Rect(40, 10, 200, 400);
    //    GUIStyle style = new GUIStyle();
    //    style.alignment = TextAnchor.UpperLeft;
    //    GUI.Box(
    //        rectObj,
    //        $"IP: {ipv4}",
    //        style
    //    );
    //}

    private void init()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void Update()
    {
        if (inputThisFrame)
        {
            TriggerInput(input);
            inputThisFrame = false;
        }
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


                input = ParseInput(text);
                lastReceivedUDPPacket = input.ToString();
                print($"received: {input.ToString()}");
                inputThisFrame = true;

                allReceivedUDPPackets = allReceivedUDPPackets + input.ToString();
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
    }

    private void TriggerInput(InputUDPPacket packet)
    {
        if (Read(packet.nw))
            OnNWClick?.Invoke();

        if (Read(packet.ne))
            OnNEClick?.Invoke();

        if (Read(packet.se))
            OnSEClick?.Invoke();

        if (Read(packet.sw))
            OnSWClick?.Invoke();

        bool Read(string key)
        {
            return key == "1";
        }
    }

}
