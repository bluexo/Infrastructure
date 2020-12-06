using System.Collections;
using System.Collections.Generic;

namespace Mecha.Network
{
    public interface INetworkedAssetsFactory
    {
        T CreateNetworkAssets<T>() where T : INetworkedAssets;
    }
}