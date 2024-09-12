using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ex1
{
    internal static class DownloadImgs
    {
        public static async Task<List<Byte[]>> RunAsync(List<string> urlsData)
        {

            List<string> imgUrls = ExtractImageUrls(urlsData);

            return await DownloadImagesAsync(imgUrls);
        }

        public static List<Byte[]> Run(List<string> urlsData)
        {

            List<string> imgUrls = ExtractImageUrls(urlsData);

            return DownloadImages(imgUrls);
        }

        internal static List<string> ExtractImageUrls(List<string> urlsData)
        {
            string pattern = @"<img\s+[^>]*?src=""([^""]*?)""";
            List<string> imgUrls = new();

            foreach (var site in urlsData)
            {
                var matches = Regex.Matches(site, pattern, RegexOptions.IgnoreCase);
                imgUrls.AddRange(matches.Cast<Match>()
                                        .Select(match => match.Groups[1].Value)
                                        .ToList());
            }

            return imgUrls;
        }

        internal static async Task<List<Byte[]>> DownloadImagesAsync(List<string> imageUrls)
        {
            Console.WriteLine("Downloading images...");
            var errors = 0;
            List<Byte[]> images = new();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using (var client = new HttpClient())
            {
                var downloadTasks = imageUrls.Select(async imageUrl =>
                {
                    try
                    {
                        var imageBytes = await client.GetByteArrayAsync(imageUrl);
                        lock (images)
                        {
                            images.Add(imageBytes);
                        }
                    }
                    catch (Exception)
                    {
                        lock (images)
                        {
                            errors++;
                        }
                    Console.WriteLine(imageUrl);
                    }
                });

                await Task.WhenAll(downloadTasks);
            }

            stopwatch.Stop();
            Console.WriteLine($"Done ({imageUrls.Count - errors} out of {imageUrls.Count}) [{stopwatch.ElapsedMilliseconds} ms]");

            return images;
        }

        internal static List<byte[]> DownloadImages(List<string> imageUrls)
        {
            Console.WriteLine("Downloading images...");
            var errors = 0;
            List<byte[]> images = new();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


            var downloadTasks = imageUrls.Select(imageUrl => Task.Run(() =>
            {
            var httpClient = new HttpClient();
                try
                {
                    Console.WriteLine($"Image downloading {imageUrl} [{stopwatch.ElapsedMilliseconds} ms]");
                    var response = httpClient.GetAsync(imageUrl).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    byte[] imageBytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    lock (images)
                    {
                        images.Add(imageBytes);
                    }
                }
                catch (Exception ex)
                {
                    lock (images)
                    {
                        errors++;
                    }
                }
            })).ToList();

            Task.WaitAll(downloadTasks.ToArray());

            stopwatch.Stop();
            Console.WriteLine($"Done ({imageUrls.Count - errors} out of {imageUrls.Count}) [{stopwatch.ElapsedMilliseconds} ms]");

            return images;
        }


    }
}
