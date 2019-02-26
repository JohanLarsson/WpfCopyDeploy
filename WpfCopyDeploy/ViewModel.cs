﻿namespace WpfCopyDeploy
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly AppData.Settings Settings = AppData.Read();
        private readonly ObservableCollection<SourceFile> sourceFiles = new ObservableCollection<SourceFile>();
        private readonly ObservableCollection<FileInfo> targetFiles = new ObservableCollection<FileInfo>();

        public ViewModel()
        {
            this.SourceAndTargetDirectory = new SourceAndTargetDirectory(Settings.SourceDirectory, Settings.TargetDirectory);
            this.SourceFiles = new ReadOnlyObservableCollection<SourceFile>(this.sourceFiles);
            this.TargetFiles = new ReadOnlyObservableCollection<FileInfo>(this.targetFiles);
            this.CopyFilesCommand = new RelayCommand(this.CopyFiles, _ => this.sourceFiles.Any());
            this.SourceAndTargetDirectory.PropertyChanged += (_, __) => this.Update();
            this.Update();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyObservableCollection<SourceFile> SourceFiles { get; }

        public ReadOnlyObservableCollection<FileInfo> TargetFiles { get; }

        public ICommand CopyFilesCommand { get; }

        public SourceAndTargetDirectory SourceAndTargetDirectory { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Update()
        {
            this.sourceFiles.Clear();
            this.targetFiles.Clear();
            if (this.SourceAndTargetDirectory.Source is DirectoryInfo source &&
                source.Exists &&
                this.SourceAndTargetDirectory.Target is DirectoryInfo target &&
                target.Exists)
            {
                foreach (var sourceFile in source.EnumerateFiles())
                {
                    if (SourceFile.TryCreate(sourceFile, source, target, out var copyFile))
                    {
                        this.sourceFiles.Add(copyFile);
                    }
                }
            }

            if (this.SourceAndTargetDirectory.Source?.FullName != Settings.SourceDirectory ||
                this.SourceAndTargetDirectory.Target?.FullName != Settings.TargetDirectory)
            {
                Settings.SourceDirectory = this.SourceAndTargetDirectory.Source?.FullName;
                Settings.TargetDirectory = this.SourceAndTargetDirectory.Target?.FullName;
                AppData.Save(Settings);
            }
        }

        private void CopyFiles(object _)
        {
            var backupDir = new DirectoryInfo(Path.Combine(this.SourceAndTargetDirectory.Target.FullName, $"Backup_{DateTime.Today.ToShortDateString()}"));
            if (!backupDir.Exists)
            {
                backupDir.Create();
            }

            foreach (var copyFile in this.sourceFiles)
            {
                try
                {
                    if (copyFile.Target.Exists)
                    {
                        var backupTarget = copyFile.Target.FullName.Replace(this.SourceAndTargetDirectory.Target.FullName, backupDir.FullName);
                        if (File.Exists(backupTarget))
                        {
                            File.Delete(backupTarget);
                        }

                        File.Move(copyFile.Target.FullName, backupTarget);
                    }

                    File.Copy(copyFile.Source.FullName, copyFile.Target.FullName);
                }
                catch (IOException)
                {
                    // Just swallowing here. Update will show that the file was not copied.
                }
            }

            this.Update();
        }
    }
}
