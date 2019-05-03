using convert_audio_message_to_text__bot.Services;
using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Core
{
    public class TelegramBotForAuth
    {
        public event Action<string> action;

        TelegramBotClient bot;
        private TgLog tgLog;

        public TelegramBotForAuth(TelegramBotProvider telegramBot, TgLog tgLog)
        {
            bot = telegramBot.bot;
            this.tgLog = tgLog;
            bot.OnMessage += Bot_OnMessage;
            bot.StartReceiving();
        }

        private void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs messageEventArgs)
        {
            var tId = string.Format("{0:d3}_newMsg_", Thread.CurrentThread.ManagedThreadId);
            tgLog.l(tId + DateTime.Now.ToString());
            var message = messageEventArgs.Message;
            //tgLog.Info(message.Chat.Id.ToString());
            if (message == null) return;
            //tgLog.l(tId + "Type " + message.Type);
            //tgLog.l(tId + "From " + message.Chat.Id);
            if (message.Type == MessageType.TextMessage)
                action?.Invoke(message.Text);
        }
    }
}