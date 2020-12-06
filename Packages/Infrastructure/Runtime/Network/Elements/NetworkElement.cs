using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;

namespace Mecha.Network
{
    public abstract class NetworkElement : INetSerializable
    {
        public ushort ObjectId { get; protected set; }
        public byte ElementId { get; protected set; }

        protected readonly NetworkObject networkObject;

        public NetworkElement()
        {

        }

        public NetworkElement(NetworkObject objectId, byte elementId)
        {
            networkObject = objectId;
            ObjectId = networkObject.ObjectId;
            ElementId = elementId;
        }

        public virtual void Serialize(NetDataWriter writer)
        {
            writer.Put(ObjectId);
            writer.Put(ElementId);
        }

        public virtual void Deserialize(NetDataReader reader)
        {

        }
    }
}