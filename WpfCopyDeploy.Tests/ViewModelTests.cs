namespace WpfCopyDeploy.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;
    using NUnit.Framework;

    public class ViewModelTests
    {
        private static readonly DirectoryInfo Directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "WpfCopyDeploy.Tests")).CreateIfNotExists();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestHelper.UseTempSettingsFile();
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

        [Test]
        public void ListensToFileSystemChanges()
        {
            var source = Directory.CreateSubdirectory("Source");
            var target = Directory.CreateSubdirectory("Target");
            using (var vm = new ViewModel(Scheduler.Immediate))
            {
                vm.Directories.Source.Directory = source;
                vm.Directories.Target.Directory = target;

                CollectionAssert.IsEmpty(vm.Files);
                source.CreateFile("App.exe");
                Thread.Sleep(TimeSpan.FromSeconds(1));

                Assert.AreEqual(source.FullName, vm.Directories.Source.Directory.FullName);
                Assert.AreEqual(target.FullName, vm.Directories.Target.Directory.FullName);
                CollectionAssert.AreEqual(new[] { "App.exe" }, vm.Files.Select(x => x.Source.Name));
            }
        }
    }
}
