using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origine
{
    public interface IInitializer
    {
        void Initialize();
    }

    public interface IInitializer<in T1>
    {
        void Initialize(T1 t1);
    }

    public interface IInitializer<in T1, in T2>
    {
        void Initialize(T1 t1, T2 t2);
    }

    public interface IInitializer<in T1, in T2, in T3>
    {
        void Initialize(T1 t1, T2 t2, T3 t3);
    }

    public interface IInitializer<in T1, in T2, in T3, in T4>
    {
        void Initialize(T1 t1, T2 t2, T3 t3, T4 t4);
    }
}
