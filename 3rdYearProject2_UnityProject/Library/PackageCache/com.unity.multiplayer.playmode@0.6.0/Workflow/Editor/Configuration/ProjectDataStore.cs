using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Unity.Multiplayer.Playmode.Common.Editor;
using Unity.Multiplayer.Playmode.Common.Runtime;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class ProjectDataStore
    {
        public enum ProjectDataStoreError
        {
            None,
            TagEmpty,
            TagDuplicate,
            TagMissing,
        } 
        
        public const string k_Filename = "VirtualProjectsConfig.json";
        const string k_InMemoryPlayerTagsKey = "playerTags";
        
        static readonly string DataStorePathRelativeToMainEditor = Paths.GetCurrentProjectDataPath("..", "ProjectSettings");
        
        // Use both the InMemoryRepository and VirtualProjectConfig to store player tags -
        // InMemoryRepository is the cache, PlayerTagsData(JSON File) is the persistence.
        readonly InMemoryRepository<string, List<string>> m_TagsRepository = new();
        PlayerTagsData m_PlayerTagsData;
        readonly string m_Path;

        internal static ProjectDataStore GetMain()
        {
            return new ProjectDataStore(DataStorePathRelativeToMainEditor);
        }

        internal ProjectDataStore(string directory)
        {
            m_Path = Path.Combine(directory, k_Filename);
            if (File.Exists(m_Path))
            {
                m_PlayerTagsData = Deserialize(m_Path);
            }
            // Create the initial cache from the persistent config
            m_TagsRepository.Create(k_InMemoryPlayerTagsKey, ReadPlayerTags());
        }

        public List<string> GetAllPlayerTags()
        {
            var nonNullTags = new List<string>();
            if (m_TagsRepository.TryGetValue(k_InMemoryPlayerTagsKey, out var tags))
            {
                foreach (var tag in tags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        nonNullTags.Add(tag);
                    }
                }
            }
            return nonNullTags;
        }

        public bool Add(string tag, out ProjectDataStoreError error)
        {
            error = ProjectDataStoreError.None;
            if (string.IsNullOrWhiteSpace(tag))
            {
                error = ProjectDataStoreError.TagEmpty;
                return false;
            }

            if (Contains(tag))
            {
                error = ProjectDataStoreError.TagDuplicate;
                return false;
            }

            var tags = new List<string>();
            if (!m_TagsRepository.ContainsKey(k_InMemoryPlayerTagsKey))
            {
                tags.Add(tag);
                m_TagsRepository.Create(k_InMemoryPlayerTagsKey, tags);
            }
            else
            {
                m_TagsRepository.Update(k_InMemoryPlayerTagsKey, t => { t.Add(tag); }, out tags);
            }

            WritePlayerTags(tags);
            return true;
        }

        public bool Has(string tag)
        {
            return !string.IsNullOrWhiteSpace(tag) && Contains(tag);
        }

        public bool Delete(string tag, out ProjectDataStoreError error)
        {
            error = ProjectDataStoreError.None;
            if (string.IsNullOrWhiteSpace(tag))
            {
                error = ProjectDataStoreError.TagEmpty;
                return false;
            }

            if (!Contains(tag))
            {
                error = ProjectDataStoreError.TagMissing;
                return false;
            }

            m_TagsRepository.Update(k_InMemoryPlayerTagsKey, tags => tags.Remove(tag), out var state);

            WritePlayerTags(state);
            return true;
        }

        internal void DeleteAll()
        {
            m_TagsRepository.Delete(k_InMemoryPlayerTagsKey);
            WritePlayerTags(new List<string>());
        }

        bool Contains(string tag)
        {
            return m_TagsRepository.TryGetValue(k_InMemoryPlayerTagsKey, out var tags) && tags.Contains(tag);
        }


        internal List<string> ReadPlayerTags()
        {
            if (!File.Exists(m_Path))
            {
                return new List<string>();
            }

            m_PlayerTagsData = Deserialize(m_Path);
            return m_PlayerTagsData.PlayerTags;
        }

        internal void WritePlayerTags(List<string> playerTags)
        {
            m_PlayerTagsData.PlayerTags = playerTags;
            var json = JsonConvert.SerializeObject(m_PlayerTagsData, Formatting.Indented);
            File.WriteAllBytes(m_Path, Encoding.UTF8.GetBytes(json));
        }

        static PlayerTagsData Deserialize(string path)
        {
            try
            {
                return JsonConvert.DeserializeObject<PlayerTagsData>(Encoding.UTF8.GetString(File.ReadAllBytes(path)));
            }
            catch (Exception e)
            {
                MppmLog.Error($"Could not load configuration file at {path} with error: {e.Message}");
                return new PlayerTagsData();
            }
        }
    }
}