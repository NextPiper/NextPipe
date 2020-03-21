using System;
using System.Diagnostics;

namespace NextPipe.Utilities.Core
{
    public static class ShellHelper
    {
        public static string Bash(this string cmd, bool logVerbose = false)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            Console.WriteLine($"Executing: {cmd} - at: {DateTime.Now.Second}");
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            if(logVerbose)
                Console.WriteLine(result);
            return result;
        }

        /// <summary>
        /// Returns true if t1 has an identical start as t2
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static bool IdenticalStart(this string t1, string t2)
        {
            for (int i = 0; i < t2.Length; i++)
            {
                if (t1[i] != t2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static int ToMillis(this int i)
        {
            return i * 1000;
        }
    }
}