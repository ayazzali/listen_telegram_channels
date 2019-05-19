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
using OpenTl.ClientApi;

namespace Core
{
    /// <summary>
    ///  в телегу пишет поставив в очередь (в 2сек)
    /// </summary>
    public class ProxyChannelFilter
    {
        TgLog logger;
        TelegramBotClient bot;
        TelegramClient tc;
        Config s;

        public ProxyChannelFilter(TelegramClient tc, TelegramBotProvider bp, Config s, TgLog logger)
        {
            this.logger = logger;
            this.bot = bp.bot;
            this.tc = tc;
            this.s = s;

            //register();
        }

        public void register()
        {
            if (string.IsNullOrWhiteSpace(s["channelFilter"]))
                return;

            // todo sign up to link channel
            // proxyToWhomId link words:kazan,bua regexp:kazan|bua; proxyToWhomId link regexp;
            List<ChannelFilter> channelFilters = new List<ChannelFilter>();
            var _chFilters = s["channelFilter"].Split(';', StringSplitOptions.RemoveEmptyEntries).Where(_ => _.Length > 0);
            foreach (var fi in _chFilters)
            {
                try
                {
                    var f = ChannelFilter.Parse(fi);
                    channelFilters.Add(f);
                }
                catch (Exception ex)
                {
                    logger.l(ex);
                }
            }

            tc.OnLoad += async () =>
            {
                foreach (var ch in channelFilters)
                {
                    try
                    {
                        var f = await tc.clientApi.ContactsService.SearchUserAsync(ch.UserName_proxyToWhom);
                        var user = f.Users.FirstOrDefault(_ => _.As<TUser>().Username == ch.UserName_proxyToWhom);
                        ch.User_proxyToWhom = user.As<TUser>();

                        var nn = await tc.clientApi.ContactsService.SearchUserAsync(ch.Shortlink);
                        var cc = nn.Chats.FirstOrDefault(_ => _.As<TChannel>().Username == ch.Shortlink)?.As<TChannel>();//.Title; //Username LE
                        ch.channel = cc;

                        var cc1 = nn.Users.FirstOrDefault(_ => _.As<TUser>().Username == ch.Shortlink)?.As<TUser>();
                        ch.bot_source = cc1;
                    }
                    catch (Exception ex)
                    {
                        logger.l(ex);
                    }
                }

                tc.clientApi.UpdatesService.AutoReceiveUpdates += update =>
                {
                    try
                    {
                        switch (update)
                        {
                            case TUpdates updates:
                                foreach (var u in updates.Updates)
                                {
                                    {
                                        if (u is TUpdateNewChannelMessage m)
                                        {
                                            if (m.Message is TMessage mm)
                                            {
                                                var match = channelFilters.Where(_ => _.channel?.Id == mm.ToId.As<TPeerChannel>().ChannelId);
                                                var msg = mm.Message.ToLower();
                                                foreach (var fi in match)
                                                    if (fi.Words.Any(w => msg.Contains(w.ToLower())))
                                                        bot.SendTextMessageAsync(fi.User_proxyToWhom.Id, mm.Message);
                                            //else if (fi.Regexp)
                                        }
                                        }
                                    }
                                    {
                                        if (u is TUpdateNewMessage m)
                                        {
                                            if (m.Message is TMessage mm)
                                            {
                                                var match = channelFilters.Where(_ => _.bot_source?.Id == mm.FromId).ToList();
                                                var msg = mm.Message.ToLower();
                                                foreach (var fi in match)
                                                {
                                                    if (fi.Words.Any(w => msg.Contains(w.ToLower())))
                                                    {
                                                        bot.SendTextMessageAsync(fi.User_proxyToWhom.Id, mm.Message);
                                                        // forward
                                                    }
                                                }
                                            //else if (fi.Regexp)
                                        }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.l(ex);
                    }
                };
            };

        }
    }

    class ChannelFilter
    {
        ///// <summary>
        ///// proxyToWhom
        ///// </summary>
        //public string ChatId;

        public string UserName_proxyToWhom;

        /// <summary>
        /// from UserName_proxyToWhom
        /// </summary>
        public TUser User_proxyToWhom;

        /// <summary>
        /// for https://t.me/ayazzali_test_channel its ayazzali_test_channel
        /// </summary>
        public string Shortlink;

        /// <summary>
        /// from Shortlink
        /// </summary>
        public TChannel channel;

        /// <summary>
        /// from Shortlink
        /// </summary>
        public TUser bot_source;

        /// <summary>
        /// if any word is finded 
        /// </summary>
        public string[] Words;

        // TODO
        public string Regexp;

        public static ChannelFilter Parse(string oneChannelFilterFromConfig)
        {
            var f = new ChannelFilter();
            var fs = oneChannelFilterFromConfig.Split(' ',StringSplitOptions.RemoveEmptyEntries);
            //f.ChatId 
            f.UserName_proxyToWhom = fs[0].Trim();
            f.Shortlink = fs[1].Trim();
            f.Words = fs[2].Substring("words:".Length).Split(',');
            if (fs.Length != 3)
                f.Regexp = fs[3].Substring("regexp:".Length);
            return f;
        }
    }
}
