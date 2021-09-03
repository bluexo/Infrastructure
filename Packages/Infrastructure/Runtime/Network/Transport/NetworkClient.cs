using System;
using LiteNetLib;

namespace Love.Network
{
    /// <summary>
    /// 游戏客户端
    /// </summary>
    public class NetworkClient : NetworkTransport
    {
        public override bool IsServer => false;

        public override int Latency => eventListener.Latency;

        private readonly TransportEventListener eventListener;

        public override bool IsStarted => netManager != null
            && netManager.FirstPeer != null
            && netManager.FirstPeer.ConnectionState == ConnectionState.Connected;

        public NetworkClient(string connectKey) : base(connectKey)
        {
            eventListener = new TransportEventListener(this, eventQueue);
        }

        public bool Start(string address, int port)
        {
            if (isNetworkActive)
            {
                Log.Warn(this, "Cannot Start Client, network already active");
                return false;
            }
            // Reset acks
            ackCallbacks.Clear();
            nextAckId = 1;
            while (eventQueue.TryDequeue(out _)) ;

            netManager = new NetManager(eventListener)
            {
                DisconnectTimeout = 30000
            };
            isNetworkActive = netManager.Start() && netManager.Connect(address, port, connectKey) != null;
            return isNetworkActive;
        }

        public override void OnReceive(TransportEventData eventData)
        {
            switch (eventData.type)
            {
                case NetworkEvent.ConnectEvent:
                    Log.Info(this, "Client::OnPeerConnected");
                    TriggerClientConnected(eventData.connectionId);
                    break;
                case NetworkEvent.DataEvent:
                    ReadPacket(eventData.connectionId, eventData.reader);
                    break;
                case NetworkEvent.DisconnectEvent:
                    Log.Info(this, "Client::OnPeerDisconnected peer. disconnectInfo.Reason: " + eventData.disconnectInfo.Reason);
                    Close();
                    TriggerClientDisconnected(eventData.disconnectInfo);
                    break;
                case NetworkEvent.ErrorEvent:
                    Log.Error(this, "Client::OnNetworkError endPoint: " + eventData.endPoint + " socketErrorCode " + eventData.socketError);
                    TriggerNetworkError(eventData.endPoint, eventData.socketError);
                    break;
            }
        }
    }
}
