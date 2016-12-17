﻿using System;
using System.Collections.Generic;

namespace LanExchange.Presentation.Interfaces
{
    /// <summary>
    /// LanExchange panel model.
    /// </summary>
    public interface IPanelModel : IFilterModel, IEquatable<IPanelModel>
    {
        event EventHandler Changed;

        event EventHandler TabNameUpdated;

        /// <summary>
        /// Gets or sets the name of the tab.
        /// </summary>
        /// <value>
        /// The name of the tab.
        /// </value>
        string TabName { get; }

        /// <summary>
        /// Gets or sets the name of the tab's image.
        /// </summary>
        /// <value>
        /// The name of the tab's image.
        /// </value>
        string ImageName { get; }

        /// <summary>
        /// Gets or sets the current view.
        /// </summary>
        /// <value>
        /// The current view.
        /// </value>
        PanelViewMode CurrentView { get; set; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets the current path.
        /// </summary>
        /// <value>
        /// The current path.
        /// </value>
        IObjectPath<PanelItemBase> CurrentPath { get; }

        /// <summary>
        /// Gets or sets the focused item text.
        /// </summary>
        /// <value>
        /// The focused item text.
        /// </value>
        PanelItemBase FocusedItem { get; set; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        IList<PanelItemBase> Items { get; }

        /// <summary>
        /// Gets the tool tip text.
        /// </summary>
        /// <value>
        /// The tool tip text.
        /// </value>
        string ToolTipText { get; }

        string DataType { get; set; }

        IColumnComparer Comparer { get; }

        /// <summary>
        /// Gets at.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        PanelItemBase GetItemAt(int index);

        /// <summary>
        /// Indexes the of.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        int IndexOf(PanelItemBase key);

        void AsyncRetrieveData(bool clearFilter);

        void Sort(IComparer<PanelItemBase> sorter);

        bool Contains(PanelItemBase panelItem);

        void SetDefaultRoot(PanelItemBase root);

        PanelFillerResult RetrieveData(bool clearFilter);

        void SetFillerResult(PanelFillerResult fillerResult, bool clearFilter);

        void OnTabNameUpdated();

        void Assign(PanelDto dto);
    }
}