using System.Collections.Generic;

namespace Origine.AI
{
    public class GoapNode<T, W> : INode<GoapState<T, W>>
    {
        private float cost;
        private IGoapPlanner<T, W> planner;
        private GoapNode<T, W> parent;
        private IGoapAction<T, W> action;
        private GoapState<T, W> actionSettings;
        private GoapState<T, W> state;
        private float g;
        private float h;
        private GoapState<T, W> goalMergedWithWorld;

        private float heuristicMultiplier = 1;

        private readonly List<INode<GoapState<T, W>>> expandList;

        private GoapNode() => expandList = new List<INode<GoapState<T, W>>>();

        private void Init(IGoapPlanner<T, W> planner, GoapState<T, W> newGoal, GoapNode<T, W> parent, IGoapAction<T, W> action, GoapState<T, W> settings)
        {
            expandList.Clear();

            this.planner = planner;
            this.parent = parent;
            this.action = action;
            if (settings != null)
                this.actionSettings = settings.Clone();

            if (parent != null)
            {
                state = parent.GetState().Clone();
                // g(node)
                g = parent.GetPathCost();
            }
            else
            {
                state = planner.GetCurrentAgent().GetMemory().GetWorldState().Clone();
            }

            var nextAction = parent == null ? null : parent.action;
            if (action != null)
            {
                // create a new instance of the goal based on the paren't goal
                Goal = GoapState<T, W>.Instantiate(newGoal);

                GoapActionStackData<T, W> stackData;
                stackData.currentState = state;
                stackData.goalState = Goal;
                stackData.next = action;
                stackData.agent = planner.GetCurrentAgent();
                stackData.settings = actionSettings;

                Preconditions = action.GetPreconditions(stackData);
                Effects = action.GetEffects(stackData);
                // addding the action's cost to the node's total cost
                g += action.GetCost(stackData);

                // adding the action's effects to the current node's state
                state.AddFromState(Effects);

                // removes from goal all the conditions that are now fullfiled in the action's effects
                Goal.ReplaceWithMissingDifference(Effects);
                // add all preconditions of the current action to the goal
                Goal.AddFromState(Preconditions);
            }
            else
            {
                Goal = newGoal;
            }
            h = Goal.Count;
            // f(node) = g(node) + h(node)
            cost = g + h * heuristicMultiplier;

            // additionally calculate the goal without any world effect to understand if we are done
            var diff = GoapState<T, W>.Instantiate();
            Goal.MissingDifference(planner.GetCurrentAgent().GetMemory().GetWorldState(), ref diff);
            goalMergedWithWorld = diff;
        }

        #region NodeFactory
        private static Stack<GoapNode<T, W>> cachedNodes;

        public static void Warmup(int count)
        {
            cachedNodes = new Stack<GoapNode<T, W>>(count);
            for (int i = 0; i < count; i++)
            {
                cachedNodes.Push(new GoapNode<T, W>());
            }
        }

        public void Recycle()
        {
            state.Recycle();
            state = null;
            Goal.Recycle();
            Goal = null;
            lock (cachedNodes)
            {
                cachedNodes.Push(this);
            }
        }

        public static GoapNode<T, W> Instantiate(IGoapPlanner<T, W> planner, GoapState<T, W> newGoal, GoapNode<T, W> parent, IGoapAction<T, W> action, GoapState<T, W> actionSettings)
        {
            GoapNode<T, W> node;
            if (cachedNodes == null)
            {
                cachedNodes = new Stack<GoapNode<T, W>>();
            }
            lock (cachedNodes)
            {
                node = cachedNodes.Count > 0 ? cachedNodes.Pop() : new GoapNode<T, W>();
            }
            node.Init(planner, newGoal, parent, action, actionSettings);
            return node;
        }
        #endregion

        public float GetPathCost() => g;

        public float GetHeuristicCost() => h;

        public GoapState<T, W> GetState() => state;

        public List<INode<GoapState<T, W>>> Expand()
        {
            expandList.Clear();

            var agent = planner.GetCurrentAgent();
            var actions = agent.GetActionsSet();

            GoapActionStackData<T, W> stackData;
            stackData.currentState = state;
            stackData.goalState = Goal;
            stackData.next = action;
            stackData.agent = agent;
            stackData.settings = null;
            for (var index = actions.Count - 1; index >= 0; index--)
            {
                var possibleAction = actions[index];

                possibleAction.Precalculations(stackData);
                var settingsList = possibleAction.GetSettings(stackData);
                foreach (var settings in settingsList)
                {
                    stackData.settings = settings;
                    var precond = possibleAction.GetPreconditions(stackData);
                    var effects = possibleAction.GetEffects(stackData);

                    if (effects.HasAny(Goal) && // any effect is the current Goal
                        !Goal.HasAnyConflict(effects, precond) && // no precondition is conflicting with the Goal or has conflict but the effects fulfils the Goal
                        !Goal.HasAnyConflict(effects) && // no effect is conflicting with the Goal
                        possibleAction.CheckProceduralCondition(stackData))
                    {
                        var newGoal = Goal;
                        expandList.Add(Instantiate(planner, newGoal, this, possibleAction, settings));
                    }
                }
            }
            return expandList;
        }

        private IGoapAction<T, W> GetAction() => action;

        public Queue<GoapActionState<T, W>> CalculatePath()
        {
            var result = new Queue<GoapActionState<T, W>>();
            CalculatePath(ref result);
            return result;
        }

        public void CalculatePath(ref Queue<GoapActionState<T, W>> result)
        {
            var node = this;
            while (node.GetParent() != null)
            {
                result.Enqueue(new GoapActionState<T, W>(node.action, node.actionSettings));
                node = (GoapNode<T, W>)node.GetParent();
            }
        }

        public int CompareTo(INode<GoapState<T, W>> other) => cost.CompareTo(other.GetCost());

        public float GetCost() => cost;

        public INode<GoapState<T, W>> GetParent() => parent;

        public bool IsGoal(GoapState<T, W> goal) => goalMergedWithWorld.Count <= 0;

        public float Priority { get; set; }
        public long InsertionIndex { get; set; }
        public int QueueIndex { get; set; }

        public string Name => action != null ? action.GetName() : "NoAction";
        public GoapState<T, W> Goal { get; private set; }
        public GoapState<T, W> Effects { get; private set; }
        public GoapState<T, W> Preconditions { get; private set; }
    }
}