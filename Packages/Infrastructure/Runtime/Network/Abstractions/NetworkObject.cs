using LiteNetLib.Utils;

using System.Collections.Concurrent;
using System.Linq;

namespace Love.Network
{
    /// <summary>
    /// 网络对象
    /// 网络对象包含同步字段和远程调用
    /// </summary>
    public abstract partial class NetworkObject
    {
        /// <summary>
        /// 网络 Id
        /// </summary>
        public ushort ObjectId { get; protected set; }

        /// <summary>
        /// 网络传输层 
        /// </summary>
        public NetworkTransport Transport { get; protected set; }

        public NetworkObjectType Type { get; protected set; }

        public bool IsServer => Transport.IsServer;

        protected byte NextElementId => baseElementIndex++;
        protected byte baseElementIndex = 0;
        protected float prevSyncTime = 0;

        /// <summary>
        /// 属性列表
        /// </summary>
        protected ConcurrentDictionary<byte, NetworkField> networkFields = new ConcurrentDictionary<byte, NetworkField>();
        protected ConcurrentDictionary<byte, NetworkFunction> networkFunctions = new ConcurrentDictionary<byte, NetworkFunction>();
        protected bool cachedElements = false;

        public NetworkObject()
        {

        }

        public NetworkObject(NetworkTransport transport, ushort objectId)
        {
            Transport = transport;
            ObjectId = objectId;
            networkFields = new ConcurrentDictionary<byte, NetworkField>();
            networkFunctions = new ConcurrentDictionary<byte, NetworkFunction>();
        }

        public void ProcessInitialFields(NetDataReader reader)
        {
            if (!cachedElements) CacheNetworkMember();
            for (byte i = 0; i < networkFields.Count; i++)
            {
                var field = networkFields[i];
                var objId = reader.GetUShort();
                if (ObjectId != objId)
                {
                    Log.Error(this, $"NetworkObject id don't match! {ObjectId}<->{objId}");
                    return;
                }
                var tmpID = reader.GetByte();

                if (field.ElementId != tmpID)
                {
                    Log.Error(this, $"server field order is not equal client!");
                    return;
                }

                field.Deserialize(reader);
            }
        }

        public void ProcessNetworkField(NetDataReader reader)
        {
            if (!cachedElements) CacheNetworkMember();

            var id = reader.GetByte();
            if (!networkFields.ContainsKey(id))
            {
                Log.Error(this, $"Cannot found fieldId {id}");
                return;
            }
            networkFields[id].Deserialize(reader);
        }

        public void ProcessNetworkFunction(NetDataReader reader)
        {
            if (!cachedElements) CacheNetworkMember();

            var id = reader.GetByte();
            if (!networkFunctions.ContainsKey(id))
            {
                Log.Error(this, $"Cannot found functionId {id}");
                return;
            }
            var func = networkFunctions[id];
            func.Deserialize(reader);
            func.Hook(reader);
        }

        public void CacheNetworkMember()
        {
            foreach (var field in GetType().GetFields())
            {
                AddMember(field.GetValue(this));
            }

            foreach (var property in GetType().GetProperties())
            {
                if (!property.CanWrite || !property.CanRead) continue;
                AddMember(property.GetValue(this));
            }

            cachedElements = true;

            void AddMember(object member)
            {
                if (member == null) return;
                if (member is NetworkField networkField)
                    networkFields.TryAdd(networkField.ElementId, networkField);
                else if (member is NetworkFunction networkFunction)
                    networkFunctions.TryAdd(networkFunction.ElementId, networkFunction);
            }
        }

        public void InitialSyncFields(NetDataWriter writer)
        {
            if (!cachedElements) CacheNetworkMember();
            for (byte i = 0; i < networkFields.Count; i++)
            {
                var field = networkFields[i];
                field.Serialize(writer);
            }
        }

        public virtual void NetworkUpdate(float deltaTime)
        {
            if (!cachedElements)
                CacheNetworkMember();
            for (byte i = 0; i < networkFields.Count; i++)
                SyncField(networkFields[i]);
        }

        /// <summary>
        /// 同步字段
        /// </summary>
        /// <param name="field"></param>
        private void SyncField(NetworkField field)
        {
            //禁止客户端向服务端同步
            if (!field.NeedUpdate || !Transport.IsServer) return;
            Transport.ServerSendPacketToAll(MessageId.UpdateSyncField, field.Qos, writer => field.Serialize(writer));
            field.NeedUpdate = false;
        }

        public void Dispose()
        {
            networkFields.Clear();
            networkFunctions.Clear();
        }
    }
}