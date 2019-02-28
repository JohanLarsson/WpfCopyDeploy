namespace WpfCopyDeploy.Tests
{
    using System.IO;
    using System.Reactive.Concurrency;
    using System.Reflection;
    using NUnit.Framework;

    public class ViewModelTests
    {
        private static readonly DirectoryInfo Directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "WpfCopyDeploy.Tests")).CreateIfNotExists();
        private static readonly FileInfo SettingsFile = new FileInfo(Path.Combine(Directory.FullName, "Settings.xml"));

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // ReSharper disable once PossibleNullReferenceException
            typeof(AppData).GetField("SettingsFile", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                           .SetValue(null, SettingsFile);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.ClearRecursive();
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
