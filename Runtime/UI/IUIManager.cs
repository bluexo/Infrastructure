using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Origine
{
    public interface IUIManager
    {
        GameObject Root { get; }
        IReadOnlyDictionary<Type, GameObject> Prefabs { get; }
        IReadOnlyList<BaseUI> Windows { get; }

        IEnumerator InitializeAsync(string rootPath);

        T Find<T>() where T : BaseUI;

        TChild CreateChild<TParent, TChild>(GameObject parentGo = null)
           where TParent : BaseUI
           where TChild : BaseUI;

        BaseUI ShowByName(string name, bool exclude = false);

        T Show<T>(bool exclude = false) where T : BaseUI;

        T GetOrCreate<T>() where T : BaseUI;

        BaseUI GetOrCreate(Type type);

        BaseUI Show(Type windowType, bool exclude = false);

        T Show<T, A>(A a, bool exclude = false) where T : BaseUI;

        T Show<T, A, B>(A a, B b, bool exclude = false) where T : BaseUI;

        T Show<T, A, B, C>(A a, B b, C c, bool exclude = false) where T : BaseUI;

        T Show<T, A, B, C, D>(A a, B b, C c, D d, bool exclude = false) where T : BaseUI;

        void Close<T>() where T : BaseUI;

        void CloseExcept<T>(params T[] exceptWindows) where T : BaseUI;

        void Destroy<T>() where T : BaseUI;
    }
}
