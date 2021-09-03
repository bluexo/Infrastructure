using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UniRx;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Origine
{
    internal sealed class UIManager : GameModule, IUIManager
    {
        public IReadOnlyDictionary<Type, GameObject> Prefabs => prefabs;
        public IReadOnlyList<BaseUI> Windows => uiWindows;
        public GameObject Root { get; private set; }
        public GameContext Context { get; private set; }

        public event EventHandler<BaseUI> UIShowEvent, UICloseEvent;

        private readonly List<BaseUI> uiWindows = new List<BaseUI>();
        private readonly Dictionary<Type, GameObject> prefabs = new Dictionary<Type, GameObject>();

        public const int DepthBetweenParent = 2;

        public override int Priority => 0;

        private readonly IEventManager _eventManager;

        public UIManager(GameContext gameContext)
        {
            Context = gameContext;
            _eventManager = gameContext.GetModule<IEventManager>();
        }

        public IEnumerator InitializeAsync(string rootPath, string variant = null)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(rootPath);
            yield return handle;
            Root = GameObject.Instantiate(handle.Result);
            GameObject.DontDestroyOnLoad(Root);

            var waitables = new List<IObservable<Unit>>();
            foreach (var type in AssemblyCollection.GetTypes(t => t.IsSubclassOf(typeof(BaseUI)) && !t.IsAbstract))
                waitables.Add(LoadPrefabAsync(type, variant).ToObservable());

            yield return Observable.WhenAll(waitables).ToAwaitableEnumerator();
        }

        private IEnumerator LoadPrefabAsync(Type uiType, string variant)
        {
            var ui = (BaseUI)Activator.CreateInstance(uiType);
            if (!string.IsNullOrWhiteSpace(variant)) ui.AssetCategroyPrefix = $"{ui.AssetCategroyPrefix}/{variant}";
            var handle = Addressables.LoadAssetAsync<GameObject>(ui.AssetName);
            yield return handle;
            if (!handle.IsValid())
            {
                Debug.LogError($"Cannot found ui asset {ui.AssetName}");
                yield break;
            }
            handle.Result.SetActive(false);
            prefabs[uiType] = handle.Result;
        }

        public override void OnUpdate(float deltaTime)
        {
            for (int i = 0; i < uiWindows.Count; i++)
            {
                var window = uiWindows[i];
                if (window.IsVisible && !window.IsDestroyed)
                {
                    window.OnUpdate(deltaTime);
                }
            }
        }

        public override void OnDispose()
        {
            for (int i = 0; i < uiWindows.Count; i++)
                uiWindows[i].OnDestroy();

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

        public BaseUI GetOrCreate(Type type)
        {
            var window = uiWindows.FirstOrDefault(w => w.GetType() == type);

            if (window == null)
            {
                window = (BaseUI)Activator.CreateInstance(type);
                type
                    .GetField("_manager", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(window, this);
                uiWindows.Add(window);
            }

            if (!prefabs.ContainsKey(type))
            {
                Debug.LogError($"Cannot found prefab {type.FullName}!");
                return window;
            }

            if (!window.Self)
            {
                window.Initialize(Root, prefabs[type]);
                SortOrder();
            }


            return window;
        }

        public T GetOrCreate<T>() where T : BaseUI => (T)GetOrCreate(typeof(T));

        public BaseUI GetOrCreate(string name)
        {
            var ui = uiWindows.FirstOrDefault(u => u.GetType().Name == name);

            if (null != ui)
                return ui;

            Type tmpType = AssemblyCollection.GetType(name);
            if (null == tmpType)
                return ui;

            return GetOrCreate(tmpType);
        }

        public void Show<T>() where T : BaseUI => Show<T>(false);

        public T Show<T>(bool exclude = false) where T : BaseUI
        {
            var window = GetOrCreate<T>();
            ShowInternal(window, exclude);
            return window;
        }

        public void Toggle<T>() where T : BaseUI
        {
            var window = GetOrCreate<T>();
            window.SetVisible(!window.IsVisible);
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

        public BaseUI Show(string name, bool exclude = false)
        {
            var window = GetOrCreate(name);
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
            {
                initializer.Initialize(a);
                foreach (var group in window.Groups)
                    if (group is IInitializer<A> groupInitializer) groupInitializer.Initialize(a);
            }

            ShowInternal(window, exclude);
            return window;
        }

        public T Show<T, A, B>(A a, B b, bool exclude = false) where T : BaseUI
        {
            var window = GetOrCreate<T>();

            if (window is IInitializer<A, B> initializer)
            {
                initializer.Initialize(a, b);
                foreach (var group in window.Groups)
                    if (group is IInitializer<A, B> groupInitializer) groupInitializer.Initialize(a, b);
            }

            ShowInternal(window, exclude);
            return window;
        }

        public T Show<T, A, B, C>(A a, B b, C c, bool exclude = false) where T : BaseUI
        {
            var window = GetOrCreate<T>();

            if (window is IInitializer<A, B, C> initializer)
            {
                initializer.Initialize(a, b, c);
                foreach (var group in window.Groups)
                    if (group is IInitializer<A, B, C> groupInitializer) groupInitializer.Initialize(a, b, c);
            }

            ShowInternal(window, exclude);
            return window;
        }

        public T Show<T, A, B, C, D>(A a, B b, C c, D d, bool exclude = false) where T : BaseUI
        {
            var window = GetOrCreate<T>();

            if (window is IInitializer<A, B, C, D> initializer)
            {
                initializer.Initialize(a, b, c, d);
                foreach (var group in window.Groups)
                    if (group is IInitializer<A, B, C, D> groupInitializer) groupInitializer.Initialize(a, b, c, d);
            }

            ShowInternal(window, exclude);
            return window;
        }

        private void ShowInternal(BaseUI ui, bool exclude)
        {
            if (!ui.CanShow())
                return;
            if (exclude) Exclude(ui, w => w.Close());
            ui.Show();
            UIShowEvent?.Invoke(this, ui);
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
            var ui = Find<T>();
            ui?.Close();
            UICloseEvent?.Invoke(this, ui);
        }

        public void CloseExcept<T>(params T[] excepWindows) where T : BaseUI
        {
            var willCloseWindows = uiWindows.Except(excepWindows);
            foreach (var window in willCloseWindows)
                if (window.IsVisible) window.Close();
        }

        public void Destroy<T>() where T : BaseUI
        {
            var ui = Find<T>();
            ui?.OnDestroy();
        }
    }
}
