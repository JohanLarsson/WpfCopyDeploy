﻿namespace WpfCopyDeploy.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;
    using NUnit.Framework;

    public class DeleteFilesCommandTests
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
        public void WhenEmpty()
        {
            using (var vm = new ViewModel(Scheduler.Immediate))
            {
                var source = Directory.CreateSubdirectory("Source");
                var target = Directory.CreateSubdirectory("Target");
                vm.Directories.Source.Directory = source;
                vm.Directories.Target.Directory = target;
                Assert.AreEqual(false, vm.DeleteFilesCommand.CanExecute(null));
            }
        }

        [TestCase("App.exe")]
        [TestCase("Foo.dll")]
        public void WhenNoBackups(string fileName)
        {
            using (var vm = new ViewModel(Scheduler.Immediate))
            {
                var source = Directory.CreateSubdirectory("Source");
                var target = Directory.CreateSubdirectory("Target");
                target.CreateFile(fileName, "Target");
                vm.Directories.Source.Directory = source;
                vm.Directories.Target.Directory = target;

                Assert.AreEqual(true, vm.DeleteFilesCommand.CanExecute(null));
                vm.DeleteFilesCommand.Execute(null);
                Thread.Sleep(TimeSpan.FromSeconds(1));

                CollectionAssert.IsEmpty(source.EnumerateFiles());
                CollectionAssert.IsEmpty(target.EnumerateFiles());
                var backupFile = new FileInfo(Path.Combine(target.EnumerateDirectories().Single().FullName, fileName));
                Assert.AreEqual("Target", File.ReadAllText(backupFile.FullName));
            }
        }

        [TestCase("App.exe")]
        [TestCase("Foo.dll")]
        public void Twice(string fileName)
        {
            using (var vm = new ViewModel(Scheduler.Immediate))
            {
                var source = Directory.CreateSubdirectory("Source");
                var target = Directory.CreateSubdirectory("Target");
                target.CreateFile(fileName, "Target");
                Thread.Sleep(TimeSpan.FromSeconds(1));
                vm.Directories.Source.Directory = source;
                vm.Directories.Target.Directory = target;

                Assert.AreEqual(true, vm.DeleteFilesCommand.CanExecute(null));
                vm.DeleteFilesCommand.Execute(null);
                Thread.Sleep(TimeSpan.FromSeconds(1));

                CollectionAssert.IsEmpty(source.EnumerateFiles());
                CollectionAssert.IsEmpty(target.EnumerateFiles());
                var backupFile = new FileInfo(Path.Combine(target.EnumerateDirectories().Single().FullName, fileName));
                Assert.AreEqual("Target", File.ReadAllText(backupFile.FullName));

                target.CreateFile(fileName, "New Target");
                Thread.Sleep(TimeSpan.FromSeconds(1));

                Assert.AreEqual(true, vm.DeleteFilesCommand.CanExecute(null));
                vm.DeleteFilesCommand.Execute(null);
                Thread.Sleep(TimeSpan.FromSeconds(1));

                CollectionAssert.IsEmpty(source.EnumerateFiles());
                CollectionAssert.IsEmpty(target.EnumerateFiles());
                Assert.AreEqual("New Target", File.ReadAllText(backupFile.FullName));
            }
        }
    }
}