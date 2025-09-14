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
    public event Action<string>? SummaryReceived;
    private Process backendProcess;
    private Process audioProcess;

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

        if (OperatingSystem.IsMacOS())
        {
            RunSox();
        }

        DispatcherTimer.RunOnce(RunPipeReader, TimeSpan.FromSeconds(2));
    }

    private void RunSox()
    {
        if (!Directory.Exists("/tmp/humi"))
        {
            Directory.CreateDirectory("/tmp/humi");
        }

        if (File.Exists("/tmp/humi/humi_recording.wav"))
        {
            File.Delete("/tmp/humi/humi_recording.wav");
        }

        var arguments = "-d humi_recording.wav";
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "sox",
            Arguments = arguments,
            WorkingDirectory = "/tmp/humi",
            UseShellExecute = true,
            CreateNoWindow = true,
        };

        audioProcess = new Process()
        {
            StartInfo = startInfo
        };

        audioProcess.Start();
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

            if (OperatingSystem.IsMacOS() && audioProcess is { HasExited: false })
            {
                audioProcess.Kill();
                audioProcess.WaitForExit();

                DispatcherTimer.RunOnce(RunTranscript, TimeSpan.FromSeconds(2));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping backend process: {ex.Message}");
        }
    }

    private void RunTranscript()
    {
        var arguments = $"backend/run_backend_llm.sh";


        string pipePath = "/tmp/humi/summary.txt";


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

        // Read asynchronously in background
        _ = Task.Run(async () =>
        {
            await using var fs = new FileStream(pipePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096,
                useAsync: true);
            using var reader = new StreamReader(fs, Encoding.UTF8);
            Console.WriteLine("Listening to pipe");
            while (true)
            {
                if (process.HasExited)
                {
                    Console.WriteLine("Summary ended");
                    break;
                }

                if (reader.EndOfStream)
                {
                    await Task.Delay(10); // Avoid busy waiting
                    continue;
                }

                OnSummaryChanged(pipePath);
            }
        });
    }

    private void OnSummaryChanged(string path)
    {
        try
        {
            string summary = File.ReadAllText(path);
            SummaryReceived?.Invoke(summary);
        }
        catch (IOException)
        {
            // File might be in use, ignore for now
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