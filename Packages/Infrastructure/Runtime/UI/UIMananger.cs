using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Origine
{
    internal sealed class UIManager : GameModule, IUIManager
    {
        public IReadOnlyDictionary<Type, GameObject> Prefabs => prefabs;
        public IReadOnlyList<BaseUI> Windows => uiWindows;
        public GameObject Root { get; private set; }

        private readonly List<BaseUI> uiWindows = new List<BaseUI>();
        private readonly Dictionary<Type, GameObject> prefabs = new Dictionary<Type, GameObject>();

        public const int DepthBetweenParent = 2;

        public override int Priority => 0;

        private readonly GameContext _gameContext;
        private readonly IEventManager _eventManager;

        public UIManager(GameContext gameContext)
        {
            _gameContext = gameContext;
            _eventManager = gameContext.GetModule<IEventManager>();
        }

        public IEnumerator InitializeAsync(string assetName)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(assetName);
            yield return handle;
            Root = GameObject.Instantiate(handle.Result);
            GameObject.DontDestroyOnLoad(Root);

            var types = Utility.AssemblyCollection.GetTypes(t => t.IsSubclassOf(typeof(BaseUI)) && !t.IsAbstract);
            var tasks = new List<Task>();
            foreach (var windowType in types)
                tasks.Add(LoadPrefabAsync(windowType));
            var completeTask = Task.WhenAll(tasks);

            yield return new WaitUntil(() => completeTask.IsCompleted || completeTask.IsFaulted);
        }

        private async Task LoadPrefabAsync(Type windowType)
        {
            var window = (BaseUI)Activator.CreateInstance(windowType);
            var windowHandle = await Addressables.LoadAssetAsync<GameObject>(window.AssetName).Task;
            if (!windowHandle)
            {
                Debug.LogError($"Cannot found ui asset {window.AssetName}");
                return;
            }
            windowHandle.SetActive(false);
            prefabs.Add(windowType, windowHandle);
        }

        public override void Update(float deltaTime)
        {
            for (int i = 0; i < uiWindows.Count; i++)
            {
                var window = uiWindows[i];
                if (window.IsVisible && !window.IsDestroyed)
                {
                    window.Update(deltaTime);
                }
            }
        }

        public override void Dispose()
        {
            for (int i = 0; i < uiWindows.Count; i++)
                uiWindows[i].Destroy();

            uiWindows.Clear();
        }

        public T Find<T>() where T : BaseUI
        {
            for (int i = 0; i < uiWindows.Count; i++)
            {
                if (uiWindows[i].GetType() == typeof(T))
                    return uiWindows[i] as T;
            }

            return default;
        }

        public BaseUI FindByName(string name)
        {
            BaseUI tmpWnd = null;
            for (int i = 0, max = uiWindows.Count; i < max; ++i)
                if (!uiWindows[i].AssetName.Equals(name))
                {
                    tmpWnd = uiWindows[i];
                    break;
                }
            return tmpWnd;
        }

        public BaseUI GetOrCreate(Type type)
        {
            var window = uiWindows.FirstOrDefault(w => w.GetType() == type);

            if (window == null)
            {
                window = (BaseUI)Activator.CreateInstance(type);
                uiWindows.Add(window);
                _eventManager.RegisterCommandHandler(window);
            }

            if (!prefabs.ContainsKey(type))
            {
                Debug.LogError($"Cannot found prefab {type.FullName}!");
                return window;
            }

            if (!window.Self)
                window.Initialize(Root, prefabs[type]);

            return window;
        }

        public T GetOrCreate<T>() where T : BaseUI => (T)GetOrCreate(typeof(T));

        public BaseUI GetOrCreateByName(string name)
        {
            var tmpWindow = FindByName(name);

            if (null != tmpWindow)
                return tmpWindow;

            Type tmpType = Type.GetType(name);
            if (null == tmpType)
                return tmpWindow;

            tmpWindow = Activator.CreateInstance(tmpType) as BaseUI;
            if (null == tmpWindow)
                return tmpWindow;
            uiWindows.Add(tmpWindow);

            return tmpWindow;
        }

        public T Show<T>(bool exclude = false) where T : BaseUI
        {
            var window = GetOrCreate<T>();
            ShowInternal(window, exclude);
            return window;
        }

        private void Exclude(BaseUI current, Action<BaseUI> action)
        {
            var others = uiWindows
                .Where(w => w.GetType() != current.GetType()
                    && !current.IsInGroup(w)
                    && !current.HasChild(w))
                .ToList();

            foreach (var ui in others)
            {
                action?.Invoke(ui);
            }
        }

        public BaseUI Show(Type windowType, bool exclude = false)
        {
            var window = GetOrCreate(windowType);
            ShowInternal(window, exclude);
            return window;
        }

        public BaseUI ShowByName(string name, bool exclude = false)
        {
            var window = GetOrCreateByName(name);
            ShowInternal(window, exclude);
            return window;
        }

        public TChild CreateChild<TParent, TChild>(GameObject parentGo = null)
            where TParent : BaseUI
            where TChild : BaseUI
        {
            var parent = Find<TParent>();
            var child = parent.GetChildWindow<TChild>();
            if (child != null) return child;

            child = Activator.CreateInstance<TChild>();
            parent.AddChildWindow(child, prefabs[typeof(TChild)], parentGo);
            return child;
        }

        public T Show<T, A>(A a, bool exclude = false) where T : BaseUI
        {
            var window = GetOrCreate<T>();
            if (window is IInitializer<A> initializer)
                initializer.Initialize(a);

            ShowInternal(window, exclude);
            return window;
        }

        public T Show<T, A, B>(A a, B b, bool exclude = false) where T : BaseUI
        {
            var window = GetOrCreate<T>();

            if (window is IInitializer<A, B> initializer)
                initializer.Initialize(a, b);

            ShowInternal(window, exclude);
            return window;
        }

        public T Show<T, A, B, C>(A a, B b, C c, bool exclude = false) where T : BaseUI
        {
            var window = GetOrCreate<T>();

            if (window is IInitializer<A, B, C> initializer)
                initializer.Initialize(a, b, c);

            ShowInternal(window, exclude);
            return window;
        }

        public T Show<T, A, B, C, D>(A a, B b, C c, D d, bool exclude = false) where T : BaseUI
        {
            var window = GetOrCreate<T>();

            if (window is IInitializer<A, B, C, D> initializer)
                initializer.Initialize(a, b, c, d);

            ShowInternal(window, exclude);
            return window;
        }

        private void ShowInternal(BaseUI window, bool exclude)
        {
            if (exclude) Exclude(window, w => w.Close());
            window.Show();
            SortOrder();
        }

        public void SortOrder()
        {
            uiWindows.Sort((a, b) => a.Order.CompareTo(b.Order));
            for (var i = 0; i < uiWindows.Count; i++)
            {
                var window = uiWindows[i];
                if (!window.Self) continue;
                window.Self.transform.SetSiblingIndex(i);
            }
        }

        public void Close<T>() where T : BaseUI
        {
            T tmpWindow = Find<T>();
            tmpWindow?.Close();
        }

        public void CloseExcept<T>(params T[] excepWindows) where T : BaseUI
        {
            var willCloseWindows = uiWindows.Except(excepWindows);
            foreach (var window in willCloseWindows)
                if (window.IsVisible) window.Close();
        }

        public void Destroy<T>() where T : BaseUI
        {
            T tmpWindow = Find<T>();

            if (null != tmpWindow)
                tmpWindow.Destroy();
        }
    }
}
