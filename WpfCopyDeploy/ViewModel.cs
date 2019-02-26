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
        private readonly ObservableCollection<SourceFile> sourceFiles = new ObservableCollection<SourceFile>();
        private readonly ObservableCollection<FileInfo> targetFiles = new ObservableCollection<FileInfo>();

        public ViewModel()
        {
            this.SourceFiles = new ReadOnlyObservableCollection<SourceFile>(this.sourceFiles);
            this.TargetFiles = new ReadOnlyObservableCollection<FileInfo>(this.targetFiles);
            this.CopyFilesCommand = new RelayCommand(this.CopyFiles, _ => this.sourceFiles.Any());
            this.SourceAndTargetDirectory.PropertyChanged += (_, __) => this.Update();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyObservableCollection<SourceFile> SourceFiles { get; }

        public ReadOnlyObservableCollection<FileInfo> TargetFiles { get; }

        public ICommand CopyFilesCommand { get; }

        public SourceAndTargetDirectory SourceAndTargetDirectory { get; } = new SourceAndTargetDirectory();

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Update()
        {
            this.sourceFiles.Clear();
            this.targetFiles.Clear();
            if (this.SourceAndTargetDirectory.Source is DirectoryInfo source &&
                this.SourceAndTargetDirectory.Target is DirectoryInfo target)
            {
                foreach (var sourceFile in source.EnumerateFiles())
                {
                    if (SourceFile.TryCreate(sourceFile, target, out var copyFile))
                    {
                        this.sourceFiles.Add(copyFile);
                    }
                }
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
