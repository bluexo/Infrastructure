using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Origine
{
    public abstract class PresenterBase : Entity
    {
        public bool Initialized { get; protected set; } = false;

        public virtual void Initialize()
        {
            Initialized = true;
        }

        public virtual void Reset()
        {
            Initialized = false;
        }
    }
}