using Origine.Fsm;

using System;
using System.Linq;

namespace Origine
{
    /// <summary>
    /// 流程管理器。
    /// </summary>
    internal sealed class StageManager : GameModule, IStageManager
    {
        private IFsmManager _fsmManager;
        private IFsm<IStageManager> _stageFsm;

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public override int Priority => -10;

        /// <summary>
        /// 初始化流程管理器。
        /// </summary>
        /// <param name="fsmManager">有限状态机管理器。</param>
        /// <param name="gameStages">流程管理器包含的流程。</param>
        public StageManager(GameContext context)
        {
            var fsmManager = context.GetModule<IFsmManager>();
            if (fsmManager == null)
                throw new GameException("FSM manager is invalid.");

            _fsmManager = fsmManager;

            var stages = Utility.AssemblyCollection
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(StageBase)))
                .Select(t => (StageBase)Activator.CreateInstance(t))
                .ToArray();

            _stageFsm = _fsmManager.CreateFsm(this, stages);
        }

        /// <summary>
        /// 获取当前流程。
        /// </summary>
        public StageBase Current
        {
            get
            {
                if (_stageFsm == null)
                    throw new GameException("You must initialize procedure first.");

                return (StageBase)_stageFsm.CurrentState;
            }
        }

        /// <summary>
        /// 获取当前流程持续时间。
        /// </summary>
        public float Duration
        {
            get
            {
                if (_stageFsm == null)
                    throw new GameException("You must initialize procedure first.");

                return _stageFsm.CurrentStateTime;
            }
        }

        /// <summary>
        /// 关闭并清理流程管理器。
        /// </summary>
        public override void OnDispose()
        {
            if (_fsmManager != null)
            {
                if (_stageFsm != null)
                {
                    _fsmManager.DestroyFsm(_stageFsm);
                    _stageFsm = null;
                }

                _fsmManager = null;
            }
        }

        /// <summary>
        /// 开始流程。
        /// </summary>
        /// <typeparam name="T">要开始的流程类型。</typeparam>
        public void Start<T>() where T : StageBase
        {
            if (_stageFsm == null)
                throw new GameException("You must initialize procedure first.");

            _stageFsm.Start<T>();
        }

        /// <summary>
        /// 开始流程。
        /// </summary>
        /// <param name="procedureType">要开始的流程类型。</param>
        public void Start(Type procedureType)
        {
            if (_stageFsm == null)
                throw new GameException("You must initialize procedure first.");

            _stageFsm.Start(procedureType);
        }

        /// <summary>
        /// 是否存在流程。
        /// </summary>
        /// <typeparam name="T">要检查的流程类型。</typeparam>
        /// <returns>是否存在流程。</returns>
        public bool Exists<T>() where T : StageBase
        {
            if (_stageFsm == null)
                throw new GameException("You must initialize procedure first.");

            return _stageFsm.HasState<T>();
        }

        /// <summary>
        /// 是否存在流程。
        /// </summary>
        /// <param name="procedureType">要检查的流程类型。</param>
        /// <returns>是否存在流程。</returns>
        public bool Exists(Type procedureType)
        {
            if (_stageFsm == null)
                throw new GameException("You must initialize procedure first.");

            return _stageFsm.HasState(procedureType);
        }

        /// <summary>
        /// 获取流程。
        /// </summary>
        /// <typeparam name="T">要获取的流程类型。</typeparam>
        /// <returns>要获取的流程。</returns>
        public T Get<T>() where T : StageBase
        {
            if (_stageFsm == null)
                throw new GameException("You must initialize procedure first.");

            return _stageFsm.GetState<T>();
        }

        /// <summary>
        /// 获取流程。
        /// </summary>
        /// <param name="procedureType">要获取的流程类型。</param>
        /// <returns>要获取的流程。</returns>
        public StageBase Get(Type procedureType)
        {
            if (_stageFsm == null)
                throw new GameException("You must initialize procedure first.");

            return (StageBase)_stageFsm.GetState(procedureType);
        }

        public void Switch<T>() where T : StageBase => Current?.ChangeState<T>();

        public void Switch(Type type) => Current?.ChangeState(type);

        public void Switch<T, T1>(T1 t1) where T : StageBase, IInitializer<T1>
        {
            var stage = Get<T>();
            if (stage is IInitializer<T1> initializer) initializer.Initialize(t1);
            Switch<T>();
        }

        public void Switch<T, T1, T2>(T1 t1, T2 t2) where T : StageBase, IInitializer<T1, T2>
        {
            var stage = Get<T>();
            if (stage is IInitializer<T1, T2> initializer) initializer.Initialize(t1, t2);
            Switch<T>();
        }
    }
}
