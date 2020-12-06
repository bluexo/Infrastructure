using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

using UnityEngine;

namespace Origine
{
    public interface IPresenterManager
    {
        T Get<T>() where T : PresenterBase;

        PresenterBase Get(Type type);
        PresenterBase GetByName(string type);

        void Reset();
    }
}
