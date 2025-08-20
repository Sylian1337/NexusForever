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
using NLog.Targets;
using System.Diagnostics;

namespace NexusForever.Game.Entity
{
    public sealed class P2PTradeManager : Singleton<P2PTradeManager>, IP2PTradeManager
    {

        private uint _nextTradeId = 0;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<uint, TradeSession> _activeTrades = new();

        public void Initialise()
        {
            var sw = Stopwatch.StartNew();
            log.Info("Initialise P2P trade manager...");

            log.Info($"P2P trade manager started in {sw.ElapsedMilliseconds}ms.");
        }

        /// <summary>
        /// Called when trade invite is sent to target.
        /// </summary>
        /// <param name="initiator"></param>
        /// <param name="target"></param>
        public void InitiateTrade(IPlayer initiator, IPlayer target)
        {
            var id = _nextTradeId++;
            var session = new TradeSession(id, initiator, target);

            // Store references in both players for convenience
            initiator.TradeId = id;
            target.TradeId = id;

            _activeTrades[id] = session;
            session.NotifyTradeInvite(initiator, target);
        }

        public void CancelTrade(TradeSession session, IPlayer initiator, IPlayer target)
        {
            _activeTrades.Remove(session.TradeId);

            initiator.TradeId = 0;
            target.TradeId = 0;

            session.NotifyTradeCancel(initiator, target);
        }


        public void DeclineTrade(ITradeSession session, IPlayer initiator, IPlayer target)
        {
            _activeTrades.Remove(session.TradeId);

            initiator.TradeId = 0;
            target.TradeId = 0;

            session.NotifyTradeDeclined(initiator, target);
        }

        public void StartTrade(ITradeSession session, IPlayer initiator, IPlayer target)
        {
            session.NotifyTradeStart(initiator, target);
        }

        public ITradeSession GetTradeById(uint tradeId)
        {
            if (_activeTrades.TryGetValue(tradeId, out var session))
            {
                return session;
            }
            else
            {
                // Something went wrong.
                return null;
            }
        }


        /// <summary>
        /// Sends a P2P trade error result to the specified player.
        /// This is used when a trade request cannot be started or continued,
        /// for example due to the target being busy, missing, or not allowed to trade.
        /// </summary>
        /// <param name="player">The player who will receive the error message.</param>
        /// <param name="reason">The specific trade failure reason to send.</param>
        public void SendTradeErrorTo(IPlayer player, ServerP2PTradeResult.P2PTradeResult reason)
        {
            if (player?.Session == null)
            {
                log.Warn("Attempted to send trade error, but player or session was null.");
                return;
            }

            player.Session.EnqueueMessageEncrypted(new ServerP2PTradeResult
            {
                Result = reason,
                Cancelled = true
            });

            log.Debug($"Trade error '{reason}' sent to player '{player.Name}' (ID: {player.EntityId}).");
        }


        /// <summary>
        /// Resolves the initiator and target players for a given trade session,
        /// given one of the players involved.
        /// </summary>
        /// <param name="currentPlayer">The player we already know about.</param>
        /// <param name="tradeSession">The trade session they are part of.</param>
        /// <param name="initiator">The resolved initiator player.</param>
        /// <param name="target">The resolved target player.</param>
        /// <returns>True if both players could be resolved; otherwise false.</returns>
        public bool TryResolvePlayers(IPlayer currentPlayer, ITradeSession tradeSession, out IPlayer initiator, out IPlayer target)
        {
            initiator = null;
            target = null;

            if (tradeSession == null)
                return false;

            if (tradeSession.IsInitiator(currentPlayer.Guid))
            {
                initiator = currentPlayer;
                target = currentPlayer.GetVisible<IPlayer>(tradeSession.TargetId);
            }
            else
            {
                target = currentPlayer;
                initiator = currentPlayer.GetVisible<IPlayer>(tradeSession.InitiatorId);
            }

            return initiator != null && target != null;
        }



























        /*
        public void CancelTrade(TradeSession session)
        {
            _activeTrades.Remove(session.Id);

            session.Initiator.ActiveTrade = null;
            session.Target.ActiveTrade = null;

            session.CancelTrade();
        }*/

        /*
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

        }*/
    }

}
