using NexusForever.Game.Entity;
using NexusForever.Network;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.P2PTrading
{
    /// <summary>
    /// Handles the client sending a trade request to another player.
    /// </summary>
    public class ClientP2PTradingInitiateTradeHandler : IMessageHandler<IWorldSession, ClientP2PTradingInitiateTrade>
    {
        public void HandleMessage(IWorldSession session, ClientP2PTradingInitiateTrade message)
        {
            session.Player.P2PTradeManager.InitiateTrade(message.TargetUnitId);
        }
    }
}
