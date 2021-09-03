using System.Collections;
using System.Collections.Generic;

namespace Love.Network
{
    public interface INetworkedAssetsFactory
    {
        T CreateNetworkAssets<T>() where T : INetworkedAssets;
    }
}