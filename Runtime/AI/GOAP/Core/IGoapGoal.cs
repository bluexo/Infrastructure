using System;
using System.Collections.Generic;
using Origine.AI;

namespace Origine.AI
{
    public interface IGoapGoal<T, W>
    {
        void Run(Action<IGoapGoal<T, W>> callback);
        // THREAD SAFE METHODS (cannot use any unity library!)
        Queue<GoapActionState<T, W>> GetPlan();
        string GetName();
        void Precalculations(IGoapPlanner<T, W> goapPlanner);
        bool IsGoalPossible();
        GoapState<T, W> GetGoalState();
        float GetPriority();
        void SetPlan(Queue<GoapActionState<T, W>> path);
        float GetErrorDelay();
    }
}