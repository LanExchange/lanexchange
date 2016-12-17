﻿using System;

namespace LanExchange.Presentation.Interfaces
{
    public interface IPagesPresenter : IPresenter<IPagesView>, IPerformEscape
    {
        event EventHandler PanelViewFocusedItemChanged;

        event EventHandler PanelViewFilterTextChanged;

        int Count { get; }

        int SelectedIndex { get; set; }

        IPanelView ActivePanelView { get; }

        PanelViewMode ViewMode { get; set; }

        void CommandCloseTab();

        void SaveInstant();

        void SetupPanelViewEvents(IPanelView panelView);

        void LoadSettings();

        bool CanSendToNewTab();

        bool CanPasteItems();

        void CommandDeleteItems();

        void CommandPasteItems();

        void CommandSendToNewTab();

        void DoPanelViewFocusedItemChanged(object sender, System.EventArgs e);

        void DoPanelViewFilterTextChanged(object sender, System.EventArgs e);

        bool SelectTabByName(string tabName);

        void CommanCloseOtherTabs();

        // int IndexOf(IPanelModel model);
        void UpdateTabName(int index);

        void CommandReRead();

        // void SetTabImageForModel(IPanelModel theModel, string imageName);
        void DoPagesReRead();

        void DoPagesCloseTab();

        void DoPagesCloseOther();
    }
}
