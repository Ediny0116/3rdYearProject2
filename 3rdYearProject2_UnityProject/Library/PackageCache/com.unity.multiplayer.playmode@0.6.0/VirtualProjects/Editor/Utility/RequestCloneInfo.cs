using System;
using Unity.Multiplayer.Playmode.Common.Editor;
using UnityEngine;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class RequestCloneInfo
    {
        [Serializable]
        internal class CloneInfo
        {
            [SerializeField] public bool IsCommunicative;
            [SerializeField] public bool IsPlaying; // This represents EditorApplication.isPlaying
        }

        public SessionStateJsonRepository<VirtualProjectIdentifier, CloneInfo> ActiveClones { get; } = 
            SessionStateJsonRepository<VirtualProjectIdentifier, CloneInfo>.GetMain(SessionStateRepository.Get, nameof(ActiveClones), out _);

        public bool TryGetCloneInfo(string identifier, out CloneInfo cloneTestInfo)
        {
            cloneTestInfo = default;
            return VirtualProjectIdentifier.TryParse(identifier, out var virtualProjectIdentifier) 
                   && ActiveClones.TryGetValue(virtualProjectIdentifier, out cloneTestInfo);
        }
    }
} 