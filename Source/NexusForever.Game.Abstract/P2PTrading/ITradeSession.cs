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

        IPlayer Initiator { get; set; }
        IPlayer Target { get; set; }


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
        void CommitTrade(IPlayer whoCommitted);
    }
}
