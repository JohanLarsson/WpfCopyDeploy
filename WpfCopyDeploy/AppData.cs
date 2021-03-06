﻿namespace WpfCopyDeploy
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    public static class AppData
    {
        private static readonly FileInfo SettingsFile = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WpfCopyDeploy", "Settings.xml"));
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(Settings));

        public static Settings Read()
        {
            SettingsFile.Refresh();
            if (SettingsFile.Exists)
            {
                using var stream = SettingsFile.OpenRead();
                return (Settings?)Serializer.Deserialize(stream) ??
                       throw new SerializationException("Could not deserialize setting");
            }

            return new Settings();
        }

        public static void Save(Settings settings)
        {
            SettingsFile.Refresh();
            if (SettingsFile.Exists)
            {
                SettingsFile.Delete();
            }

            if (SettingsFile.Directory is { Exists: false } directory)
            {
                directory.Create();
            }

            using var stream = SettingsFile.OpenWrite();
            Serializer.Serialize(stream, settings);
        }

#pragma warning disable INPC001 // The class has mutable properties and should implement INotifyPropertyChanged.
        public class Settings
#pragma warning restore INPC001 // The class has mutable properties and should implement INotifyPropertyChanged.
        {
            public string? SourceDirectory { get; set; }

            public string? TargetDirectory { get; set; }
        }
    }
}
