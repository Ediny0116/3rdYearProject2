using System;
using UnityEngine;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    [Serializable]
    class VirtualProjectState
    {
        public string[] LaunchArgs;
        [SerializeField]
        public DateTime FirstHeartbeatReceived;
        [SerializeField]
        public DateTime LastHeartbeatReceived;
        public bool IsDebuggerAttached;

        public override string ToString()
        {
            return $"{nameof(FirstHeartbeatReceived)}: {FirstHeartbeatReceived}  {nameof(LastHeartbeatReceived)}: {LastHeartbeatReceived}";
        }
    }
}