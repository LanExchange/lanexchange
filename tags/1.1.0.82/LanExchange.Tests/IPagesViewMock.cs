﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LanExchange.Sdk;

namespace Tests
{
    class IPagesViewMock : IPagesView
    {
        public string SelectedTabText
        {
            get { return String.Empty; }
            set
            {
            }
        }

        public int TabPagesCount
        {
            get { return 0; }
        }

        public int PopupSelectedIndex
        {
            get { return 0; }
        }

        public int SelectedIndex
        {
            get { return 0; }
            set
            {
            }
        }

        public IPanelView ActivePanelView
        {
            get { return null; }
        }

        public void RemoveTabAt(int index)
        {
        }

        public void SetTabToolTip(int index, string value)
        {
        }

        public void FocusPanelView()
        {
        }

        public ITabSettingView CreateTabSettingsView()
        {
            return null;
        }

        public IPanelView CreatePanelView(IPanelModel info)
        {
            return null;
        }
    }
}