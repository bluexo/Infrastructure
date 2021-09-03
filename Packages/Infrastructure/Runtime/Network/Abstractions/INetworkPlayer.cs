using System.Collections;
using System.Collections.Generic;

namespace Love.Network
{

    public interface INetworkPlayer
    {
        ushort ObjectId { get; }

        NetworkSession Session { get; set; }

        void NetworkUpdate(float deltaTime);
    }
}