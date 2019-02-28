namespace WpfCopyDeploy
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
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
            this.DeleteCommand = new RelayCommand(_ => this.Delete(), _ => this.ShouldDelete);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FileInfo Source { get; }

        public FileInfo Target { get; }

        public DirectoryInfo TargetDirectory { get; }

        public ICommand CopyCommand { get; }

        public ICommand DeleteCommand { get; }

        public bool ShouldCopy => this.Source.Exists &&
                                  (!this.Target.Exists ||
                                   this.Source.LastWriteTimeUtc != this.Target.LastWriteTimeUtc);

        public bool ShouldDelete => !this.Source.Exists;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void Copy()
        {
            try
            {
                this.Delete();
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
        }

        internal void Delete()
        {
            if (this.Target.Exists)
            {
                var backupTarget = new FileInfo(Path.Combine(this.Target.Directory.FullName.Replace(this.TargetDirectory.FullName, BackUpDir().FullName), this.Target.Name));
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

            DirectoryInfo BackUpDir()
            {
                var dir = new DirectoryInfo(Path.Combine(this.TargetDirectory.FullName, $"Backup_{DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}"));
                if (!dir.Exists)
                {
                    dir.Create();
                }

                return dir;
            }
        }
    }
}
