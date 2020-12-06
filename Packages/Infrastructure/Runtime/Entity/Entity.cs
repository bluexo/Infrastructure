using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Origine
{
    public partial class Entity
    {
        protected readonly Dictionary<Type, EntityComponent> _componentDict = new Dictionary<Type, EntityComponent>();
        protected readonly List<EntityComponent> _componentList = new List<EntityComponent>();
        protected bool disposed = false;

        public T AddComponent<T>() where T : EntityComponent, new()
        {
            if (_componentDict.ContainsKey(typeof(T)))
            {
                UnityEngine.Debug.LogError($"AddComponent, component already exist, component: {typeof(T).Name}");
                return default;
            }

            var tmpComponent = ReferencePool.Take<T>();
            _componentList.Add(tmpComponent);
            _componentDict.Add(tmpComponent.GetType(), tmpComponent);

            if (tmpComponent is IInitializer initializer)
                initializer.Initialize();
            tmpComponent.OnInit(this);
            tmpComponent.Enabled = true;

            return tmpComponent;
        }

        public bool HasComponent<T>() => _componentDict.ContainsKey(typeof(T));

        public T AddComponent<T, A>(A a) where T : EntityComponent, new()
        {
            T tmpComponent = AddComponent<T>();
            if (tmpComponent is IInitializer<A> initializer)
                initializer.Initialize(a);

            return tmpComponent;
        }

        public T AddComponent<T, A, B>(A a, B b) where T : EntityComponent, new()
        {
            T tmpComponent = AddComponent<T>();
            if (tmpComponent is IInitializer<A, B> initializer)
                initializer.Initialize(a, b);
            return tmpComponent;
        }

        public T AddComponent<T, A, B, C>(A a, B b, C c) where T : EntityComponent, new()
        {
            T tmpComponent = AddComponent<T>();
            if (tmpComponent is IInitializer<A, B, C> initializer)
                initializer.Initialize(a, b, c);
            return tmpComponent;
        }

        public T AddComponent<T, A, B, C, D>(A a, B b, C c, D d) where T : EntityComponent, new()
        {
            T tmpComponent = AddComponent<T>();
            if (tmpComponent is IInitializer<A, B, C, D> initializer)
                initializer.Initialize(a, b, c, d);
            return tmpComponent;
        }

        public T GetComponent<T>() where T : EntityComponent
        {
            if (!_componentDict.TryGetValue(typeof(T), out EntityComponent tmpComponent))
                return default;

            return (T)tmpComponent;
        }

        public bool TryGetComponent<T>(out T comp) where T : EntityComponent
        {
            comp = GetComponent<T>();
            return comp != null;
        }

        public void RemoveComponent<T>() where T : EntityComponent
        {
            if (!_componentDict.TryGetValue(typeof(T), out EntityComponent tmpComponent))
                return;
            tmpComponent.Enabled = false;
            _componentDict.Remove(typeof(T));
            _componentList.Remove(tmpComponent);
            ReferencePool.Return(tmpComponent);
        }

        public virtual void Destroy()
        {
            foreach (var comp in _componentList)
            {
                comp.Enabled = false;
                ReferencePool.Return(comp);
            }

            foreach (var co in _coroutines.ToArray())
            {
                if (co == null) continue;
                StopCoroutine(co);
            }

            _componentList.Clear();
            _componentDict.Clear();
            disposed = true;
        }

        public virtual void Update(float deltaTime)
        {
            for (int i = 0; i < _componentList.Count; i++)
            {
                if (!_componentList[i].Enabled) continue;
                _componentList[i].OnUpdate(deltaTime);
            }
        }
    }
}
