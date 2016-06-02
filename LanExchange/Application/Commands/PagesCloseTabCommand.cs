﻿using LanExchange.Presentation.Interfaces;

namespace LanExchange.Application.Commands
{
    internal sealed class PagesCloseTabCommand : PagesCommandBase
    {
        public PagesCloseTabCommand(IPagesPresenter pagesPresenter) : base(pagesPresenter)
        {
        }

        public override void Execute()
        {
            pagesPresenter.CommandCloseTab();
        }
    }
}