using NexusForever.Game.Abstract.Entity;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.P2PTrading
{
    public class ClientP2PTradingAcceptTradeHandler : IMessageHandler<IWorldSession, ClientP2PTradingAcceptInvite>
    {
        #region Dependency Injection
        private readonly IP2PTradeManager tradeManager;

        public ClientP2PTradingAcceptTradeHandler(
            IP2PTradeManager tradeManager)
        {
            this.tradeManager = tradeManager;

        }
        #endregion

        public void HandleMessage(IWorldSession session, ClientP2PTradingAcceptInvite message)
        {
            // Store player.
            IPlayer player = session.Player;

            // Trade session.
            var tradeSession = tradeManager.GetTradeById(player.TradeId);
            if (!tradeManager.TryResolvePlayers(player, tradeSession, out var initiator, out var target))
            {
                tradeManager.SendTradeErrorTo(player, ServerP2PTradeResult.P2PTradeResult.MissingPlayer);
                return;
            }

            tradeManager.StartTrade(tradeSession, initiator, target);






            //session.Player.P2PTradeManager?.InitiateAcceptInvite();
        }
    }
}
