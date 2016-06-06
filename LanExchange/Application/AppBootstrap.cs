﻿using System;
using System.Diagnostics.Contracts;
using LanExchange.Presentation.Interfaces;
using LanExchange.Properties;

namespace LanExchange.Application
{
    internal sealed class AppBootstrap : IAppBootstrap
    {
        private readonly IAppView appView;
        private readonly IMainPresenter mainPresenter;
        private readonly IPagesPresenter pagesPresenter;
        private readonly IImageManager imageManager;
        private readonly IAddonManager addonManager;
        private readonly IPluginManager pluginManager;
        private readonly ITranslationService translationService;
        private readonly IWindowFactory windowFactory;

        public AppBootstrap(
            IAppView appView,
            IMainPresenter mainPresenter,
            IPagesPresenter pagesPresenter,
            IImageManager imageManager,
            IAddonManager addonManager,
            IPluginManager pluginManager,
            ITranslationService translationService,
            IWindowFactory windowFactory)
        {
            Contract.Requires<ArgumentNullException>(appView != null);
            Contract.Requires<ArgumentNullException>(mainPresenter != null);
            Contract.Requires<ArgumentNullException>(pagesPresenter != null);
            Contract.Requires<ArgumentNullException>(imageManager != null);
            Contract.Requires<ArgumentNullException>(addonManager != null);
            Contract.Requires<ArgumentNullException>(pluginManager != null);
            Contract.Requires<ArgumentNullException>(translationService != null);
            Contract.Requires<ArgumentNullException>(windowFactory != null);

            this.appView = appView;
            this.mainPresenter = mainPresenter;
            this.pagesPresenter = pagesPresenter;
            this.imageManager = imageManager;
            this.addonManager = addonManager;
            this.pluginManager = pluginManager;
            this.translationService = translationService;
            this.windowFactory = windowFactory;

            Initialize();
        }

        private void Initialize()
        {
            // global map interfaces to classes
            translationService.SetResourceManagerTo<Resources>();
            // load plugins
            pluginManager.LoadPlugins();
            // load addons
            addonManager.LoadAddons();
        }


        public void Run()
        {
            // create main form
            //App.Presenter.ConfigOnChanged(App.Config, new ConfigChangedArgs(ConfigNames.Language));
            using (var mainView = windowFactory.CreateMainView())
            {
                pagesPresenter.LoadSettings();
                // run application
                appView.Run(mainView);
            }
        }
    }
}