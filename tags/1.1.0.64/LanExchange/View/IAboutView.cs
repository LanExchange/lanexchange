﻿using System;
using System.Drawing;

namespace LanExchange.View
{
    public interface IAboutView
    {
        string WebText { get; set; }
        string EmailText { get; set; }

        void ShowMessage(string text, Color color);
        void ShowProgressBar();
        void CancelView();
        void ShowUpdateButton(Version version);
    }
}