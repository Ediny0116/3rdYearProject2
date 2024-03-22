using System.IO;
using Unity.Multiplayer.Playmode.InternalBridge.Editor;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    static class ModeSwitcher
    {
        const string k_LayoutDirectory = "Packages/com.unity.multiplayer.playmode/Workflow/Layouts";
        const string k_TempLayoutDirectory = "Library/UserSettings/Layouts";
        const string k_LayoutExtension = "wlt";

        const string k_CurrentModeKey = "vpmode";
        const string k_CurrentWindowIdKey = "vpwindow-id";
        const string k_CurrentWindowSetKey = "vpwindow-set";

        const string k_WindowId = "VPWindow";

        const string k_UnknownPlayerWindowTitle = "Unknown Player";

        public static string CurrentMode
        {
            get => SessionState.GetString(k_CurrentModeKey, string.Empty);
            set
            {
                if (value == null)
                {
                    SessionState.EraseString(k_CurrentModeKey);
                }
                else
                {
                    SessionState.SetString(k_CurrentModeKey, value);
                }
            }
        }

        public static ContainerWindowProxy CurrentWindow
        {
            get
            {
                if (SessionState.GetBool(k_CurrentWindowSetKey, false))
                {
                    var value = SessionState.GetInt(k_CurrentWindowIdKey, 0);
                    return ContainerWindowProxy.FromInstanceID(value);
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    SessionState.EraseInt(k_CurrentWindowIdKey);
                    SessionState.EraseBool(k_CurrentWindowSetKey);
                }
                else
                {
                    SessionState.SetInt(k_CurrentWindowIdKey, value.GetInstanceID());
                    SessionState.SetBool(k_CurrentWindowSetKey, true);
                }
            }
        }

        public static bool IsView(LayoutFlags layoutFlags)
        {
            return EditorModeViewsUtil.GenerateLayoutName(layoutFlags) == CurrentMode;
        }

        public static void SaveCurrentView()
        {
            var view = CurrentMode;
            if (string.IsNullOrWhiteSpace(CurrentMode))
            {
                // Previous mode wasn't initialized but we still need to capture whatever changes have happened
                var defaultData = CloneData.NewDefault();
                var defaultViewFlags = EditorApplication.isPlaying
                    ? defaultData.PlayModeLayoutFlags
                    : defaultData.EditModeLayoutFlags;
                var defaultView = EditorModeViewsUtil.GenerateLayoutName(defaultViewFlags);
                view = defaultView;
            }

            // Save previous layout since we are switching
            Debug.Assert(!string.IsNullOrWhiteSpace(view), "The view is not supposed to be empty. Should be previous or default!");
            var lytFile = Path.ChangeExtension(Path.Combine(k_TempLayoutDirectory, view), k_LayoutExtension);
            WindowLayout.SaveWindowLayout(lytFile);
        }

        public static void SwitchToView(LayoutFlags layoutFlags)
        {
            Debug.Assert(layoutFlags != LayoutFlags.None, $"Layout of {LayoutFlags.None} is not supposed to be used.");
            // if the window got reopened somehow we should close it
            Debug.Assert(CurrentWindow == null, "The window was not null and we wanted to switch views.");
            CloseCurrentWindow();

            // Attempt to load existing layout for the view we are switching to
            var viewToSwitchTo = EditorModeViewsUtil.GenerateLayoutName(layoutFlags);
            var nextLayoutFilepathBasedOnEditorMode = Path.ChangeExtension(Path.Combine(k_TempLayoutDirectory, viewToSwitchTo), k_LayoutExtension);
            
            // LoadWindowLayout - loads a previously saved layout from SaveWindowLayout or a default layout
            // ShowWindowWithDynamicLayout - just loads a chunked layout file
            if (File.Exists(nextLayoutFilepathBasedOnEditorMode))
            {
                CurrentWindow = WindowLayout.LoadWindowLayout(k_WindowId, nextLayoutFilepathBasedOnEditorMode);
            }
            if (CurrentWindow == null)
            {
                var layoutFilePath = GetLayoutFilePath(viewToSwitchTo);
                CurrentWindow = WindowLayout.ShowWindowWithDynamicLayout(k_WindowId, layoutFilePath);
            }
            
            if (CurrentWindow != null)
            {
                var hasPlayer = Filters.FindFirstPlayerWithVirtualProjectsIdentifier(VirtualProjectWorkflow.WorkflowCloneContext.SystemDataStore.LoadAllPlayerJson(),VirtualProjectsEditor.CloneIdentifier, out var player);
                if (hasPlayer)
                {
                    var tag = player.Tags.Count <= 0 ? string.Empty : $" [{string.Join('|', player.Tags)}]";
                    CurrentWindow.title = $"{player.Name}{tag}";
                }
                else
                {
                    CurrentWindow.title = k_UnknownPlayerWindowTitle;
                }
            }
            
            CurrentMode = viewToSwitchTo;
            InternalEditorUtility.RepaintAllViews();
        }

        public static void CloseCurrentWindow()
        {
            if (CurrentWindow != null)
            {
                CurrentWindow.Close();
                CurrentWindow = null;
            }
        }

        public static bool TryOpenProjectSettingsWindow(string settingPath)
        {
            return WindowLayout.TryOpenProjectSettingsWindow(settingPath);
        }

        static string GetLayoutFilePath(string layout) => $"{k_LayoutDirectory}/{layout}.sjson";
    }
}