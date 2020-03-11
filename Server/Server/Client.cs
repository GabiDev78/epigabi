using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Client
    {

        public int id;
        public TCP tcp;
        public static int BufferSize = 4096;

        public Client(int _clientID)
        {
            id = _clientID;
            tcp = new TCP(id);

        }

        public class TCP
        {
            public TcpClient socket;
            private NetworkStream stream;
            private byte[] receiveBuffer;
            private readonly int id;

            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = BufferSize;
                socket.SendBufferSize = BufferSize;

                stream = socket.GetStream();

                receiveBuffer = new byte[BufferSize];

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
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiveing TCP data : {ex}");
                    // Next : disconnect client instead or error
                }
            }
        }
    }
}
