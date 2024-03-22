using System;
using System.Collections.Generic;

namespace Unity.Multiplayer.Playmode.Common.Editor
{
    class InMemoryRepository<TKey, TValue> where TValue : class, new()
    {
        readonly IDictionary<TKey, TValue> m_States = new Dictionary<TKey, TValue>();

        public bool ContainsKey(TKey identifier) => m_States.ContainsKey(identifier);

        public bool TryGetValue(TKey identifier, out TValue value) => m_States.TryGetValue(identifier, out value);

        public bool Create(TKey identifier, TValue state)
        {
            if (m_States.ContainsKey(identifier))
            {
                return false;
            }
            m_States[identifier] = state;
            return true;
        }

        public bool Update(TKey identifier, Action<TValue> update, out TValue state)
        {
            if (m_States.ContainsKey(identifier))
            {
                state = m_States[identifier];
                update.Invoke(state);
                return true;
            }

            state = default;
            return false;
        }

        public bool Delete(TKey identifier)
        {
            if (m_States.ContainsKey(identifier))
            {
                if (m_States.Remove(identifier))
                {
                    return true;
                }
                return true;
            }

            return false;
        }
    }
}