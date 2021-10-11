using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DownloadDummy.Core;

namespace DownloadDummy
{
    static class Program
    {
        // ReSharper disable once UnusedParameter.Local
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        static async Task Main()
        {
            Console.WriteLine("Start");
            Console.WriteLine();

            const string link = "https://download01.logi.com/web/ftp/pub/techsupport/options/Options_9.20.389.exe";
            var file = $@"{Directory.GetCurrentDirectory()}\Options_9.20.389.exe";

            var client = new HttpClient();

            var isValid = false;

            while (!isValid)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now:s}");
                    Console.WriteLine($"Try to download from '{link}'");

                    var response = await client.GetAsync(link);
                    await using (var fs = new FileStream(file, FileMode.CreateNew))
                    {
                        await response.Content.CopyToAsync(fs);
                    }

                    isValid = ExeChecker.IsValidExe(file);
                    Console.WriteLine();

                    if (!isValid)
                    {
                        Console.WriteLine("File not valid. Retry in one minute.");
                        File.Delete(file);
                        Thread.Sleep((int)TimeSpan.FromMinutes(1).TotalMilliseconds);
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine(file);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            Console.WriteLine($"End: {DateTime.Now:s}");
        }
    }
}