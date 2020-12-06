using System;
using System.Collections.Generic;

namespace Origine.AI
{
    public class GoapPlanner<T, W> : IGoapPlanner<T, W>
    {
        private IGoapAgent<T, W> goapAgent;
        private IGoapGoal<T, W> currentGoal;
        public bool Calculated;
        private readonly AStar<GoapState<T, W>> astar;
        private readonly GoapPlannerSettings settings;

        public GoapPlanner(GoapPlannerSettings settings = null)
        {
            this.settings = settings ?? new GoapPlannerSettings();
            astar = new AStar<GoapState<T, W>>(this.settings.MaxNodesToExpand);
        }

        public IGoapGoal<T, W> Plan(IGoapAgent<T, W> agent, IGoapGoal<T, W> blacklistGoal = null, Queue<GoapActionState<T, W>> currentPlan = null, Action<IGoapGoal<T, W>> callback = null)
        {
            if (GoapLogger.Level == GoapLogger.DebugLevel.Full)
                GoapLogger.Log("[ReGoalPlanner] Starting planning calculation for agent: " + agent);
            goapAgent = agent;
            Calculated = false;
            currentGoal = null;
            var possibleGoals = new List<IGoapGoal<T, W>>();
            foreach (var goal in goapAgent.GetGoalsSet())
            {
                if (goal == blacklistGoal)
                    continue;
                goal.Precalculations(this);
                if (goal.IsGoalPossible())
                    possibleGoals.Add(goal);
            }
            possibleGoals.Sort((x, y) => x.GetPriority().CompareTo(y.GetPriority()));

            var currentState = agent.GetMemory().GetWorldState();

            while (possibleGoals.Count > 0)
            {
                currentGoal = possibleGoals[possibleGoals.Count - 1];
                possibleGoals.RemoveAt(possibleGoals.Count - 1);
                var goalState = currentGoal.GetGoalState();

                // can't work with dynamic actions, of course
                if (!settings.UsingDynamicActions)
                {
                    var wantedGoalCheck = currentGoal.GetGoalState();
                    GoapActionStackData<T, W> stackData;
                    stackData.agent = goapAgent;
                    stackData.currentState = currentState;
                    stackData.goalState = goalState;
                    stackData.next = null;
                    stackData.settings = null;
                    // we check if the goal can be archived through actions first, so we don't brute force it with A* if we can't
                    foreach (var action in goapAgent.GetActionsSet())
                    {
                        action.Precalculations(stackData);
                        if (!action.CheckProceduralCondition(stackData))
                        {
                            continue;
                        }
                        // check if the effects of all actions can archieve currentGoal
                        var previous = wantedGoalCheck;
                        wantedGoalCheck = GoapState<T, W>.Instantiate();
                        previous.MissingDifference(action.GetEffects(stackData), ref wantedGoalCheck);
                    }
                    // finally push the current world state
                    var current = wantedGoalCheck;
                    wantedGoalCheck = GoapState<T, W>.Instantiate();
                    current.MissingDifference(GetCurrentAgent().GetMemory().GetWorldState(), ref wantedGoalCheck);
                    // can't validate goal 
                    if (wantedGoalCheck.Count > 0)
                    {
                        currentGoal = null;
                        continue;
                    }
                }

                goalState = goalState.Clone();
                var leaf = (GoapNode<T, W>)astar.Run(
                    GoapNode<T, W>.Instantiate(this, goalState, null, null, null), goalState, settings.MaxIterations, settings.PlanningEarlyExit, debugPlan: settings.DebugPlan);
                if (leaf == null)
                {
                    currentGoal = null;
                    continue;
                }

                var result = leaf.CalculatePath();
                if (currentPlan != null && currentPlan == result)
                {
                    currentGoal = null;
                    break;
                }
                if (result.Count == 0)
                {
                    currentGoal = null;
                    continue;
                }
                currentGoal.SetPlan(result);
                break;
            }
            Calculated = true;

            callback?.Invoke(currentGoal);

            if (currentGoal != null)
            {
                GoapLogger.Log(string.Format("[ReGoapPlanner] Calculated plan for goal '{0}', plan length: {1}", currentGoal, currentGoal.GetPlan().Count));
                if (GoapLogger.Level == GoapLogger.DebugLevel.Full)
                {
                    int i = 0;
                    GoapActionStackData<T, W> stackData;
                    stackData.agent = agent;
                    stackData.currentState = currentState;
                    stackData.goalState = currentGoal.GetGoalState();
                    stackData.next = null;
                    foreach (var action in currentGoal.GetPlan())
                    {
                        stackData.settings = action.Settings;
                        GoapLogger.Log(string.Format("[ReGoapPlanner] {0}) {1}", i++, action.Action.ToString(stackData)));
                    }
                }
            }
            else
                GoapLogger.LogWarning("[ReGoapPlanner] Error while calculating plan.");
            return currentGoal;
        }

        public IGoapGoal<T, W> GetCurrentGoal() => currentGoal;
        public IGoapAgent<T, W> GetCurrentAgent() => goapAgent;
        public bool IsPlanning() => !Calculated;
        public GoapPlannerSettings GetSettings() => settings;
    }
}