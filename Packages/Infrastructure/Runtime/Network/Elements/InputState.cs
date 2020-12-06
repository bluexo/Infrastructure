using System;
using System.Numerics;
using LiteNetLib.Utils;

namespace Mecha.Network
{
    [Flags]
    public enum Direction : byte
    {
        None = 0,
        Up = 1 << 1,
        Down = 1 << 2,
        Left = 1 << 3,
        Right = 1 << 4,
    }

    public struct InputState : INetSerializable
    {
        public Direction Direction { get; set; }
        public byte ActionId { get; set; }

        public void GetVector(ref Vector3 original, float factor = 1f)
        {
            if ((Direction & Direction.Up) == Direction)
                original.Z += factor;
            if ((Direction & Direction.Down) == Direction)
                original.Z -= factor;

            if ((Direction & Direction.Right) == Direction)
                original.X += factor;
            if ((Direction & Direction.Left) == Direction)
                original.X -= factor;
        }

        public void Deserialize(NetDataReader reader)
        {
            Direction = (Direction)reader.GetByte();
            ActionId = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Direction);
            writer.Put(ActionId);
        }
    }

}