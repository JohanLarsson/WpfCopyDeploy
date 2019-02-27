namespace WpfCopyDeploy
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class Files : INotifyPropertyChanged
    {
        public Files(FileInfo source, FileInfo target, DirectoryInfo targetDirectory)
        {
            this.Source = source;
            this.Target = target;
            this.TargetDirectory = targetDirectory;
            this.CopyCommand = new RelayCommand(_ => this.Copy(), _ => this.ShouldCopy);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FileInfo Source { get; }

        public FileInfo Target { get; }

        public DirectoryInfo TargetDirectory { get; }

        public ICommand CopyCommand { get; }

        public bool ShouldCopy => !this.Target.Exists ||
                                this.Source.LastWriteTimeUtc != this.Target.LastWriteTimeUtc;

        public static bool TryCreate(FileInfo source, DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, out Files result)
        {
            var target = new FileInfo(source.FullName.Replace(sourceDirectory.FullName, targetDirectory.FullName));
            if (target.Exists)
            {
                result = new Files(source, target, targetDirectory);
                return true;
            }

            result = new Files(source, target, targetDirectory);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void Copy()
        {
            try
            {
                if (this.Target.Exists)
                {
                    var backupTarget = new FileInfo(this.Target.FullName.Replace(this.TargetDirectory.FullName, BackUpDir().FullName));
                    if (backupTarget.Exists)
                    {
                        backupTarget.Delete();
                    }
                    else if (backupTarget.Directory?.Exists == false)
                    {
                        backupTarget.Directory.Create();
                    }

                    File.Move(this.Target.FullName, backupTarget.FullName);
                }

                if (this.Target.Directory?.Exists == false)
                {
                    this.Target.Directory.Create();
                }

                File.Copy(this.Source.FullName, this.Target.FullName);
            }
            catch (IOException)
            {
                // Just swallowing here. Update will show that the file was not copied.
            }

            DirectoryInfo BackUpDir()
            {
                var dir = new DirectoryInfo(Path.Combine(this.TargetDirectory.FullName, $"Backup_{DateTime.Today.ToShortDateString()}"));
                if (!dir.Exists)
                {
                    dir.Create();
                }

                return dir;
            }
        }
    }
}
