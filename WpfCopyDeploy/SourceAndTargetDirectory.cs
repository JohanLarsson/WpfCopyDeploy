namespace WpfCopyDeploy
{
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class SourceAndTargetDirectory : INotifyPropertyChanged
    {
        private DirectoryInfo source;
        private DirectoryInfo target;

        public SourceAndTargetDirectory(string sourceDirectory, string targetDirectory)
        {
            this.source = DirOrNull(sourceDirectory);
            this.target = DirOrNull(targetDirectory);
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

        public DirectoryInfo Source
        {
            get => this.source;
            set
            {
                if (ReferenceEquals(value, this.source))
                {
                    return;
                }

                this.source = value;
                this.OnPropertyChanged();
            }
        }

        public DirectoryInfo Target
        {
            get => this.target;
            set
            {
                if (ReferenceEquals(value, this.target))
                {
                    return;
                }

                this.target = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenSource(object _)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                this.Source = new DirectoryInfo(dialog.SelectedPath);
            }
        }

        private void OpenTarget(object _)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                this.Target = new DirectoryInfo(dialog.SelectedPath);
            }
        }
    }
}
