using LiteNetLib;
using System.Collections.Generic;
using System.Net;

namespace Mecha.Network
{
    /// <summary>
    /// 游戏服务器
    /// </summary>
    public class NetworkServer : NetworkTransport
    {
        public override bool IsStarted => netManager != null && netManager.IsRunning;
        public override bool IsServer => true;

        public NetworkServer(string connectKey) : base(connectKey)
        {

        }

        public void Start(int port)
        {
            if (isNetworkActive)
            {
                Log.Warn(this, "[TransportHandler] Cannot Start Server, network already active");
                return;
            }
            isNetworkActive = true;
            // Reset acks
            ackCallbacks.Clear();
            nextAckId = 1;
            ServerPort = port;

            serverPeers.Clear();
            while (eventQueue.TryDequeue(out _)) ;
            netManager = new NetManager(new TransportEventListener(this, eventQueue, serverPeers))
            {
                DisconnectTimeout = 45000
            };
            if (!netManager.Start(port))
                throw new System.OperationCanceledException($"Cannot start server at {port}");
        }

        public override void OnReceive(TransportEventData eventData)
        {
            switch (eventData.type)
            {
                case NetworkEvent.ConnectEvent:
                    Log.Info(this, "Server::OnPeerConnected peer.ConnectionId: " + eventData.connectionId);
                    TriggerPeerConnected(eventData.connectionId);
                    break;
                case NetworkEvent.DataEvent:
                    ReadPacket(eventData.connectionId, eventData.reader);
                    break;
                case NetworkEvent.DisconnectEvent:
                    Log.Info(this, "Server::OnPeerDisconnected peer.ConnectionId: " + eventData.connectionId + " disconnectInfo.Reason: " + eventData.disconnectInfo.Reason);
                    TriggerPeerDisconnected(eventData.connectionId, eventData.disconnectInfo);
                    break;
                case NetworkEvent.ErrorEvent:
                    Log.Error(this, "Server::OnNetworkError endPoint: " + eventData.endPoint + " socketErrorCode " + eventData.socketError);
                    TriggerNetworkError(eventData.endPoint, eventData.socketError);
                    break;
            }
        }
    }
}