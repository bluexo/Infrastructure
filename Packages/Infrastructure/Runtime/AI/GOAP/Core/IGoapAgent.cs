using System.Collections.Generic;

namespace Origine.AI
{
    public interface IGoapAgent<T, W>
    {
        IGoapMemory<T, W> GetMemory();
        IGoapGoal<T, W> GetCurrentGoal();
        // called from a goal when the goal is available
        void WarnPossibleGoal(IGoapGoal<T, W> goal);
        bool IsActive();
        List<GoapActionState<T, W>> GetStartingPlan();
        W GetPlanValue(T key);
        void SetPlanValue(T key, W value);
        bool HasPlanValue(T target);
        // THREAD SAFE
        List<IGoapGoal<T, W>> GetGoalsSet();
        List<IGoapAction<T, W>> GetActionsSet();
        GoapState<T, W> InstantiateNewState();

    }
}
