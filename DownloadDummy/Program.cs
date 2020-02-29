using System;
using System.IO;
using System.Net;
using System.Threading;
using DownloadDummy.Core;

namespace DownloadDummy
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            Console.WriteLine();

            using WebClient webClient = new WebClient();
            var link = "http://ssd.samsungsemi.com/ecomobile/ssd/update15.do?fname=/Samsung_NVM_Express_Driver_3.3.exe";
            var file = $@"{Directory.GetCurrentDirectory()}\Samsung_NVM_Express_Driver_3.3.exe";

            var isValid = false;

            while (!isValid)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now:s}");
                    Console.WriteLine($"Try to download from '{link}'");
                    webClient.DownloadFile(new Uri(link), file);

                    isValid = ExeChecker.IsValidExe(file);
                    Console.WriteLine();

                    if (!isValid)
                    {
                        Console.WriteLine("File not valid. Retry in one minute.");
                        File.Delete(file);
                        Thread.Sleep((int) TimeSpan.FromMinutes(1).TotalMilliseconds);
                        Console.WriteLine();
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