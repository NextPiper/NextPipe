using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace NextPipe.Core.Helpers
{
    public static class ShellHelper
    {
        public static string Bash(this string cmd, bool logVerbose = false)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var builder = new StringBuilder();
            
            builder.AppendLine("------------------------------------------------");
            builder.AppendLine($"Executing: {cmd} - at: {DateTime.Now.ToString()}");
            builder.AppendLine("------------------------------------------------");
            
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
            builder.AppendLine(result);
            process.WaitForExit();
            if (logVerbose)
            {
                Console.WriteLine(builder.ToString());
            }

            return builder.ToString();
        }
        
        public static async Task BashAsync(this string cmd, ILogHandler handler, bool verboseLogging = false)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var builder = new StringBuilder();
            
            builder.AppendLine("------------------------------------------------");
            builder.AppendLine($"Executing: {cmd} - at: {DateTime.Now.ToString()}");
            builder.AppendLine("------------------------------------------------");
            await handler.WriteLine(builder.ToString(), verboseLogging);
            builder.Clear();
            
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            builder.AppendLine(result);
            
            await handler.WriteLine(builder.ToString(), verboseLogging);
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