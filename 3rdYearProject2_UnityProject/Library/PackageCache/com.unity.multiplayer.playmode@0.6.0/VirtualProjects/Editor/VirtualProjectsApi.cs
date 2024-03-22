using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Multiplayer.Playmode.Common.Editor;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    struct GetAPIErrorInfo
    {
        public GetAPIError Error;
        public string[] Directories;
    }
    enum GetAPIError
    {
        None,
        ProjectNotFound,
        MissingRequiredDirectories,
    }
    struct CreateAPIErrorInfo
    {
        public CreateAPIError Error;
        public string ShellCommand;
        public string ShellError;
    }
    enum CreateAPIError
    {
        None,
        ProjectUnableToBeCreated,
        SymLinkUnableToBePerformed,
    }
    enum DeleteAPIError
    {
        None,
        ProjectNotFound,
        ProjectCurrentlyInUse,
    }

    class VirtualProjectsApi 
    {
        internal const string k_FilterAll = "__all";
        
        internal delegate VirtualProjectIdentifier CreateVirtualProjectIdentifierFunc(string prefix);

        readonly VirtualProjectFileRepository m_FileRepository;
        readonly VirtualProjectProcessRepository m_ProcessRepository;
        readonly SessionStateJsonRepository<VirtualProjectIdentifier, VirtualProjectState> m_StateRepository;
        readonly MainEditorSystems m_MainEditorSystems;
        readonly CreateVirtualProjectIdentifierFunc m_CreateVirtualProjectIdentifierFunc;
        
        internal VirtualProjectsApi() {   /*for moq*/ }
        
        VirtualProjectsApi(VirtualProjectFileRepository fileRepository, VirtualProjectProcessRepository processRepository, SessionStateJsonRepository<VirtualProjectIdentifier, VirtualProjectState> stateRepository, MainEditorSystems mainEditorSystems, CreateVirtualProjectIdentifierFunc createVirtualProjectIdentifierFunc)
        {
            m_FileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            m_ProcessRepository = processRepository ?? throw new ArgumentNullException(nameof(processRepository));
            m_StateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
            m_MainEditorSystems = mainEditorSystems ?? throw new ArgumentNullException(nameof(mainEditorSystems));
            m_CreateVirtualProjectIdentifierFunc = createVirtualProjectIdentifierFunc ?? throw new ArgumentNullException(nameof(createVirtualProjectIdentifierFunc));
        }
        
        public static VirtualProjectsApi GetMain(VirtualProjectFileRepository fileRepository, VirtualProjectProcessRepository processRepository, SessionStateJsonRepository<VirtualProjectIdentifier, VirtualProjectState> stateRepository, MainEditorSystems mainEditorSystems)
        {
            return new VirtualProjectsApi(fileRepository, processRepository, stateRepository, mainEditorSystems, VirtualProjectIdentifier.NewVirtualProjectIdentifier);
        }

        public static VirtualProjectsApi GetTest(VirtualProjectFileRepository fileRepository, VirtualProjectProcessRepository processRepository, SessionStateJsonRepository<VirtualProjectIdentifier, VirtualProjectState> stateRepository, MainEditorSystems mainEditorSystems, CreateVirtualProjectIdentifierFunc createVirtualProjectIdentifierFunc)
        {
            return new VirtualProjectsApi(fileRepository, processRepository, stateRepository, mainEditorSystems, createVirtualProjectIdentifierFunc);
        }

        public virtual VirtualProject[] GetProjects(string prefix)
        {
            var virtualProjects = new List<VirtualProject>();
            foreach (var vpi in m_FileRepository.GetProjects())
            {
                if (prefix == k_FilterAll || HasPrefix(vpi, prefix))
                {
                    virtualProjects.Add(new VirtualProject(vpi, m_FileRepository, m_ProcessRepository, m_StateRepository, m_MainEditorSystems, VirtualProject.Default));
                }
            }

            return virtualProjects.ToArray();
        }

        public virtual bool TryGet(
            VirtualProjectIdentifier virtualProjectIdentifier,
            [NotNullWhen(true)] out VirtualProject virtualProject, out GetAPIErrorInfo errorState)
        {
            virtualProject = m_FileRepository.HasProject(virtualProjectIdentifier, out errorState)
                ? new VirtualProject(virtualProjectIdentifier, m_FileRepository, m_ProcessRepository, m_StateRepository, m_MainEditorSystems, VirtualProject.Default)
                : null;

            return errorState.Error == GetAPIError.None;
        }

        public virtual bool IsNullOrInvalid(VirtualProject virtualProject)
        {
            if (virtualProject == null) return true;
            if (!Contains(virtualProject.Identifier)) return true;
            return false;
        }

        public virtual bool Create(string prefix, out VirtualProject project, out string projectDirectory, out CreateAPIErrorInfo errorState)
        {
            project = null;
            projectDirectory = string.Empty;
            errorState = default;
            
            for (var attempts = 0; attempts < 10; attempts++)
            {
                var identifier = m_CreateVirtualProjectIdentifierFunc(prefix);
                
                project = null;
                if (!Contains(identifier))
                {
                    var hasCreated = m_FileRepository.CreateProject(identifier, out projectDirectory, out errorState);
                    if (hasCreated)
                    {
                        project = new VirtualProject(identifier, m_FileRepository, m_ProcessRepository,
                            m_StateRepository,
                            m_MainEditorSystems, VirtualProject.Default);
                    } 
                    else if(errorState.Error == CreateAPIError.SymLinkUnableToBePerformed)
                    {
                        // Remove empty directory since we failed to create the directory
                        // No longer attempt to retry since we will never succeed
                        m_FileRepository.DeleteProject(identifier);  
                        return false;
                    }
                }

                if (project != null)
                {
                    return true;
                }
            }
            
            return false;
        }

        public virtual bool Delete(VirtualProject virtualProject, out DeleteAPIError errorState)
        {
            errorState = DeleteAPIError.None;
            var hasProject = Contains(virtualProject.Identifier);
            if (!hasProject)
            {
                errorState = DeleteAPIError.ProjectNotFound;
                return false;
            }
            if (virtualProject.EditorState != EditorState.NotLaunched)
            {
                errorState = DeleteAPIError.ProjectCurrentlyInUse;
                return false;
            }

            m_FileRepository.DeleteProject(virtualProject.Identifier);
            return true;
        }

        bool Contains(VirtualProjectIdentifier virtualProjectIdentifier)
        {
            foreach (var vp in GetProjects(virtualProjectIdentifier.Prefix))
            {
                if (vp.Identifier == virtualProjectIdentifier) return true;
            }
            return false;
        }

        public static bool HasPrefix(VirtualProjectIdentifier virtualProjectIdentifier, string prefix)
        {
            return string.IsNullOrWhiteSpace(prefix) && string.IsNullOrWhiteSpace(virtualProjectIdentifier.Prefix) ||
                   prefix?.Replace("-", string.Empty).Trim() == virtualProjectIdentifier.Prefix?.Replace("-", string.Empty).Trim();
        }
    }
}