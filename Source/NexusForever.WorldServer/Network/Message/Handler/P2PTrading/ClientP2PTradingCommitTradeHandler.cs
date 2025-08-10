using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.P2PTrading
{
    public class ClientP2PTradingCommitTradeHandler : IMessageHandler<IWorldSession, ClientP2PTradingCommit>
    {
        public void HandleMessage(IWorldSession session, ClientP2PTradingCommit message)
        {
            //session.Player.P2PTradeManager?.InitiateCommit();
        }
    }
}
