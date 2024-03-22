using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Multiplayer.Playmode.Common.Runtime;
using Unity.Multiplayer.Playmode.Common.Editor;
using UnityEngine;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    enum SystemDataStoreError
    {
        None,
        SystemDataIsNull,
        SystemDataPlayerDataIsNullOrEmpty,
        SystemDataPlayersAreNull,
        SystemDataPlayersHaveInvalidIdentifiers,
    }
    
    class SystemDataStore
    {
        public const string Filename = "SystemData.json";
        const string SystemDataKey = "SystemData";

        static readonly string DataStorePathRelativeToMainEditor = Paths.CurrentProjectVirtualProjectsFolder;
        static readonly string DataStorePathRelativeToCloneEditor = Paths.GetCurrentProjectDataPath("..", "..");

        readonly InMemoryRepository<string, SystemData> m_Cache = new();
        readonly string m_FullPath;

        internal static SystemDataStore GetMain()
        {
            var directoryPath = DataStorePathRelativeToMainEditor;
            var fullPath = Path.Combine(directoryPath, Filename);
            var dataStore = new SystemDataStore(fullPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var hasNoSystemDataOnDisk = !File.Exists(dataStore.m_FullPath);
            if (hasNoSystemDataOnDisk)
            {
                // In case the user deletes the library folder with the editor open
                var state = new SystemData
                {
                    EditorVersion = Application.unityVersion,
                    IsMppmActive = true,
                    Data =
                    {
                        {
                            1, PlayerStateJson.NewMain()
                        },
                        {
                            2, PlayerStateJson.NewClone(2)
                        },
                        {
                            3, PlayerStateJson.NewClone(3)
                        },
                        {
                            4, PlayerStateJson.NewClone(4)
                        },
                    },
                };
                dataStore.m_Cache.Create(SystemDataKey, state);
                SerializeToPath(dataStore.m_FullPath, state);
            }
            else
            {
                ReadFromFilePopulateDataStore(dataStore);
            }

            return dataStore;
        }

        internal static SystemDataStore GetClone()
        {
            var directoryPath = DataStorePathRelativeToCloneEditor;
            var fullPath = Path.Combine(directoryPath, Filename);
            Debug.Assert(File.Exists(fullPath), "A clone started up without a system data store. The main editor should have already created this!");
            
            var dataStore = new SystemDataStore(fullPath); 
            ReadFromFilePopulateDataStore(dataStore);
            return dataStore;
        }

        internal static SystemDataStore GetTest(string directoryPath, bool shouldCreateSystemData = false)
        {
            var fullPath = Path.Combine(directoryPath, Filename);
            var dataStore = new SystemDataStore(fullPath);

            if (shouldCreateSystemData)
            {
                var state = new SystemData
                {
                    IsMppmActive = true,
                    Data =
                    {
                        {
                            1, PlayerStateJson.NewMain()
                        },
                        {
                            2, PlayerStateJson.NewClone(2)
                        },
                        {
                            3, PlayerStateJson.NewClone(3)
                        },
                        {
                            4, PlayerStateJson.NewClone(4)
                        },
                    },
                };
                dataStore.m_Cache.Create(SystemDataKey, state);
                SerializeToPath(dataStore.m_FullPath, state);
            }
            
            return dataStore;
        }

        internal SystemDataStore() {   /*for moq*/ }

        SystemDataStore(string fullPath)
        {
            m_FullPath = fullPath;
        }

        public virtual void SavePlayerJson(int index, PlayerStateJson playerStateJson)
        {
            SystemData state;
            if (!m_Cache.ContainsKey(SystemDataKey))
            {
                state = new SystemData
                {
                    Data =
                    {
                        [index] = playerStateJson,
                    },
                };
                m_Cache.Create(SystemDataKey, state);
            }
            else
            {
                m_Cache.Update(SystemDataKey, players=>players.Data[index] = playerStateJson, out state);
            }
            
            SerializeToPath(m_FullPath, state);
        }

        public virtual bool TryLoadPlayerJson(int index, out PlayerStateJson playerStateJson)
        {
            playerStateJson = null;
            return m_Cache.TryGetValue(SystemDataKey, out var state) && state.Data.TryGetValue(index, out playerStateJson);
        }

        public virtual Dictionary<int, PlayerStateJson> LoadAllPlayerJson()
        {
            if (!m_Cache.TryGetValue(SystemDataKey, out var state))
            {
                state = new SystemData();
                m_Cache.Create(SystemDataKey, state);
            }
            
            // Make sure we have no null tags
            foreach (var kv in state.Data)
            {
                kv.Value.Tags ??= new List<string>();
            }

            return state.Data;
        }

        public bool GetIsMppmActive()
        {
            // NOTE: We are active by default
            return !m_Cache.TryGetValue(SystemDataKey, out var state) || state.IsMppmActive;
        }

        // Should only being called by MultiplayerPlayModeSettings
        public void UpdateIsMppmActive(bool isActive)
        {
            SystemData state;
            if (!m_Cache.ContainsKey(SystemDataKey))
            {
                state = new SystemData
                {
                    IsMppmActive = isActive,
                };
                m_Cache.Create(SystemDataKey, state);
            }
            else
            {
                m_Cache.Update(SystemDataKey, players => players.IsMppmActive = isActive, out state);
            }

            SerializeToPath(m_FullPath, state);
        }
        
        public string GetEditorVersion()
        {
            return m_Cache.TryGetValue(SystemDataKey, out var state)
                ? state.EditorVersion
                : string.Empty;
        }

        public void UpdateEditorVersion(string version)
        {
            SystemData state;
            if (!m_Cache.ContainsKey(SystemDataKey))
            {
                state = new SystemData
                {
                    EditorVersion = version,
                };
                m_Cache.Create(SystemDataKey, state);
            }
            else
            {
                m_Cache.Update(SystemDataKey, players => players.EditorVersion = version, out state);
            }

            SerializeToPath(m_FullPath, state);
        }

        public void DeleteAll() // Note: This deletes the editor version data as well
        {
            m_Cache.Delete(SystemDataKey);
            if (File.Exists(m_FullPath))
            {
                File.Delete(m_FullPath);
            }
        }

        public static void ReadFromFilePopulateDataStore(SystemDataStore dataStore)
        {
            var playerJson = DeserializeFromPath(dataStore.m_FullPath);
            if (IsNullOrInvalid(playerJson, out var error))
            {
                MppmLog.Warning($"Player data at '{dataStore.m_FullPath}' is null or invalid! [{error}]");
            }

            dataStore.m_Cache.Create(SystemDataKey, playerJson);
        }

        static void SerializeToPath(string path, SystemData systemData)
        {
            File.WriteAllBytes(path, Encoding.UTF8.GetBytes(SystemData.Serialize(systemData)));
        }

        static SystemData DeserializeFromPath(string path)
        {
            var json = Encoding.UTF8.GetString(File.ReadAllBytes(path));
            var hasDeserialized = SystemData.TryDeserialize(json, out var data);
            Debug.Assert(hasDeserialized, $@"Failed to Deserialize System Data [{json}] at path '{path}'.");
            return data;
        }

        internal static bool IsNullOrInvalid(SystemData systemData, out SystemDataStoreError error)
        {
            if (systemData == null)
            {
                error = SystemDataStoreError.SystemDataIsNull;
                return true;
            }

            if (systemData.Data == null || systemData.Data.Count == 0)
            {
                error = SystemDataStoreError.SystemDataPlayerDataIsNullOrEmpty;
                return true;
            }

            foreach (var p in systemData.Data.Values)
            {
                // We currently don't have null/empty players. but if we ever decide to do so then this needs to change
                if (p == null)
                {
                    error = SystemDataStoreError.SystemDataPlayersAreNull;
                    return true;
                }
                if (p.PlayerIdentifier == null || p.PlayerIdentifier.Guid == Guid.Empty)
                {
                    error = SystemDataStoreError.SystemDataPlayersHaveInvalidIdentifiers;
                    return true;
                }
            }

            error = SystemDataStoreError.None;
            return false;
        }
    }
}