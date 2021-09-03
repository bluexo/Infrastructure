using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace Origine
{
    public sealed class PresenterManager : GameModule, IPresenterManager
    {
        private readonly Dictionary<Type, PresenterBase> presenterDict = new Dictionary<Type, PresenterBase>();
        private readonly List<PresenterBase> presenters = new List<PresenterBase>();
        private readonly Dictionary<string, Type> typeCaches = new Dictionary<string, Type>();
        public override int Priority => 0;

        private readonly IEventManager _eventManager;

        public PresenterManager(GameContext gameContext)
        {
            _eventManager = gameContext.GetModule<IEventManager>();
            var types = AssemblyCollection.GetTypes(t => t.IsSubclassOf(typeof(PresenterBase)) && !t.IsAbstract);
            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<AliasAttribute>();
                var name = attr?.Name ?? type.Name;
                typeCaches[name] = type;
            }
        }

        public void Initialize()
        {
            foreach (var type in typeCaches.Values)
            {
                try
                {
                    if (presenterDict.ContainsKey(type)) 
                        continue;
                    var pster = Create(type);
                    if (pster.Initialized) continue;
                    pster.Initialize();
                    Debug.Log($"Initialize presenter {pster.GetType().Name}");
                }
                catch(Exception exc)
                {
                    Debug.LogError(exc);
                    continue;
                }
            }
        }

        private PresenterBase Create(Type type)
        {
            var pster = (PresenterBase)Activator.CreateInstance(type);
            if (pster == null)
                throw new InvalidCastException($"{type.Name}->{nameof(PresenterBase)}");
            if (!presenters.Contains(pster))
                presenters.Add(pster);
            if (!presenterDict.ContainsKey(type))
                presenterDict.Add(type, pster);
            _eventManager.RegisterCommandHandler(pster);
            return pster;
        }

        public override void OnUpdate(float deltaTime)
        {
            for (int i = 0; i < presenters.Count; i++)
            {
                if (!presenters[i].Initialized) continue;
                presenters[i].OnUpdate(deltaTime);
            }
        }

        public override void OnDispose()
        {
            for (int i = 0; i < presenters.Count; i++)
            {
                _eventManager.UnregisterCommandHandler(presenters[i]);
                presenters[i].Reset();
            }

            presenters.Clear();
        }

        public T Get<T>() where T : PresenterBase => (T)Get(typeof(T));

        public PresenterBase Get(Type type)
        {
            var pster = presenterDict.ContainsKey(type) 
                    ? presenterDict[type]
                    : Create(type);

            if (!pster.Initialized)
                pster.Initialize();

            return pster;
        }

        public PresenterBase Get(string typeName)
        {
            var type = typeCaches.ContainsKey(typeName)
                ? typeCaches[typeName]
                : AssemblyCollection.GetType(typeName);
            return Get(type);
        }

        public void Reset()
        {
            for (int i = 0, max = presenters.Count; i < max; ++i)
            {
                presenters[i].Reset();
            }
        }
    }
}
