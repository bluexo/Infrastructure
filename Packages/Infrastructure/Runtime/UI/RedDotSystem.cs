using Origine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

    /// <summary>
    /// 红点事件参数 
    /// </summary>
    public class RedDotEventArgs : GameEventArgs<RedDotEventArgs>
    {
        public string Name { get; set; }
        public bool Clear { get; set; }
        public int? Count { get; set; }
    }

    public interface IRedDotSystem
    {
        IEnumerator InitializeAsync(string assetId);
        void RegisterHandler(object handler);
        void UnregisterHandler(object handler);

        void AddRedDot(string eventName, Graphic graphic);
        void RemoveRedDot(Graphic graphic);
        void Clear(string eventName);
    }

    public class RedDotSystem : GameModule, IRedDotSystem
    {
        private readonly Dictionary<Type, HashSet<Graphic>> handlersDict = new Dictionary<Type, HashSet<Graphic>>();
        private readonly Dictionary<string, HashSet<Graphic>> eventsDict = new Dictionary<string, HashSet<Graphic>>();
        private readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
        private readonly GameContext _gameContext;
        private GameObject prefab;

        public RedDotSystem(GameContext gameContext)
        {
            _gameContext = gameContext;
        }

        public IEnumerator InitializeAsync(string assetId)
        {
            var evtMgr = _gameContext.GetModule<IEventManager>();
            evtMgr.Subscribe(RedDotEventArgs.EventId, OnRedDotEvent);
            var handle = Addressables.LoadAssetAsync<GameObject>(assetId);
            yield return handle;
            prefab = handle.Result;
        }

        private void OnRedDotEvent(object sender, GameEventArgs e)
        {
            var args = e as RedDotEventArgs;
            if (!eventsDict.ContainsKey(args.Name))
                return;

            foreach (var objRef in eventsDict[args.Name])
            {
                var redDot = objRef.gameObject.Find(prefab.name);
                redDot.SetActive(true);
                PointerEventListener.GetOrAdd(objRef.gameObject).onClick = _ => Clear(args.Name);

                if (!args.Count.HasValue) continue;
                var textComp = redDot.GetComponentInChildren<Text>();

                if (!textComp) continue;
                textComp.text = args.Count.HasValue ? args.Count.ToString() : string.Empty;
            }
        }

        public void RegisterHandler(object objRef)
        {
            var fields = objRef.GetType().GetFields(bindingFlags)
                .Select(f => (f, attr: f.GetCustomAttribute<RedDotAttribute>()))
                .Where(v => v.attr != null)
                .ToList();

            foreach (var (f, a) in fields)
            {
                if (!(f.GetValue(objRef) is Component component))
                    continue;

                var maskable = component.GetComponentInChildren<MaskableGraphic>();
                if (!maskable)
                    continue;

                AddRedDot(a.Name, maskable);
                var type = objRef.GetType();
                if (!handlersDict.ContainsKey(type))
                    handlersDict.Add(type, new HashSet<Graphic>());
                handlersDict[type].Add(maskable);
            }
        }

        public void UnregisterHandler(object objRef)
        {
            if (!handlersDict.ContainsKey(objRef.GetType())) return;
            var handler = handlersDict[objRef.GetType()];

            foreach (var item in handler)
            {
                RemoveRedDot(item);
            }
        }

        /// <summary>
        /// 清理事件
        /// </summary>
        /// <param name="eventName"></param>
        public void Clear(string eventName)
        {
            if (!eventsDict.ContainsKey(eventName)) return;
            var refs = eventsDict[eventName];
            foreach (var r in refs) GameObject.Destroy(r.gameObject.Find(prefab.name));
            refs.Clear();
        }

        /// <summary>
        /// 添加红点
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="graphic"></param>
        public void AddRedDot(string eventName, Graphic graphic)
        {
            if (!eventsDict.ContainsKey(eventName))
                eventsDict[eventName] = new HashSet<Graphic>();
            eventsDict[eventName].Add(graphic);
            var redDot = GameObject.Instantiate(prefab, graphic.transform);
            redDot.SetActive(false);
            redDot.name = prefab.name;
            redDot.transform.SetParent(graphic.transform);
        }

        /// <summary>
        /// 移除红点
        /// </summary>
        /// <param name="graphic"></param>
        public void RemoveRedDot(Graphic graphic)
        {
            foreach (var pair in eventsDict)
            {
                if (!pair.Value.Contains(graphic)) continue;
                pair.Value.Remove(graphic);
                GameObject.Destroy(graphic.gameObject.Find(prefab.name));
                break;
            }
        }
    }
}
