namespace WpfCopyDeploy.Tests
{
    using System;
    using System.IO;
    using NUnit.Framework;

    public class CopyFilesCommand
    {
        private static readonly DirectoryInfo Directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "WpfCopyDeploy.Tests"));

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
    }
}
