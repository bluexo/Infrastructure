namespace Origine
{
    public interface IScriptScope
    {
        void SetValue(string key, object parameter);

        void Execute(string src);

        bool TryExecute<T>(string src, out T value);

        void OnDispose();
    }
}
