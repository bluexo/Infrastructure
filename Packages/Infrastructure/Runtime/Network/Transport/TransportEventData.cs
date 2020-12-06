using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Mecha.Network
{
    public enum NetworkEvent
    {
        DataEvent,
        ConnectEvent,
        DisconnectEvent,
        ErrorEvent,
    }

    //TODO
    public class TransportEventData
    {
        public NetworkEvent type;
        public int connectionId;
        public NetDataReader reader;
        public DisconnectInfo disconnectInfo;
        public IPEndPoint endPoint;
        public SocketError socketError;
    }
}