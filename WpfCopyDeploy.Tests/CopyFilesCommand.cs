namespace WpfCopyDeploy.Tests
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using NUnit.Framework;

    public class CopyFilesCommand
    {
        private static readonly DirectoryInfo Directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "WpfCopyDeploy.Tests"));

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // ReSharper disable once PossibleNullReferenceException
            typeof(AppData).GetField("SettingsFile", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                           .SetValue(null, new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WpfCopyDeploy.Tests", "Settings.xml")));
        }

        [SetUp]
        public void SetUp()
        {
            Directory.DeleteRecursive();
            Directory.CreateIfNotExists();
        }

        [TearDown]
        public void TearDown()
        {
            Directory.DeleteRecursive();
        }

        [Test]
        public void WhenEmpty()
        {
            var vm = new ViewModel();
            var source = Directory.CreateSubdirectory("Source");
            var target = Directory.CreateSubdirectory("Target");
            vm.SourceAndTargetDirectory.Source = source;
            vm.SourceAndTargetDirectory.Target = target;
            Assert.AreEqual(false, vm.CopyFilesCommand.CanExecute(null));
        }

        [TestCase("App.exe")]
        [TestCase("App.exe.config")]
        [TestCase("App.pdb")]
        [TestCase("Foo.dll")]
        public void WhenFileInSourceOnly(string fileName)
        {
            var vm = new ViewModel();
            var source = Directory.CreateSubdirectory("Source");
            var sourceFile = source.CreateFile(fileName);
            var target = Directory.CreateSubdirectory("Target");
            vm.SourceAndTargetDirectory.Source = source;
            vm.SourceAndTargetDirectory.Target = target;
            Assert.AreEqual(true, vm.CopyFilesCommand.CanExecute(null));
            vm.CopyFilesCommand.Execute(null);
            Assert.AreEqual(true, File.Exists(sourceFile.FullName));
            Assert.AreEqual(true, File.Exists(Path.Combine(target.FullName, fileName)));
        }

        [TestCase("Foo.dll")]
        public void WhenFileInSourceAndTarget(string fileName)
        {
            var vm = new ViewModel();
            var source = Directory.CreateSubdirectory("Source");
            var sourceFile = source.CreateFile(fileName, "Source");
            var target = Directory.CreateSubdirectory("Target");
            var targetFile = target.CreateFile(fileName, "Target");
            vm.SourceAndTargetDirectory.Source = source;
            vm.SourceAndTargetDirectory.Target = target;
            Assert.AreEqual(true, vm.CopyFilesCommand.CanExecute(null));
            vm.CopyFilesCommand.Execute(null);
            Assert.AreEqual("Source", File.ReadAllText(sourceFile.FullName));
            Assert.AreEqual("Source", File.ReadAllText(targetFile.FullName));
            var backupFile = new FileInfo(Path.Combine(target.EnumerateDirectories().Single().FullName, fileName));
            Assert.AreEqual("Target", File.ReadAllText(backupFile.FullName));
        }
    }
}
