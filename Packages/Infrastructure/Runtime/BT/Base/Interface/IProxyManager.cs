using System;
using System.Collections;

namespace Origine
{
    public interface IProxyManager
    {
        void Initialize();
        BaseNodeProxy Create(BaseNode classType);
        ProxyData GetProxyData(string classType);
    }
}