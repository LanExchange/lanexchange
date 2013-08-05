﻿namespace LanExchange.SDK
{
    /// <summary>
    /// Column header interface returns by <cref>PanelItemBase</cref>.
    /// </summary>
    public class PanelColumnHeader
    {
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text { get; set; }
        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public bool Visible { get; set; }

        public int Width { get; set; }

        public PanelColumnHeader(string text, int width = 0)
        {
            Text = text;
            Width = width == 0 ? 130 : width;
            Visible = true;
        }
    }
}