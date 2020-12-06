using System;
using System.Collections.Generic;

namespace Origine
{
    public static partial class ReferencePool
    {
        private sealed class ReferenceCollection
        {
            private readonly Queue<IReference> m_References;

            public ReferenceCollection(Type referenceType)
            {
                m_References = new Queue<IReference>();
                ReferenceType = referenceType;
                UsingReferenceCount = 0;
                AcquireReferenceCount = 0;
                ReleaseReferenceCount = 0;
                AddReferenceCount = 0;
                RemoveReferenceCount = 0;
            }

            public Type ReferenceType { get; }

            public int UnusedReferenceCount => m_References.Count;

            public int UsingReferenceCount { get; private set; }

            public int AcquireReferenceCount { get; private set; }

            public int ReleaseReferenceCount { get; private set; }

            public int AddReferenceCount { get; private set; }

            public int RemoveReferenceCount { get; private set; }

            public T Take<T>() where T : class, IReference, new()
            {
                if (typeof(T) != ReferenceType)
                {
                    throw new GameException("Type is invalid.");
                }

                UsingReferenceCount++;
                AcquireReferenceCount++;
                lock (m_References)
                {
                    if (m_References.Count > 0)
                    {
                        return (T)m_References.Dequeue();
                    }
                }

                AddReferenceCount++;
                return new T();
            }

            public IReference Take()
            {
                UsingReferenceCount++;
                AcquireReferenceCount++;
                lock (m_References)
                {
                    if (m_References.Count > 0)
                    {
                        return m_References.Dequeue();
                    }
                }

                AddReferenceCount++;
                return (IReference)Activator.CreateInstance(ReferenceType);
            }

            public void Return(IReference reference)
            {
                reference.OnDestroy();
                lock (m_References)
                {
                    if (EnableStrictCheck && m_References.Contains(reference))
                    {
                        throw new GameException("The reference has been released.");
                    }

                    m_References.Enqueue(reference);
                }

                ReleaseReferenceCount++;
                UsingReferenceCount--;
            }

            public void Add<T>(int count) where T : class, IReference, new()
            {
                if (typeof(T) != ReferenceType)
                {
                    throw new GameException("Type is invalid.");
                }

                lock (m_References)
                {
                    AddReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Enqueue(new T());
                    }
                }
            }

            public void Add(int count)
            {
                lock (m_References)
                {
                    AddReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Enqueue((IReference)Activator.CreateInstance(ReferenceType));
                    }
                }
            }

            public void Remove(int count)
            {
                lock (m_References)
                {
                    if (count > m_References.Count)
                    {
                        count = m_References.Count;
                    }

                    RemoveReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Dequeue();
                    }
                }
            }

            public void RemoveAll()
            {
                lock (m_References)
                {
                    RemoveReferenceCount += m_References.Count;
                    m_References.Clear();
                }
            }
        }
    }
}
