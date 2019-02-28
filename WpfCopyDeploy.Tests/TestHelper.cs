namespace WpfCopyDeploy.Tests
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    public static class TestHelper
    {
        public static FileInfo SettingsFile
        {
            get
            {
                var dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "WpfCopyDeploy.Tests.AppData")).CreateIfNotExists();
                return new FileInfo(Path.Combine(dir.FullName, "Settings.xml"));
            }
        }

        public static void WaitForIO()
        {
            if (Path.GetTempPath().StartsWith(@"C:\Users\VssAdministrator\AppData\Local\Temp"))
            {
                TestHelper.WaitForIO();
            }

            Thread.Sleep(TimeSpan.FromSeconds(0.1));
        }

        /// <summary>
        /// Hacking it like this so that running the tests do not mess with %APPDATA% if the same machine is used for developing this tool and using it.
        /// </summary>
        public static void UseTempSettingsFile()
        {
            // ReSharper disable once PossibleNullReferenceException
            typeof(AppData).GetField("SettingsFile", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                           .SetValue(null, SettingsFile);
        }

        public static DirectoryInfo CreateIfNotExists(this DirectoryInfo dir)
        {
            dir.Refresh();
            if (!dir.Exists)
            {
                dir.Create();
            }

            return dir;
        }

        public static void ClearRecursive(this DirectoryInfo dir)
        {
            foreach (var file in dir.EnumerateFiles())
            {
                file.Delete();
            }

            foreach (var subDir in dir.EnumerateDirectories())
            {
                subDir.DeleteRecursive();
            }
        }

        public static void DeleteRecursive(this DirectoryInfo dir)
        {
            dir.Refresh();
            if (dir.Exists)
            {
                foreach (var sub in dir.EnumerateDirectories())
                {
                    DeleteRecursive(sub);
                }

                dir.Delete(recursive: true);
            }
        }

        public static FileInfo CreateFile(this DirectoryInfo dir, string fileName, string content = null)
        {
            var file = new FileInfo(Path.Combine(dir.FullName, fileName));
            File.WriteAllText(file.FullName, content ?? string.Empty);
            return file;
        }
    }
}
