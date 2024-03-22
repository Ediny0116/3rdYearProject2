using System;
using Newtonsoft.Json;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class MessageId
    {
        [JsonProperty]
        readonly Guid m_Id;

        [JsonConstructor]
        MessageId(Guid id)
        {
            m_Id = id;
        }

        public static bool operator ==(MessageId lhs, MessageId rhs)
        {
            // Check for null on left side.
            if (ReferenceEquals(lhs, null))
            {
                if (ReferenceEquals(rhs, null))
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }

            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(MessageId lhs, MessageId rhs)
        {
            return !(lhs == rhs);
        }

        public static MessageId NewMessageId()
        {
            return new MessageId(Guid.NewGuid());
        }

        public static bool TryParse(string input, out MessageId identifier)
        {
            identifier = null;
            
            if (!Guid.TryParse(input, out var guid))
            {
                return false;
            }

            identifier = new MessageId(guid);
            return true;
        }

        public override string ToString()
        {
            return $"{m_Id:N}";
        }

        public override bool Equals(object obj)
        {
            return obj is MessageId identifier
                   && Equals(m_Id, identifier.m_Id);
        }

        public override int GetHashCode()
        {
            return m_Id.GetHashCode();
        }
    }
}
