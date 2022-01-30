using System;
using System.Net.Sockets;

class Client
{
    public int id;
    public TCP tcp;

    public Guid accountData;
    public bool downloadingData;
    public int signinAttempts = 0;

    public void Connect(TcpClient _socket, int _id)
    {
        id = _id;

        tcp = new TCP();
        tcp.Connect(_socket, _id);
    }

    public class TCP
    {
        public TcpClient socket;

        int id;
        NetworkStream stream;
        Packet receivedData;
        byte[] receiveBuffer;

        public void Connect(TcpClient _socket, int _id)
        {
            id = _id;

            socket = _socket;
            socket.ReceiveBufferSize = 4096;
            socket.SendBufferSize = 256000;

            stream = socket.GetStream();

            receivedData = new Packet();
            receiveBuffer = new byte[4096];

            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);

            ServerSend.Welcome(id);
        }

        public void SendData(byte[] data)
        {
            try { stream.BeginWrite(data, 0, 256000, null, null); }
            catch (Exception _ex) { Console.WriteLine($"Error sending data to player {id} via TCP: {_ex}"); }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    Program.clients[id].Disconnect();
                    return;
                }

                byte[] data = new byte[_byteLength];
                Array.Copy(receiveBuffer, data, _byteLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    if (data[1] == 1)
                    {
                        new Packet(data, id);
                    }
                    else
                    {
                        switch (data[2])
                        {
                            case 0: Program.Recving.Add(data[0], new Packet(data, id)); break;
                            case 1: Program.Recving[data[0]].AddChunk(data); break;
                            case 2: Program.Recving[data[0]].AddChunk(data); Program.packetHandlers[data[0]](id, Program.Recving[data[0]]); Program.Recving.Remove(data[0]); break;
                        }
                    }
                });

                receivedData.Reset(true);
                stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error receiving TCP data: {_ex}");
                Program.clients[id].Disconnect();
            }
        }

        public void Disconnect()
        {
            socket.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    private void Disconnect()
    {
        Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

        tcp.Disconnect();
        tcp = null;

        Program.CachedClients.Add(id);
        GC.SuppressFinalize(this);
    }
}