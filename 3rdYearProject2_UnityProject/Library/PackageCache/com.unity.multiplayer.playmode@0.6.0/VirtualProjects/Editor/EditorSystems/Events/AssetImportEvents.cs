using System;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class AssetImportEvents
    {
        public event Action<bool, int> RequestImport;
        public event Action<VirtualProjectIdentifier> FailedImport;
        public void InvokeRequestImport(bool didDomainReload, int numAssetsChanged)
        {
            RequestImport?.Invoke(didDomainReload, numAssetsChanged);
        }
        public bool InvokeFailedImport(VirtualProjectIdentifier identifier)
        {
            if (FailedImport != null)
            {
                FailedImport.Invoke(identifier);
                return true;
            }

            return false;
        }
    }
}