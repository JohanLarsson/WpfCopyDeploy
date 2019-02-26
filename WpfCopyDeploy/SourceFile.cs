namespace WpfCopyDeploy
{
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;

    public class SourceFile : INotifyPropertyChanged
    {
        public SourceFile(FileInfo source, FileInfo target)
        {
            this.Source = source;
            this.Target = target;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FileInfo Source { get; }
        
        public FileInfo Target { get; }

        public static bool TryCreate(FileInfo source, DirectoryInfo targetDirectory, out SourceFile result)
        {
            var target = new FileInfo(Path.Combine(targetDirectory.FullName, source.Name));
            if (target.Exists)
            {
                if (source.CreationTimeUtc == target.CreationTimeUtc)
                {
                    result = null;
                    return false;
                }

                result = new SourceFile(source, target);
                return true;
            }

            result = new SourceFile(source, target);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
