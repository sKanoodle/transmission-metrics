using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Transmission.Api;
using Transmission.Api.Entities;

namespace TransmissionMetrics
{
    class Program
    {
        static void Main(string[] args)
        {
            Run();
            Console.Read();

            // torrents paused, downloading, seeding, total
            // downspeed, upspeed
            // free space
        }

        private static async Task Run()
        {
            Console.Write("password: ");
            SecureString ss = new SecureString();
            ConsoleKeyInfo key;
            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                ss.AppendChar(key.KeyChar);
                Console.Write("*");
            }
            

            Random random = new Random();
            var getMetrics = MakeMetricsFunc();

            while (true)
            {
                var point = await getMetrics(client);
                await influxClient.Client.WriteAsync(point, "Transmission");
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        private static Func<Client, Task<Point>> MakeMetricsFunc()
        {
            DateTime previousRun = DateTime.Now;
            ulong previousDownloaded = 0;
            ulong previousUploaded = 0;

            (double Down, double Up) Speed(Stats session)
            {
                DateTime now = DateTime.Now;
                double down = (previousDownloaded < 1 ? (double)session.DownloadSpeed : (session.AllTimeStats.DownloadedBytes - previousDownloaded) / (now - previousRun).TotalSeconds) / 1024 / 1024;
                double up = (previousUploaded < 1 ? (double)session.UploadSpeed : (session.AllTimeStats.UploadedBytes - previousUploaded) / (now - previousRun).TotalSeconds) / 1024 / 1024;
                previousRun = now;
                previousDownloaded = session.AllTimeStats.DownloadedBytes;
                previousUploaded = session.AllTimeStats.UploadedBytes;
                return (down, up);
            }

            return async client =>
            {
                var torrents = client.TorrentGetAsync(TorrentFields.Status);
                var space = client.GetFreeSpaceAsync();
                var sessionInfo = client.GetSessionStats();

                await Task.WhenAll(torrents, space, sessionInfo);

                var groups = torrents.Result.GroupBy(t => t.Status);
                (var down, var up) = Speed(sessionInfo.Result);

                return new Point
                {
                    Name = "torrents",
                    Fields = new Dictionary<string, object>
                    {
                        ["Space"] = (double)space.Result.Size / 1024 / 1024 / 1024,
                        ["Seeding"] = groups.Single(g => g.Key == Status.Seed).Count(),
                        ["Downloading"] = groups.Single(g => g.Key == Status.Download).Count(),
                        ["Paused"] = groups.Single(g => g.Key == Status.Stopped).Count(),
                        ["Downspeed"] = down,
                        ["Upspeed"] = up,
                    },
                };
            };
        }
    }
}
