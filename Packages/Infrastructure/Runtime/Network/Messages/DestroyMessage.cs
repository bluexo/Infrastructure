using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;

namespace Mecha
{
    public class DestroyMessage : INetSerializable
    {
        public ushort objectId;
        public byte reasons;

        public void Deserialize(NetDataReader reader)
        {
            objectId = reader.GetUShort();
            reasons = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(objectId);
            writer.Put(reasons);
        }
    }
}
