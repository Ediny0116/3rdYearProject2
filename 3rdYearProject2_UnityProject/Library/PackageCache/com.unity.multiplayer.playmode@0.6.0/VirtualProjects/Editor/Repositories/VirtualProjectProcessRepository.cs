using System;
using System.Collections.Generic;
using Unity.Multiplayer.Playmode.Common.Editor;
using UnityEngine;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class VirtualProjectProcessRepository
    {
        readonly ProcessKill m_ProcessKill;
        readonly int m_OurProcessID;
        readonly SessionStateJsonRepository<VirtualProjectIdentifier, ProcessId> m_ProcessRepository;
        readonly TryRunProcessFunc m_TryRunProcess;

        internal VirtualProjectProcessRepository() {   /*for moq*/ }
        
        public delegate bool TryRunProcessFunc(string filename, string arguments, out int processID, out string error);
        VirtualProjectProcessRepository(ProcessKill processKill, int ourProcessID, SessionStateJsonRepository<VirtualProjectIdentifier, ProcessId> processRepository, TryRunProcessFunc tryRunProcess)
        {
            m_ProcessRepository = processRepository;
            m_ProcessKill = processKill;
            m_OurProcessID = ourProcessID;
            m_TryRunProcess = tryRunProcess;
        }

        public static VirtualProjectProcessRepository GetMain(SessionStateJsonRepository<VirtualProjectIdentifier, ProcessId> processRepository)
        {
            return new VirtualProjectProcessRepository(InternalProcessRunner.Kill, InternalProcessRunner.GetCurrentProcessId(),  processRepository, InternalProcessRunner.TryRunProcess);
        }

        public static VirtualProjectProcessRepository GetTest(ProcessKill processKill, int ourProcessID, SessionStateJsonRepository<VirtualProjectIdentifier, ProcessId> processRepository, TryRunProcessFunc func)
        {
            return new VirtualProjectProcessRepository(processKill, ourProcessID, processRepository, func);
        }

        public virtual int Launch(VirtualProjectIdentifier identifier, string[] args)
        {
            var virtualProjectSpecificArgs = new[]
            {
                CommandLineParameters.BuildProjectPathArgument(PathsUtility.GetProjectPathByIdentifier(identifier)),
                CommandLineParameters.k_CloneProcess,
                CommandLineParameters.k_ForgetProjectPath,
                CommandLineParameters.k_VirtualLibraryFolder,
                CommandLineParameters.k_AssetDatabaseReadOnly,
                CommandLineParameters.k_DisableDirectoryMonitor,
#if !UNITY_MPPM_FORCE_MAIN_WINDOW
                CommandLineParameters.k_NoMainWindow,
                CommandLineParameters.k_SuppressDefaultMenuEntries,
#endif
                CommandLineParameters.BuildLogFileArgument(identifier),
                CommandLineParameters.k_ChannelServiceOnStartup,
                CommandLineParameters.BuildChannelServiceChannelNameArgument(MessagingServiceConstants.DefaultChannelName),
                CommandLineParameters.BuildChannelServicePortArgument,
                CommandLineParameters.BuildMainProcessIdArgument(m_OurProcessID.ToString()),
                CommandLineParameters.BuildVirtualProjectIdentifierArgument(identifier),
            };

            var allArgs = args == null ? new List<string>() : new List<string>(args);
            allArgs.AddRange(virtualProjectSpecificArgs);
            var arguments = string.Join(" ", allArgs);
            var executablePath = Paths.GetApplicationPath(Application.platform == RuntimePlatform.OSXEditor ? "Contents/MacOS/Unity" : "");

            if (!m_TryRunProcess(executablePath, arguments, out var p, out var error))
            {
                Debug.LogError($"Launch Failed: {executablePath}{Environment.NewLine}Error:{Environment.NewLine}{error}");
                return -1;
            }

            var processId = new ProcessId(p);

            m_ProcessRepository.Create(identifier, processId);

            return processId.Value;
        }

        public virtual bool TryGetProcessInfo(VirtualProjectIdentifier identifier, out int processId)
        {
            processId = -1;
            if (m_ProcessRepository.TryGetValue(identifier, out var boxedProcessId))
            {
                processId = boxedProcessId.Value;
            }
            return processId != -1;
        }

        public virtual void Close(VirtualProjectIdentifier identifier)
        {
            if (!m_ProcessRepository.TryGetValue(identifier, out var processId)) return;
            if (processId.Value == -1) return;

            m_ProcessKill(processId.Value);
            m_ProcessRepository.Delete(identifier);
        }
    }
}