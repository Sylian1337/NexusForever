using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.P2PTrading
{
    public class ClientP2PTradingSetMoneyTradeHandler : IMessageHandler<IWorldSession, ClientP2PTradingSetMoney>
    {
        public void HandleMessage(IWorldSession session, ClientP2PTradingSetMoney message)
        {
            session.Player.P2PTradeManager.InitiateSetTradeMoney(message.Credits);
        }
    }
}
