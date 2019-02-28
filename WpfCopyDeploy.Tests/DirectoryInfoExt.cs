namespace WpfCopyDeploy.Tests
{
    using System.IO;

    public static class DirectoryInfoExt
    {
        public static DirectoryInfo CreateIfNotExists(this DirectoryInfo dir)
        {
            dir.Refresh();
            if (!dir.Exists)
            {
                dir.Create();
            }

            return dir;
        }

        public static void ClearRecursive(this DirectoryInfo dir)
        {
            foreach (var file in dir.EnumerateFiles())
            {
                file.Delete();
            }

            foreach (var subDir in dir.EnumerateDirectories())
            {
                subDir.DeleteRecursive();
            }
        }

            public static void DeleteRecursive(this DirectoryInfo dir)
        {
            dir.Refresh();
            if (dir.Exists)
            {
                foreach (var sub in dir.EnumerateDirectories())
                {
                    DeleteRecursive(sub);
                }

                dir.Delete(recursive: true);
            }
        }

        public static FileInfo CreateFile(this DirectoryInfo dir, string fileName, string content = null)
        {
            var file = new FileInfo(Path.Combine(dir.FullName, fileName));
            File.WriteAllText(file.FullName, content ?? string.Empty);
            return file;
        }
    }
}
