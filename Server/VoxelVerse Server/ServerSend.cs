using System;
using System.Collections.Generic;
using System.Text;

class ServerSend
{
    public static void Welcome(int _toClient)
    {
        Packet packet = new Packet();
        packet.Write(_toClient);
        packet.Send(ServerPackets.welcome, _toClient);
    }
}