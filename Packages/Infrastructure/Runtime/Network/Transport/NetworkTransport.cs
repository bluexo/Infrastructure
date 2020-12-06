using LiteNetLib;
using LiteNetLib.Utils;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

using static LiteNetLib.EventBasedNetListener;

namespace Mecha.Network
{
    public enum AckResponseCode : byte
    {
        Default = 0,
        Success = 1,
        Timeout = 2,
        Error = 3,
    }

    public class BaseAckMessage : INetSerializable
    {
        public uint ackId;
        public AckResponseCode responseCode;

        public virtual void Deserialize(NetDataReader reader)
        {
            ackId = reader.GetUInt();
            responseCode = (AckResponseCode)reader.GetByte();
        }

        public virtual void Serialize(NetDataWriter writer)
        {
            writer.Put(ackId);
            writer.Put((byte)responseCode);
        }
    }

    public delegate void MessageHandlerDelegate(MessageHandler messageHandler);
    public delegate void AckMessageCallback(AckResponseCode responseCode, BaseAckMessage messageData);

    /// <summary>
    /// 网络传输层
    /// </summary>
    public abstract class NetworkTransport
    {
        #region Fields & Properties
        public NetManager netManager { get; protected set; }
        public string connectKey { get; protected set; }
        public virtual int Latency { get; }

        public readonly IDictionary<int, NetworkSession> Sessions = new Dictionary<int, NetworkSession>();
        protected readonly IDictionary<int, NetPeer> serverPeers;
        protected readonly ConcurrentQueue<TransportEventData> eventQueue;

        public int ServerPort { get; protected set; }

        public ICollection<int> ConnectionIds => serverPeers.Keys;

        protected readonly IDictionary<byte, MessageHandlerDelegate> messageHandlers = new ConcurrentDictionary<byte, MessageHandlerDelegate>();
        protected readonly IDictionary<uint, AckMessageCallback> ackCallbacks = new ConcurrentDictionary<uint, AckMessageCallback>();
        protected readonly NetDataWriter _writer = new NetDataWriter(true, 40960);
        protected TransportEventData tempEventData;
        protected uint nextAckId = 1;

        public ushort NextObjectId => objectId++;
        private ushort objectId = 1;

        public abstract bool IsStarted { get; }
        public abstract bool IsServer { get; }

        protected bool isNetworkActive;

        public int AckCallbacksCount => ackCallbacks.Count;
        #endregion

        #region Network Events & Trigger
        public event Action<int> ClientConnectedEvent;
        public event Action<DisconnectInfo> ClientDisconnectedEvent;

        public event Action<int> PeerConnectedEvent;
        public event Action<int, DisconnectInfo> PeerDisconnectedEvent;

        public event OnNetworkError NetworkErrorEvent;
        public event OnNetworkLatencyUpdate NetworkLatencyUpdateEvent;
        public event OnNetworkReceiveUnconnected NetworkReceiveUnconnectedEvent;

        public void TriggerClientConnected(int connectionId) => ClientConnectedEvent?.Invoke(connectionId);
        public void TriggerClientDisconnected(DisconnectInfo disconnectInfo) => ClientDisconnectedEvent?.Invoke(disconnectInfo);

        public void TriggerPeerConnected(int connectionId) => PeerConnectedEvent?.Invoke(connectionId);
        public void TriggerPeerDisconnected(int connectionId, DisconnectInfo disconnectInfo) => PeerDisconnectedEvent?.Invoke(connectionId, disconnectInfo);

        public void TriggerNetworkError(IPEndPoint endPoint, SocketError socketError) => NetworkErrorEvent?.Invoke(endPoint, socketError);
        #endregion

        #region Methods
        public NetworkTransport(string connectKey)
        {
            this.connectKey = connectKey;
            serverPeers = new Dictionary<int, NetPeer>();
            eventQueue = new ConcurrentQueue<TransportEventData>();
        }

        public abstract void OnReceive(TransportEventData eventData);

        public void Update()
        {
            if (!isNetworkActive) return;
            while (Receive(out tempEventData))
                OnReceive(tempEventData);
        }

        public void Close()
        {
            isNetworkActive = false;
            if (netManager != null)
                netManager.Stop();
            netManager = null;
        }

        protected void ReadPacket(int connectionId, NetDataReader reader)
        {
            var msgType = reader.GetByte();
            MessageHandlerDelegate handlerDelegate;
            if (messageHandlers.TryGetValue(msgType, out handlerDelegate))
            {
                MessageHandler messageHandler = new MessageHandler(msgType, connectionId, reader);
                handlerDelegate.Invoke(messageHandler);
            }
        }

        public void RegisterMessage(byte msgType, MessageHandlerDelegate handlerDelegate) => messageHandlers[msgType] = handlerDelegate;

        public void UnregisterMessage(byte msgType) => messageHandlers.Remove(msgType);

        public uint AddAckCallback(AckMessageCallback callback)
        {
            uint ackId = nextAckId++;
            lock (ackCallbacks) ackCallbacks.Add(ackId, callback);
            return ackId;
        }

