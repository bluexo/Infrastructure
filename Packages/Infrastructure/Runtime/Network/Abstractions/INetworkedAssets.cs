using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

namespace Love.Network
{
    /// <summary>
    /// 网络资源
    /// </summary>
    public interface INetworkedAssets
    {
        /// <summary>
        /// 服务端生成玩家对象
        /// </summary>
        /// <returns></returns>
        IList<T> SpawnPlayers<T>() where T : NetworkObject;

        void Add<T>(T networkObj) where T : NetworkObject;

        void Remove<T>(T networkObj) where T : NetworkObject;

        /// <summary>
        /// 客户端生成对象
        /// </summary>
        /// <returns></returns>
        //void Spawn<TMessage>(TMessage msg, out NetworkObject networkObject, NetDataReader objReader = null) where TMessage : SpawnMessage;

        void Spawn<TMessage, TNetworkObject>(TMessage msg, out TNetworkObject networkObject, NetDataReader objReader = null)
            where TMessage : SpawnMessage
            where TNetworkObject : NetworkObject;

        /// <summary>
        /// 销毁对象
        /// </summary>
        /// <returns></returns>
        void Destroy(DestroyMessage message);

        /// <summary>
        /// 初始化资源
        /// </summary>
        void Initialize();

        /// <summary>
        /// 获取资产
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetId"></param>
        /// <returns></returns>
        T GetAsset<T>(string assetId);

        void Dispose();

        /// <summary>
        /// 已经生成的对象
        /// </summary>
        IReadOnlyDictionary<ushort, NetworkObject> NetworkObjects { get; }
    }
}