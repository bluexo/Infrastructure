using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Mecha.Network
{
    public class TransportEventListener : INetEventListener
    {
        private NetworkTransport transport;
        private ConcurrentQueue<TransportEventData> eventQueue;
        private IDictionary<int, NetPeer> peersDict;
        public int Latency { get; private set; }

        public TransportEventListener(NetworkTransport transport, ConcurrentQueue<TransportEventData> eventQueue)
        {
            this.transport = transport;
            this.eventQueue = eventQueue;
        }

        public TransportEventListener(NetworkTransport transport,
            ConcurrentQueue<TransportEventData> eventQueue, IDictionary<int, NetPeer> peersDict) : this(transport, eventQueue)
        {
            this.peersDict = peersDict;
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            request.AcceptIfKey(transport.connectKey);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            eventQueue.Enqueue(new TransportEventData()
            {
                type = NetworkEvent.ErrorEvent,
                endPoint = endPoint,
                socketError = socketError,
            });
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            Latency = latency;
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            eventQueue.Enqueue(new TransportEventData()
            {
                type = NetworkEvent.DataEvent,
                connectionId = peer.Id,
                reader = reader,
            });
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnPeerConnected(NetPeer peer)
        {
            if (peersDict != null)
                peersDict[peer.Id] = peer;

            eventQueue.Enqueue(new TransportEventData()
            {
                type = NetworkEvent.ConnectEvent,
                connectionId = peer.Id,
            });
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (peersDict != null)
                peersDict.Remove(peer.Id);

            eventQueue.Enqueue(new TransportEventData()
            {
                type = NetworkEvent.DisconnectEvent,
                connectionId = peer.Id,
                disconnectInfo = disconnectInfo,
            });
        }
    }
}
