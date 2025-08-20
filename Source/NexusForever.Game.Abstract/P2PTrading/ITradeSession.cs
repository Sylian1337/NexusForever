using NexusForever.Game.Abstract.Entity;
using NexusForever.Network.World.Message.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.Game.Abstract.P2PTrading
{
    public interface ITradeSession
    {
        uint TradeId { get; set; }

        uint InitiatorId { get; set; }
        uint TargetId { get; set; }


        void NotifyTradeInvite(IPlayer initiator, IPlayer target);
        void NotifyTradeStart(IPlayer initiator, IPlayer target);

        void NotifyTradeCancel(IPlayer initiator, IPlayer target);
        void NotifyTradeDeclined(IPlayer initiator, IPlayer target);

        void SendResultTo(ServerP2PTradeResult.P2PTradeResult result, bool cancelled, IPlayer player);

        bool IsInitiator(uint guid);

        /*



        bool InitiatorCommitted {  get; set; }
        bool TargetCommitted {  get; set; }


        void StartTradeInvite();

        void DeclineTradeInvite();

        void RemoveItem(ulong itemGuid, IPlayer whoRemovedIt);

        void AddItem(ulong itemGuid, IPlayer whoAddedIt);

        void CancelTrade();

        void SendResult(ServerP2PTradeResult.P2PTradeResult result, bool cancelled);

        void Clear();

        void AcceptTradeInvite();
        void SetMoneyTrade(ulong credits, IPlayer whoUpdated);
        void CommitTrade(IPlayer whoCommitted);*/
    }
}
