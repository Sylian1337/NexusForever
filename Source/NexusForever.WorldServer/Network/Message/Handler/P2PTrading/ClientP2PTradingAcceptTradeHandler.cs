using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.P2PTrading
{
    public class ClientP2PTradingAcceptTradeHandler : IMessageHandler<IWorldSession, ClientP2PTradingAcceptInvite>
    {
        public void HandleMessage(IWorldSession session, ClientP2PTradingAcceptInvite message)
        {
            session.Player.P2PTradeManager?.InitiateAcceptInvite();
        }
    }
}
