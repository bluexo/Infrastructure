using System;
using System.Runtime.CompilerServices;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Love.Network
{
    public enum Receivers : byte
    {
        Target,
        All,
    }

    public class NetworkFunction : NetworkElement
    {
        /// <summary>
        /// Quality of Send
        /// </summary>
        public DeliveryMethod Qos { get; set; } = DeliveryMethod.ReliableOrdered;

        private readonly Action<NetDataReader> callback;

        public NetworkFunction(NetworkObject behaviour, byte elementId)
            : base(behaviour, elementId)
        {

        }

        public NetworkFunction(NetworkObject behaviour, byte elementId, Action<NetDataReader> callback)
            : this(behaviour, elementId)
        {
            this.callback = callback;
        }

        public void Hook(NetDataReader reader) => callback?.Invoke(reader);

        protected void SerializeForClient(NetDataWriter writer, Receivers receivers, int connectionId)
        {
            writer.Put((byte)receivers);
            if (receivers == Receivers.Target)
                writer.Put(connectionId);
            Serialize(writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ServerSendCall(int connectionId, Action<NetDataWriter> writeParams)
        {
            networkObject.Transport.ServerSendPacket(connectionId, MessageId.CallFunction, Qos, (writer) =>
            {
                Serialize(writer);
                writeParams?.Invoke(writer);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ClientSendCall(Receivers receivers, Action<NetDataWriter> writeParams, int targetConnectionId)
        {
            networkObject.Transport.ClientSendPacket(MessageId.CallFunction, Qos, writer =>
            {
                SerializeForClient(writer, receivers, targetConnectionId);
                writeParams?.Invoke(writer);
            });
        }

        protected void Send(Receivers receivers, Action<NetDataWriter> writeParams, int targetConnectionId = -1)
        {
            if (!networkObject.Transport.IsServer)
            {
                if (networkObject.Transport.IsStarted) ClientSendCall(receivers, writeParams, targetConnectionId);
                return;
            }
            switch (receivers)
            {
                case Receivers.Target:
                    if (targetConnectionId == -1)
                    {
                        Log.Error(this, $"Invalid connectionId!");
                        return;
                    }
                    ServerSendCall(targetConnectionId, writeParams);
                    break;
                case Receivers.All:
                    foreach (var connectionId in networkObject.Transport.ConnectionIds)
                        ServerSendCall(connectionId, writeParams);
                    break;
                default:
                    throw new InvalidOperationException($"Invalide server to server Rpc!");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Call(DeliveryMethod qos, Receivers receivers, Action<NetDataWriter> writeParams = null)
        {
            Qos = qos;
            Send(receivers, writeParams);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Call(Receivers receivers = Receivers.All, Action<NetDataWriter> writeParams = null) => Send(receivers, writeParams);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Call(Action<NetDataWriter> writeParams) => Send(Receivers.All, writeParams);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Call<TParam>(TParam param, Receivers receivers = Receivers.All)
            where TParam : INetSerializable
           => Send(receivers, writer => param.Serialize(writer));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Call(int connectionId, Action<NetDataWriter> writeParams = null) => Send(Receivers.Target, writeParams, connectionId);
    }
}
