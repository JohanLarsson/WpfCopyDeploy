namespace WpfCopyDeploy.Tests
{
    using System;
    using System.IO;
    using System.Threading;

    public class Wait
    {
        public static void ForIO()
        {
            if (Path.GetTempPath().StartsWith(@"C:\Users\VssAdministrator\AppData\Local\Temp"))
            {
                // We are running on devops.
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(20));
        }
    }
}
