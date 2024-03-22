using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class WindowLayoutCheckboxes : VisualElement
    {
        static readonly string UXML = $"{UXMLPaths.UXMLWorkflowRoot}/UI/WindowLayoutPopout.uxml";

        public struct Marks
        {
            public bool Console;
            public bool Game;
            public bool Hierarchy;
            public bool Inspector;
            public bool Scene;

            internal static Marks FromWindowLayoutCheckboxes(WindowLayoutCheckboxes checkboxes)
            {
                return new Marks
                {
                    Console = checkboxes.Console.value,
                    Game = checkboxes.Game.value,
                    Hierarchy = checkboxes.Hierarchy.value,
                    Inspector = checkboxes.Inspector.value,
                    Scene = checkboxes.Scene.value,
                };
            }
        }

        Toggle Console => this.Q<Toggle>(nameof(Console));
        Toggle Game => this.Q<Toggle>(nameof(Game));
    new Toggle Hierarchy => this.Q<Toggle>(nameof(Hierarchy));
        Toggle Inspector => this.Q<Toggle>(nameof(Inspector));
        Toggle Scene => this.Q<Toggle>(nameof(Scene));
        Button Apply => this.Q<Button>(nameof(Apply));

        static bool s_HasRegistered;

        public event Action<Marks> ApplyEvent;

        public WindowLayoutCheckboxes()
        {
            RefreshLayout();
        }

        public void RefreshLayout()
        {
            Clear();
            RemoveFromHierarchy();
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML).CloneTree(this);

            var mode = EditorApplication.isPlaying
                ? CloneDataFile.LoadFromFile(VirtualProjectWorkflow.WorkflowCloneContext.CloneDataFile).PlayModeLayoutFlags
                : CloneDataFile.LoadFromFile(VirtualProjectWorkflow.WorkflowCloneContext.CloneDataFile).EditModeLayoutFlags;

            Console.value = mode.HasFlag(LayoutFlags.ConsoleWindow);
            Game.value = mode.HasFlag(LayoutFlags.GameView);

            if (EditorApplication.isPlaying)
            {
                Inspector.value = mode.HasFlag(LayoutFlags.InspectorWindow);
                Hierarchy.value = mode.HasFlag(LayoutFlags.SceneHierarchyWindow);
                Scene.value = mode.HasFlag(LayoutFlags.SceneView);
            }
            Inspector.SetEnabled(EditorApplication.isPlaying);
            Hierarchy.SetEnabled(EditorApplication.isPlaying);
            Scene.SetEnabled(EditorApplication.isPlaying);
        }

        void HandleUIUpdated(ChangeEvent<bool> ev)
        {
            if (!HasAnyToggle(Toggles))
            {
                var toggle = (Toggle)ev.target;
                toggle.SetValueWithoutNotify(true);
            }
        }

        void HandleApply(ClickEvent _)
        {
            ApplyEvent?.Invoke(Marks.FromWindowLayoutCheckboxes(this));
        }

        static bool HasAnyToggle(IReadOnlyList<Toggle> toggles)
        {
            for (var index = 0; index < toggles.Count; index++)
            {
                var t = toggles[index];
                if (t.value) return true;
            }

            return false;
        }

        public void FromMarksWithoutNotify(Marks marks)
        {
            Console.SetValueWithoutNotify(marks.Console);
            Game.SetValueWithoutNotify(marks.Game);
            Hierarchy.SetValueWithoutNotify(marks.Hierarchy);
            Inspector.SetValueWithoutNotify(marks.Inspector);
            Scene.SetValueWithoutNotify(marks.Scene);
        }

        public static void ListenToCheckBoxes(WindowLayoutCheckboxes windowLayoutCheckboxes)
        {
            if (s_HasRegistered)
            {
                windowLayoutCheckboxes.Console.UnregisterValueChangedCallback(windowLayoutCheckboxes.HandleUIUpdated);
                windowLayoutCheckboxes.Game.UnregisterValueChangedCallback(windowLayoutCheckboxes.HandleUIUpdated);
                windowLayoutCheckboxes.Apply.UnregisterCallback<ClickEvent>(windowLayoutCheckboxes.HandleApply);

                windowLayoutCheckboxes.Hierarchy.UnregisterValueChangedCallback(windowLayoutCheckboxes.HandleUIUpdated);
                windowLayoutCheckboxes.Inspector.UnregisterValueChangedCallback(windowLayoutCheckboxes.HandleUIUpdated);
                windowLayoutCheckboxes.Scene.UnregisterValueChangedCallback(windowLayoutCheckboxes.HandleUIUpdated);
                s_HasRegistered = false;
            }

            if (!s_HasRegistered)
            {
                windowLayoutCheckboxes.Console.RegisterValueChangedCallback(windowLayoutCheckboxes.HandleUIUpdated);
                windowLayoutCheckboxes.Game.RegisterValueChangedCallback(windowLayoutCheckboxes.HandleUIUpdated);
                windowLayoutCheckboxes.Apply.RegisterCallback<ClickEvent>(windowLayoutCheckboxes.HandleApply);

                windowLayoutCheckboxes.Hierarchy.RegisterValueChangedCallback(windowLayoutCheckboxes.HandleUIUpdated);
                windowLayoutCheckboxes.Inspector.RegisterValueChangedCallback(windowLayoutCheckboxes.HandleUIUpdated);
                windowLayoutCheckboxes.Scene.RegisterValueChangedCallback(windowLayoutCheckboxes.HandleUIUpdated);
                s_HasRegistered = true;
            }
        }

        public static Vector2 ToggleDimensions { get; } = new Vector2(117, 20);

        public Toggle[] Toggles => new[] { Scene, Inspector, Hierarchy, Game, Console };
    }
}