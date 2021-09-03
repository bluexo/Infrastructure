using LiteNetLib.Utils;

namespace Love.Network.Messages
{
    public class LoginMessage : INetSerializable
    {
        public string UserId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
        }
    }
}
