﻿using System;
using System.IO;
using LanExchange.Model;
using NUnit.Framework;

namespace LanExchange.Tests.Model
{
    [TestFixture]
    public class PagesModelTest
    {
        private PagesModel m_Model;
        private bool m_EventFired;

        [SetUp]
        public void SetUp()
        {
            m_Model = new PagesModel();
            m_EventFired = false;
        }

        [TearDown]
        public void TearDown()
        {
            m_Model = null;
        }

        [Test]
        public void TestPagesModel()
        {
            Assert.AreEqual(0, m_Model.Count);
        }

        public void Model_AfterAppend_AfterRename(object sender, PanelItemListEventArgs e)
        {
            m_EventFired = true;
        }

        public void Model_AfterRemove_IndexChanged(object sender, PanelIndexEventArgs e)
        {
            m_EventFired = true;
        }

        [Test]
        public void TestDelTab()
        {
            m_Model.AddTab(new PanelItemList("MyTab"));
            m_Model.DelTab(0);
            Assert.AreEqual(0, m_Model.Count);
            Assert.IsFalse(m_EventFired);
            m_Model.AddTab(new PanelItemList("MyTab"));
            m_Model.AfterRemove += Model_AfterRemove_IndexChanged;
            m_Model.DelTab(0);
            Assert.IsTrue(m_EventFired);
        }

        [Test]
        public void TestAddTab()
        {
            m_Model.AddTab(new PanelItemList("MyTab"));
            Assert.IsFalse(m_EventFired);
            Assert.AreEqual("MyTab", m_Model.GetTabName(0));
            Assert.AreEqual(null, m_Model.GetTabName(-1));
            Assert.AreEqual(null, m_Model.GetTabName(1));
            m_Model.AfterAppendTab += Model_AfterAppend_AfterRename;
            m_Model.AddTab(new PanelItemList("YourTab"));
            Assert.IsTrue(m_EventFired);
        }

        [Test]
        public void TestRenameTab()
        {
            m_Model.AddTab(new PanelItemList("MyTab"));
            Assert.IsFalse(m_EventFired);
            m_Model.RenameTab(0, "YourTab");
            Assert.AreEqual("YourTab", m_Model.GetTabName(0));
            m_Model.AfterRename += Model_AfterAppend_AfterRename;
            m_Model.RenameTab(0, "MyTab");
            Assert.IsTrue(m_EventFired);
        }

        [Test]
        public void TestIndexChanged()
        {
            m_Model.AddTab(new PanelItemList("MyTab"));
            m_Model.AddTab(new PanelItemList("YourTab"));
            m_Model.SelectedIndex = 1;
            Assert.IsFalse(m_EventFired);
            Assert.AreEqual(1, m_Model.SelectedIndex);
            m_Model.IndexChanged += Model_AfterRemove_IndexChanged;
            m_Model.SelectedIndex = 0;
            Assert.IsTrue(m_EventFired);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ExceptionGetItem()
        {
            var item = m_Model.GetItem(0);
        }

        [Test]
        public void TestGetItem()
        {
            m_Model.AddTab(new PanelItemList("MyTab"));
            var item = m_Model.GetItem(0);
            Assert.NotNull(item);
            Assert.AreEqual("MyTab", item.TabName);
        }

        [Test]
        public void TestGetItemIndex()
        {
            var info = new PanelItemList("MyTab");
            m_Model.AddTab(info);
            Assert.AreEqual(-1, m_Model.GetItemIndex(null));
            Assert.AreEqual(0, m_Model.GetItemIndex(info));
        }

        [Test]
        public void TestGetConfigFileName()
        {
            var fileName = PagesModel.GetConfigFileName();
            Assert.AreEqual(PagesModel.CONFIG_FNAME, Path.GetFileName(fileName));
        }
    }
}
