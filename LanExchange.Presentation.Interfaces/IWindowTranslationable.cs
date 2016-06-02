﻿namespace LanExchange.Presentation.Interfaces
{
    public interface IWindowTranslationable : IWindow, ITranslationable
    {
        bool RightToLeft { get; set; }
    }
}