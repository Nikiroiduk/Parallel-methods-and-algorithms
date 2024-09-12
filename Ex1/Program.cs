using System;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string projectDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\.."));
        string filePath = Path.Combine(projectDirectory, "Ex1List.txt");

        List<string> urls = new();

        using (var reader = new StreamReader(filePath))
        {
            try
            {
                    string line;
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

        using (var client = new HttpClient())
        {
            try
            {
                foreach (var url in urls)
                {
                    string data = await client.GetStringAsync(url);
                    Console.WriteLine(data);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error downloading data: {e.Message}");
            }
        }
    }
}