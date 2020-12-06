using System.Numerics;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using System;

namespace Mecha.Network
{
    public struct TransformState
    {
        public Vector3 Position { get; set; }
        public float Angle { get; set; }
    }

    public class NetworkTransform : NetworkField<TransformState>
    {
        public NetworkTransform(NetworkObject networkObject, byte elementId) : base(networkObject, elementId)
        {
            Qos = DeliveryMethod.Unreliable;
            OnlyTriggerClientEvent = true;
        }

        public override void Deserialize(NetDataReader reader)
        {
            val = new TransformState
            {
                Position = new Vector3(reader.GetFloat(), 0, reader.GetFloat()),
                Angle = reader.GetFloat()
            };
            if (OnlyTriggerClientEvent && networkObject.IsServer) return;
            ChangeEvent?.Invoke(this, val);
        }

        public override void Serialize(NetDataWriter writer)
        {
            writer.Put(ObjectId);
            writer.Put(ElementId);

            writer.Put(Value.Position.X);
            writer.Put(Value.Position.Z);
            writer.Put(Value.Angle);
        }
    }
}