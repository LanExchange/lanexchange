﻿using LanExchange.SDK;

namespace LanExchange.Plugin.Network
{
    public sealed class ComputerPanelItemFactory : PanelItemFactoryBase
    {
        public override PanelItemBase CreatePanelItem(PanelItemBase parent, string name)
        {
            return new ComputerPanelItem(parent, name);
        }

        public override PanelItemBase CreateDefaultRoot()
        {
            return null;
        }
    }
}