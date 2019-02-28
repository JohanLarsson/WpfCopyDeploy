using NUnit.Framework;
using System;
using System.Collections;

namespace WpfCopyDeploy.Tests
{
    public class DebugDevops
    {
        [Test]
        public void GetEnvironmentVariables()
        {
            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
        }
    }
}
