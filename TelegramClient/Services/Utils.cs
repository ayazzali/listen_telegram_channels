using Core.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Telegram.Bot;

namespace convert_audio_message_to_text__bot.Services
{

    using Settings = Config;
    //public class Settings
    //{//cfg = builder.Build();

    //    public IConfigurationRoot cfg { get; set; }

    //    public Settings() => cfg = new ConfigurationBuilder().AddJsonFile("cfg.json").Build();
    //}

    public class TelegramBotProvider
    {
        public TelegramBotClient bot { get; set; }
        public TelegramBotProvider(Settings s)
        {
            var cfg = s;
            Console.WriteLine(cfg["telegramKey"] + string.Format("_{0:d3} init", Thread.CurrentThread.ManagedThreadId));
            bot = new TelegramBotClient(cfg["telegramKey"]);
        }
    }
}
