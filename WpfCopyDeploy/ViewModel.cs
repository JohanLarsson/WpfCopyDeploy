namespace WpfCopyDeploy
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public sealed class ViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly ReadOnlyObservableCollection<Files> EmptyFiles = new ReadOnlyObservableCollection<Files>(new ObservableCollection<Files>());
        private readonly AppData.Settings settings = AppData.Read();
        private readonly IDisposable disposable;
        private ReadOnlyObservableCollection<Files> files;

        public ViewModel()
        {
            this.Directories = new Directories(this.settings.SourceDirectory, this.settings.TargetDirectory);
            this.files = EmptyFiles;
            this.CopyFilesCommand = new RelayCommand(this.CopyFiles, _ => this.Files.Any(x => x.ShouldCopy));
            this.DeleteFilesCommand = new RelayCommand(this.DeleteFiles, _ => this.Files.Any(x => x.ShouldDelete));
            this.disposable = Observable.Merge(
                                            Observe(this.Directories.Source),
                                            Observe(this.Directories.Target))
                                        .StartWith(EventArgs.Empty)
                                        .Subscribe(_ => this.Update());

            IObservable<EventArgs> Observe(DirectoryWatcher watcher)
            {
                return Observable.FromEvent<EventHandler, EventArgs>(
                    h => (_, e) => h(e),
                    h => watcher.Changed += h,
                    h => watcher.Changed -= h);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyObservableCollection<Files> Files
        {
            get => this.files;
            private set
            {
                if (ReferenceEquals(value, this.files))
                {
                    return;
                }

                this.files = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand CopyFilesCommand { get; }

        public ICommand DeleteFilesCommand { get; }

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
            this.Files = GetFiles(this.Directories);

            if (this.Directories.Source.Directory?.FullName != this.settings.SourceDirectory ||
                this.Directories.Target.Directory?.FullName != this.settings.TargetDirectory)
            {
                this.settings.SourceDirectory = this.Directories.Source.Directory?.FullName;
                this.settings.TargetDirectory = this.Directories.Target.Directory?.FullName;
                AppData.Save(this.settings);
            }
        }

        private void CopyFiles(object _)
        {
            foreach (var file in this.files)
            {
                if (file.ShouldCopy)
                {
                    file.Copy();
                }
            }
        }

        private void DeleteFiles(object _)
        {
            foreach (var file in this.files)
            {
                if (file.ShouldDelete)
                {
                    file.Delete();
                }
            }
        }

        private static ReadOnlyObservableCollection<Files> GetFiles(Directories directories)
        {
            if (directories.Source.Directory is DirectoryInfo source &&
                source.Exists &&
                directories.Target.Directory is DirectoryInfo target &&
                target.Exists)
            {
                var files = new ObservableCollection<Files>();
                foreach (var sourceFile in GetFiles(source))
                {
                    files.Add(new Files(
                        sourceFile,
                        new FileInfo(sourceFile.FullName.Replace(source.FullName, target.FullName)),
                        target));
                }

                foreach (var targetFile in GetFiles(target))
                {
                    if (files.All(x => x.Target.FullName != targetFile.FullName))
                    {
                        files.Add(new Files(
                            new FileInfo(targetFile.FullName.Replace(target.FullName, source.FullName)),
                            targetFile,
                            target));
                    }
                }

                return new ReadOnlyObservableCollection<Files>(files);
            }
            else
            {
                return EmptyFiles;
            }

            IEnumerable<FileInfo> GetFiles(DirectoryInfo directory)
            {
                var sattelites = GetSattelites(directory).ToArray();
                foreach (var file in directory.EnumerateFiles())
                {
                    yield return file;

                    if (file.Extension == ".dll" ||
                        file.Extension == ".exe")
                    {
                        foreach (var sattelite in sattelites)
                        {
                            if (sattelite.Name == $"{Path.GetFileNameWithoutExtension(file.FullName)}.resources.dll")
                            {
                                yield return sattelite;
                            }
                        }
                    }
                }
            }

            IEnumerable<FileInfo> GetSattelites(DirectoryInfo directory)
            {
                foreach (var satellite in directory.EnumerateFiles($"*.resources.dll", SearchOption.AllDirectories))
                {
                    if (satellite.Directory?.Parent?.FullName == directory.FullName)
                    {
                        yield return satellite;
                    }
                }
            }
        }
    }
}
