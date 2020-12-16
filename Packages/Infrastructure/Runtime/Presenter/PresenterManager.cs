using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Origine
{
    public sealed class PresenterManager : GameModule, IPresenterManager
    {
        private readonly List<PresenterBase> _controllers = new List<PresenterBase>();

        public override int Priority => 0;

        private readonly IEventManager _eventManager;

        public PresenterManager(GameContext gameContext)
        {
            _eventManager = gameContext.GetModule<IEventManager>();
        }

        public override void OnUpdate(float deltaTime)
        {
            for (int i = 0; i < _controllers.Count; i++)
            {
                _controllers[i].OnUpdate(deltaTime);
            }
        }

        public override void OnDispose()
        {
            for (int i = 0; i < _controllers.Count; i++)
            {
                _eventManager.UnregisterCommandHandler(_controllers[i]);
                _controllers[i].Reset();
            }

            _controllers.Clear();
        }

        public T Get<T>() where T : PresenterBase => (T)Get(typeof(T));

        public PresenterBase Get(Type type)
        {
            var controller = _controllers.FirstOrDefault(c => c.GetType() == type);

            if (controller == null)
            {
                controller = (PresenterBase)Activator.CreateInstance(type);
                if (controller == null)
                    throw new InvalidCastException($"{type.Name}->{nameof(PresenterBase)}");
                _controllers.Add(controller);
                _eventManager.RegisterCommandHandler(controller);
            }

            if (!controller.Initialized)
                controller.Initialize();

            return controller;
        }

        public PresenterBase GetByName(string typeName)
        {
            var type = Utility.AssemblyCollection.GetType(typeName);
            return Get(type);
        }

        public void Reset()
        {
            for (int i = 0, max = _controllers.Count; i < max; ++i)
            {
                _controllers[i].Reset();
            }
        }
    }
}
