using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.P2PTrading
{
    public class ClientP2PTradingCancelTradeHandler : IMessageHandler<IWorldSession, ClientP2PTradingCancelTrade>
    {
        public void HandleMessage(IWorldSession session, ClientP2PTradingCancelTrade message)
        {
            //session.Player.P2PTradeManager.InitiateCancelTrade();
        }
    }
}
