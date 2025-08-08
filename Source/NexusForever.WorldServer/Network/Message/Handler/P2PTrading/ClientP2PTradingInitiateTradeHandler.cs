using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Entity;
using NexusForever.Game.Static.Entity;
using NexusForever.GameTable;
using NexusForever.Network;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;
using System;
using static NexusForever.Network.World.Message.Model.Shared.StoryMessage;

namespace NexusForever.WorldServer.Network.Message.Handler.P2PTrading
{
    /// <summary>
    /// Handles the client sending a trade request to another player.
    /// </summary>
    public class ClientP2PTradingInitiateTradeHandler : IMessageHandler<IWorldSession, ClientP2PTradingInitiateTrade>
    {
        #region Dependency Injection
        private readonly IP2PTradeManager tradeManager;

        public ClientP2PTradingInitiateTradeHandler(
            IP2PTradeManager tradeManager)
        {
            this.tradeManager = tradeManager;
        }
        #endregion

        public void HandleMessage(IWorldSession session, ClientP2PTradingInitiateTrade message)
        {
            var initiator = session.Player;
            var targetUnit = initiator.Map.GetEntity<IUnitEntity>(message.TargetUnitId);

            // Ensure the target is actually a player.
            if (targetUnit is not IPlayer targetPlayer)
            {
                tradeManager.SendTradeErrorTo(initiator, ServerP2PTradeResult.P2PTradeResult.MissingPlayer);
                return;
            }

            // Ensure target isnt trading already.
            if (targetPlayer.TradeId != 0)
            {
                tradeManager.SendTradeErrorTo(initiator, ServerP2PTradeResult.P2PTradeResult.TargetBusy);
                return;
            }

            var entitlement = targetPlayer.Account.EntitlementManager
                .GetEntitlement(EntitlementType.EconomyParticipation);

            // Ensure entitlement isnt null, if it is, means that player isnt allowed to trade.
            if (entitlement == null)
            {
                tradeManager.SendTradeErrorTo(initiator, ServerP2PTradeResult.P2PTradeResult.TargetNotAllowedToTrade);
                return;
            }

            // Pass the validated players to the manager
            tradeManager.StartTrade(initiator, targetPlayer);
        }
    }
}
