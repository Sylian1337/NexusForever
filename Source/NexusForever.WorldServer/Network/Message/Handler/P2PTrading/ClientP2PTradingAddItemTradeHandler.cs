using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.P2PTrading
{
    public class ClientP2PTradingAddItemTradeHandler : IMessageHandler<IWorldSession, ClientP2PTradingAddItem>
    {
        public void HandleMessage(IWorldSession session, ClientP2PTradingAddItem message)
        {
            session.Player.P2PTradeManager?.InitiateAddItem(message.ItemGuid);
        }
    }
}
