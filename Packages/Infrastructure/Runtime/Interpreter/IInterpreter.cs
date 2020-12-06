using Jint;

namespace Origine
{
    public struct InterpreterContext
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    /// <summary>
    /// 脚本解释器
    /// </summary>
    public interface IInterpreter
    {
        void SetValue(string key, object parameter);

        object Execute(string src);

        bool TryExecute<T>(string src, out T value);

        object Execute(string src, params InterpreterContext[] contexts);
    }
}