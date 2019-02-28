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
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            Thread.Sleep(TimeSpan.FromSeconds(0.1));
        }
    }
}
