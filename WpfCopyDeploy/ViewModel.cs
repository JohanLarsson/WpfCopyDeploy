namespace WpfCopyDeploy
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public sealed class ViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly AppData.Settings Settings = AppData.Read();
        private readonly ObservableCollection<SourceFile> files = new ObservableCollection<SourceFile>();
        private readonly IDisposable disposable;

        public ViewModel()
        {
            this.Directories = new Directories(Settings.SourceDirectory, Settings.TargetDirectory);
            this.Files = new ReadOnlyObservableCollection<SourceFile>(this.files);
            this.CopyFilesCommand = new RelayCommand(this.CopyFiles, _ => this.files.Any(x => x.HasDiff));
            this.disposable = Observable.Merge(
                          Observe(this.Directories.Source),
                          Observe(this.Directories.Target))
                      .ObserveOnDispatcher()
                      .StartWith(EventArgs.Empty).Subscribe(_ => this.Update());

            IObservable<EventArgs> Observe(DirectoryWatcher watcher)
            {
                return Observable.FromEvent<EventHandler, EventArgs>(
                    h => (_, e) => h(e),
                    h => watcher.Changed += h,
                    h => watcher.Changed -= h);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyObservableCollection<SourceFile> Files { get; }

        public ICommand CopyFilesCommand { get; }

        public Directories Directories { get; }

        public void Dispose()
        {
            this.Directories.Dispose();
            this.disposable?.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Update()
        {
            this.files.Clear();
            if (this.Directories.Source.Directory is DirectoryInfo source &&
                source.Exists &&
                this.Directories.Target.Directory is DirectoryInfo target &&
                target.Exists)
            {
                foreach (var sourceFile in source.EnumerateFiles())
                {
                    if (SourceFile.TryCreate(sourceFile, source, target, out var copyFile))
                    {
                        this.files.Add(copyFile);
                    }

                    if (sourceFile.Extension == ".dll" ||
                        sourceFile.Extension == ".exe")
                    {
                        foreach (var satellite in source.EnumerateFiles($"{Path.GetFileNameWithoutExtension(sourceFile.FullName)}.resources.dll", SearchOption.AllDirectories))
                        {
                            if (satellite.Directory?.Parent?.FullName == source.FullName &&
                                SourceFile.TryCreate(satellite, source, target, out copyFile))
                            {
                                this.files.Add(copyFile);
                            }
                        }
                    }
                }
            }

            if (this.Directories.Source.Directory?.FullName != Settings.SourceDirectory ||
                this.Directories.Target.Directory?.FullName != Settings.TargetDirectory)
            {
                Settings.SourceDirectory = this.Directories.Source.Directory?.FullName;
                Settings.TargetDirectory = this.Directories.Target.Directory?.FullName;
                AppData.Save(Settings);
            }
        }

        private void CopyFiles(object _)
        {
            foreach (var copyFile in this.files)
            {
                if (copyFile.HasDiff)
                {
                    try
                    {
                        if (copyFile.Target.Exists)
                        {
                            var backupTarget = new FileInfo(copyFile.Target.FullName.Replace(this.Directories.Target.Directory.FullName, BackUpDir().FullName));
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
                var dir = new DirectoryInfo(Path.Combine(this.Directories.Target.Directory.FullName, $"Backup_{DateTime.Today.ToShortDateString()}"));
                if (!dir.Exists)
                {
                    dir.Create();
                }

                return dir;
            }
        }
    }
}
