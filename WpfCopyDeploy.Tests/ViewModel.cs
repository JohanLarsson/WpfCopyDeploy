namespace WpfCopyDeploy.Tests
{
    using System;
    using System.IO;
    using System.Reactive.Concurrency;
    using System.Reflection;
    using NUnit.Framework;

    public class ViewModelTests
    {
        private static readonly DirectoryInfo Directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "WpfCopyDeploy.Tests"));
        private static readonly FileInfo SettingsFile = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WpfCopyDeploy.Tests", "Settings.xml"));

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // ReSharper disable once PossibleNullReferenceException
            typeof(AppData).GetField("SettingsFile", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                           .SetValue(null, SettingsFile);
        }

        [SetUp]
        public void SetUp()
        {
            SettingsFile.Delete();
            SettingsFile.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            Directory.DeleteRecursive();
        }

        [Test]
        public void RestoresFromSettings()
        {
            var source = Directory.CreateSubdirectory("Source");
            var target = Directory.CreateSubdirectory("Target");
            AppData.Save(new AppData.Settings { SourceDirectory = source.FullName, TargetDirectory = target.FullName });
            using (var vm = new ViewModel(Scheduler.Immediate))
            {
                Assert.AreEqual(source.FullName, vm.Directories.Source.Directory.FullName);
                Assert.AreEqual(target.FullName, vm.Directories.Target.Directory.FullName);
                Assert.AreEqual(false, vm.CopyFilesCommand.CanExecute(null));
            }
        }
    }
}
