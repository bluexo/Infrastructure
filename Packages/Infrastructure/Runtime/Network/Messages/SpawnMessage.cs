using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;

using System.Numerics;

namespace Mecha
{
    public class SpawnPlayerMessage : SpawnMessage
    {
        public string userId;
        public int connectionId;

        public override void Deserialize(NetDataReader reader)
        {
            userId = reader.GetString();
            connectionId = reader.GetInt();
            base.Deserialize(reader);
        }

        public override void Serialize(NetDataWriter writer)
        {
            writer.Put(userId);
            writer.Put(connectionId);
            base.Serialize(writer);
        }

        public override string ToString() => $"uid:{userId},conn:{connectionId},type:{typeId}";
    }

    public class SpawnMessage : INetSerializable
    {
        public string assetId;
        public string typeId;
        public ushort objectId;
        public Vector3 position;
        public Quaternion rotation;

        public virtual void Deserialize(NetDataReader reader)
        {
            typeId = reader.GetString();
            assetId = reader.GetString();
            objectId = reader.GetUShort();
            position = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            rotation = new Quaternion(reader.GetFloat(), reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        }

        public virtual void Serialize(NetDataWriter writer)
        {
            writer.Put(typeId);
            writer.Put(assetId);
            writer.Put(objectId);
            writer.Put(position.X);
            writer.Put(position.Y);
            writer.Put(position.Z);
            writer.Put(rotation.X);
            writer.Put(rotation.Y);
            writer.Put(rotation.Z);
            writer.Put(rotation.W);
        }
    }
}
