using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Toolbars;
using PopupWindow = UnityEditor.PopupWindow;
#if UNITY_USE_MULTIPLAYER_ROLES
using Unity.Multiplayer.Editor;
#endif

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    static class TopViewPermanence
    {
        public const string k_FrameAfterTopViewSwitch = "frameAfterTopViewSwitch";

        public static readonly bool Initialized; // Note: the constructor won't run without some data being in the static class
        
        static TopViewPermanence()
        {
            Initialized = true;
            // The window can't fully close with TopView having a callback.
            // So we come here to make sure we can fully close the window
            EditorApplication.update += () =>
            {
                if (SessionState.GetBool(k_FrameAfterTopViewSwitch, false))
                {
                    SessionState.SetBool(k_FrameAfterTopViewSwitch, false);
                    Debug.Assert(TopView.NumberOfTopViews == 0 || TopView.NumberOfTopViews == 1, $"An editor should only have 0 or 1 TopView. [{TopView.NumberOfTopViews}]");
                    var editorMode = EditorApplication.isPlaying
                        ? CloneDataFile.LoadFromFile(VirtualProjectWorkflow.WorkflowCloneContext.CloneDataFile).PlayModeLayoutFlags
                        : CloneDataFile.LoadFromFile(VirtualProjectWorkflow.WorkflowCloneContext.CloneDataFile).EditModeLayoutFlags;
                    ModeSwitcher.SwitchToView(editorMode);
                }
            };
        }
    }

    class TopView : EditorWindow
    {
        class MarksApplied
        {
            public WindowLayoutCheckboxes.Marks Marks;
        }

        MarksApplied m_MarksFromOpening;
        public static int NumberOfTopViews;

        public void OnDestroy()
        {
            NumberOfTopViews--;
        }

        public void CreateGUI()
        {
            Debug.Assert(TopViewPermanence.Initialized, "The TopView did not initialize correctly.");
            NumberOfTopViews++;
            if (NumberOfTopViews > 1)
            {
                Debug.LogWarning("An editor should only have 0 or 1 TopView so something bad happened");
                return;
            }

            var windowLayoutPopup = new WindowLayoutPopoutWindow();
            var toolbar = new Toolbar();

#if UNITY_USE_MULTIPLAYER_ROLES
            AddMultiplayerRoleDropdown(toolbar);
#endif

            var layoutDropdown = new EditorToolbarDropdown
            {
                text = "Layout",
            };
            toolbar.Add(layoutDropdown);

            // Apply styles for the entire TopView to more match the standard editor
            rootVisualElement.AddToClassList("unity-editor-toolbar-container");
            toolbar.AddToClassList("unity-editor-toolbar-container__zone");
            toolbar.style.height = 30f;
            toolbar.style.borderBottomWidth = 0f;
            LoadStyleSheets("MainToolbar", rootVisualElement);
            LoadStyleSheets("EditorToolbar", toolbar);

            rootVisualElement.Add(toolbar);
            rootVisualElement.style.flexDirection = FlexDirection.RowReverse;
            rootVisualElement.style.maxHeight = 30f;
            m_MarksFromOpening = new MarksApplied
            {
                Marks = new WindowLayoutCheckboxes.Marks { Console = true }
            }; // Have to have at least one marked since you must have a layout

            EditorApplication.playModeStateChanged += _ => { windowLayoutPopup.CheckBoxes.RefreshLayout(); };
            layoutDropdown.RegisterCallback<ClickEvent>(_ =>
            {
                WindowLayoutCheckboxes.ListenToCheckBoxes(windowLayoutPopup.CheckBoxes);
                PopupWindow.Show(layoutDropdown.worldBound, windowLayoutPopup);
            });
            windowLayoutPopup.CloseEvent += () =>
            {
                if (m_MarksFromOpening != null)
                {
                    windowLayoutPopup.CheckBoxes.FromMarksWithoutNotify(m_MarksFromOpening.Marks);
                }

                m_MarksFromOpening = null;
            };
            windowLayoutPopup.OpenEvent += () =>
            {
                m_MarksFromOpening = new MarksApplied
                    { Marks = WindowLayoutCheckboxes.Marks.FromWindowLayoutCheckboxes(windowLayoutPopup.CheckBoxes) };

                windowLayoutPopup.CheckBoxes.ApplyEvent += (marks) =>
                {
                    m_MarksFromOpening = null;
                    // On UI event update the local state
                    var cloneConfiguration = VirtualProjectWorkflow.WorkflowCloneContext.CloneDataFile;
                    var cloneData = cloneConfiguration.Data;
                    if (EditorApplication.isPlaying)
                    {
                        cloneData.PlayModeLayoutFlags = EditorModeViewsUtil.SetFlag(cloneData.PlayModeLayoutFlags,
                            LayoutFlags.GameView, marks.Game);
                        cloneData.PlayModeLayoutFlags = EditorModeViewsUtil.SetFlag(cloneData.PlayModeLayoutFlags,
                            LayoutFlags.ConsoleWindow, marks.Console);
                        cloneData.PlayModeLayoutFlags = EditorModeViewsUtil.SetFlag(cloneData.PlayModeLayoutFlags,
                            LayoutFlags.InspectorWindow, marks.Inspector);
                        cloneData.PlayModeLayoutFlags = EditorModeViewsUtil.SetFlag(cloneData.PlayModeLayoutFlags,
                            LayoutFlags.SceneHierarchyWindow, marks.Hierarchy);
                        cloneData.PlayModeLayoutFlags = EditorModeViewsUtil.SetFlag(cloneData.PlayModeLayoutFlags,
                            LayoutFlags.SceneView, marks.Scene);
                    }
                    else
                    {
                        cloneData.EditModeLayoutFlags = EditorModeViewsUtil.SetFlag(cloneData.EditModeLayoutFlags,
                            LayoutFlags.GameView, marks.Game);
                        cloneData.EditModeLayoutFlags = EditorModeViewsUtil.SetFlag(cloneData.EditModeLayoutFlags,
                            LayoutFlags.ConsoleWindow, marks.Console);
                    }

                    // Update and save to disk
                    cloneConfiguration.Data = cloneData;
                    CloneDataFile.SaveToFile(cloneConfiguration);

                    // Use local state to update editor mode
                    var editorMode = EditorApplication.isPlaying
                        ? cloneData.PlayModeLayoutFlags
                        : cloneData.EditModeLayoutFlags;

                    if (!ModeSwitcher.IsView(editorMode))
                    {
#if UNITY_2023_2_OR_NEWER
                        ModeSwitcher.SaveCurrentView();
                        // Close the existing window if its open since we are about to reopen once we switch to view
                        // NOTE: We close the window AFTER saving the window layout!
                        ModeSwitcher.CloseCurrentWindow();
                        focusedWindow?.Close();  // Close the focused window so it doesn't appear to be open after we switch layouts
                        SessionState.SetBool(TopViewPermanence.k_FrameAfterTopViewSwitch, true);
#endif
                    }
                };
            };
        }

        static void LoadStyleSheets(string name, VisualElement target)
        {
            const string k_StyleSheetsPath = "StyleSheets/Toolbars/";

            var path = k_StyleSheetsPath + name;

            var common = EditorGUIUtility.Load($"{path}Common.uss") as StyleSheet;
            if (common != null)
                target.styleSheets.Add(common);

            var themeSpecificName = EditorGUIUtility.isProSkin ? "Dark" : "Light";
            var themeSpecific = EditorGUIUtility.Load($"{path}{themeSpecificName}.uss") as StyleSheet;
            if (themeSpecific != null)
                target.styleSheets.Add(themeSpecific);
        }

#if UNITY_USE_MULTIPLAYER_ROLES
        static void AddMultiplayerRoleDropdown(Toolbar toolbar)
        {
            var toolbarButton = new EditorToolbarDropdown();
            ToolbarExtensions.OnCreateMultiplayerRoleDropdown(toolbarButton);
            toolbar.Add(toolbarButton);
        }
#endif
    }
}