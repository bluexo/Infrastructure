using System;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;

namespace Mecha.Network
{
    public partial class NetworkManager
    {
        #region Message Register & Unregister

        protected void RegisterServerMessages()
        {
            Transport.PeerConnectedEvent += OnPeerConnected;
            Transport.PeerDisconnectedEvent += OnPeerDisconnected;
            Transport.RegisterMessage(MessageId.ClientEnterGame, HandleClientEnterGame);
            Transport.RegisterMessage(MessageId.ClientReady, HandleClientReady);
            Transport.RegisterMessage(MessageId.ClientNotReady, HandleClientNotReady);

            //Transport.RegisterMessage(MessageId.InitialSyncField, HandleClientInitialSyncField);
            Transport.RegisterMessage(MessageId.UpdateSyncField, HandleClientUpdateSyncField);
            Transport.RegisterMessage(MessageId.CallFunction, HandleClientCallFunction);
        }

        protected void RegisterClientMessages()
        {
            Transport.ClientConnectedEvent += OnClientConnected;
            Transport.RegisterMessage(MessageId.ServerSpawnPlayer, HandleServerSpawnPlayerObject);
            Transport.RegisterMessage(MessageId.ServerSpawnObject, HandleServerSpawnObject);
            Transport.RegisterMessage(MessageId.ServerDestroyObject, HandleServerDestroyObject);

            Transport.RegisterMessage(MessageId.ServerTime, HandleServerTime);
            Transport.RegisterMessage(MessageId.ServerError, HandleServerError);

            Transport.RegisterMessage(MessageId.InitialSyncField, HandleServerInitialSyncField);
            Transport.RegisterMessage(MessageId.UpdateSyncField, HandleServerUpdateSyncField);
            Transport.RegisterMessage(MessageId.CallFunction, HandleServerCallFunction);
        }

        public void RegisterMessage(byte messageId, MessageHandlerDelegate handlerDelegate) => Transport.RegisterMessage(messageId, handlerDelegate);

        public void UnregisterMessage(byte messageId) => Transport.UnregisterMessage(messageId);
        #endregion

        #region Network Events
        public void OnPeerConnected(int connectionId)
        {
            if (Transport.Sessions.ContainsKey(connectionId)) return;
            SendServerTime(connectionId);
            Transport.Sessions[connectionId] = new NetworkSession(connectionId);
        }

        public void OnPeerDisconnected(int connectionId, DisconnectInfo disconnectInfo)
        {
            if (!Transport.Sessions.ContainsKey(connectionId)) return;
            var session = Transport.Sessions[connectionId];
            session.ClearSubscribing();
            Transport.Sessions.Remove(connectionId);
        }

        public void OnClientConnected(int connectionId) => SendClientEnterGame();

        public void OnServerError(ServerErrorMessage message)
        {
            if (IsServer) return;
            if (!message.shouldDisconnect) return;
            Stop();
        }

        public void SetPlayerReady(int connectionId, NetDataReader reader)
        {
            if (!IsServer)
                return;

            var player = Transport.Sessions[connectionId];
            if (player.IsReady)
                return;

            player.IsReady = true;

            Log.Info(this, $"Client {connectionId} ready!");
        }

        public void SetPlayerNotReady(int connectionId, NetDataReader reader)
        {
            if (!IsServer)
                return;

            var player = Transport.Sessions[connectionId];
            if (!player.IsReady)
                return;

            player.IsReady = false;
            player.ClearSubscribing();

            Log.Info(this, $"Client {connectionId} not ready!");
        }
        #endregion

        #region Send Messages
        public void SendClientEnterGame()
        {
            if (!IsNetworkActive)
                return;
            Transport.ClientSendPacket(MessageId.ClientEnterGame);
        }

        public void SendClientReady(string userId)
        {
            if (!IsNetworkActive)
                return;
            Transport.ClientSendPacket(MessageId.ClientReady, new StringMessage { value = userId });
        }

        public void SendClientNotReady()
        {
            if (!IsNetworkActive)
                return;
            Transport.ClientSendPacket(MessageId.ClientNotReady);
        }

        public void SendServerTime()
        {
            if (!IsServer)
                return;
            foreach (var connectionId in Transport.ConnectionIds)
                SendServerTime(connectionId);
        }

        public void Send(byte messageId)
        {
            if (!IsServer) Transport.ClientSendPacket(messageId);
            else Transport.ServerSendPacketToAll(messageId);
        }

        public void Send<T>(byte messageId, T message) where T : INetSerializable
        {
            if (!IsServer) Transport.ClientSendPacket(messageId, messageData: message);
            else Transport.ServerSendPacketToAll(messageId, messageData: message);
        }

