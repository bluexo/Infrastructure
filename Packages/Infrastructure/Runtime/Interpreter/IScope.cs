using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Origine
{
    public struct InterpreterContext
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public interface IScope 
    {
        void SetValue(string key, object parameter);

        object Execute(string src);

        bool TryExecute<T>(string src, out T value);

        object Execute(string src, params InterpreterContext[] contexts);

        void OnDispose();
    }
}
