using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using camtt.Services;
using Core.Services;
using Newtonsoft.Json;
using OpenTl.ClientApi;
using OpenTl.ClientApi.MtProto.Exceptions;
using OpenTl.Schema;
using OpenTl.Schema.Channels;
using OpenTl.Schema.Messages;
using OpenTl.Schema.Updates;
using Telegram.Bot;
using TelegramClient.Services.Utils;
using TelegramClient.Utils;

namespace Core
{
    public class TelegramClient
    {
        public IClientApi clientApi;

        public void Run()
        {
            try
            {
                var m = new TaskFactory().StartNew(async () => await StartN(), TaskCreationOptions.LongRunning);
                Task.WaitAll(m);
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                logger.l(ex);
            }
        }
        //Action<TMessage> callback
        TelegramBotForAuth botForAuth;
        Config config;
        TelegramBotClient bot;
        private TgLog logger;
        public event Action OnLoad;

        public TelegramClient(TelegramBotProvider telegramBot, TelegramBotForAuth botForAuth, Config config, TgLog tgLog)
        {
            this.botForAuth = botForAuth;
            this.config = config;
            bot = telegramBot.bot;
            logger = tgLog;
        }

        async Task StartN()
        {
            logger.l("StartN...");
            clientApi = await createClientApi();
            await Auth();
            clientApi.UpdatesService.StartReceiveUpdates(TimeSpan.FromSeconds(1));

            //await GetChannels();
            //await SaveMessagesFromChat(chats[0], 10);
            SubscribeOnMessage();
            //SubscribeOnMessage3();
            clientApi.KeepAliveConnection();
            logger.l("Started!");
            OnLoad();
        }

        async Task<IClientApi> createClientApi()
        {
            var settings = new FactorySettings
            {
                AppHash = AppConfig.API_HASH,//e.g 456a6654ad8f52c54bc4542505884cad,
                AppId = AppConfig.API_ID,// e.g 12345
                ServerAddress = AppConfig.IP,
                ServerPublicKey = AppConfig.PUBLIC_KEY,
                ServerPort = AppConfig.PORT,// e.g 443
                SessionTag = "session",

                Properties = new ApplicationProperties
                {
                    AppVersion = "1.0.1", // You can leave as in the example
                    DeviceModel = "PC", // You can leave as in the example
                    LangCode = "en", // You can leave as in the example
                    LangPack = "tdesktop", // You can leave as in the example
                    SystemLangCode = "en", // You can leave as in the example
                    SystemVersion = "os x" // You can leave as in the example
                }
            };
            return await ClientFactory.BuildClientAsync(settings).ConfigureAwait(false);
        }

        async Task Auth()
        {
            if (!clientApi.AuthService.CurrentUserId.HasValue)
            {
                // Auth
                var phone = config["phone"]; // User phone number with plus
                var sentCode = await clientApi.AuthService.SendCodeAsync(phone);
                Console.Write("Enter Code: ");
                //Console.ReadLine();
                botForAuth.action += async (code) =>
                {
                    TUser user;
                    try
                    {
                        user = await clientApi.AuthService.SignInAsync(phone, sentCode, code).ConfigureAwait(false);
                    }
                    // If the phone code is invalid
                    catch (PhoneCodeInvalidException ex)
                    {
                        logger.l(ex);
                    }
                };
                while (true)
                {
                    if (clientApi.AuthService.CurrentUserId != null)
                        break;
                    Thread.Sleep(1000);
                }
            }
        }

        public event OpenTl.ClientApi.Services.Interfaces.AutoUpdateHandler AutoReceiveUpdates;
        
        HttpClient httpClient = new HttpClient();
        void SubscribeOnMessage()
        {
            clientApi.UpdatesService.AutoReceiveUpdates += AutoReceiveUpdates;
            clientApi.UpdatesService.AutoReceiveUpdates += async update =>
            {
                // handle updates
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
                                    var channelName = "";
                                    if (updates.Chats.Count == 1)
                                    {
                                        var t = updates.Chats[0].As<TChannel>();
                                        if (t != null)
                                        {
                                            channelName = t.Title;
                                        }
                                        var tt = updates.Chats[0].As<TUser>();
                                    }
                                    else if (updates.Chats.Count > 1)
                                        logger.l("updates.Chats.Count=" + updates.Chats.Count);

                                    var msg = Helper.DeleteUrls(mm.Message);
                                    var YaTtsKey = config["YaTtsKey"];
                                    var url = $"https://tts.voicetech.yandex.net/generate?text={msg}&format=mp3&lang=ru-RU&speaker=jane&emotion=good&key={YaTtsKey}";
                                    try
                                    {
                                        var st = await httpClient.GetStreamAsync(url);
                                        var r = await bot.SendVoiceAsync(config["ttsToWhom"], new Telegram.Bot.Types.FileToSend(channelName, st), channelName);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.l(e);
                                    }
                                }
                            }
                        }
                        break;
                    case TUpdatesCombined updatesCombined:
                        break;
                    case TUpdateShort updateShort:
                        break;
                    case TUpdateShortChatMessage updateShortChatMessage:
                        break;
                    case TUpdateShortMessage updateShortMessage:
                        break;
                    case TUpdateShortSentMessage updateShortSentMessage:
                        break;
                    case TUpdatesTooLong updatesTooLong:
                        break;
                }
            };
        }

        //void SubscribeOnMessage3()
        //{
        //    clientApi.UpdatesService.StartReceiveUpdates(TimeSpan.FromSeconds(1));
        //    // Send message to myself
        //    clientApi.UpdatesService.ManualReceiveUpdates += diff =>
        //    {
        //        // handle updates
        //        switch (diff)
        //        {
        //            case TDifference difference:
        //                break;
        //            case TDifferenceSlice differenceSlice:
        //                break;
        //            case TDifferenceTooLong differenceTooLong:
        //                break;
        //        }
        //    };
        //}

        //async Task<IDialogs> GetChannels()
        //{
        //    var channels = await clientApi.MessagesService.GetUserDialogsAsync();

        //    chats = (channels as TDialogs).Chats;

        //    string json = JsonConvert.SerializeObject(chats);

        //    CreateFileHelper.CreateFile("channels.json", json);
        //    return channels;
        //}

        //async Task SaveMessagesFromChat(IChat chat, int count)
        //{
        //    IInputPeer peer;

        //    peer = PeerFabrica.CreatePeer(chat);

        //    var messages = await clientApi.MessagesService.GetHistoryAsync(peer, 0, int.MaxValue, count);
        //}

        //async Task<IMessages> GetMessages(IChat chat, int count)
        //{
        //    IInputPeer peer = PeerFabrica.CreatePeer(chat);

        //    var messages = await clientApi.MessagesService.GetHistoryAsync(peer, 0, int.MaxValue, count);

        //    return messages;
        //}
    }
}
