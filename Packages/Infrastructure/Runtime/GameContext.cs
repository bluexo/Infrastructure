//------------------------------------------------------------
//

//
//
//------------------------------------------------------------

using Esprima.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Origine
{
    /// <summary>
    /// 游戏上下文
    /// </summary>
    public partial class GameContext
    {
        private readonly List<GameModule> modules = new List<GameModule>();

        public void Initialize()
        {

        }

        /// <summary>
        /// 所有游戏框架模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(float deltaTime)
        {
            for (var i = 0; i < modules.Count; i++)
            {
                modules[i].OnUpdate(deltaTime);
            }
        }

        /// <summary>
        /// 关闭并清理所有游戏框架模块。
        /// </summary>
        public void Dispose()
        {
            for (var i = modules.Count - 1; i >= 0; i--)
            {
                modules[i].OnDispose();
            }

            modules.Clear();
            ReferencePool.ClearAll();
        }

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <typeparam name="T">要获取的游戏框架模块类型。</typeparam>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        public T GetModule<T>() where T : class => GetModule(typeof(T)) as T;

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要获取的游戏框架模块类型。</param>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        private GameModule GetModule(Type moduleType)
        {
            var module = modules.FirstOrDefault(m => m.GetType() == moduleType || m.GetType().GetInterface(moduleType.FullName) != null);
            return module ?? CreateModule(moduleType);
        }

        /// <summary>
        /// 创建游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要创建的游戏框架模块类型。</param>
        /// <returns>要创建的游戏框架模块。</returns>
        private GameModule CreateModule(Type moduleType)
        {
            Debug.Log($"Create module interface={moduleType.FullName}");

            var implType = moduleType.IsInterface
                ? AssemblyCollection.GetTypes()
                    .Where(t => t.GetInterfaces().Any(i => i == moduleType))
                    .FirstOrDefault()
                : moduleType;

            if (implType == null)
                throw new GameException($"Can not found implement type , interface='{moduleType.FullName}'.");

            var hasDefaultCtor = implType.GetConstructors().Any(c => c.GetParameters().Length == 0);

            var module = (GameModule)(hasDefaultCtor
                ? Activator.CreateInstance(implType)
                : Activator.CreateInstance(implType, this));

            if (module == null)
                throw new GameException($"Can not create module '{moduleType.FullName}'.");

            modules.Add(module);
            modules.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            return module;
        }
    }
}
