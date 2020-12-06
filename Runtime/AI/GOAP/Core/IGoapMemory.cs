namespace Origine.AI
{
    public interface IGoapMemory<T, W>
    {
        GoapState<T, W> GetWorldState();
    }
}