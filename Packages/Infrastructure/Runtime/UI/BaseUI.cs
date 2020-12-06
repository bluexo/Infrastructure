using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Origine
{
    public abstract partial class BaseUI : GameEntity
    {
        public GameObject Parent { get; protected set; }
        public IReadOnlyList<BaseUI> Childs => _childWindows;

        protected BaseUI _parentWindow;
        protected GameObject _prefab;
        protected IUIManager _manager;
        protected readonly List<BaseUI> _childWindows = new List<BaseUI>();
        protected HashSet<BaseUI> _groupWindows = new HashSet<BaseUI>();

        public bool IsVisible => Self && Self.activeSelf;
        public bool IsDestroyed { get; protected set; }
        public int Order { get; protected set; }

        public virtual string AssetCategroyPrefix { get; protected set; } = "UI";
        public virtual string AssetName { get; protected set; }

        protected BaseUI()
        {
            AssetName = $"{AssetCategroyPrefix}/{GetType().Name}.prefab";
        }

        protected virtual void InitializeChildWindow()
        {
        }

        protected virtual void AfterInit()
        {
        }

        protected virtual void ConfigureGroup(ICollection<BaseUI> groupWindows)
        {
        }

        public bool IsInGroup(BaseUI window) => _groupWindows.Contains(window);

        protected virtual void AfterShow() => AfterShowComponent();

        protected virtual void BeforeClose() => BeforeCloseComponent();

        protected virtual void BeforeDestory() => BeforeDestoryComponent();

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            for (int i = 0; i < _childWindows.Count; i++)
            {
                var child = _childWindows[i];
                if (child.IsVisible && !child.IsDestroyed)
                    child.Update(deltaTime);
            }
        }

        public void SetVisible(bool visible)
        {
            if (visible) Show();
            else Close();
        }

        public void Initialize(GameObject parent, GameObject prefab)
        {
            Parent = parent;
            _prefab = prefab;
            Create();
        }

        private void Create()
        {
            Self = GameObject.Instantiate(_prefab);
            Self.name = GetType().Name;
            Self.transform.SetParent(Parent.transform, false);
            Self.SetActive(false);

            var parentCanvas = Self.GetComponent<Canvas>();
            var canvases = Self.GetComponentsInChildren<Canvas>(true);
            foreach (var canvas in canvases)
            {
                if (!canvas.overrideSorting || canvas.gameObject == Self) continue;
                canvas.sortingOrder += parentCanvas.sortingOrder;
            }
            CollectAutoReferences(Self);
            ConfigureGroup(_groupWindows);
            AfterInit();
            InitializeChildWindow();
        }

        public virtual void Show()
        {
            if (_prefab && !Self)
                Create();

            if (Self)
            {
                if (Self.activeSelf) return;
                Self.SetActive(true);
            }

            AfterShow();

            foreach (var ui in _groupWindows)
                ui.Show();

            AfterShowGroup();
        }

        protected virtual void AfterShowGroup()
        {
        }

        public void Close()
        {
            if (!IsVisible)
                return;

            foreach (var window in _childWindows)
                window.Close();

            foreach (var window in _groupWindows)
                window.Close();

            BeforeClose();

            if (Self)
                Self.SetActive(false);
        }

        public override void Destroy()
        {
            BeforeDestory();
            base.Destroy();
            IsDestroyed = true;

            if (Self)
            {
                GameObject.Destroy(Self);
                Self = null;
            }
        }

        public T GetChildWindow<T>() where T : BaseUI
        {
            for (int i = 0, max = _childWindows.Count; i < max; ++i)
            {
                BaseUI tmpChildWindow = _childWindows[i];

                if (null != tmpChildWindow && tmpChildWindow.GetType() == typeof(T))
                    return tmpChildWindow as T;
            }

            return null;
        }

        public BaseUI GetChildWindow(Type type)
        {
            for (int i = 0, max = _childWindows.Count; i < max; ++i)
            {
                BaseUI tmpChildWindow = _childWindows[i];

                if (null != tmpChildWindow && tmpChildWindow.GetType() == type)
                    return tmpChildWindow;
            }

            return null;
        }

        public BaseUI GetChildWindowByName(string name)
        {
            for (int i = 0, max = _childWindows.Count; i < max; ++i)
            {
                BaseUI tmpChildWindow = _childWindows[i];

                if (tmpChildWindow != null && tmpChildWindow.AssetName.Equals(name))
                {
                    return tmpChildWindow;
                }
            }

            return null;
        }

        public bool HasChild(BaseUI child) => _childWindows.Contains(child);

        public virtual void AddChildWindow<TChild>(TChild child, GameObject prefab, GameObject parentGo = null) where TChild : BaseUI
        {
            child._parentWindow = this;
            child.Initialize(parentGo ? parentGo : Self, prefab);
            _childWindows.Add(child);
        }

        public T Find<T>(string name) where T : Component => Utility.Find<T>(Self, name);

        public T FindByTitleCase<T>(string name) where T : Component => Utility.Find<T>(Self, name.TitleCase());

        protected static T Find<T>(GameObject go, string name) where T : Component => Utility.Find<T>(go, name);

        protected static T Find<T>(Component comp, string name) where T : Component => Utility.Find<T>(comp.gameObject, name);

        protected static T FindByTitleCase<T>(GameObject go, string name) where T : Component => Utility.Find<T>(go, name.TitleCase());

        protected static T FindByTitleCase<T>(Component comp, string name) where T : Component => Utility.Find<T>(comp.gameObject, name.TitleCase());

        public GameObject Find(string name) => Utility.Find(Self, name);

        public GameObject FindByTitleCase(string name) => Utility.Find(Self, name.TitleCase());

        protected static GameObject Find(GameObject go, string name) => Utility.Find(go, name);

        protected static GameObject Find(Component comp, string name) => Utility.Find(comp.gameObject, name);

        protected static GameObject FindByTitleCase(GameObject go, string name) => Utility.Find(go, name.TitleCase());

        protected static GameObject FindByTitleCase(Component comp, string name) => Utility.Find(comp.gameObject, name.TitleCase());

        public static void SetActive(GameObject go, bool active) => Utility.SetActive(go, active);

        public static void SetActive<T>(T instance, bool active) where T : Component => Utility.SetActive(instance, active);

        protected static void RegisterEventClick(GameObject go, Action<PointerEventData> handle)
        {
            if (null == go || null == handle)
                return;

            PointerEventListener.GetOrAdd(go).onClick = handle;
        }

        protected static void RegisterEventClick(Component comp, Action<PointerEventData> handle)
        {
            if (null == comp || null == handle)
                return;

            PointerEventListener.GetOrAdd(comp.gameObject).onClick = handle;
        }

        protected static void RegisterEventClickDown(GameObject go, Action<PointerEventData> handle)
        {
            if (null == go || null == handle)
                return;

            PointerEventListener.GetOrAdd(go).onDown = handle;
        }

        protected static void RegisterEventClickUp(GameObject go, Action<PointerEventData> handle)
        {
            if (null == go || null == handle)
                return;

            PointerEventListener.GetOrAdd(go).onUp = handle;
        }

        protected static void RegisterEventClickEnter(GameObject go, Action<PointerEventData> handle)
        {
            if (null == go || null == handle)
                return;

            PointerEventListener.GetOrAdd(go).onEnter = handle;
        }

        protected static void RegisterEventClickExit(GameObject go, Action<PointerEventData> handle)
        {
            if (null == go || null == handle)
                return;

            PointerEventListener.GetOrAdd(go).onExit = handle;
        }
    }
}