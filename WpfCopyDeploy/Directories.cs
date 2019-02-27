namespace WpfCopyDeploy
{
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public sealed class Directories : INotifyPropertyChanged, System.IDisposable
    {
        public Directories(string sourceDirectory, string targetDirectory)
        {
            this.Source = new DirectoryWatcher(DirOrNull(sourceDirectory));
            this.Target = new DirectoryWatcher(DirOrNull(targetDirectory));
            this.OpenSourceCommand = new RelayCommand(this.OpenSource);
            this.OpenTargetCommand = new RelayCommand(this.OpenTarget);

            DirectoryInfo DirOrNull(string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return null;
                }

                return new DirectoryInfo(path);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand OpenSourceCommand { get; }

        public ICommand OpenTargetCommand { get; }

        public DirectoryWatcher Source { get; }

        public DirectoryWatcher Target { get; }

        public void Dispose()
        {
            this.Source.Dispose();
            this.Target.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenSource(object _)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                this.Source.Directory = new DirectoryInfo(dialog.SelectedPath);
            }
        }

        private void OpenTarget(object _)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                this.Target.Directory = new DirectoryInfo(dialog.SelectedPath);
            }
        }
    }
}
