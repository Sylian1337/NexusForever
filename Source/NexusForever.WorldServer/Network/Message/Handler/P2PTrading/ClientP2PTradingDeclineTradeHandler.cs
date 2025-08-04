using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.P2PTrading
{
    public class ClientP2PTradingDeclineTradeHandler : IMessageHandler<IWorldSession, ClientP2PTradingDeclineInvite>
    {
        public void HandleMessage(IWorldSession session, ClientP2PTradingDeclineInvite message)
        {
            var player = session.Player;
            player.P2PTradeManager?.InitiateDeclineInvite();
        }
    }
}
