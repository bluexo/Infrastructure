﻿using System.Collections.Generic;

namespace Origine.Planner
{
    public class AStar<T>
    {
        private readonly FastPriorityQueue<INode<T>, T> frontier;
        private readonly Dictionary<T, INode<T>> stateToNode;
        private readonly Dictionary<T, INode<T>> explored;
        private readonly List<INode<T>> createdNodes;
        // Debug
        public AStar(int maxNodesToExpand = 1000)
        {
            frontier = new FastPriorityQueue<INode<T>, T>(maxNodesToExpand);
            stateToNode = new Dictionary<T, INode<T>>();
            explored = new Dictionary<T, INode<T>>(); // State -> node
            createdNodes = new List<INode<T>>(maxNodesToExpand);
        }

        void ClearNodes()
        {
            foreach (var node in createdNodes)
            {
                node.Recycle();
            }
            createdNodes.Clear();
        }

       


        public INode<T> Run(INode<T> start, T goal, int maxIterations = 100, bool earlyExit = true, bool clearNodes = true, bool debugPlan = false)
        {

            frontier.Clear();
            stateToNode.Clear();
            explored.Clear();
            if (clearNodes)
            {
                ClearNodes();
                createdNodes.Add(start);
            }

            frontier.Enqueue(start, start.GetCost());


            var iterations = 0;
            while ((frontier.Count > 0) && (iterations < maxIterations) && (frontier.Count + 1 < frontier.MaxSize))
            {
                var node = frontier.Dequeue();
                if (node.IsGoal(goal))
                {
                    GoapLogger.Log("[Astar] Success iterations: " + iterations);
                    return node;
                }
                explored[node.GetState()] = node;


                foreach (var child in node.Expand())
                {
                    iterations++;
                    if (clearNodes)
                    {
                        createdNodes.Add(child);
                    }
                    if (earlyExit && child.IsGoal(goal))
                    {
                        GoapLogger.Log("[Astar] (early exit) Success iterations: " + iterations);
                        return child;
                    }
                    var childCost = child.GetCost();
                    var state = child.GetState();
                    if (explored.ContainsKey(state))
                        continue;
                    INode<T> similiarNode;
                    stateToNode.TryGetValue(state, out similiarNode);
                    if (similiarNode != null)
                    {
                        if (similiarNode.GetCost() > childCost)
                            frontier.Remove(similiarNode);
                        else
                            break;
                    }

                    //Utilities.ReGoapLogger.Log(string.Format("    Enqueue frontier: {0}, cost: {1}", child.Name, childCost));
                    frontier.Enqueue(child, childCost);
                    stateToNode[state] = child;
                }
            }
            GoapLogger.LogWarning("[Astar] failed.");
            return null;
        }
    }

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
            if (result == 0)
                return 1;
            return result;
        }
    }
}