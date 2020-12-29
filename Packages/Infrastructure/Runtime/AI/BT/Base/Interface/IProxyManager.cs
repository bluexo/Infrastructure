using System;
using System.Collections;

namespace Origine.BT
{
    public interface IProxyManager
    {
        IEnumerator InitializeAsync();
        BaseNodeProxy Create(BaseNode classType);
        ProxyData GetProxyData(string classType);
    }
}