using Mecha.Network;
using System.Collections;
using System.Collections.Generic;

namespace Mecha.Network
{
    /// <summary>
    /// 玩家组 
    /// </summary>
    public interface IPlayerGroup
    {
        byte Id { get; }

        ICollection<int> Connections { get; }

        bool AddPlayer(NetworkSession session);

        bool TryGetPlayer(int connectionId, out NetworkSession session);
    }

}