

namespace Origine
{
    internal sealed partial class EventPool<T> where T : GameEventArgs
    {
        /// <summary>
        /// 事件结点。
        /// </summary>
        private sealed class Event : IReference
        {
            public object Sender { get; private set; }
            public T EventArgs { get; private set; }
            
            public Event()
            {
                Sender = null;
                EventArgs = null;
            }

            public static Event Create(object sender, T e)
            {
                Event eventNode = ReferencePool.Take<Event>();
                eventNode.Sender = sender;
                eventNode.EventArgs = e;
                return eventNode;
            }

            public void OnDestroy()
            {
                Sender = null;
                EventArgs = null;
            }
        }
    }
}
