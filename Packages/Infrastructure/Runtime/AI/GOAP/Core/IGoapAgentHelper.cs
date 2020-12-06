using System;

// interface needed only in Unity to use GetComponent and such features for generic agents
namespace Origine.AI
{
    public interface IGoapAgentHelper
    {
        Type[] GetGenericArguments();
    }
}
