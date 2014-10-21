﻿using System;
using System.Collections.Generic;
using LanExchange.SDK;

namespace LanExchange.Plugin.Network
{
    public sealed class ComputerFiller : PanelFillerBase
    {
        public override bool IsParentAccepted(PanelItemBase parent)
        {
            // computers can be only into domains
            return parent is DomainPanelItem;
        }

        public override void SyncFill(PanelItemBase parent, ICollection<PanelItemBase> result)
        {
        }

        public override void AsyncFill(PanelItemBase parent, ICollection<PanelItemBase> result)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            //Thread.Sleep(5000);
            //result.Add(new PanelItemDoubleDot(parent));
            // get server list via OS api
            foreach (var item in NetApi32Utils.NetServerEnum(parent.Name, NativeMethods.SV_101_TYPES.SV_TYPE_ALL))
            {
                var si = ServerInfo.FromNetApi32(item);
                si.ResetUtcUpdated();
                result.Add(new ComputerPanelItem(parent, si));
            }
        }
    }
}