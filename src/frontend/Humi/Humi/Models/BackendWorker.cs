using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using Humi.Analyzer;

namespace Humi.Models;

public class BackendWorker
{
    public event Action<string>? DataReceived;
    private Process backendProcess;
    
    public void StartBackend(int screen)
    {
        var arguments = $"backend/run_backend.sh analyze {screen}";
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = arguments,
            WorkingDirectory = "../../../../../..",
            UseShellExecute = true,
            CreateNoWindow = true,
        };

        Process process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();
        
        backendProcess = process;

        DispatcherTimer.RunOnce(RunPipeReader, TimeSpan.FromSeconds(2));
    }
    
    public void StopBackend()
    {
        try
        {
            if (!backendProcess.HasExited)
            {
                backendProcess.Kill();
                backendProcess.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping backend process: {ex.Message}");
        }
    }

    private void RunPipeReader()
    {
        string pipePath = "/tmp/emotions_feed";

        // Read asynchronously in background
        _ = Task.Run(async () =>
        {
            await using var fs = new FileStream(pipePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096,
                useAsync: true);
            using var reader = new StreamReader(fs, Encoding.UTF8);
            Console.WriteLine("Listening to pipe");
            while (true)
            {
                if (backendProcess.HasExited)
                {
                    Console.WriteLine("Backend process has exited. Stopping pipe reader.");
                    break;
                }
                
                if (reader.EndOfStream)
                {
                    await Task.Delay(10); // Avoid busy waiting
                    continue;
                }

                string? line = await reader.ReadLineAsync();
                if (line != null)
                {
                    DataReceived?.Invoke(line);
                }
            }
        });
    }
}