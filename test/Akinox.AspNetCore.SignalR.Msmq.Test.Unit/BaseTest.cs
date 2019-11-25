using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Akinox.AspNetCore.SignalR.Msmq.Test.Unit
{
    public abstract class BaseTest
    {
        public IConfiguration Configuration { get; private set; }

        public BaseTest()
        {
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Split("bin")[0];
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true, reloadOnChange: false);
            this.Configuration = builder.Build();
        }
    }
}
