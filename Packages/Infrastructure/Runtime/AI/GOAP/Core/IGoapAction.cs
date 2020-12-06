using System;
using System.Collections.Generic;

namespace Origine.AI
{
    public struct GoapActionStackData<T, W>
    {
        public GoapState<T, W> currentState;
        public GoapState<T, W> goalState;
        public IGoapAgent<T, W> agent;
        public IGoapAction<T, W> next;
        public GoapState<T, W> settings;
    }

    public interface IGoapAction<T, W>
    {
        // this should return current's action calculated parameter, will be added to the run method
        // userful for dynamic actions, for example a GoTo action can save some informations (wanted position)
        // while being chosen from the planner, we save this information and give it back when we run the method
        // most of actions would return a single item list, but more complex could return many items
        List<GoapState<T, W>> GetSettings(GoapActionStackData<T, W> stackData);
        void Run(IGoapAction<T, W> previousAction, IGoapAction<T, W> nextAction, GoapState<T, W> settings, GoapState<T, W> goalState, Action<IGoapAction<T, W>> done, Action<IGoapAction<T, W>> fail);
        // Called when the action has been added inside a running Plan
        void PlanEnter(IGoapAction<T, W> previousAction, IGoapAction<T, W> nextAction, GoapState<T, W> settings, GoapState<T, W> goalState);
        // Called when the plan, which had this action, has either failed or completed
        void PlanExit(IGoapAction<T, W> previousAction, IGoapAction<T, W> nextAction, GoapState<T, W> settings, GoapState<T, W> goalState);
        void Exit(IGoapAction<T, W> nextAction);
        string GetName();
        bool IsActive();
        bool IsInterruptable();
        void AskForInterruption();
        // MUST BE IMPLEMENTED AS THREAD SAFE
        GoapState<T, W> GetPreconditions(GoapActionStackData<T, W> stackData);
        GoapState<T, W> GetEffects(GoapActionStackData<T, W> stackData);
        bool CheckProceduralCondition(GoapActionStackData<T, W> stackData);
        float GetCost(GoapActionStackData<T, W> stackData);
        // DO NOT CHANGE RUNTIME ACTION VARIABLES, precalculation can be runned many times even while an action is running
        void Precalculations(GoapActionStackData<T, W> stackData);

        string ToString(GoapActionStackData<T, W> stackData);
    }
}