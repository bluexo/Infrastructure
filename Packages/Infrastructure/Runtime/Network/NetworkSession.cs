using System.Collections.Generic;

namespace Mecha.Network
{
    public class NetworkSession
    {
        public int ConnectionId { get; protected set; }
        public string UserId { get; set; }

        private INetworkPlayer player;

        public INetworkPlayer Player
        {
            get => player;
            set
            {
                player = value;
                if (player != null)
                    player.Session = this;
            }
        }

        public bool IsReady { get; set; }

        /// <summary>
        /// 玩家控制的子物体
        /// </summary>
        public readonly HashSet<NetworkObject> SubscribingObjects = new HashSet<NetworkObject>();

        public NetworkSession(int connectionId)
        {
            ConnectionId = connectionId;
            IsReady = true;
        }

        public void AddSubscribing(NetworkObject identity)
        {
            Log.Info(this, $"Add subscribing UserId:{UserId}, ConnectionId:{ConnectionId}, ObjectId:{identity.ObjectId}, HashCode: {identity.GetHashCode()}");
            SubscribingObjects.Add(identity);
        }

        public void RemoveSubscribing(NetworkObject identity)
        {
            Log.Info(this, $"Remove subscribing UserId:{UserId}, ObjectId:{identity.ObjectId}");
            SubscribingObjects.Remove(identity);
        }

        public void ClearSubscribing()
        {
            Log.Info(this, $"Clear all subscribing {SubscribingObjects.Count}");
            Player = null;
            SubscribingObjects.Clear();
        }
    }
}
