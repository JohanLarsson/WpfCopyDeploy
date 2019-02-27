namespace WpfCopyDeploy
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public sealed class ViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly AppData.Settings settings = AppData.Read();
        private readonly ObservableCollection<Files> files = new ObservableCollection<Files>();
        private readonly object gate = new object();
        private readonly IDisposable disposable;


        public ViewModel()
            : this(DispatcherScheduler.Current)
        {
        }

        public ViewModel(IScheduler scheduler)
        {
            this.Directories = new Directories(this.settings.SourceDirectory, this.settings.TargetDirectory);
            this.Files = new ReadOnlyObservableCollection<Files>(this.files);
            this.CopyFilesCommand = new RelayCommand(this.CopyFiles, _ => this.files.Any(x => x.ShouldCopy));
            this.DeleteFilesCommand = new RelayCommand(this.DeleteFiles, _ => this.files.Any(x => x.ShouldDelete));
            this.disposable = Observable.Merge(
                          Observe(this.Directories.Source),
                          Observe(this.Directories.Target))
                      .ObserveOn(scheduler)
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

        public ReadOnlyObservableCollection<Files> Files { get; }

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
            lock (this.gate)
            {
                this.files.Clear();
                if (this.Directories.Source.Directory is DirectoryInfo source &&
                    source.Exists &&
                    this.Directories.Target.Directory is DirectoryInfo target &&
                    target.Exists)
                {
                    foreach (var sourceFile in GetFiles(source))
                    {
                        this.files.Add(new Files(
                            sourceFile,
                            new FileInfo(sourceFile.FullName.Replace(source.FullName, target.FullName)),
                            target));
                    }


                    foreach (var targetFile in GetFiles(target))
                    {
                        if (this.files.All(x => x.Target.FullName != targetFile.FullName))
                        {
                            this.files.Add(new Files(
                                new FileInfo(targetFile.FullName.Replace(target.FullName, source.FullName)),
                                targetFile,
                                target));
                        }
                    }
                }

                if (this.Directories.Source.Directory?.FullName != this.settings.SourceDirectory ||
                    this.Directories.Target.Directory?.FullName != this.settings.TargetDirectory)
                {
                    this.settings.SourceDirectory = this.Directories.Source.Directory?.FullName;
                    this.settings.TargetDirectory = this.Directories.Target.Directory?.FullName;
                    AppData.Save(this.settings);
                }
            }

            IEnumerable<FileInfo> GetFiles(DirectoryInfo directory)
            {
                foreach (var file in directory.EnumerateFiles())
                {
                    yield return file;

                    if (file.Extension == ".dll" ||
                        file.Extension == ".exe")
                    {
                        foreach (var satellite in directory.EnumerateFiles($"{Path.GetFileNameWithoutExtension(file.FullName)}.resources.dll", SearchOption.AllDirectories))
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

        private void CopyFiles(object _)
        {
            lock (this.gate)
            {
                foreach (var file in this.files)
                {
                    if (file.ShouldCopy)
                    {
                        file.Copy();
                    }
                }

            }

            this.Update();
        }

        private void DeleteFiles(object _)
        {
            lock (this.gate)
            {
                foreach (var file in this.files)
                {
                    if (file.ShouldDelete)
                    {
                        file.Delete();
                    }
                }

            }

            this.Update();
        }
    }
}
