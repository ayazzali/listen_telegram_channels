using Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace camtt.Services
{
    /// <summary>
    ///  в телегу пишет поставив в очередь (в 2сек)
    /// </summary>
    public class TgLog
    {
        TelegramBotClient bot { get; set; }
        string logBackTgId = "";
        readonly ILogger<TgLog> logger;
        public TgLog(TelegramBotProvider bp, Config  s,ILogger<TgLog> logger)
        {
            this.logger = logger;
            bot = bp.bot;
            logBackTgId = s["logBack"];
            if (string.IsNullOrWhiteSpace(logBackTgId))
                Console.WriteLine("WARN: logs will not be sent to your telegram chat");
            else
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((e, r) => { l(r.ExceptionObject.ToString()); });

            if (!isRunningQueueLogHandler)
                Task.Run(async () =>
                {
                    isRunningQueueLogHandler = true;
                    while (true)
                    {
                        await Task.Delay(4000);
                        if (vs.TryDequeue(out string f))
                            _l(f);
                    }
                });
        }

        static bool isRunningQueueLogHandler; // если вдруг не синглтон - useless
        static Queue<string> vs = new Queue<string>();
        public void l(string s)
        {
            logger.LogInformation(s);
            vs.Enqueue(s);
        }
        public void l(Exception e)
        {
            var s = e.Message + Environment.NewLine + e.StackTrace;
            logger.LogInformation(s);
            vs.Enqueue(s);
        }

        static int lBackMessageId = 0;
        private void _l(string s)
        {
            if (!string.IsNullOrWhiteSpace(logBackTgId))
                try
                {
                    if (lBackMessageId == 0)
                        lBackMessageId = bot.SendTextMessageAsync(logBackTgId, "LOG: "+s).Result.MessageId;

                    bot.EditMessageTextAsync(logBackTgId, lBackMessageId, s);
                }
                catch (Exception e)
                {
                    logger.LogError("tg send error: ", e);
                }
        }
    }
}
