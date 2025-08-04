using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Character;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.P2PTrading;
using NexusForever.Game.Character;
using NexusForever.Game.Trading;
using NexusForever.Network.Session;
using NexusForever.Network.World.Message.Model;
using NexusForever.Shared;
using NLog;

namespace NexusForever.Game.Entity
{
    public class P2PTradeManager : IP2PTradeManager
    {

        // Logger, of course
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();


        // Player who owns this instance.
        private readonly IPlayer player;


        /// <summary>
        /// TradeSession, holds data about the trade session.
        /// </summary>
        public ITradeSession TradeSession { get; set; }

        public P2PTradeManager(IPlayer owner)
        {
            player = owner;
        }


        public void InitiateTrade(uint targetUnitId)
        {
            // Trades can only be initiated by the local player on another player.
            // The client UI already prevents trading more than 1 at a time, so no need to re-check here.

            // Convert the given unit ID to an in-world entity.
            IUnitEntity targetUnit = player.Map.GetEntity<IUnitEntity>(targetUnitId);

            // Ensure the target is actually a player.
            if (targetUnit is not IPlayer targetPlayer)
            {
                log.Warn("Trade initiation failed: target is not a valid player.");
                return;
            }
            var entitlement = targetPlayer.Account.EntitlementManager.GetEntitlement(Static.Entity.EntitlementType.EconomyParticipation);

            if(entitlement == null)
            {
                // Inform the initiator that the target is currently unavailable for trading.
                var notAllowedToTradePacket = new ServerP2PTradeResult
                {
                    Result = ServerP2PTradeResult.P2PTradeResult.TargetNotAllowedToTrade,
                    Cancelled = true
                };

                player.Session.EnqueueMessageEncrypted(notAllowedToTradePacket);
                return;
            }


            // If the target already has an active trade session, they are considered busy.
            if (targetPlayer.P2PTradeManager.TradeSession != null)
            {
                // Inform the initiator that the target is currently unavailable for trading.
                var busyPacket = new ServerP2PTradeResult
                {
                    Result = ServerP2PTradeResult.P2PTradeResult.TargetBusy,
                    Cancelled = true
                };

                player.Session.EnqueueMessageEncrypted(busyPacket);
                return;
            }

            // Create a new trade session and assign it to both players.
            TradeSession = new TradeSession(player, targetPlayer);
            targetPlayer.P2PTradeManager.TradeSession = TradeSession;

            player.P2PTradeManager.TradeSession.StartTradeInvite();

            log.Info($"Trade request sent: {player.Name} [{player.Guid}] → {targetPlayer.Name} [{targetPlayer.Guid}]");
        }


        // Called when this player chooses to cancel the trade (UI button click)
        public void InitiateCancelTrade()
        {
            if (HasActiveTrade)
                TradeSession.CancelTrade();
        }

        public void Update(double lastTick)
        {
            
        }

        // Called when this player declines a trade invitation
        public void InitiateDeclineInvite()
        {
            if (HasActiveTrade)
                TradeSession.DeclineTradeInvite();

            log.Info($"Trade request declined");
        }

        // This is coming from the Partner.
        public void InitiateAcceptInvite()
        {
            if (HasActiveTrade)
                player.P2PTradeManager.TradeSession.AcceptTradeInvite();

            log.Info($"Trade request accepted");
        }

        public void InitiateSetTradeMoney(ulong credits)
        {
            if (TradeSession == null)
                return;

            TradeSession.SetMoneyTrade(credits, player);

            log.Info($"Trade money update from [{player.Name}] [{player.Guid}] - Amount: {credits}");
        }

        public bool HasActiveTrade => TradeSession != null;


        // Called when this player chooses to cancel the trade (UI button click)
        public void InitiateAddItem(ulong itemGuid)
        {
            if (HasActiveTrade)
                TradeSession.AddItem(itemGuid, player);
        }

        
        public void InitiateRemoveItem(ulong itemGuid)
        {
            if (HasActiveTrade)
                TradeSession.RemoveItem(itemGuid, player);
        }


        public void InitiateCommit()
        {
            if (HasActiveTrade)
                player.P2PTradeManager.TradeSession.CommitTrade(player);

        }
    }

}
