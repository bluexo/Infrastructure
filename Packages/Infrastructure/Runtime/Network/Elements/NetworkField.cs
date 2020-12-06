using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Numerics;

namespace Mecha.Network
{
    public enum SyncMode
    {
        ServerToOwnerClient,
        ClientMulticast,
        ServerToClients
    }

    public abstract class NetworkField : NetworkElement
    {
        public DeliveryMethod Qos { get; set; } = DeliveryMethod.ReliableOrdered;
        public SyncMode SyncMode { get; set; } = SyncMode.ServerToClients;

        public bool OnlyTriggerClientEvent = false;

        public NetworkField(NetworkObject networkId, byte elementId) : base(networkId, elementId) { }

        public bool NeedUpdate { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NetworkField<T> : NetworkField
    {
        /// <summary>
        /// Action for initial state, data this will be invoked when data changes
        /// </summary>
        public EventHandler<T> ChangeEvent;

        protected T val;

        public T Value
        {
            get => val;
            set
            {
                if (val != null && val.Equals(value)) return;
                val = value;
                NeedUpdate = true;
            }
        }

        public void Set(T value, bool initial = false)
        {
            Value = value;
            if (initial) return;
            if (OnlyTriggerClientEvent && networkObject.IsServer) return;
            ChangeEvent?.Invoke(this, Value);
        }

        public NetworkField(NetworkObject networkId, byte elementId, T initialValue = default) : base(networkId, elementId)
        {
            val = initialValue;
            if (networkId.IsServer) NeedUpdate = true;
        }

        public override void Deserialize(NetDataReader reader)
        {
            val = (T)reader.GetValue(typeof(T));
            if (OnlyTriggerClientEvent && networkObject.IsServer) return;
            ChangeEvent?.Invoke(this, val);
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.PutValue(val);
        }
    }

}