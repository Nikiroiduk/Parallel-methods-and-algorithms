using Ex1;
using System;
using System.Diagnostics;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string projectDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory()));

        string? urlsFile = null;
        string? archiveAdress = null;

        var arguments = new Dictionary<string, string>();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-i":
                    if (i + 1 < args.Length)
                    {
                        arguments["input"] = args[++i];
                    }
                    else
                    {
                        Console.WriteLine("Error: Missing value for -i.");
                        return;
                    }
                    break;

                case "-o":
                    if (i + 1 < args.Length)
                    {
                        arguments["output"] = args[++i];
                    }
                    else
                    {
                        Console.WriteLine("Error: Missing value for -o.");
                        return;
                    }
                    break;

                default:
                    if (urlsFile == null)
                    {
                        urlsFile = args[i];
                    }
                    else if (archiveAdress == null)
                    {
                        archiveAdress = args[i];
                    }
                    else
                    {
                        Console.WriteLine($"Error: Unexpected argument '{args[i]}'.");
                        return;
                    }
                    break;
            }
        }

        if (arguments.ContainsKey("input"))
        {
            urlsFile = arguments["input"];
        }

        if (arguments.ContainsKey("output"))
        {
            archiveAdress = arguments["output"];
        }

        if (args.Length == 0)
        {
            urlsFile = Path.Combine(projectDirectory, "urlsList.txt");
            archiveAdress = Path.Combine(projectDirectory, "images.zip");

            if (!File.Exists(urlsFile))
            {
                Console.WriteLine("File urlsList.txt not found");
                return;
            }
        }



        Stopwatch stopwatchAsync = new();
        stopwatchAsync.Start();
        var urlsData = DownloadUrls.Run(urlsFile);

        var images = DownloadImgs.Run(urlsData);

        Archive.ArchiveByteArray(images, archiveAdress);
        stopwatchAsync.Stop();
        Console.WriteLine($"\nTotal sync time: {stopwatchAsync.ElapsedMilliseconds} ms.");

        Console.WriteLine();

        Stopwatch stopwatchSync = new();
        stopwatchSync.Start();
        var urlsDataAsync = await DownloadUrls.RunAsync(urlsFile);

        var imagesAsync = await DownloadImgs.RunAsync(urlsDataAsync);

        await Archive.ArchiveByteArrayAsync(imagesAsync, archiveAdress);
        stopwatchSync.Stop();
        Console.WriteLine($"\nTotal async time: {stopwatchSync.ElapsedMilliseconds} ms.");

        Console.WriteLine();

        Console.WriteLine($"Async is {(double)stopwatchAsync.ElapsedMilliseconds / stopwatchSync.ElapsedMilliseconds:F2} times faster");


        Console.ReadKey();
    }
}