using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;

namespace Love.Network
{
    public struct MessageHandler
    {
        public byte msgType { get; private set; }
        public int connectionId { get; private set; }
        public NetDataReader reader { get; private set; }

        public MessageHandler(byte msgType, int connectionId, NetDataReader reader)
        {
            this.msgType = msgType;
            this.connectionId = connectionId;
            this.reader = reader;
        }

        public T ReadMessage<T>() where T : INetSerializable, new()
        {
            T msg = new T();
            msg.Deserialize(reader);
            return msg;
        }
    }
}