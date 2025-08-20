using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.P2PTrading;
using NexusForever.Game.Entity;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;
using System;

namespace NexusForever.WorldServer.Network.Message.Handler.P2PTrading
{
    public class ClientP2PTradingDeclineTradeHandler : IMessageHandler<IWorldSession, ClientP2PTradingDeclineInvite>
    {

        #region Dependency Injection
        private readonly IP2PTradeManager tradeManager;

        public ClientP2PTradingDeclineTradeHandler(
            IP2PTradeManager tradeManager)
        {
            this.tradeManager = tradeManager;

        }
        #endregion


        public void HandleMessage(IWorldSession session, ClientP2PTradingDeclineInvite message)
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

            tradeManager.DeclineTrade(tradeSession, initiator, target);
        }
    }
}
