namespace WpfCopyDeploy
{
    using System;
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
            :this(DispatcherScheduler.Current)
        {
        }

        public ViewModel(IScheduler scheduler)
        {
            this.Directories = new Directories(this.settings.SourceDirectory, this.settings.TargetDirectory);
            this.Files = new ReadOnlyObservableCollection<Files>(this.files);
            this.CopyFilesCommand = new RelayCommand(this.CopyFiles, _ => this.files.Any(x => x.ShouldCopy));
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
                    foreach (var sourceFile in source.EnumerateFiles())
                    {
                        if (WpfCopyDeploy.Files.TryCreate(sourceFile, source, target, out var copyFile))
                        {
                            this.files.Add(copyFile);
                        }

                        if (sourceFile.Extension == ".dll" ||
                            sourceFile.Extension == ".exe")
                        {
                            foreach (var satellite in source.EnumerateFiles($"{Path.GetFileNameWithoutExtension(sourceFile.FullName)}.resources.dll", SearchOption.AllDirectories))
                            {
                                if (satellite.Directory?.Parent?.FullName == source.FullName &&
                                    WpfCopyDeploy.Files.TryCreate(satellite, source, target, out copyFile))
                                {
                                    this.files.Add(copyFile);
                                }
                            }
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
    }
}
