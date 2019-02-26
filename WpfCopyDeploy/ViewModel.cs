namespace WpfCopyDeploy
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class ViewModel : INotifyPropertyChanged
    {
        private DirectoryInfo source;
        private DirectoryInfo target;
        private readonly ObservableCollection<SourceFile> sourceFiles = new ObservableCollection<SourceFile>();
        private readonly ObservableCollection<FileInfo> targetFiles = new ObservableCollection<FileInfo>();

        public ViewModel()
        {
            this.SourceFiles = new ReadOnlyObservableCollection<SourceFile>(this.sourceFiles);
            this.TargetFiles = new ReadOnlyObservableCollection<FileInfo>(this.targetFiles);
            this.OpenSourceCommand = new RelayCommand(this.OpenSource);
            this.OpenTargetCommand = new RelayCommand(this.OpenTarget);
            this.CopyFilesCommand = new RelayCommand(this.CopyFiles, _ => this.sourceFiles.Any());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyObservableCollection<SourceFile> SourceFiles { get; }

        public ReadOnlyObservableCollection<FileInfo> TargetFiles { get; }

        public ICommand OpenSourceCommand { get; }

        public ICommand OpenTargetCommand { get; }

        public ICommand CopyFilesCommand { get; }

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
                this.Update();
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
                this.Update();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Update()
        {
            this.sourceFiles.Clear();
            this.targetFiles.Clear();
            if (this.source != null &&
                this.target != null)
            {
                foreach (var sourceFile in this.source.EnumerateFiles())
                {
                    if (SourceFile.TryCreate(sourceFile, this.target, out var copyFile))
                    {
                        this.sourceFiles.Add(copyFile);
                    }
                }
            }
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

        private void CopyFiles(object obj)
        {
            foreach (var copyFile in this.sourceFiles)
            {
                try
                {
                    if (copyFile.Target.Exists)
                    {
                        copyFile.Target.Delete();
                    }

                    File.Move(copyFile.Source.FullName, copyFile.Target.FullName);
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
