using System;
using System.Collections.Generic;
using System.Reflection;

namespace Origine
{
    /// <summary>
    /// 事件管理器。
    /// </summary>
    internal sealed class EventManager : GameModule, IEventManager
    {
        private readonly EventPool<GameEventArgs> _eventPool;
        private readonly Dictionary<string, List<Action<CommandEventArgs>>> commands = new Dictionary<string, List<Action<CommandEventArgs>>>();

        /// <summary>
        /// 初始化事件管理器的新实例。
        /// </summary>
        public EventManager()
        {
            _eventPool = new EventPool<GameEventArgs>(EventPoolMode.AllowNoHandler | EventPoolMode.AllowMultiHandler);
            _eventPool.Subscribe(CommandEventArgs.EventId, OnCommandHandler);
        }

        /// <summary>
        /// 获取事件处理函数的数量。
        /// </summary>
        public int EventHandlerCount => _eventPool.EventHandlerCount;

        /// <summary>
        /// 获取事件数量。
        /// </summary>
        public int EventCount => _eventPool.EventCount;

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public override int Priority => 100;

        /// <summary>
        /// 事件管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public override void OnUpdate(float deltaTime) => _eventPool.Update(deltaTime);

        /// <summary>
        /// 关闭并清理事件管理器。
        /// </summary>
        public override void OnDispose() => _eventPool.Shutdown();

        /// <summary>
        /// 获取事件处理函数的数量。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <returns>事件处理函数的数量。</returns>
        public int Count(int id) => _eventPool.Count(id);

        /// <summary>
        /// 检查是否存在事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要检查的事件处理函数。</param>
        /// <returns>是否存在事件处理函数。</returns>
        public bool Contains(int id, EventHandler<GameEventArgs> handler) => _eventPool.Check(id, handler);

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        public void Subscribe(int id, EventHandler<GameEventArgs> handler) => _eventPool.Subscribe(id, handler);

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler) => _eventPool.Unsubscribe(id, handler);

        /// <summary>
        /// 设置默认事件处理函数。
        /// </summary>
        /// <param name="handler">要设置的默认事件处理函数。</param>
        public void SetDefaultHandler(EventHandler<GameEventArgs> handler) => _eventPool.SetDefaultHandler(handler);

        /// <summary>
        /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        public void Publish(object sender, GameEventArgs e) => _eventPool.Fire(sender, e);

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        public void PublishImmediately(object sender, GameEventArgs e) => _eventPool.FireNow(sender, e);

        private void OnCommandHandler(object sender, GameEventArgs e)
        {
            var args = e as CommandEventArgs;
            if (!commands.ContainsKey(args.Command)) return;
            for (var i = 0; i < commands[args.Command].Count; i++)
            {
                var action = commands[args.Command][i];
                action.Invoke(args);
            }
        }

        public void RegisterCommandHandler(object target)
        {
            var type = target.GetType();
            foreach (var method in type.GetMethods(BindingFlags.Public
                   | BindingFlags.NonPublic
                   | BindingFlags.Instance
                   | BindingFlags.Static
                   | BindingFlags.InvokeMethod))
            {
                if (!method.TryGetAttribute(out CommandHandler handler)) continue;

                if (string.IsNullOrWhiteSpace(handler.Command))
                    handler.Command = method.Name;

                if (!commands.ContainsKey(handler.Command))
                    commands.Add(handler.Command, new List<Action<CommandEventArgs>>());

                var action = (Action<CommandEventArgs>)method.CreateDelegate(typeof(Action<CommandEventArgs>), target);
                commands[handler.Command].Add(action);
            }
        }

        public void UnregisterCommandHandler(object target)
        {
            var type = target.GetType();

            foreach (var method in type.GetMethods(BindingFlags.Public
                 | BindingFlags.NonPublic
                 | BindingFlags.Instance
                 | BindingFlags.Static
                 | BindingFlags.InvokeMethod))
            {
                if (!method.TryGetAttribute(out CommandHandler handler)) continue;

                if (string.IsNullOrWhiteSpace(handler.Command))
                    handler.Command = method.Name;

                if (!commands.ContainsKey(handler.Command)) continue;

                var dels = commands[handler.Command];
                dels.RemoveAll(del => del.Target == target);
            }
        }
    }
}
