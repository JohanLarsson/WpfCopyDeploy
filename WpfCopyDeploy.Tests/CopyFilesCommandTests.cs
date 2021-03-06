﻿namespace WpfCopyDeploy.Tests
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;

    public class CopyFilesCommandTests
    {
        private static DirectoryInfo Directory => new DirectoryInfo(Path.Combine(Path.GetTempPath(), "WpfCopyDeploy.Tests", nameof(CopyFilesCommandTests))).CreateIfNotExists();

        [TearDown]
        public void TearDown()
        {
            Directory.ClearRecursive();
        }

        [Test]
        public void WhenEmpty()
        {
            using (var vm = new ViewModel())
            {
                var source = Directory.CreateSubdirectory("Source");
                var target = Directory.CreateSubdirectory("Target");
                vm.Directories.Source.Directory = source;
                vm.Directories.Target.Directory = target;
                Assert.AreEqual(false, vm.CopyFilesCommand.CanExecute(null));
            }
        }

        [TestCase("App.exe")]
        [TestCase("App.exe.config")]
        [TestCase("App.pdb")]
        [TestCase("Foo.dll")]
        public void WhenSourceOnly(string fileName)
        {
            using (var vm = new ViewModel())
            {
                var source = Directory.CreateSubdirectory("Source");
                var sourceFile = source.CreateFile(fileName);
                var target = Directory.CreateSubdirectory("Target");
                vm.Directories.Source.Directory = source;
                vm.Directories.Target.Directory = target;

                Assert.AreEqual(true, vm.CopyFilesCommand.CanExecute(null));
                vm.CopyFilesCommand.Execute(null);
                Wait.ForIO();

                Assert.AreEqual(true, File.Exists(sourceFile.FullName));
                Assert.AreEqual(true, File.Exists(Path.Combine(target.FullName, fileName)));
            }
        }

        [TestCase("App.exe")]
        [TestCase("Foo.dll")]
        public void WhenBackingUp(string fileName)
        {
            using (var vm = new ViewModel())
            {
                var source = Directory.CreateSubdirectory("Source");
                var sourceFile = source.CreateFile(fileName, "Source");
                var target = Directory.CreateSubdirectory("Target");
                var targetFile = target.CreateFile(fileName, "Target");
                vm.Directories.Source.Directory = source;
                vm.Directories.Target.Directory = target;

                Assert.AreEqual(true, vm.CopyFilesCommand.CanExecute(null));
                vm.CopyFilesCommand.Execute(null);
                Wait.ForIO();

                Assert.AreEqual("Source", File.ReadAllText(sourceFile.FullName));
                Assert.AreEqual("Source", File.ReadAllText(targetFile.FullName));
                var backupFile = new FileInfo(Path.Combine(target.EnumerateDirectories().Single().FullName, fileName));
                Assert.AreEqual("Backup_" + DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), backupFile.Directory.Name);
                Assert.AreEqual(fileName, backupFile.Name);
                Assert.AreEqual("Target", File.ReadAllText(backupFile.FullName));
            }
        }

        [TestCase("App.exe", "App.resources.dll")]
        [TestCase("Foo.dll", "Foo.resources.dll")]
        public void WhenSatelliteAssemblies(string fileName, string resourceName)
        {
            using (var vm = new ViewModel())
            {
                var source = Directory.CreateSubdirectory("Source");
                var sourceFile = source.CreateFile(fileName, "Source");
                source.CreateSubdirectory("en").CreateFile(resourceName, "en");
                source.CreateSubdirectory("sv").CreateFile(resourceName, "sv");
                var target = Directory.CreateSubdirectory("Target");
                vm.Directories.Source.Directory = source;
                vm.Directories.Target.Directory = target;

                Assert.AreEqual(true, vm.CopyFilesCommand.CanExecute(null));
                vm.CopyFilesCommand.Execute(null);
                Wait.ForIO();

                Assert.AreEqual("Source", File.ReadAllText(sourceFile.FullName));
                Assert.AreEqual("Source", File.ReadAllText(Path.Combine(target.FullName, fileName)));
                Assert.AreEqual("en", File.ReadAllText(Path.Combine(target.FullName, "en", resourceName)));
                Assert.AreEqual("sv", File.ReadAllText(Path.Combine(target.FullName, "sv", resourceName)));
            }
        }

        [TestCase("App.exe", "App.resources.dll")]
        [TestCase("Foo.dll", "Foo.resources.dll")]
        public void WhenBackingUpSatelliteAssemblies(string fileName, string resourceName)
        {
            using (var vm = new ViewModel())
            {
                var source = Directory.CreateSubdirectory("Source");
                var sourceFile = source.CreateFile(fileName, "Source");
                source.CreateSubdirectory("en").CreateFile(resourceName, "en");
                source.CreateSubdirectory("sv").CreateFile(resourceName, "sv");
                var target = Directory.CreateSubdirectory("Target");
                target.CreateSubdirectory("en").CreateFile(resourceName, "old en");
                target.CreateSubdirectory("sv").CreateFile(resourceName, "old sv");
                vm.Directories.Source.Directory = source;
                vm.Directories.Target.Directory = target;

                Wait.ForIO();
                Assert.AreEqual(true, vm.CopyFilesCommand.CanExecute(null));
                vm.CopyFilesCommand.Execute(null);
                Wait.ForIO();

                Assert.AreEqual("Source", File.ReadAllText(sourceFile.FullName));
                Assert.AreEqual("Source", File.ReadAllText(Path.Combine(target.FullName, fileName)));
                Assert.AreEqual("en", File.ReadAllText(Path.Combine(target.FullName, "en", resourceName)));
                Assert.AreEqual("sv", File.ReadAllText(Path.Combine(target.FullName, "sv", resourceName)));
                var backupDir = target.EnumerateDirectories().Single(x => x.Name.StartsWith("Backup_"));
                Assert.AreEqual("old en", File.ReadAllText(Path.Combine(backupDir.FullName, "en", resourceName)));
                Assert.AreEqual("old sv", File.ReadAllText(Path.Combine(backupDir.FullName, "sv", resourceName)));
            }
        }
    }
}
