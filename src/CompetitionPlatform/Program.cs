using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using AzureStorage.Blob;
using System.Security.Cryptography.X509Certificates;
using Common;
using CompetitionPlatform.Data.AzureRepositories.Settings;

namespace CompetitionPlatform
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:53395/")
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}
