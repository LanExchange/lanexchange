﻿using System.Collections.Generic;
using System.Xml.Serialization;

namespace LanExchange.Intf.Addon
{
    [XmlType("PanelItemTypeRef")]
    public class AddonItemTypeRef : AddonObjectId
    {
        private readonly List<AddonMenuItem> m_Items;

        public AddonItemTypeRef()
        {
            m_Items = new List<AddonMenuItem>();
        }

        public List<AddonMenuItem> ContextMenuStrip
        {
            get { return m_Items; }
        }

        public int CountVisible
        {
            get
            {
                var result = 0;
                foreach (var item in m_Items)
                    if (item.Visible)
                        result++;
                return result;
            }
        }
    }
}
