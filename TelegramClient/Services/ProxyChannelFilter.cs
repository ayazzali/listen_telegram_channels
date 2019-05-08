using convert_audio_message_to_text__bot.Services;
using Core.Services;
using Microsoft.Extensions.Logging;
using OpenTl.Schema;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Linq;

namespace Core
{
    /// <summary>
    ///  в телегу пишет поставив в очередь (в 2сек)
    /// </summary>
    public class ProxyChannelFilter
    {
        TelegramBotClient bot { get; set; }
        string logBackTgId = "";
        TgLog logger;
        TelegramBotProvider bp;
        TelegramClient tc;
        Config s;

        public ProxyChannelFilter(TelegramClient tc, TelegramBotProvider bp, Config s, TgLog logger)
        {
            this.logger = logger;
            this.bp = bp;
            this.tc = tc;
            this.s = s;

            register();
        }

        void register()
        {
            // todo sign up to link channel
            // proxyToWhomId link words:kazan,bua regexp:kazan|bua; proxyToWhomId link regexp;

            List<ChannelFilter> channelFilters = new List<ChannelFilter>();
            var _chFilters = s["channelFilter"].Split(';');
            foreach (var fi in _chFilters)
            {
                try
                {
                    ChannelFilter.Parse(fi);
                }
                catch (Exception ex)
                {
                    logger.l(ex);
                }
            }

            tc.AutoReceiveUpdates += update =>
            {
                switch (update)
                {
                    case TUpdates updates:
                        foreach (var u in updates.Updates)
                        {
                            if (u is TUpdateNewChannelMessage m)
                            {
                                if (m.Message is TMessage mm)
                                {
                                    logger.l(mm.Message);
                                    var msg = mm.Message;
                                    foreach (var fi in channelFilters)
                                        if (fi.Words.Any(w => msg.Contains(w)))
                                            bot.SendTextMessageAsync(fi.ChatId, mm.Message);
                                    //else if (fi.Regexp)
                                }
                            }
                        }
                        break;
                }
            };

        }
    }

    class ChannelFilter
    {
        /// <summary>
        /// proxyToWhom
        /// </summary>
        public string ChatId;

        /// <summary>
        /// for https://t.me/ayazzali_test_channel its ayazzali_test_channel
        /// </summary>
        public string Shortlink;

        /// <summary>
        /// if any word is finded 
        /// </summary>
        public string[] Words;

        public string Regexp;

        public static ChannelFilter Parse(string oneChannelFilterFromConfig)
        {
            var f = new ChannelFilter();
            var fs = oneChannelFilterFromConfig.Split(' ');
            f.ChatId = fs[0].Trim();
            f.Shortlink = fs[1].Trim();
            f.Words = fs[2].Split(',');
            f.Regexp = fs[3];
            return f;
        }
    }
}
