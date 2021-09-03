
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UniRx;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Component = UnityEngine.Component;

namespace Origine
{
    /// <summary>
    /// 红点字段特性
    /// </summary>
    public class RedDotAttribute : Attribute
    {
        public string Name { get; set; }
        public RedDotAttribute() { }
        public RedDotAttribute(string name) => Name = name;
    }

    public class RedDotSystem : GameModule, IRedDotSystem
    {
        private readonly Dictionary<Type, HashSet<GameObject>> reigsterHandlers = new Dictionary<Type, HashSet<GameObject>>();
        private readonly Dictionary<string, HashSet<GameObject>> eventsDicts = new Dictionary<string, HashSet<GameObject>>();
        private readonly Dictionary<Func<bool>, string> eventTriggers = new Dictionary<Func<bool>, string>();
        private readonly Dictionary<string, int> cachedEvents = new Dictionary<string, int>();

        private readonly GameContext _gameContext;
        private readonly IEventManager _eventManager;
        private CompositeDisposable disposables = new CompositeDisposable();

        private GameObject prefab;

        public RedDotSystem(GameContext gameContext)
        {
            _gameContext = gameContext;
            _eventManager = gameContext.GetModule<IEventManager>();
        }

        public IEnumerator InitializeAsync(string assetId)
        {
            _eventManager.Subscribe(RedDotEventArgs.EventId, OnRedDotEvent);
            var handle = Addressables.LoadAssetAsync<GameObject>(assetId);
            yield return handle;
            prefab = handle.Result;
        }

        private void OnRedDotEvent(object sender, GameEventArgs e)
        {
            var args = e as RedDotEventArgs;
            cachedEvents[args.Name] = args.Count;

            if (!eventsDicts.ContainsKey(args.Name))
                eventsDicts[args.Name] = new HashSet<GameObject>();

            foreach (var objRef in eventsDicts[args.Name])
                ConfigureRedDot(objRef, args.Name, args.Count, args.ClickClear);
        }

        private void ConfigureRedDot(GameObject objRef, string name, int count, bool clickClear = false)
        {
            var redDot = objRef.GetComponentInChildren<RedDotRef>(true);
            if (!redDot) return;

            if (!redDot.Events.Contains(name))
                redDot.Events.Add(name);
            redDot.SetActive(true);

            if (clickClear)
            {
                var pointer = PointerEventListener.GetOrAdd(objRef.gameObject);
                pointer.onClick += _ =>
                {
                    var invocation = pointer.onClick.GetInvocationList().FirstOrDefault(d => d.Method.ReflectedType.DeclaringType == typeof(RedDotSystem));
                    if (invocation == null) return;
                    ClearEvent(name);
                    pointer.onClick -= (Action<PointerEventData>)invocation;
                };
            }

            var textComp = redDot.GetComponentInChildren<Text>();
            if (!textComp) return;

            textComp.text = count > 0 ? count.ToString() : string.Empty;
        }

        public void RegisterHandler(object objRef)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

            var fields = objRef.GetType().GetFields(bindingFlags)
                    .Select(f => (f, attr: f.GetCustomAttribute<RedDotAttribute>()))
                    .Where(v => v.attr != null)
                    .ToList();

            foreach (var (f, a) in fields)
            {
                if (!(f.GetValue(objRef) is GameObject go))
                    continue;

                if (!go)
                    continue;

                AddRedDot(go, a.Name);
                var type = objRef.GetType();
                if (!reigsterHandlers.ContainsKey(type))
                    reigsterHandlers.Add(type, new HashSet<GameObject>());
                reigsterHandlers[type].Add(go);
            }
        }

        public void UnregisterHandler(object objRef)
        {
            if (!reigsterHandlers.ContainsKey(objRef.GetType()))
                return;

            var handler = reigsterHandlers[objRef.GetType()];
            foreach (var item in handler)
                RemoveRedDot(item);
        }

        /// <summary>
        /// 清理事件
        /// </summary>
        /// <param name="eventName"></param>
        public void ClearEvent(string eventName)
        {
            if (cachedEvents.ContainsKey(eventName))
                cachedEvents.Remove(eventName);

            if (!eventsDicts.ContainsKey(eventName))
                return;

            foreach (var r in eventsDicts[eventName])
            {
                var rd = r.GetComponentInChildren<RedDotRef>(true);
                if (!rd)
                    continue;

                rd.Events.Remove(eventName);
                if (rd.Events.Count > 0)
                    continue;

                rd.gameObject.SetActive(false);
            }
        }

        public void AddRedDot<T>(T graphic, string eventName) where T : Component
            => AddRedDot(graphic.gameObject, eventName);

        public void RemoveRedDot<T>(T graphic) where T : Component
            => RemoveRedDot(graphic.gameObject);

        /// <summary>
        /// 添加红点
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="graphic"></param>
        public void AddRedDot(GameObject graphic, string eventName)
        {
            if (!eventsDicts.ContainsKey(eventName))
                eventsDicts[eventName] = new HashSet<GameObject>();
            eventsDicts[eventName].Add(graphic);

            var rd = graphic.GetComponentInChildren<RedDotRef>(true);
            if (!rd)
            {
                var redDot = GameObject.Instantiate(prefab, graphic.transform);
                redDot.SetActive(false);
                redDot.name = prefab.name;
                rd = redDot.GetComponent<RedDotRef>();
                if (!rd) rd = redDot.AddComponent<RedDotRef>();
                redDot.transform.SetParent(graphic.transform);
            }

            /*
            if (!rd.Events.Contains(eventName))
                rd.Events.Add(eventName);
            */
            if (!cachedEvents.ContainsKey(eventName))
                return;

            var count = cachedEvents[eventName];
            ConfigureRedDot(graphic, eventName, count);
        }

        /// <summary>
        /// 移除红点
        /// </summary>
        /// <param name="graphic"></param>
        public void RemoveRedDot(GameObject graphic)
        {
            foreach (var pair in eventsDicts)
            {
                if (!pair.Value.Contains(graphic)) continue;
                pair.Value.Remove(graphic);
                var rd = graphic.GetComponentInChildren<RedDotRef>();
                if (rd)
                {
                    rd.SetActive(false);
                    rd.Events = new List<string>();
                }
                break;
            }
        }

        public void PublishEvent(string eventName, int count = 0, bool clickClear = false)
        {
            var args = ReferencePool.Take<RedDotEventArgs>();
            args.Name = eventName;
            args.Count = count;
            args.ClickClear = clickClear;
            _eventManager.Publish(this, args);
        }

        public void HandleEvent(bool condition, string eventName, int count = 0)
        {
            if (condition) PublishEvent(eventName, count);
            else ClearEvent(eventName);
        }

        public IDisposable ObserveValue<TSource, TProperty>(TSource source,
            Func<TSource, TProperty> propertySelector,
            string eventName,
            Func<TProperty, bool> nextAction) where TSource : class
        {
            var disposeable = ObserveExtensions
                 .ObserveEveryValueChanged(source, propertySelector)
                 .Sample(TimeSpan.FromSeconds(1))
                 .Subscribe(p => HandleEvent(nextAction.Invoke(p), eventName)).AddTo(disposables);

            return disposeable;
        }

        public void ClearObserveable()
        {
            disposables.Dispose();
            disposables = new CompositeDisposable();
        }
    }
}
