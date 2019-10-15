namespace WpfCopyDeploy
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;

    public sealed class DirectoryWatcher : IDisposable, INotifyPropertyChanged
    {
        private readonly FileSystemWatcher watcher;

        public DirectoryWatcher(DirectoryInfo? directory)
        {
            this.watcher = new FileSystemWatcher { Filter = "*.*" };
            this.watcher.Changed += (_, e) => this.Changed?.Invoke(this, e);
            this.watcher.Created += (_, e) => this.Changed?.Invoke(this, e);
            this.watcher.Deleted += (_, e) => this.Changed?.Invoke(this, e);
            this.watcher.Renamed += (_, e) => this.Changed?.Invoke(this, e);
            this.Directory = directory;
        }

        public event EventHandler? Changed;

        public event PropertyChangedEventHandler? PropertyChanged;

        public DirectoryInfo? Directory
        {
            get => this.watcher.EnableRaisingEvents ? new DirectoryInfo(this.watcher.Path) : null;
            set
            {
                if (value is { } directory &&
                    System.IO.Directory.Exists(directory.FullName))
                {
                    this.watcher.Path = directory.FullName;
                    this.watcher.EnableRaisingEvents = true;
                }
                else
                {
                    this.watcher.EnableRaisingEvents = false;
                }

                this.OnPropertyChanged();
                this.Changed?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Directory)));
            }
        }

        public void Dispose()
        {
            this.watcher.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
