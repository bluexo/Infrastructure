using System;
using System.Collections.Generic;

namespace Origine.AI
{
    public interface IGoapPlanner<T, W>
    {
        IGoapGoal<T, W> Plan(IGoapAgent<T, W> goapAgent, IGoapGoal<T, W> blacklistGoal, Queue<GoapActionState<T, W>> currentPlan, Action<IGoapGoal<T, W>> callback);
        IGoapGoal<T, W> GetCurrentGoal();
        IGoapAgent<T, W> GetCurrentAgent();
        bool IsPlanning();
        GoapPlannerSettings GetSettings();
    }
}