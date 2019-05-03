using System;
using OpenTl.Schema;

namespace TelegramClient.Utils
{
    public class PeerFabrica
    {
        public PeerFabrica()
        {
        }

        public static IInputPeer CreatePeer(IChat chat)
        {
            IInputPeer peer;

            if (chat is TChat) peer = CreatePeer((TChat)chat);

            else if (chat is TChannel) peer = CreatePeer((TChannel)chat);

            else if (chat is TChatForbidden) peer = CreatePeer((TChatForbidden)chat);

            else if (chat is TChannelForbidden) peer = CreatePeer((TChannelForbidden)chat);

            else peer = new TInputPeerChat();

            return peer;
        }

        private static IInputPeer CreatePeer(TChat chat)
        {
            return new TInputPeerChat() { ChatId = chat.Id };
        }

        private static IInputPeer CreatePeer(TChatForbidden chat)
        {
            return new TInputPeerChat() { ChatId = chat.Id };
        }

        private static IInputPeer CreatePeer(TChannel chat)
        {
            return new TInputPeerChannel() { ChannelId = chat.Id, AccessHash = chat.AccessHash};
        }

        private static IInputPeer CreatePeer(TChannelForbidden chat)
        {
            return new TInputPeerChannel() { ChannelId = chat.Id, AccessHash = chat.AccessHash };
        }
    }
}
