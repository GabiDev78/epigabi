using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;

    public static int BufferSize = 4096;

    public string ip = "127.0.0.1";

    public int port = 26950;

    public int myId = 0;

    public TCP tcp;

    private delegate void PacketHandler(Packet _packet);
    
    private static Dictionary<int, PacketHandler> packetHandlers; 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            Debug.Log("lol");

        }
        else if (instance != this)
        {
            Debug.Log("instance already existing, destroying object");
            Destroy(this);

        }
    }

    private void Start()
    {
        tcp = new TCP();
    }

    public void ConnectToServer()
    {
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;

        private Packet receivedData;

        private NetworkStream stream;

        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = BufferSize,
                SendBufferSize = BufferSize
            };

            receiveBuffer = new byte[BufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);

            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();

            stream.BeginRead(receiveBuffer, 0, BufferSize, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int byteLength = stream.EndRead(_result);
                if (byteLength <= 0)
                {
                    //Next : disconnect client
                    return;
                }
                else
                {
                    byte[] _data = new byte[byteLength];
                    Array.Copy(receiveBuffer, _data, byteLength);

                    receivedData.Reset();
                    receivedData.Reset(HandleData(_data));

                    stream.BeginRead(receiveBuffer, 0, BufferSize, ReceiveCallback, null);
                }
            }
            catch
            {
                // Next : disconnect
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });

                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }
    }

    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome }
        };
        Debug.Log("Initalize packets");
    }
}
