using LiteNetLib.Utils;

namespace Mecha
{
    public class ServerTimeMessage : INetSerializable
    {
        public long serverUnixTime;
        public long serverTime;
        public ulong sendPackCount, receivePackCount;

        public void Deserialize(NetDataReader reader)
        {
            serverUnixTime = reader.GetLong();
            serverTime = reader.GetLong();
            sendPackCount = reader.GetULong();
            receivePackCount = reader.GetULong();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(serverUnixTime);
            writer.Put(serverTime);
            writer.Put(sendPackCount);
            writer.Put(receivePackCount);
        }
    }
}
