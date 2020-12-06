using Origine.Fsm;

namespace Origine
{
    /// <summary>
    /// 流程基类。
    /// </summary>
    public abstract class StageBase : FsmState<IStageManager>
    {
        /// <summary>
        /// 状态初始化时调用，统一订阅状态转换事件
        /// </summary>
        /// <param name="owner">流程持有者。</param>
        public override void Init(IFsm<IStageManager> owner)
        {
            base.Init(owner);
        }

        /// <summary>
        /// 进入状态时调用。
        /// </summary>
        /// <param name="procedureOwner">流程持有者。</param>
        public override void Enter()
        {
            base.Enter();
        }

        /// <summary>
        /// 状态轮询时调用。
        /// </summary>
        /// <param name="procedureOwner">流程持有者。</param>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

        /// <summary>
        /// 离开状态时调用。
        /// </summary>
        /// <param name="procedureOwner">流程持有者。</param>
        /// <param name="isShutdown">是否是关闭状态机时触发。</param>
        public override void Leave(bool isShutdown)
        {
            base.Leave(isShutdown);
        }

        /// <summary>
        /// 状态销毁时调用。
        /// </summary>
        /// <param name="procedureOwner">流程持有者。</param>
        public override void Destroy()
        {
            base.Destroy();
        }
    }
}
