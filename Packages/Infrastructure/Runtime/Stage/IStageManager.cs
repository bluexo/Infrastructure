
using System;

namespace Origine
{
    /// <summary>
    /// 流程管理器接口。
    /// </summary>
    public interface IStageManager
    {
        /// <summary>
        /// 获取当前流程。
        /// </summary>
        StageBase Current { get; }

        /// <summary>
        /// 获取当前流程持续时间。
        /// </summary>
        float Duration { get; }

        /// <summary>
        /// 开始流程。
        /// </summary>
        /// <typeparam name="T">要开始的流程类型。</typeparam>
        void Start<T>() where T : StageBase;

        /// <summary>
        /// 开始流程。
        /// </summary>
        /// <param name="procedureType">要开始的流程类型。</param>
        void Start(Type procedureType);

        /// <summary>
        /// 是否存在流程。
        /// </summary>
        /// <typeparam name="T">要检查的流程类型。</typeparam>
        /// <returns>是否存在流程。</returns>
        bool Exists<T>() where T : StageBase;

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void Switch<T>() where T : StageBase;

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void Switch<T, T1>(T1 t1) where T : StageBase, IInitializer<T1>;

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void Switch<T, T1, T2>(T1 t1, T2 t2) where T : StageBase, IInitializer<T1, T2>;

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <param name="type"></param>
        void Switch(Type type);

        /// <summary>
        /// 是否存在流程。
        /// </summary>
        /// <param name="procedureType">要检查的流程类型。</param>
        /// <returns>是否存在流程。</returns>
        bool Exists(Type procedureType);

        /// <summary>
        /// 获取流程。
        /// </summary>
        /// <typeparam name="T">要获取的流程类型。</typeparam>
        /// <returns>要获取的流程。</returns>
        T Get<T>() where T : StageBase;

        /// <summary>
        /// 获取流程。
        /// </summary>
        /// <param name="procedureType">要获取的流程类型。</param>
        /// <returns>要获取的流程。</returns>
        StageBase Get(Type procedureType);
    }
}
