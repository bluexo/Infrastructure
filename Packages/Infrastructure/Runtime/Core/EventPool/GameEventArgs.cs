

namespace Origine
{
    /// <summary>
    /// 事件基类。
    /// </summary>
    public class GameEventArgs : BaseEventArgs
    {
        /// <summary>
        /// 获取类型编号。
        /// </summary>
        public virtual int Id { get; }

        public static readonly GameEventArgs Default = new GameEventArgs();
    }

    public abstract class GameEventArgs<T> : GameEventArgs where T : GameEventArgs
    {
        public static readonly int EventId = typeof(T).GetHashCode();

        public override int Id => EventId;
    }
}
