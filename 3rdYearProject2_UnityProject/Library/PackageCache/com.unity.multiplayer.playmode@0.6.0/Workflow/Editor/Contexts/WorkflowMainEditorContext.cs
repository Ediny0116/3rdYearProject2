using Unity.Multiplayer.Playmode.Common.Editor;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class WorkflowMainEditorContext
    {
        public WorkflowMainEditorContext(MainEditorContext mainEditorContext)
        {
            MainPlayerSystems = new MainPlayerSystems();
            {
                LogsRepository = new InMemoryRepository<PlayerIdentifier, BoxedLogCounts>();
                ProjectDataStore = ProjectDataStore.GetMain();
                SystemDataStore = SystemDataStore.GetMain();
                var workflow = new StandardMainEditorWorkflow();
                workflow.Initialize(mppmContext: this, vpContext: mainEditorContext);
            }
            MainPlayerSystems.Listen(mppmContext: this, vpContext: mainEditorContext);
        }


        internal InMemoryRepository<PlayerIdentifier, BoxedLogCounts> LogsRepository { get; }
        internal ProjectDataStore ProjectDataStore { get; }
        internal SystemDataStore SystemDataStore { get; }
        
        internal MainPlayerSystems MainPlayerSystems { get; }
    }
}