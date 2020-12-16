using System;

namespace Origine.Fsm
{
    /// <summary>
    /// 有限状态机状态基类。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    public abstract class FsmState<T> : Entity where T : class
    {
        public IFsm<T> Fsm { get; protected set; }

        /// <summary>
        /// 初始化有限状态机状态基类的新实例。
        /// </summary>
        public FsmState()
        {
        }

        /// <summary>
        /// 有限状态机状态初始化时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        public virtual void OnInit(IFsm<T> fsm)
        {
            Fsm = fsm;
        }

        /// <summary>
        /// 有限状态机状态进入时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        public virtual void OnEnter()
        {
        }

        /// <summary>
        /// 有限状态机状态离开时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        /// <param name="isShutdown">是否是关闭有限状态机时触发。</param>
        public virtual void OnLeave(bool isShutdown)
        {
        }

        /// <summary>
        /// 有限状态机状态销毁时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        public virtual void Destroy()
        {
        }

        /// <summary>
        /// 切换当前有限状态机状态。
        /// </summary>
        /// <typeparam name="TState">要切换到的有限状态机状态类型。</typeparam>
        /// <param name="fsm">有限状态机引用。</param>
        public void ChangeState<TState>() where TState : FsmState<T>
        {
            Fsm<T> fsmImplement = (Fsm<T>)Fsm;
            if (fsmImplement == null)
            {
                throw new GameException("FSM is invalid.");
            }

            fsmImplement.ChangeState<TState>();
        }

        /// <summary>
        /// 切换当前有限状态机状态。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        /// <param name="stateType">要切换到的有限状态机状态类型。</param>
        public void ChangeState(Type stateType)
        {
            Fsm<T> fsmImplement = (Fsm<T>)Fsm;
            if (fsmImplement == null)
                throw new GameException("FSM is invalid.");

            if (stateType == null)
                throw new GameException("State type is invalid.");

            if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
                throw new GameException(Utility.Text.Format("State type '{0}' is invalid.", stateType.FullName));

            fsmImplement.ChangeState(stateType);
        }
    }
}
