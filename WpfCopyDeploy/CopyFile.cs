namespace WpfCopyDeploy
{
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;

    public class CopyFile : INotifyPropertyChanged
    {
        public CopyFile(FileInfo source, FileInfo target)
        {
            this.Source = source;
            this.Target = target;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FileInfo Source { get; }
        
        public FileInfo Target { get; }

        public static bool TryCreate(FileInfo source, DirectoryInfo targetDirectory, out CopyFile result)
        {
            var target = new FileInfo(Path.Combine(targetDirectory.FullName, source.Name));
            if (target.Exists)
            {
                if (source.CreationTimeUtc == target.CreationTimeUtc)
                {
                    result = null;
                    return false;
                }

                result = new CopyFile(source, target);
                return true;
            }

            result = new CopyFile(source, target);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
