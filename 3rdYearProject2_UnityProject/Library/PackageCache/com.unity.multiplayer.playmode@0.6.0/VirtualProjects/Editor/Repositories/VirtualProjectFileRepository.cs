using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Multiplayer.Playmode.Common.Editor;
using UnityEngine;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class VirtualProjectFileRepository 
    {
        // We subtract longest file length found in this package from the max length supported by Windows
        // to account for additional path length applied to subdirectories (i.e 259 minus 77 chars)
        const int k_MaxPathLengthForWindows = 182;
        const string k_WindowsExtendedPathLengthPrefix = "\\\\?\\";
        
        internal static readonly IReadOnlyCollection<string> PathsRequiredForProject = new[]
        {
            "Assets",
            "ProjectSettings",
            "Packages",
        };

        static readonly IReadOnlyCollection<string> PathsRequiringSymlink = new[]
        {
            "Assets",
            "ProjectSettings",
        };

        readonly FileSystemGetParentPath m_FileSystemGetParentPath;
        readonly FileSystemCreateDirectory m_FileSystemCreateDirectory;
        readonly FileSystemGetDirectoryNames m_FileSystemGetDirectoryNames;
        readonly FileSystemIsPathValid m_FileSystemIsPathValid;
        readonly FileSystemCopyFile m_FileSystemCopyFile;
        readonly FileSystemReadFile m_FileSystemReadFile;
        readonly FileSystemWriteFile m_FileSystemWriteFile;
        readonly TryRunProcessFunc m_TryRunProcessFunc;
        readonly TryDeleteFunc m_TryDeleteFunc;

        internal VirtualProjectFileRepository() {   /*for moq*/ }

        VirtualProjectFileRepository(FileSystemGetParentPath fileSystemGetParentPath, FileSystemCreateDirectory fileSystemCreateDirectory, FileSystemGetDirectoryNames fileSystemGetDirectoryNames, FileSystemIsPathValid fileSystemIsPathValid, FileSystemCopyFile fileSystemCopyFile, FileSystemReadFile fileSystemReadFile, FileSystemWriteFile fileSystemWriteFile, TryRunProcessFunc tryRunFunc, TryDeleteFunc tryDeleteFunc)
        {
            m_FileSystemGetParentPath = fileSystemGetParentPath;
            m_FileSystemCreateDirectory = fileSystemCreateDirectory;
            m_FileSystemGetDirectoryNames = fileSystemGetDirectoryNames;
            m_FileSystemIsPathValid = fileSystemIsPathValid;
            m_FileSystemCopyFile = fileSystemCopyFile;
            m_FileSystemReadFile = fileSystemReadFile;
            m_FileSystemWriteFile = fileSystemWriteFile;
            m_TryRunProcessFunc = tryRunFunc;
            m_TryDeleteFunc = tryDeleteFunc;
        }

        public delegate bool TryRunProcessFunc(string filename, string arguments, out int processID, out string error);
        public delegate void TryDeleteFunc(string directory);
        public static VirtualProjectFileRepository GetMain()
        {
            return new VirtualProjectFileRepository(InternalFileSystem.GetParentPath, InternalFileSystem.CreateDirectory, InternalFileSystem.GetDirectoryNames, IsPathValid, InternalFileSystem.CopyFile, InternalFileSystem.ReadFile, InternalFileSystem.WriteFile, InternalProcessRunner.TryRunProcessWaitForExit, InternalFileSystem.Delete);
        }

        public static VirtualProjectFileRepository GetTest(FileSystemGetParentPath fileSystemGetParentPath, FileSystemCreateDirectory fileSystemCreateDirectory, FileSystemGetDirectoryNames fileSystemGetDirectoryNames, FileSystemIsPathValid fileSystemIsPathValid, FileSystemCopyFile fileSystemCopyFile, FileSystemReadFile fileSystemReadFile, FileSystemWriteFile fileSystemWriteFile, TryRunProcessFunc tryRunProcessFunc, TryDeleteFunc tryDeleteFunc)
        {
            return new VirtualProjectFileRepository(fileSystemGetParentPath, fileSystemCreateDirectory,  fileSystemGetDirectoryNames,  fileSystemIsPathValid, fileSystemCopyFile, fileSystemReadFile, fileSystemWriteFile, tryRunProcessFunc, tryDeleteFunc);
        }

        public virtual bool CreateProject(VirtualProjectIdentifier identifier, 
                out string cloneProjectPath, out CreateAPIErrorInfo errorState)
        {
            errorState = default;
            cloneProjectPath = PathsUtility.GetProjectPathByIdentifier(identifier);

            // This is to verify if the directory on Windows respects the supported path length
            if (!m_FileSystemIsPathValid(cloneProjectPath))
            {
                errorState = new CreateAPIErrorInfo { Error = CreateAPIError.ProjectUnableToBeCreated };
                return false;
            }

            m_FileSystemCreateDirectory(cloneProjectPath);

            var mainProjectDirectory = m_FileSystemGetParentPath(Paths.GetCurrentProjectDataPath());
            var cloneProjectPathCopyForLambda = cloneProjectPath;
            foreach (var path in PathsRequiringSymlink)
            {
                var source = Path.Combine(mainProjectDirectory, path);
                var destination = Path.Combine(cloneProjectPathCopyForLambda, path);
                // build the symlink command and then run it for every folder in our required folders
                var (buildLinkCommand, filename, arguments) = BuildLinkCommand(source, destination);
                if (!m_TryRunProcessFunc(filename, arguments, out _, out var error))
                {
                    errorState = new CreateAPIErrorInfo
                    {
                        Error = CreateAPIError.SymLinkUnableToBePerformed,
                        ShellCommand = buildLinkCommand,
                        ShellError = error,
                    };
                    return false;
                }
            }

            m_FileSystemCreateDirectory(Path.Combine(cloneProjectPath, "Packages"));

            // Copy packages-lock.json as-is. Even if it contained relative paths, those will get
            // updated automatically once the new manifest is loaded.
            m_FileSystemCopyFile(
                Path.Combine(mainProjectDirectory, "Packages", "packages-lock.json"),
                Path.Combine(cloneProjectPath, "Packages", "packages-lock.json"));

            return true;
        }

        public virtual void DeleteProject(VirtualProjectIdentifier identifier)
        {
            var cloneProjectPath = PathsUtility.GetProjectPathByIdentifier(identifier);
            
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                // Deleting on windows works better with bash OS cmd call
                var (removeDirectoryCommand, filename, arguments) = WindowsBuildRemoveDirectoryCommand(cloneProjectPath);
                if (!m_TryRunProcessFunc(filename, arguments, out _, out var error))
                {
                    Debug.LogError($"Command Failed: {removeDirectoryCommand}{Environment.NewLine}Error:{Environment.NewLine}{error}");
                }
            }
            else
            {
                m_TryDeleteFunc(cloneProjectPath);
            }
        }

        public virtual bool HasProject(VirtualProjectIdentifier identifier, out GetAPIErrorInfo errorState)
        {
            foreach (var projectDirectoryName in m_FileSystemGetDirectoryNames(Paths.CurrentProjectVirtualProjectsFolder))
            {
                var hasParse = VirtualProjectIdentifier.TryParse(projectDirectoryName, out var identifierFromDirectory);
                var isSpecifiedDirectory = hasParse && identifierFromDirectory == identifier;
                if (isSpecifiedDirectory)
                {
                    if (!HasRequiredDirectoriesForClone(identifier, out var missingDirectories))
                    {
                        errorState = new GetAPIErrorInfo
                        {
                            Error = GetAPIError.MissingRequiredDirectories,
                            Directories = missingDirectories,
                        };
                        return false;
                    }

                    errorState = default; 
                    return true;
                }
            }

            errorState = new GetAPIErrorInfo { Error = GetAPIError.ProjectNotFound };
            return false;
        }

        public virtual VirtualProjectIdentifier[] GetProjects()
        {
            var results = new List<VirtualProjectIdentifier>();
            foreach (var name in m_FileSystemGetDirectoryNames(Paths.CurrentProjectVirtualProjectsFolder))
            {
                var hasParse = VirtualProjectIdentifier.TryParse(name, out var identifier);
                var hasFiles = hasParse && HasRequiredDirectoriesForClone(identifier, out _);

                if (hasFiles)
                {
                    results.Add(identifier);
                }
            }

            return results.ToArray();
        }

        bool HasRequiredDirectoriesForClone(VirtualProjectIdentifier identifier, out string[] missingDirectories)
        {
            var projectPath = PathsUtility.GetProjectPathByIdentifier(identifier);
            var folderNames = m_FileSystemGetDirectoryNames(projectPath);
            var resultMissingDirectories = new List<string>(PathsRequiredForProject);
            for (var index = resultMissingDirectories.Count - 1; index >= 0; index--)
            {
                var path = resultMissingDirectories[index];
                foreach (var info in folderNames)
                {
                    if (path == info)
                    {
                        resultMissingDirectories.Remove(path);
                    }
                }
            }

            missingDirectories = resultMissingDirectories.ToArray();
            return resultMissingDirectories.Count == 0;
        }

        public static bool IsPathValid(string path) =>
            !string.IsNullOrWhiteSpace(path) &&
            !(Application.platform == RuntimePlatform.WindowsEditor && path.Length > k_MaxPathLengthForWindows);

        public virtual void RegeneratePackageManifest(VirtualProjectIdentifier identifier)
        {
            var mainProjectPath = m_FileSystemGetParentPath(Paths.GetCurrentProjectDataPath());
            var mainProjectManifestPath = Path.Combine(mainProjectPath, "Packages", "manifest.json");

            var cloneProjectPath = PathsUtility.GetProjectPathByIdentifier(identifier);
            var cloneManifestPath = Path.Combine(cloneProjectPath, "Packages", "manifest.json");

            var mainProjectManifest = m_FileSystemReadFile(mainProjectManifestPath);
            var cloneProjectManifest = RewriteManifestRelativePaths(mainProjectManifest);

            m_FileSystemWriteFile(cloneManifestPath, cloneProjectManifest);
        }

        static string RewriteManifestRelativePaths(string manifestContent)
        {
            var manifest = JObject.Parse(manifestContent);
            var dependencies = manifest["dependencies"].ToObject<Dictionary<string, string>>();
            var keys = new List<string>(dependencies.Keys);

            foreach (var package in keys)
            {
                if (dependencies[package].StartsWith("file:"))
                {
                    var path = dependencies[package].Remove(0, "file:".Length).Replace("\\", "/");
                    if (!Path.IsPathRooted(path))
                    {
                        dependencies[package] = "file:../../../../Packages/" + path;
                    }
                }
            }

            manifest["dependencies"] = JObject.FromObject(dependencies);
            return manifest.ToString(Formatting.Indented);
        }
        
        static (string buildLinkCommand, string filename, string arguments) BuildLinkCommand(string sourcePath, string destinationPath)
        {
            string buildLinkCommand;
            switch (Application.platform)
            {
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.OSXEditor:
                    sourcePath = sourcePath.Replace("'", "'\\''");
                    destinationPath = destinationPath.Replace("'", "'\\''");
                    buildLinkCommand = $"ln -s '{sourcePath}' '{destinationPath}'";
                    break;
                case RuntimePlatform.WindowsEditor:
                    buildLinkCommand =
                        $"mklink /J \"{k_WindowsExtendedPathLengthPrefix}{destinationPath}\" \"{k_WindowsExtendedPathLengthPrefix}{sourcePath}\"";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Application.platform));
            }

            var filename = "/bin/bash";
            var argumentPrefix = "-c";

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                filename = "cmd.exe";
                argumentPrefix = "/C";
            }

            var arguments = $"{argumentPrefix} \"{buildLinkCommand}\"";
            return (buildLinkCommand, filename, arguments);
        }

        static (string removeDirectoryCommand, string filename, string arguments) WindowsBuildRemoveDirectoryCommand(string path)
        {
            // Windows has many issues deleting folders containing symlinks,
            // and will most of the time throw an UnauthorizedAccessException
            // when attempting to delete with Directory.Delete.
            var removeDirectoryCommand = $"rmdir /s /q \"{path}\"";
            var filename = "cmd.exe";
            var argumentPrefix = "/C";
            var arguments = $"{argumentPrefix} \"{removeDirectoryCommand}\"";
            
            return (removeDirectoryCommand, filename, arguments);
        }
    }
}