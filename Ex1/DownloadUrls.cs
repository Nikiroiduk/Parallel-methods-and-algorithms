using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ex1
{
    internal static class DownloadUrls
    {
        public static async Task<List<string>> RunAsync(string filePath)
        {

            List<string> urls = ParseFile(filePath);
            
            List<string> urlsData = await DownloadDataAsync(urls);

            return urlsData;

        }

        public static List<string> Run(string filePath)
        {

            List<string> urls = ParseFile(filePath);

            List<string> urlsData = DownloadData(urls);

            return urlsData;

        }

        internal static List<string> ParseFile(string filePath)
        {
            List<string> urls = new();

            using (var reader = new StreamReader(filePath))
            {
                try
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        urls.Add(line);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file: {ex.Message}");
                }
            }

            return urls;
        }


        internal static async Task<List<string>> DownloadDataAsync(List<string> urls)
        {
            List<string> urlsData = new();
            using (var client = new HttpClient())
            {
                var downloadTasks = urls.Select(async url =>
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    Console.WriteLine($"Start downloading: {url}");

                    try
                    {
                        string data = await client.GetStringAsync(url);
                        lock (urlsData)
                        {
                            urlsData.Add(data);
                        }

                        stopwatch.Stop();
                        Console.WriteLine($"Download complete: {url} [{stopwatch.ElapsedMilliseconds} ms]");
                    }
                    catch (HttpRequestException e)
                    {
                        stopwatch.Stop();
                        Console.WriteLine($"Error downloading data from {url}: {e.Message}");
                    }
                });

                await Task.WhenAll(downloadTasks);
            }

            return urlsData;

        }


        internal static List<string> DownloadData(List<string> urls)
        {
            List<string> urlsData = new();
            var httpClient = new HttpClient();

            var downloadTasks = urls.Select(url => Task.Run(() =>
            {
                Stopwatch taskStopwatch = new Stopwatch();
                taskStopwatch.Start();
                Console.WriteLine($"Start downloading: {url}");

                try
                {
                    var response = httpClient.GetAsync(url).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    string data = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    lock (urlsData)
                    {
                        urlsData.Add(data);
                    }
                    taskStopwatch.Stop();
                    Console.WriteLine($"Download complete: {url} [{taskStopwatch.ElapsedMilliseconds} ms]");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error downloading data from {url}: {e.Message}");
                }
            })).ToList();

            Task.WaitAll(downloadTasks.ToArray());

            return urlsData;
        }


    }

}