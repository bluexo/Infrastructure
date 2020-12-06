using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;

namespace Mecha
{
    public class ServerErrorMessage : INetSerializable
    {
        public bool shouldDisconnect;
        public short errorCode;
        public string errorMessage;

        public void Deserialize(NetDataReader reader)
        {
            shouldDisconnect = reader.GetBool();
            errorCode = reader.GetShort();
            errorMessage = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(shouldDisconnect);
            writer.Put(errorCode);
            writer.Put(errorMessage);
        }
    }
}