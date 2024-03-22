using System;
using JetBrains.Annotations;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    [Flags]
    enum LayoutFlags
    {
        None = 0,

        SceneHierarchyWindow = 1 << 0,
        GameView = 1 << 1,
        SceneView = 1 << 2,
        ConsoleWindow = 1 << 3,
        InspectorWindow = 1 << 4,
    }

    static class EditorModeViewsUtil
    {
        [NotNull]
        public static string GenerateLayoutName(LayoutFlags layoutFlags)
        {
            return $"layout_{(int)layoutFlags:0000}";
        }

        public static LayoutFlags SetFlag(LayoutFlags flags, LayoutFlags flag, bool set)
        {
            return set ? flag | flags : ~flag & flags;
        }
    }
}