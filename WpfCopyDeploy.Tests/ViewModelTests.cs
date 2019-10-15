namespace WpfCopyDeploy.Tests
{
    using System.IO;
    using System.Linq;
    using NUnit.Framework;

    public class ViewModelTests
    {
        private static readonly DirectoryInfo Directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "WpfCopyDeploy.Tests", nameof(ViewModelTests))).CreateIfNotExists();

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
            using (var vm = new ViewModel())
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
            using (var vm = new ViewModel())
            {
                vm.Directories.Source.Directory = source;
                vm.Directories.Target.Directory = target;

                CollectionAssert.IsEmpty(vm.Files);
                source.CreateFile("App.exe");

                Assert.AreEqual(source.FullName, vm.Directories.Source.Directory.FullName);
                Assert.AreEqual(target.FullName, vm.Directories.Target.Directory.FullName);
                CollectionAssert.AreEqual(new[] { "App.exe" }, vm.Files.Select(x => x.Source.Name));
            }
        }
    }
}
