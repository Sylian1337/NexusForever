using NexusForever.Database.Character;
using NexusForever.Game.Abstract.P2PTrading;
using NexusForever.Shared;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IP2PTradeManager : IUpdate
    {
        ITradeSession TradeSession { get; set;}

        // Starts a trade with the targeted player in question.
        void InitiateTrade(uint targetUnitId);

        // Call to cancel trade.
        void InitiateCancelTrade();

        void InitiateDeclineInvite();
        void InitiateAcceptInvite();
        void InitiateAddItem(ulong itemGuid);
        void InitiateRemoveItem(ulong itemGuid);
        void InitiateSetTradeMoney(ulong credits);
        void InitiateCommit();
    }
}
