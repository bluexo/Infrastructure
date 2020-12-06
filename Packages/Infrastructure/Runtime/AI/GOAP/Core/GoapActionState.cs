namespace Origine.AI
{
    public class GoapActionState<T, W>
    {
        public IGoapAction<T, W> Action;
        public GoapState<T, W> Settings;

        public GoapActionState(IGoapAction<T, W> action, GoapState<T, W> settings)
        {
            Action = action;
            Settings = settings;
        }
    }
}