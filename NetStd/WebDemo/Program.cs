using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;


namespace WebDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string logName = Path.Combine("logs", "WebDemo.log");
            // Add logging using Serilog
            // https://github.com/serilog/serilog-sinks-file

            // https://github.com/serilog/serilog/wiki/Formatting-Output
            // https://github.com/serilog/serilog/wiki/Configuration-Basics
            string logFormat = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";
#if DEBUG      
            bool bufferLog = false;
#else
            bool bufferLog = true;
#endif
            Serilog.Log.Logger = new Serilog.LoggerConfiguration()
                .WriteTo.File(
                    logName, 
                    rollingInterval: RollingInterval.Day, 
                    retainedFileCountLimit: 31,
                    outputTemplate: logFormat,
                    buffered: bufferLog)
                .CreateLogger();

            try {
                Log.Information("Web host started");
                BuildWebHost(args).Run();
                Log.Information("Web host stopped");
                }
            catch (Exception ex) {
                Log.Fatal(ex, "Host terminated due to exception");
                }
            finally {
                Log.CloseAndFlush();
                }
           }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();
    }
}
