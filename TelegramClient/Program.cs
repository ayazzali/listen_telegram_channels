using Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using convert_audio_message_to_text__bot;
using convert_audio_message_to_text__bot.Services;

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
                Thread.Sleep(2000);
                //log.LogError("", r);
            });

            var rootPath = System.IO.Path.GetPathRoot(AppContext.BaseDirectory);
            var logDir = System.IO.Path.Combine(rootPath, "logs", "TGToSpeech");
            System.IO.Directory.CreateDirectory(logDir);
            var logPath = System.IO.Path.Combine(logDir, "app.log");

            var serv = new ServiceCollection()
                .AddLogging(loggingBuilder => loggingBuilder
                    //.AddFile(logPath, append: true)
                    .AddConsole())
                //.ConfigureLogging()
                .AddSingleton<Config>()
                .AddSingleton<Settings>()
                .AddSingleton<TelegramBotProvider>()
                .AddSingleton<TgLog>()
                .AddSingleton<TelegramBotForAuth>()
                .AddSingleton<TelegramClient>()
                .BuildServiceProvider();

            var my = serv.GetService<TelegramClient>();
            my.Run();
            //var t = Task.Run( () =>  InstanceOfBot.Auth());
            //t.Wait();
            //if (t.Exception != null)
            //{
            //    Console.WriteLine(t.Exception?.Message);
            //    Thread.Sleep(5000);
            //}

            //Thread.Sleep(Timeout.Infinite);
        }
    }
}
