using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.P2PTrading
{
    public class ClientP2PTradingRemoveItemTradeHandler : IMessageHandler<IWorldSession, ClientP2PTradingRemoveItem>
    {
        public void HandleMessage(IWorldSession session, ClientP2PTradingRemoveItem message)
        {
            session.Player.P2PTradeManager?.InitiateRemoveItem(message.ItemGuid);
        }
    }
}
