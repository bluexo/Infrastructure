using System.Collections.Generic;

namespace Origine.AI
{
    public interface INode<T>
    {
        T GetState();
        List<INode<T>> Expand();
        int CompareTo(INode<T> other);
        float GetCost();
        float GetHeuristicCost();
        float GetPathCost();
        INode<T> GetParent();
        bool IsGoal(T goal);

        string Name { get; }
        T Goal { get; }
        T Effects { get; }
        T Preconditions { get; }

        int QueueIndex { get; set; }
        float Priority { get; set; }
        void Recycle();
    }

    public class NodeComparer<T> : IComparer<INode<T>>
    {
        public int Compare(INode<T> x, INode<T> y)
        {
            var result = x.CompareTo(y);
            return result == 0 ? 1 : result;
        }
    }
}
