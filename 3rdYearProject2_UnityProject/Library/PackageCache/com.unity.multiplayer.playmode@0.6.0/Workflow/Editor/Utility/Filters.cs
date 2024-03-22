using System.Collections.Generic;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    static class Filters
    {
        internal static bool FindFirstPlayerWithVirtualProjectsIdentifier(Dictionary<int, PlayerStateJson> playerStateJSONs, VirtualProjectIdentifier virtualProjectIdentifier, out PlayerStateJson result)
        {
            result = null;
            
            foreach (var (_, playerStateJson) in playerStateJSONs)
            {
                if (Equals(playerStateJson.TypeDependentPlayerInfo.VirtualProjectIdentifier, virtualProjectIdentifier))
                {
                    result = playerStateJson;
                    return true;
                }
            }

            return false;
        }
        
        internal static bool FindFirstPlayerWithPlayerType(Dictionary<int, PlayerStateJson> playerStateJSONs, PlayerType playerType, out PlayerStateJson result)
        {
            result = null;
            
            foreach (var (_, playerStateJson) in playerStateJSONs)
            {
                if (Equals(playerStateJson.Type, playerType))
                {
                    result = playerStateJson;
                    return true;
                }
            }

            return false;
        }

        internal static VirtualProject FindFirstAvailableProject(VirtualProject[] projects, params VirtualProjectIdentifier[] identifiers)
        {
            foreach (var p in projects)
            {
                var contains = false;
                foreach (var project in identifiers)
                {
                    if (Equals(project, p.Identifier))
                    {
                        contains = true;
                        break;
                    }
                }

                if (!contains)
                {
                    return p;
                }
            }

            return null;
        }
    }
}