        public uint ClientSendAckPacket<T>(
            byte msgType,
            DeliveryMethod deliveryMethod,
            T messageData,
            AckMessageCallback callback,
            Action<NetDataWriter> extraSerializer = null) where T : BaseAckMessage
        {
            messageData.ackId = AddAckCallback(callback);
            ClientSendPacket(msgType, deliveryMethod, writer =>
            {
                messageData.Serialize(writer);
                extraSerializer?.Invoke(writer);
            });
            return messageData.ackId;
        }

        public uint ServerSendAckPacket<T>(int connectionId,
            DeliveryMethod deliveryMethod,
            byte msgType,
            T messageData,
            AckMessageCallback callback,
            Action<NetDataWriter> extraSerializer = null) where T : BaseAckMessage
        {
            messageData.ackId = AddAckCallback(callback);
            ServerSendPacket(connectionId, msgType, deliveryMethod, (writer) =>
            {
                messageData.Serialize(writer);
                extraSerializer?.Invoke(writer);
            });
            return messageData.ackId;
        }

        public void TriggerAck<T>(uint ackId, AckResponseCode responseCode, T messageData) where T : BaseAckMessage
        {
            lock (ackCallbacks)
            {
                AckMessageCallback ackCallback;
                if (!ackCallbacks.TryGetValue(ackId, out ackCallback)) return;
                ackCallbacks.Remove(ackId);
                ackCallback(responseCode, messageData);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClientSendPacket(byte msgType, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered, Action<NetDataWriter> serializer = null)
        {
            lock (_writer)
            {
                _writer.Reset();
                _writer.Put(msgType);
                serializer?.Invoke(_writer);
                ClientSend(deliveryMethod, _writer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ServerSendPacket(int connectionId,
            byte msgType,
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered,
            Action<NetDataWriter> serializer = null)
        {
            lock (_writer)
            {
                _writer.Reset();
                _writer.Put(msgType);
                serializer?.Invoke(_writer);
                ServerSend(connectionId, deliveryMethod, _writer);
            }
        }

        public bool Receive(out TransportEventData eventData)
        {
            eventData = default;
            if (netManager == null)
                return false;
            netManager.PollEvents();
            return eventQueue.TryDequeue(out eventData);
        }

        public bool ClientSend(DeliveryMethod deliveryMethod, NetDataWriter writer)
        {
            if (!IsStarted) return false;
            netManager.FirstPeer.Send(writer, deliveryMethod);
            return true;
        }

        public bool ServerSend(int connectionId, DeliveryMethod deliveryMethod, NetDataWriter writer)
        {
            if (!IsStarted || !serverPeers.ContainsKey(connectionId))
            {
                Log.Error(this, $"Server Send fail , network not ready or connection not exists!");
                return false;
            }
            serverPeers[connectionId].Send(writer, deliveryMethod);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClientSendPacket<T>(byte msgType,
            T messageData = default,
            DeliveryMethod options = DeliveryMethod.ReliableOrdered,
            Action<NetDataWriter> extraSerializer = null) where T : INetSerializable
            => ClientSendPacket(msgType, options, writer =>
            {
                messageData.Serialize(writer);
                extraSerializer?.Invoke(writer);
            });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ServerSendPacket<T>(int connectionId,
            byte msgType,
            T messageData = default,
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered,
            Action<NetDataWriter> extraSerializer = null) where T : INetSerializable
            => ServerSendPacket(connectionId, msgType, deliveryMethod, writer =>
            {
                messageData?.Serialize(writer);
                extraSerializer?.Invoke(writer);
            });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ServerSendPacket(int connectionId, byte msgType, DeliveryMethod options = DeliveryMethod.ReliableOrdered)
            => ServerSendPacket(connectionId, msgType, options, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ServerSendPacketToAll(byte msgType, DeliveryMethod deliveryMethod, Action<NetDataWriter> serializer)
        {
            if (!(this is NetworkServer server))
                throw new ArgumentNullException($"{nameof(ServerSendPacketToAll)}");

            foreach (var connectionId in server.ConnectionIds)
                ServerSendPacket(connectionId, msgType, deliveryMethod, serializer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ServerSendPacketToAll<T>(byte msgType,
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered,
            T messageData = default) where T : INetSerializable
        {
            if (!(this is NetworkServer server))
                throw new ArgumentNullException($"{nameof(ServerSendPacketToAll)}");

            foreach (var connectionId in ConnectionIds)
                ServerSendPacket(connectionId, msgType, messageData, deliveryMethod);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ServerSendPacketToAll(byte msgType, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (!(this is NetworkServer server))
                throw new ArgumentNullException($"{nameof(ServerSendPacketToAll)}");

            foreach (var connectionId in server.ConnectionIds)
                ServerSendPacket(connectionId, msgType, deliveryMethod);
        }

        public bool ServerDisconnect(int connectionId)
        {
            if (!IsStarted || !serverPeers.ContainsKey(connectionId))
                return false;
            netManager.DisconnectPeer(serverPeers[connectionId]);
            return true;
        }

        public virtual void Reset()
        {
            objectId = 0;
            netManager.DisconnectAll();
            serverPeers.Clear();
        }
        #endregion
    }
}
