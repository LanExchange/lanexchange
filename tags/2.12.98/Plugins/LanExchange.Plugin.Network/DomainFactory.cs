﻿using System.Security.Permissions;
using LanExchange.Plugin.Network.Properties;
using LanExchange.SDK;

namespace LanExchange.Plugin.Network
{
    public sealed class DomainFactory : PanelItemFactoryBase
    {
        public override PanelItemBase CreatePanelItem(PanelItemBase parent, string name)
        {
            return new DomainPanelItem(parent, name);
        }

        /// <summary>
        /// Starts with curent users's workgroup/domain as root.
        /// </summary>
        /// <returns></returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public override PanelItemBase CreateDefaultRoot()
        {
            var domain = NetApi32Utils.GetMachineNetBiosDomain(null);
            return new DomainPanelItem(PluginNetwork.ROOT_OF_DOMAINS, domain);
        }

        public override void RegisterColumns(IPanelColumnManager columnManager)
        {
            columnManager.RegisterColumn<DomainPanelItem>(new PanelColumnHeader(Resources.DomainName));
        }
    }
}
