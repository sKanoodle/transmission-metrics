﻿using InfluxData.Net.InfluxDb;
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
            SecureString ss = new SecureString();
            ConsoleKeyInfo key;
            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                ss.AppendChar(key.KeyChar);
                Console.Write("*");
            }
            


            Random random = new Random();

            while (true)
            {
                //var point = new Point
                //{
                //    Name = "torrents",
                //    Fields = new Dictionary<string, object>
                //    {
                //        ["Space"] = 12 + random.Next(2),
                //        ["Seeding"] = 24 + random.Next(1),
                //        ["Downloading"] = 46 + random.Next(1),
                //        ["Paused"] = 64,
                //        ["Downspeed"] = random.Next(24),
                //        ["Upspeed"] = random.Next(24),
                //    }
                //};

                var point = await GetMetrics(client);
                //await influxClient.Client.WriteAsync(point, "Transmission");
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        private static async Task<Point> GetMetrics(Client client)
        {
            var foo = client.SimpleGETAsync();

            var torrents = await client.TorrentGetAsync(TorrentFields.Status);
            var space = await client.GetFreeSpaceAsync();
            var sessionInfo = await client.GetSessionStats();
            return null;
            //await Task.WhenAll(torrents, space, sessionInfo);

            //var groups = torrents.Result.GroupBy(t => t.Status);

            //return new Point
            //{
            //    Name = "torrents",
            //    Fields = new Dictionary<string, object>
            //    {
            //        ["Space"] = space.Result.Size / 1024d / 1024 / 1024,
            //        ["Seeding"] = groups.Single(g => g.Key == Status.Seed).Count(),
            //        ["Downloading"] = groups.Single(g => g.Key == Status.Download).Count(),
            //        ["Paused"] = groups.Single(g => g.Key == Status.Stopped).Count(),
            //        ["Downspeed"] = sessionInfo.Result.DownloadSpeed / 1024d / 1024,
            //        ["Upspeed"] = sessionInfo.Result.UploadSpeed / 1024d / 1024,
            //    },
            //};
        }
    }
}
