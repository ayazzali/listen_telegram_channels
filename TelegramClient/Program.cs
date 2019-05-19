using Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using convert_audio_message_to_text__bot;
using convert_audio_message_to_text__bot.Services;
using Microsoft.Extensions.Configuration;

namespace Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initialization Main");
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((e, r) =>
            {
                Console.WriteLine();
                Console.WriteLine(r.ExceptionObject);
                Thread.Sleep(2000);//log.LogError("", r);
            });

            var rootPath = System.IO.Path.GetPathRoot(AppContext.BaseDirectory);
            var logDir = System.IO.Path.Combine(rootPath, "logs", "TGToSpeech");
            System.IO.Directory.CreateDirectory(logDir);
            var logPath = System.IO.Path.Combine(logDir, "app.log");

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("cfg.json")
                .Build();

            var serv = new ServiceCollection()
                .AddSingleton(configuration)

                .AddLogging(loggingBuilder => loggingBuilder
                    .AddConsole())
                .AddSingleton<Config>()
                .AddSingleton<TelegramBotProvider>()
                .AddSingleton<TgLog>()
                .AddSingleton<TelegramBotForAuth>()
                .AddSingleton<TelegramClient>()
                .AddSingleton<ProxyChannelFilter>()
                .BuildServiceProvider();

            Task.Run(() =>
            {
                var my = serv.GetService<TelegramClient>();
                my.Run();
            });

            var p = serv.GetService<ProxyChannelFilter>();
            p.register();

            Thread.Sleep(Timeout.Infinite);
            //.AddFile(logPath, append: true)
            //.Configure<LoggerFilterOptions>(_=>_.MinLevel=LogLevel.Trace)
            // dotnet add package Microsoft.Extensions.Configuration.EnvironmentVariables
            //.AddEnvironmentVariables()

            //var logger = serv.GetService<ILogger<Object>>();
            //logger.LogInformation("234234");

            //Thread.Sleep(Timeout.Infinite);
        }
    }
}
