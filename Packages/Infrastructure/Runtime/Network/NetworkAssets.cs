using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using LiteNetLib.Utils;

namespace Love.Network
{
    public class NetworkedAssets
    {
        protected readonly ConcurrentDictionary<ushort, NetworkObject> spawnedObjects = new ConcurrentDictionary<ushort, NetworkObject>();
        protected readonly ConcurrentDictionary<string, object> preloadAssets = new ConcurrentDictionary<string, object>();
        protected readonly NetworkTransport transport;

        public NetworkedAssets(NetworkTransport transport)
        {
            this.transport = transport;
        }

        public virtual T GetAsset<T>(string assetId)
        {
            if (!preloadAssets.ContainsKey(assetId))
                throw new InvalidCastException($"Invalid UnityObject !!!");

            return (T)preloadAssets[assetId];
        }

        public virtual IList<TObject> SpawnPlayers<TObject>()
            where TObject : NetworkObject
        {
            var list = new List<TObject>();
            foreach (var session in transport.Sessions.Values)
            {
                var networkObject = (TObject)Activator.CreateInstance(typeof(TObject), transport, transport.NextObjectId);
                session.Player = networkObject as INetworkPlayer;
                session.AddSubscribing(networkObject);
                Log.Info(this, $"connectionId:{session.ConnectionId},ObjectId:{networkObject.ObjectId}");
                spawnedObjects.TryAdd(networkObject.ObjectId, networkObject);
                list.Add(networkObject);
            }
            return list;
        }

        public void Add<T>(T networkObj) where T : NetworkObject
        {
            if (null == networkObj)
                return;

            spawnedObjects.TryAdd(networkObj.ObjectId, networkObj);
        }

        public void Remove<T>(T networkObj) where T : NetworkObject
        {
            if (null == networkObj)
                return;

            spawnedObjects.TryRemove(networkObj.ObjectId, out _);
        }

        public virtual void Spawn<TMessage, TObject>(TMessage msg, out TObject networkObject, NetDataReader objReader = null)
            where TMessage : SpawnMessage
            where TObject : NetworkObject
        {
            networkObject = ObjectMapper.CreateInstance<TObject>(msg.typeId, transport, msg.objectId);
            spawnedObjects.TryAdd(networkObject.ObjectId, networkObject);

            if (null != objReader)
                networkObject.ProcessInitialFields(objReader);
        }

        public virtual void Destroy(DestroyMessage message) => spawnedObjects.TryRemove(message.objectId, out _);

        public virtual void Dispose() => spawnedObjects.Clear();
    }

}