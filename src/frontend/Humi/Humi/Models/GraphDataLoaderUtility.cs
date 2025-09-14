using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using GraphData = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>;

namespace Humi.Models
{
    public class GraphDataLoaderUtility
    {
        public GraphData LoadedData { get; private set; } = new GraphData();
        
        public GraphData LoadFiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                return LoadedData;
            }

            var files = Directory.GetFiles(directoryPath, "*.txt");

            foreach (var file in files)
            {
                foreach (var line in File.ReadLines(file))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 6)
                    {
                        Console.WriteLine($"Skipping invalid line in file {file}: {line}");
                        continue;
                    }

                    try
                    {
                        var numbers = parts.Select(int.Parse).ToArray();
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(file);
                        DateTime date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(nameWithoutExt)).UtcDateTime;
                        LoadedData.Add(date.ToString(), numbers.ToList());
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"Skipping line with invalid integers in file {file}: {line}");
                    }
                }
            }
            return LoadedData;
        }
    }
}