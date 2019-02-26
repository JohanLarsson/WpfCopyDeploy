namespace WpfCopyDeploy
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
            this.CopyFilesCommand = new RelayCommand(this.CopyFiles, _ => this.sourceFiles.Any(x => x.HasDiff));
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

                    if (sourceFile.Extension == ".dll" ||
                        sourceFile.Extension == ".exe")
                    {
                        foreach (var satellite in source.EnumerateFiles($"{Path.GetFileNameWithoutExtension(sourceFile.FullName)}.resources.dll", SearchOption.AllDirectories))
                        {
                            if (satellite.Directory?.Parent?.FullName == source.FullName &&
                                SourceFile.TryCreate(satellite, source, target, out copyFile))
                            {
                                this.sourceFiles.Add(copyFile);
                            }
                        }
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
            foreach (var copyFile in this.sourceFiles)
            {
                if (copyFile.HasDiff)
                {
                    try
                    {
                        if (copyFile.Target.Exists)
                        {
                            var backupTarget = new FileInfo(copyFile.Target.FullName.Replace(this.SourceAndTargetDirectory.Target.FullName, BackUpDir().FullName));
                            if (backupTarget.Exists)
                            {
                                backupTarget.Delete();
                            }
                            else if (backupTarget.Directory?.Exists == false)
                            {
                                backupTarget.Directory.Create();
                            }

                            File.Move(copyFile.Target.FullName, backupTarget.FullName);
                        }

                        if (copyFile.Target.Directory?.Exists == false)
                        {
                            copyFile.Target.Directory.Create();
                        }

                        File.Copy(copyFile.Source.FullName, copyFile.Target.FullName);
                    }
                    catch (IOException)
                    {
                        // Just swallowing here. Update will show that the file was not copied.
                    }
                }
            }

            this.Update();

            DirectoryInfo BackUpDir()
            {
                var dir = new DirectoryInfo(Path.Combine(this.SourceAndTargetDirectory.Target.FullName, $"Backup_{DateTime.Today.ToShortDateString()}"));
                if (!dir.Exists)
                {
                    dir.Create();
                }

                return dir;
            }
        }
    }
}