        public void SendServerTime(int connectionId)
        {
            if (!IsServer)
                return;
            var message = new ServerTimeMessage();
            message.serverUnixTime = ServerUnixTime;
            message.serverTime = ServerTime;
            message.sendPackCount = Transport.netManager.Statistics.PacketsSent;
            message.receivePackCount = Transport.netManager.Statistics.PacketsReceived;
            Transport.ServerSendPacket(connectionId, MessageId.ServerTime, message, DeliveryMethod.Sequenced);
        }

        public void SendServerMessage<T>(byte messageId, T message = default) where T : INetSerializable
        {
            if (!IsServer)
                return;

            foreach (var connectionId in Transport.ConnectionIds)
                SendServerMessage(connectionId, messageId, message);
        }

        public void SendServerMessage<TMessage>(int connectionId, byte messageId, TMessage message) where TMessage : INetSerializable
        {
            if (!IsServer)
                return;
            if (!Transport.Sessions.TryGetValue(connectionId, out NetworkSession player) || !player.IsReady)
                return;

            Transport.ServerSendPacket(connectionId, messageId, message, DeliveryMethod.ReliableOrdered);
        }

        public void SendServerSpawnObject<TMessage>(NetworkObject identity, TMessage message, bool isPlayerObject = false) where TMessage : INetSerializable
        {
            if (!IsServer)
                return;

            foreach (var connectionId in Transport.ConnectionIds)
                SendServerSpawnObject(connectionId, identity, message, isPlayerObject);
        }

        public void SendServerSpawnObject<TMessage>(int connectionId, NetworkObject identity, TMessage message, bool isPlayerObject = false) where TMessage : INetSerializable
        {
            if (!IsServer)
                return;
            if (!Transport.Sessions.TryGetValue(connectionId, out NetworkSession player) || !player.IsReady)
                return;
            var msgId = isPlayerObject ? MessageId.ServerSpawnPlayer : MessageId.ServerSpawnObject;
            Transport.ServerSendPacket(connectionId, msgId, message, DeliveryMethod.ReliableOrdered, identity.InitialSyncFields);
        }

        public void SendServerDestroyObject<TMessage>(TMessage message) where TMessage : INetSerializable
        {
            if (!IsServer)
                return;
            foreach (var connectionId in Transport.ConnectionIds)
                SendServerDestroyObject(connectionId, message);
        }

        public void SendServerDestroyObject<TMessage>(int connectionId, TMessage message) where TMessage : INetSerializable
        {
            if (!IsServer)
                return;
            if (!Transport.Sessions.TryGetValue(connectionId, out NetworkSession player) || !player.IsReady)
                return;
            Transport.ServerSendPacket(connectionId, MessageId.ServerDestroyObject, message, DeliveryMethod.ReliableOrdered);
        }

        public void SendServerError(short errorCode, string errorMessage)
        {
            if (!IsServer)
                return;
            foreach (var connectionId in Transport.ConnectionIds)
                SendServerError(connectionId, errorCode, errorMessage);
        }

        public void SendServerError(int connectionId, short errorCode, string errorMessage)
        {
            if (!IsServer)
                return;
            //if (!Transport.Sessions.TryGetValue(connectionId, out NetworkSession player) || !player.IsReady)
            //    return;
            ServerErrorMessage message = new ServerErrorMessage();
            message.errorCode = errorCode;
            message.errorMessage = errorMessage;
            Transport.ServerSendPacket(connectionId, MessageId.ServerError, message, DeliveryMethod.ReliableOrdered);
        }
        #endregion

        #region Message Handlers
        protected void HandleClientEnterGame(MessageHandler messageHandler)
        {
            ClientEnterGameEvent?.Invoke(this, messageHandler.connectionId);
            Log.Info(this, $"Handle message {nameof(HandleClientEnterGame)}");
        }

        protected void HandleClientReady(MessageHandler messageHandler)
        {
            var userName = messageHandler.ReadMessage<StringMessage>();
            Log.Info(this, $"Handle message {nameof(HandleClientReady)}");
            ClientReadyEvent?.Invoke(this, (messageHandler.connectionId, userName.value));
            SetPlayerReady(messageHandler.connectionId, messageHandler.reader);
        }

        protected void HandleClientNotReady(MessageHandler messageHandler)
        {
            Log.Info(this, $"Handle message {nameof(HandleClientNotReady)}");

            SetPlayerNotReady(messageHandler.connectionId, messageHandler.reader);
        }

        protected void HandleClientInitialSyncField(MessageHandler messageHandler)
        {
            Log.Info(this, $"Handle message {nameof(HandleClientInitialSyncField)}");

            if (!IsServer)
                return;

            if (VerifyNetworkObject(messageHandler, out NetworkObject networkObject))
                networkObject.ProcessNetworkField(messageHandler.reader);
        }

        protected void HandleClientUpdateSyncField(MessageHandler messageHandler)
        {
            if (Log.LogSeverityLevel >= LogLevel.Information)
                Log.Info(this, $"Handle message {nameof(HandleClientUpdateSyncField)}");

            if (!IsServer)
                return;

            if (VerifyNetworkObject(messageHandler, out NetworkObject networkObject))
                networkObject.ProcessNetworkField(messageHandler.reader);
        }

