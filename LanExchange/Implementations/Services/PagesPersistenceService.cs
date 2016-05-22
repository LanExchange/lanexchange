﻿using LanExchange.Interfaces.Services;
using System;
using LanExchange.SDK;
using System.IO;
using LanExchange.Model;
using System.Diagnostics;

namespace LanExchange.Implementations.Services
{
    internal sealed class PagesPersistenceService : IPagesPersistenceService
    {
        private readonly IFolderManager folderManager;
        private readonly IPanelItemFactoryManager factoryManager;

        public PagesPersistenceService(IFolderManager folderManager, IPanelItemFactoryManager factoryManager)
        {
            if (folderManager == null) throw new ArgumentNullException(nameof(folderManager));
            if (factoryManager == null) throw new ArgumentNullException(nameof(factoryManager));

            this.folderManager = folderManager;
            this.factoryManager = factoryManager;
        }

        public void LoadSettings(out IPagesModel pages)
        {
            var fileFName = folderManager.TabsConfigFileName;
            pages = null;
            if (File.Exists(fileFName))
                try
                {
                    pages =
                        (PagesModel)
                        SerializeUtils.DeserializeObjectFromXmlFile(fileFName, typeof(PagesModel),
                                                                    factoryManager.ToArray());
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                }
        }

        public void SaveSettings(IPagesModel pages)
        {
            try
            {
                SerializeUtils.SerializeObjectToXmlFile(folderManager.TabsConfigFileName, pages, factoryManager.ToArray());
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }
    }
}