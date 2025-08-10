using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.P2PTrading;
using NexusForever.Game.Static.Entity;
using NexusForever.GameTable;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Static;
using NexusForever.Shared;
using System.Net.Sockets;

namespace NexusForever.Game.Trading
{

    /// <summary>
    /// Reason for needing to keep a ref of the IPlayer, this is because to lookup a player, you need to have a player to actually look up others.
    /// </summary>
    public class TradeSession : ITradeSession
    {
        /// <summary>
        /// Trade sessions Id.
        /// </summary>
        public uint TradeId { get; set; }

        public IPlayer Initiator { get; set; }
        public IPlayer Target { get; set; }

        /// <summary>
        /// Initiator´s Unit Id.
        /// </summary>
        public uint InitiatorId => Initiator.Guid;

        /// <summary>
        /// The target´s Unit Id.
        /// </summary>
        public uint TargetId => Target.Guid;


        public TradeSession(uint tradeId, IPlayer initiator, IPlayer target)
        {
            TradeId = tradeId;
            Initiator = initiator;
            Target = target;
        }

        /// <summary>
        /// Notify target about a new trade invite.
        /// </summary>
        public void NotifyTradeInvite()
        {
            // Everything is wrong here, it has to do with that damn message id, but cant figure out the problem.
            var packet = new ServerP2PTradeInvite
            {
                TradeInviterUnitId = InitiatorId
            };

            Target.Session.EnqueueMessageEncrypted(packet);
        }

        /// <summary>
        /// Notify other player about trade declined.
        /// </summary>
        public void NotifyTradeCancel()
        {
            /*
            var packet = new ServerP2PTradeInvite
            {
                TradeInviterUnitId = InitiatorId
            };

            Target.Session.EnqueueMessageEncrypted(packet);
            */
        }

        /// <summary>
        /// Notify initiator about trade invite declined.
        /// </summary>
        public void NotifyTradeDeclined()
        {
            // Both needs to be sent, otherwise one cant trade.
            SendResultTo(ServerP2PTradeResult.P2PTradeResult.PlayerDeclinedInvite, true, Initiator);
            SendResultTo(ServerP2PTradeResult.P2PTradeResult.PlayerDeclinedInvite, true, Target);
        }

        /// <summary>
        /// Sends the result of the trade to <paramref name="player"/>
        /// </summary>
        /// <param name="result"></param>
        /// <param name="cancelled"></param>
        /// <param name="player"></param>
        public void SendResultTo(ServerP2PTradeResult.P2PTradeResult result, bool cancelled, IPlayer player)
        {
            var packet = new ServerP2PTradeResult
            {
                Result = result,
                Cancelled = cancelled
            };

            player.Session.EnqueueMessageEncrypted(packet);
        }

    }



















