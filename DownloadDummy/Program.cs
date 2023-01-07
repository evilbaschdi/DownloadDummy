namespace DownloadDummy;

static class Program
{
    // ReSharper disable once UnusedParameter.Local
    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    static async Task Main()
    {
        Console.WriteLine("Start");
        Console.WriteLine();

        var client = new HttpClient();

        for (int i = 0; i < 10000; i++)
        {
            var fileName = $"{i}.gif";
            var link = $"https://www.martin-perscheid.de/image/cartoon/{fileName}";
            var file = $@"{Directory.GetCurrentDirectory()}\{fileName}";
            try
            {
                Console.WriteLine($"{DateTime.Now:s}");
                Console.WriteLine($"Try to download from '{link}'");

                var response = await client.GetAsync(link);
                await using (var fs = new FileStream(file, FileMode.CreateNew))
                {
                    await response.Content.CopyToAsync(fs);
                }

                Console.WriteLine();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        Console.WriteLine($"End: {DateTime.Now:s}");
    }
}