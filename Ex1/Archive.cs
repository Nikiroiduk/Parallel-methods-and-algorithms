using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Ex1
{
    internal static class Archive
    {
        public static async Task ArchiveByteArrayAsync(List<byte[]> data, string path)
        {
            Console.WriteLine("Adding images to archive...");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using (var fileStream = File.Create(path))
            using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create))
            {
                var tasks = data.Select((element, index) => Task.Run(() =>
                {
                    try
                    {
                        lock (zipArchive)
                        {
                        var fileName = $"image_{index}.jpg";
                        var zipEntry = zipArchive.CreateEntry(fileName);

                            using (var entryStream = zipEntry.Open())
                            using (var memoryStream = new MemoryStream(element))
                            {
                                memoryStream.CopyTo(entryStream);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding image {index} to archive: {ex.Message}");
                    }
                })).ToList();

                await Task.WhenAll(tasks);
            }

            stopwatch.Stop();
            Console.WriteLine($"Done [{stopwatch.ElapsedMilliseconds} ms]");
        }

        public static void ArchiveByteArray(List<byte[]> data, string path)
        {
            Console.WriteLine("Adding images to archive...");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using (var fileStream = File.Create(path))
            using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create))
            {
                var tasks = data.Select((element, index) => Task.Run(() =>
                {
                    try
                    {
                        lock (zipArchive)
                        {
                        var fileName = $"image_{index}.jpg";
                        var zipEntry = zipArchive.CreateEntry(fileName);

                            using (var entryStream = zipEntry.Open())
                            using (var memoryStream = new MemoryStream(element))
                            {
                                memoryStream.CopyTo(entryStream);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding image {index} to archive: {ex.Message}");
                    }
                })).ToList();

                Task.WaitAll(tasks.ToArray());
            }

            stopwatch.Stop();
            Console.WriteLine($"Done [{stopwatch.ElapsedMilliseconds} ms]");
        }
    }
}
