using LiteNetLib.Utils;

namespace Love.Network.Messages
{
    /// <summary>
    /// 游戏停止消息
    /// </summary>

    public class GameStopMessage : INetSerializable
    {
        public Faction Winner { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((sbyte)Winner);
        }

        public void Deserialize(NetDataReader reader)
        {
            Winner = (Faction)reader.GetSByte();
        }
    }
}
