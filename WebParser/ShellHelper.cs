using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WebParser
{
    public static class ShellHelper
    {
        private static Task<object> RunAsync(this Process process)
        {
            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) => tcs.TrySetResult(process.StandardOutput.ReadToEnd());
            // not sure on best way to handle false being returned
            if (!process.Start()) tcs.SetException(new Exception("Failed to start process."));
            return tcs.Task;
        }
        
        public static async Task<string> Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            
            var process = new Process
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
            return (string)await process.RunAsync();
        }
    }
}