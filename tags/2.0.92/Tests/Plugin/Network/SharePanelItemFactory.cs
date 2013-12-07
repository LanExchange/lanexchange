﻿using NUnit.Framework;

namespace LanExchange.Plugin.Network
{
    [TestFixture]
    class SharePanelItemFactoryTest
    {
        [Test]
        public void TestFactory()
        {
            var factory = new SharePanelItemFactory();
            Assert.IsNotNull(factory.CreatePanelItem(null, null));
            Assert.IsNull(factory.CreateDefaultRoot());
        }
    }
}