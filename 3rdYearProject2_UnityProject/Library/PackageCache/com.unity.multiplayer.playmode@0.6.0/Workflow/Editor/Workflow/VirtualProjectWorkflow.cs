using System;
using Unity.Multiplayer.Playmode.Common.Editor;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    [InitializeOnLoad]
    static class VirtualProjectWorkflow
    {
        public static event Action<bool> OnInitialized
        {
            add
            {
                if (IsInitialized)
                {
                    value?.Invoke(IsMainEditor);
                    return;
                }

                s_PendingOnInitializedCallbacks += value;
            }
            remove
            {
                if (IsInitialized)
                {
                    return;
                }

                s_PendingOnInitializedCallbacks -= value;
            }
        }

        public static event Action<bool> OnDisabled;

        public static bool IsInitialized { get; private set; }
        public static bool IsMainEditor { get; private set; }

        static Action<bool> s_PendingOnInitializedCallbacks;

        static WorkflowMainEditorContext s_WorkflowMainEditorContext;
        static WorkflowCloneContext s_WorkflowCloneContext;

        static VirtualProjectWorkflow()
        {
            DuplicateKeyChecker.Clear();
            EditorContexts.OnInitialized += () =>
            {
                Debug.Assert(!CommandLineParameters.ReadNoDownChainDependencies());
                var systemDataStore = VirtualProjectsEditor.IsClone
                    ? SystemDataStore.GetClone()
                    : SystemDataStore.GetMain();

                if (systemDataStore.GetIsMppmActive())
                {
                    InitializeMPPMContexts();
                }
            };
        }

        // Should only being called by MultiplayerPlayModeSettings
        public static void UpdateMPPMRuntimeState(bool isMppmActive)
        {
            if (!IsInitialized && isMppmActive)
            {
                InitializeMPPMContexts();
            }
            else if (IsInitialized && !isMppmActive)
            {
                DisableMPPMContexts();
            }
        }

        static void InitializeMPPMContexts()
        {
            if (VirtualProjectsEditor.IsClone)
            {
                s_WorkflowCloneContext = new WorkflowCloneContext(EditorContexts.CloneContext);
            }
            else
            {
                s_WorkflowMainEditorContext = new WorkflowMainEditorContext(EditorContexts.MainEditorContext);
            }

            IsInitialized = true;
            IsMainEditor = s_WorkflowMainEditorContext != null;
            s_PendingOnInitializedCallbacks?.Invoke(IsMainEditor);
            s_PendingOnInitializedCallbacks = null;
        }

        static void DisableMPPMContexts()
        {
            OnDisabled?.Invoke(IsMainEditor);
            IsInitialized = false;
            IsMainEditor = default;
            s_WorkflowCloneContext = null;
            s_WorkflowMainEditorContext = null;
            DuplicateKeyChecker.Clear();
        }

        internal static WorkflowMainEditorContext WorkflowMainEditorContext
        {
            get
            {
                if (VirtualProjectsEditor.IsClone)
                {
                    throw new NotSupportedException("Main Editor functionality cannot be accessed from clones.");
                }

                return s_WorkflowMainEditorContext;
            }
        }

        internal static WorkflowCloneContext WorkflowCloneContext
        {
            get
            {
                if (!VirtualProjectsEditor.IsClone)
                {
                    throw new NotSupportedException("Clone functionality cannot be accessed from the main Editor.");
                }

                return s_WorkflowCloneContext;
            }
        }
    }
}