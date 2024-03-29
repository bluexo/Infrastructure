﻿using System;

namespace Origine
{
    /// <summary>
    /// 事件管理器接口。
    /// </summary>
    public interface IEventManager
    {
        /// <summary>
        /// 获取事件处理函数的数量。
        /// </summary>
        int EventHandlerCount { get; }

        /// <summary>
        /// 获取事件数量。
        /// </summary>
        int EventCount { get; }

        /// <summary>
        /// 获取事件处理函数的数量。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <returns>事件处理函数的数量。</returns>
        int Count(int id);

        /// <summary>
        /// 检查是否存在事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要检查的事件处理函数。</param>
        /// <returns>是否存在事件处理函数。</returns>
        bool Contains(int id, EventHandler<GameEventArgs> handler);

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        void Subscribe(int id, EventHandler<GameEventArgs> handler);

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        void Unsubscribe(int id, EventHandler<GameEventArgs> handler);

        /// <summary>
        /// 设置默认事件处理函数。
        /// </summary>
        /// <param name="handler">要设置的默认事件处理函数。</param>
        void SetDefaultHandler(EventHandler<GameEventArgs> handler);

        /// <summary>
        /// 发布简单事件
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="e"></param>
        void Publish<TEventArgs>(TEventArgs e = default) where TEventArgs : GameEventArgs, new();
        void PublishImmediately<TEventArgs>(TEventArgs e = default) where TEventArgs : GameEventArgs, new();

        /// <summary>
        /// 发布事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        void Publish(object sender, GameEventArgs e);

        /// <summary>
        /// 立即发布事件，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        void PublishImmediately(object sender, GameEventArgs e);

        /// <summary>
        /// 注册命令事件处理
        /// </summary>
        /// <param name="obj"></param>
        void RegisterCommandHandler(object obj);

        /// <summary>
        /// 注销命令事件处理
        /// </summary>
        /// <param name="obj"></param>
        void UnregisterCommandHandler(object obj);
    }
}