        /*
        public class TradeSession : ITradeSession
        {

            public IPlayer Initiator { get; set; }

            public IPlayer Target { get; set; }

            public bool InitiatorCommitted { get; set; }
            public bool TargetCommitted {  get; set; }

            // Credits.
            public ulong InitiatorMoneyCommitted { get; set; }
            public ulong TargetMoneyCommitted { get; set; }



            public Dictionary<ulong, IItem> InitiatorItemsCommitted { get; } = new();
            public Dictionary<ulong, IItem> TargetItemsCommitted { get; } = new();


            // Called when Initiator starts a new trade.
            public TradeSession(IPlayer initiator, IPlayer target)
            {
                Initiator = initiator;
                Target = target;

                Initiator.P2PTradeManager.TradeSession = this;
                Target.P2PTradeManager.TradeSession = this;

            }

            public void StartTradeInvite()
            {
                var invitePacket = new ServerP2PTradeInvite
                {
                    TradeInviterUnitId = Initiator.Guid,
                };

                Target.Session.EnqueueMessageEncrypted(invitePacket);
            }

            public void CancelTrade()
            {
                SendResult(ServerP2PTradeResult.P2PTradeResult.PlayerCanceled, true);
                Clear();
            }

            public void DeclineTradeInvite()
            {
                SendResult(ServerP2PTradeResult.P2PTradeResult.PlayerDeclinedInvite, true);
                Clear();
            }

            public void SendResult(ServerP2PTradeResult.P2PTradeResult result, bool cancelled)
            {
                var packet = new ServerP2PTradeResult
                {
                    Result = result,
                    Cancelled = cancelled
                };

                Initiator.Session.EnqueueMessageEncrypted(packet);
                Target.Session.EnqueueMessageEncrypted(packet);
            }

            public void AcceptTradeInvite()
            {
                SendResult(ServerP2PTradeResult.P2PTradeResult.PlayerAcceptedInvite, false);
            }

            public void SetMoneyTrade(ulong credits, IPlayer whoUpdated)
            {
                var packet = new ServerP2PTradeUpdateMoney
                {
                    UnitId = whoUpdated.Guid,
                    Credits = credits
                };

                // Send to the other player
                if (whoUpdated == Initiator)
                {
                    InitiatorMoneyCommitted = credits;
                    Target.Session.EnqueueMessageEncrypted(packet);
                }
                else
                {
                    TargetMoneyCommitted = credits;
                    Initiator.Session.EnqueueMessageEncrypted(packet);
                }


            }

            public void RemoveItem(ulong itemGuid, IPlayer whoRemovedIt)
            {
                // TODO: Add checks for if it errors from removing the item.



                if (whoRemovedIt == Initiator)
                    InitiatorItemsCommitted.Remove(itemGuid);
                else if (whoRemovedIt == Target)
                    TargetItemsCommitted.Remove(itemGuid);


                var removeItemPacket = new ServerPTPTradeItemRemoved
                {
                    ItemGuid = itemGuid
                };

                Target.Session.EnqueueMessageEncrypted(removeItemPacket);
                Initiator.Session.EnqueueMessageEncrypted(removeItemPacket);
            }

            public void AddItem(ulong itemGuid, IPlayer whoAddedIt)
            {
                Console.WriteLine("Hello, added item");
                Console.WriteLine($"{itemGuid}");


                var item = whoAddedIt.Inventory.GetItem(itemGuid);


                // Check if Item is null, and send an error if it is.
                if (item == null)
                {
                    SendResult(ServerP2PTradeResult.P2PTradeResult.ErrorAddingItem, false);
                    return;
                }

                if(whoAddedIt == Initiator)
                {
                    InitiatorItemsCommitted[item.Guid] = item;
                }
                else
                {
                    TargetItemsCommitted[item.Guid] = item;
                }

                // Works fine with the stacking.
                // Might add so when you add another item that can stack with an already offered item that isnt at max stack count, it will just put it on-top of it.
                // Might be harder than I think, since that would require a lot of extra work, which is dumb, need to create new item and delete them, might just drop it.
                var addItemPacket = new ServerP2PTradeUpdateItem
                {
                    // Note: Not sure about TradeIndex.
                    // Tested if the TradeIndex maybe was connected to something with the slots, but that isnt what its used for.
                    // Last guess is that each trade has a certain ID it uses to connect with the trades.
                    // TODO: Test tomorrow with 4 accounts trading each other. ---- Look below.
                    // Trade index did nothing when having the 4 players do 2 different trades at the same time,
                    // so have no idea what TradeIndex is used for.
                    TradeIndex = 0,
                    OwnerUnitId = whoAddedIt.Guid,
                    Item2Id = item.Info.Id,
                    ItemGuid = itemGuid,
                    Quantity = item.StackCount,
                    //Unknown1_64bit = 0,                 // <- Not sure yet,
                    //Unknown2_32bit = 0,                 // <- Not sure yet, 
                    //Unknown3_64bit = 0,                 // <- Not sure yet, 
                    //Unknown4_18bit = 0,                 // <- Not sure yet, might match the Items uint Unknown70, but not known yet.
                    //UnknownArray = item.Info.StatEntry.ItemStatData
                    UnknownArray = new uint[5] { 0, 0, 0, 0, 0 }
                };

                Initiator.Session.EnqueueMessageEncrypted(addItemPacket);
                Target.Session.EnqueueMessageEncrypted(addItemPacket);
            }

            /// <summary>
            /// Sends the commit message to the other player.
            /// </summary>
            public void CommitTrade(IPlayer whoCommitted)
            {
                if (whoCommitted == Initiator)
                {
                    SendResult(ServerP2PTradeResult.P2PTradeResult.InitiatorCommitted, false);
                    InitiatorCommitted = true;
                }
                else
                {
                    SendResult(ServerP2PTradeResult.P2PTradeResult.TargetCommitted, false);
                    TargetCommitted = true;
                }

                // Check if both have committed
                if (InitiatorCommitted && TargetCommitted)
                {
                    Console.WriteLine($"Initiator money -> [{InitiatorMoneyCommitted}]");
                    Console.WriteLine($"Target money -> [{TargetMoneyCommitted}]");

                    // Maybe keep it here, maybe not.
                    // It basically say Trade Failed in the chat, dont know if it should.
                    if(InitiatorMoneyCommitted == 0 && TargetMoneyCommitted == 0 && TargetItemsCommitted.Count == 0 && InitiatorItemsCommitted.Count == 0)
                    {
                        SendResult(ServerP2PTradeResult.P2PTradeResult.NothingToTrade, true);
                        Clear();
                        return;
                    }

                    //Trade items.

                    Console.WriteLine(Target.Inventory.GetEnumerator().Current.Slots);
                    Console.WriteLine(Initiator.Inventory.GetEnumerator().Current.Slots);


                    // Think I know why it doesnt wanna work with AddItem.
                    //Need to call "ItemMove"
                    foreach (var item in TargetItemsCommitted.Values)
                    {



                        //Initiator.Inventory.ItemCreate(InventoryLocation.Inventory, item.Info, item.StackCount, ItemUpdateReason.Trade);
                        //Target.Inventory.ItemRemove(item);

                        uint? nextBagSlot = Initiator.Inventory.GetEnumerator().Current.GetFirstAvailableBagIndex();

                        Console.WriteLine(nextBagSlot);

                        //Initiator.Inventory.ItemMove(item, InventoryLocation.Inventory, (uint)nextBagSlot);
                        //Target.Inventory.ItemRemove(item);
                    }

                    foreach (var item in InitiatorItemsCommitted.Values)
                    {

                        //Target.Inventory.ItemCreate(InventoryLocation.Inventory, item.Info, item.StackCount, ItemUpdateReason.Trade);
                        //Initiator.Inventory.ItemRemove(item);


                        uint? nextBagSlot = Target.Inventory.GetEnumerator().Current.GetFirstAvailableBagIndex();


                        Console.WriteLine(nextBagSlot);

                        //Target.Inventory.ItemMove(item, InventoryLocation.Inventory, (uint)nextBagSlot);
                        //Initiator.Inventory.ItemRemove(item);
                    }
























                    // TODO: Transfer items + money
                    // Credits now work, only thing is we need to check each player has hit the limit.
                    // If they have the limit, we need to fix that first, when players hit the limit, it should send a mail to the player with the gold
                    // (Above is what I read online, might be wrong.)
                    Initiator.CurrencyManager.CurrencyAddAmount(
                        Static.Entity.CurrencyType.Credits, TargetMoneyCommitted, false);
                    Target.CurrencyManager.CurrencyAddAmount(
                        Static.Entity.CurrencyType.Credits, InitiatorMoneyCommitted, false);

                    // Take currency away when you give it to another player.
                    Initiator.CurrencyManager.CurrencySubtractAmount(
                        Static.Entity.CurrencyType.Credits, InitiatorMoneyCommitted, false);
                    Target.CurrencyManager.CurrencySubtractAmount(
                        Static.Entity.CurrencyType.Credits, TargetMoneyCommitted, false);

                    SendResult(ServerP2PTradeResult.P2PTradeResult.FinishedSuccess, true);
                    Clear();
                    Console.WriteLine("Trade was successful!");
                }
            }

            public void Clear()
            {
                Initiator.P2PTradeManager.TradeSession = null;
                Target.P2PTradeManager.TradeSession = null;
            }
    }*/
    }
