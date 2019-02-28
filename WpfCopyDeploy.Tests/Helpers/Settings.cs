namespace WpfCopyDeploy.Tests
{
    using System.IO;
    using System.Reflection;

    public static class Settings
    {
        public static FileInfo File
        {
            get
            {
                var dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "WpfCopyDeploy.Tests.AppData")).CreateIfNotExists();
                return new FileInfo(Path.Combine(dir.FullName, "Settings.xml"));
            }
        }


        /// <summary>
        /// Hacking it like this so that running the tests do not mess with %APPDATA% if the same machine is used for developing this tool and using it.
        /// </summary>
        public static void UseTemp()
        {
            // ReSharper disable once PossibleNullReferenceException
            typeof(AppData).GetField("SettingsFile", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                           .SetValue(null, File);
        }
    }
}
