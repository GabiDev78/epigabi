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
                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    // Next : handle the data received
                    stream.BeginRead(receiveBuffer, 0, BufferSize, ReceiveCallback, null);
                }
            }
            catch
            {
                // Next : disconnect
            }
        }
    }
}
