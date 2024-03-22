using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Multiplayer.Playmode.Common.Editor;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    enum EditorState
    {
        NotLaunched,
        Launching,
        Launched,
        UnexpectedlyStopped,
    }

    enum LaunchProjectError
    {
        None,
        ProjectNotFound,
        ProjectCurrentlyInUse,
    }

    enum CloseProjectError
    {
        None,
        ProjectNotFound,
    }

    class VirtualProject
    {
        internal static readonly ProcessIsRunning Default = InternalProcessRunner.IsRunning;
        
        readonly ProcessIsRunning m_ProcessIsRunning;
        readonly VirtualProjectFileRepository m_FileRepository;
        readonly VirtualProjectProcessRepository m_ProcessRepository;
        readonly SessionStateJsonRepository<VirtualProjectIdentifier, VirtualProjectState> m_StateRepository;
        readonly MainEditorSystems m_MainEditorSystems;
        DateTime m_TimeSinceStartingLaunch;

        internal VirtualProject(
            VirtualProjectIdentifier identifier,
            VirtualProjectFileRepository fileRepository,
            VirtualProjectProcessRepository processRepository,
            SessionStateJsonRepository<VirtualProjectIdentifier, VirtualProjectState> stateRepository,
            MainEditorSystems mainEditorSystems,
            ProcessIsRunning processIsRunning)
        {
            Identifier = identifier;

            m_ProcessIsRunning = processIsRunning;
            m_FileRepository = fileRepository;
            m_ProcessRepository = processRepository;
            m_StateRepository = stateRepository;
            m_MainEditorSystems = mainEditorSystems;
        }

        bool IsCreated
        {
            get
            {
                foreach (var project in m_FileRepository.GetProjects())
                {
                    if (Equals(project, Identifier)) return true;
                }
                return false;
            }
        }

        bool IsRunning => m_ProcessRepository.TryGetProcessInfo(Identifier, out var processInfo) && processInfo != -1 && m_ProcessIsRunning(processInfo);

        bool IsCommunicative =>
            m_StateRepository.TryGetValue(Identifier, out var state)
            && state.FirstHeartbeatReceived != default
            && state.LastHeartbeatReceived != default
            && (m_TimeSinceStartingLaunch - state.LastHeartbeatReceived).Seconds <= MainEditorSystems.k_UnresponsiveTimeoutSeconds;

        internal int ProcessId => m_ProcessRepository.TryGetProcessInfo(Identifier, out var processInfo) ? processInfo : -1;

        public virtual EditorState EditorState
        {
            get
            {
                return (IsRunning, IsCommunicative) switch
                {
                    (false, false) => EditorState.NotLaunched,
                    (false, true) => EditorState.UnexpectedlyStopped,
                    (true, false) => EditorState.Launching,
                    (true, true) => EditorState.Launched,
                };
            }
        }

        [NotNull] public virtual VirtualProjectIdentifier Identifier { get; }

        [NotNull] public string Directory => PathsUtility.GetProjectPathByIdentifier(Identifier);

        public virtual bool Launch(out LaunchProjectError errorState, out int processId, params string[] args)
        {
            errorState = LaunchProjectError.None;
            processId = -1;
            if (!IsCreated)
            {
                errorState = LaunchProjectError.ProjectNotFound;
                return false;
            }

            if (IsRunning)
            {
                errorState = LaunchProjectError.ProjectCurrentlyInUse;
                return false;
            }

            if (!m_StateRepository.ContainsKey(Identifier))
            {
                m_StateRepository.Create(Identifier, new VirtualProjectState());
            }
            else
            {
                m_StateRepository.Update(Identifier, state => state.LastHeartbeatReceived = DateTime.UtcNow, out _);
            }
            
            m_TimeSinceStartingLaunch = DateTime.UtcNow;

            // Recreate the manifest in case the dependencies changed.
            m_FileRepository.RegeneratePackageManifest(Identifier);

            processId = m_ProcessRepository.Launch(Identifier, args);
            m_MainEditorSystems.ApplicationEvents.InvokeCloneLaunching(Identifier, args);
            return true;
        }

        public virtual bool Close(out CloseProjectError errorState)
        {
            errorState = CloseProjectError.None;
            if (!IsCreated)
            {
                errorState = CloseProjectError.ProjectNotFound;
                return false;
            }

            // Clear out the existing state
            if (m_StateRepository.ContainsKey(Identifier))
            {
                m_StateRepository.Delete(Identifier);    
            }
            m_StateRepository.Create(Identifier, new VirtualProjectState());
            m_TimeSinceStartingLaunch = default;

            m_ProcessRepository.Close(Identifier);
            return true;
        }
    }
}