        protected void HandleClientCallFunction(MessageHandler messageHandler)
        {
            //Log.Info(this, $"Handle message {nameof(HandleClientCallFunction)}");

            var reader = messageHandler.reader;
            var receivers = (Receivers)reader.GetByte();
            var connectionId = -1;
            if (receivers == Receivers.Target)
                connectionId = reader.GetInt();

            if (!VerifyNetworkObject(messageHandler, out NetworkObject networkObject))
            {
                Log.Error(this, $"Invalid {nameof(HandleClientCallFunction)},ConnectionId={messageHandler.connectionId},ObjectId:{networkObject?.ObjectId},HashCode:{networkObject?.GetHashCode()}");
                return;
            }
            networkObject.ProcessNetworkFunction(reader);
        }

        private bool VerifyNetworkObject(MessageHandler messageHandler, out NetworkObject networkObject)
        {
            networkObject = null;

            if (!Transport.Sessions.TryGetValue(messageHandler.connectionId, out NetworkSession player))
                return false;
            var reader = messageHandler.reader;
            var objectId = reader.GetUShort();

            if (!NetworkAssets.NetworkObjects.TryGetValue(objectId, out networkObject))
                return false;

            return player.SubscribingObjects.Contains(networkObject);
        }

        protected void HandleServerSpawnPlayerObject(MessageHandler messageHandler)
        {
            Log.Info(this, $"Handle message {nameof(HandleServerSpawnPlayerObject)}");
            var message = messageHandler.ReadMessage<SpawnPlayerMessage>();
            NetworkAssets.Spawn(message, out NetworkObject networkObject, messageHandler.reader);

            if (!Transport.Sessions.ContainsKey(message.connectionId))
                Transport.Sessions.Add(message.connectionId, new NetworkSession(message.connectionId));

            var session = Transport.Sessions[message.connectionId];
            session.Player = networkObject as INetworkPlayer;
            session.AddSubscribing(networkObject);
        }

        protected void HandleServerSpawnObject(MessageHandler messageHandler)
        {
            Log.Info(this, $"Handle message {nameof(HandleServerSpawnObject)}");
            var message = messageHandler.ReadMessage<SpawnMessage>();
            NetworkAssets.Spawn(message, out NetworkObject networkObject, messageHandler.reader);
        }

        protected void HandleServerDestroyObject(MessageHandler messageHandler)
        {
            Log.Info(this, $"Handle message {nameof(HandleServerDestroyObject)}");
            var message = messageHandler.ReadMessage<DestroyMessage>();
            NetworkAssets.Destroy(message);
        }

        protected void HandleServerInitialSyncField(MessageHandler messageHandler)
        {
            Log.Info(this, $"Handle message {nameof(HandleServerInitialSyncField)}");

            if (IsServer)
                return;
            var reader = messageHandler.reader;
            var objectId = reader.GetUShort();
            if (NetworkAssets.NetworkObjects.TryGetValue(objectId, out NetworkObject identity))
                identity.ProcessInitialFields(reader);
        }

        protected void HandleServerUpdateSyncField(MessageHandler messageHandler)
        {
            //Log.Info(this, $"Handle message {nameof(HandleServerUpdateSyncField)}");

            if (IsServer)
                return;
            var reader = messageHandler.reader;
            var objectId = reader.GetUShort();
            if (NetworkAssets.NetworkObjects.TryGetValue(objectId, out NetworkObject identity))
                identity.ProcessNetworkField(reader);
        }

        protected void HandleServerCallFunction(MessageHandler messageHandler)
        {
            //Log.Info(this, $"Handle message {nameof(HandleServerCallFunction)}");

            if (IsServer)
                return;
            var reader = messageHandler.reader;
            var objectId = reader.GetUShort();
            if (NetworkAssets.NetworkObjects.TryGetValue(objectId, out NetworkObject identity))
                identity.ProcessNetworkFunction(reader);
        }

        protected void HandleServerTime(MessageHandler messageHandler)
        {
            //Log.Info(this, $"Handle message {nameof(HandleServerTime)}");

            if (IsServer)
                return;
            var message = messageHandler.ReadMessage<ServerTimeMessage>();
            ServerUnixTimeOffset = message.serverUnixTime - Timestamp;
            ServerTimeOffset = message.serverTime - Timer.UnscaledTime;
        }

        protected void HandleServerError(MessageHandler messageHandler)
        {
            Log.Info(this, $"Handle message {nameof(HandleServerError)}");

            var message = messageHandler.ReadMessage<ServerErrorMessage>();
            //OnServerError(message);
            ServerErrorEvent?.Invoke(this, (message.errorCode, message.errorMessage));
        }
        #endregion
    }
}