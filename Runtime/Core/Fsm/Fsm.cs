﻿using System;

namespace Origine.Fsm
{
    /// <summary>
    /// 有限状态机基类。
    /// </summary>
    public abstract class Fsm
    {
        private string m_Name;

        /// <summary>
        /// 初始化有限状态机基类的新实例。
        /// </summary>
        public Fsm() { m_Name = string.Empty; }

        /// <summary>
        /// 获取有限状态机名称。
        /// </summary>
        public string Name
        {
            get => m_Name;
            protected set => m_Name = value ?? string.Empty;
        }

        /// <summary>
        /// 获取有限状态机完整名称。
        /// </summary>
        public string FullName => new TypeNamePair(OwnerType, m_Name).ToString();

        /// <summary>
        /// 获取有限状态机持有者类型。
        /// </summary>
        public abstract Type OwnerType { get; }

        /// <summary>
        /// 获取有限状态机中状态的数量。
        /// </summary>
        public abstract int FsmStateCount { get; }

        /// <summary>
        /// 获取有限状态机是否正在运行。
        /// </summary>
        public abstract bool IsRunning { get; }

        /// <summary>
        /// 获取有限状态机是否被销毁。
        /// </summary>
        public abstract bool IsDestroyed { get; }

        /// <summary>
        /// 获取当前有限状态机状态名称。
        /// </summary>
        public abstract string CurrentStateName { get; }

        /// <summary>
        /// 获取当前有限状态机状态持续时间。
        /// </summary>
        public abstract float CurrentStateTime { get; }

        /// <summary>
        /// 有限状态机轮询。
        /// </summary>
        internal abstract void Update(float deltaTime);

        /// <summary>
        /// 关闭并清理有限状态机。
        /// </summary>
        internal abstract void Shutdown();
    }
}
