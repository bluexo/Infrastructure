using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origine
{
    public class ResourcePool<T> : IReference
    {
        public Dictionary<string, T> res = new Dictionary<string, T>();

        public T Get<T>(string name)
        {
            return default;
        }

        public void OnDestroy()
        {
            throw new NotImplementedException();
        }
    }
}
