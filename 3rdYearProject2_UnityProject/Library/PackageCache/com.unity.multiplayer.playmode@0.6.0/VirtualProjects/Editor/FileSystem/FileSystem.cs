using System;
using System.Collections.Generic;
using System.IO;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    delegate string FileSystemGetParentPath(string path);
    delegate void FileSystemCreateDirectory(string path);
    delegate IReadOnlyCollection<string> FileSystemGetDirectoryNames(string path);
    delegate bool FileSystemIsPathValid(string path);
    delegate void FileSystemCopyFile(string sourcePath, string destinationPath);
    delegate string FileSystemReadFile(string path);
    delegate void FileSystemWriteFile(string path, string content);

    static class InternalFileSystem
    {
        public static string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Could not extract parent path.", nameof(path));
            }

            return new DirectoryInfo(path).Parent?.FullName ?? string.Empty;
        }

        public static void CreateDirectory(string projectPath)
        {
            if (Directory.Exists(projectPath))
            {
                return;
            }

            Directory.CreateDirectory(projectPath);
        }

        public static string[] GetDirectoryNames(string path)
        {
            if (!Directory.Exists(path))
            {
                return Array.Empty<string>();
            }

            var directories = Directory.GetDirectories(path);
            var select = new List<string>();
            foreach (var id in directories)
            {
                select.Add(Path.GetFileName(id));
            }
            return select.ToArray();
        }

        public static void CopyFile(string sourcePath, string destinationPath)
        {
            File.Copy(sourcePath, destinationPath);
        }

        public static string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        public static void WriteFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public static void Delete(string path)
        {
            Directory.Delete(path, true);
        }
    }
}