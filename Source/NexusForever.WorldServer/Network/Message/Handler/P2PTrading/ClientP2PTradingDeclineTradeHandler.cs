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
            var p = PlayerManager.Instance.GetPlayer(session.Player.Identity.Id);
            var p2 = PlayerManager.Instance.GetPlayer(session.Player.Guid);

            Console.WriteLine($"{p?.Name}  P1");
            Console.WriteLine($"{p2?.Name} P2");


            // Get trade by ID.
            ITradeSession tradeSession = tradeManager.GetTradeById(session.Player.TradeId);

            //TODO: Add more checks.
            // Maybe even a decline/cancel to both players if the trade was not found somehow.

            //Send the trade session that should be cancelled.
            tradeManager.DeclineTrade(tradeSession);
        }
    }
}
