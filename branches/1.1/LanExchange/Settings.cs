﻿using System;
using System.IO;
using LanExchange.Utils;
using System.ComponentModel;

namespace LanExchange
{
    public class Settings
    {
        private const string UpdateURL_Default = "http://skivsoft.net/lanexchange/update/";
        private const string WebSiteURL_Default = "skivsoft.net/lanexchange/";
        private const string EmailAddress_Default = "skivsoft@gmail.com";

        private static Settings m_Instance;

        public Settings()
        {
            RefreshTimeInSec = 5 * 60;
        }

        public static Settings Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    Settings temp = new Settings();
                    m_Instance = temp;
                }
                return m_Instance;
            }
        }

        public static void LoadSettings()
        {
            try
            {
                Settings temp = (Settings)SerializeUtils.DeserializeObjectFromXMLFile(GetConfigFileName(), typeof(Settings));
                m_Instance = null;
                m_Instance = temp;
            }
            catch { }
        }

        public static void SaveSettings()
        {
            SerializeUtils.SerializeTypeToXMLFile(GetConfigFileName(), Instance, typeof(Settings));
        }

        public static string GetExecutableFileName()
        {
            string[] Params = Environment.GetCommandLineArgs();
            return Params[0];
        }

        public static string GetConfigFileName()
        {
            return Path.ChangeExtension(GetExecutableFileName(), ".cfg");
        }

        public static bool IsAutorun
        {
            get
            {
                return AutorunUtils.Autorun_Exists(GetExecutableFileName());
            }
            set
            {
                string ExeFName = GetExecutableFileName();
                if (value)
                    AutorunUtils.Autorun_Add(ExeFName);
                else
                    AutorunUtils.Autorun_Delete(ExeFName);
            }
        }

        public bool RunMinimized { get; set; }

        public bool AdvancedMode { get; set; }

        public int RefreshTimeInSec { get; set; }

        public string UpdateURL { get; set; }

        public string WebSiteURL { get; set; }

        public string EmailAddress { get; set; }

        public string GetUpdateURL()
        {
            return String.IsNullOrEmpty(UpdateURL) ? UpdateURL_Default : UpdateURL;
        }

        public string GetWebSiteURL()
        {
            return String.IsNullOrEmpty(WebSiteURL) ? WebSiteURL_Default : WebSiteURL;
        }

        public string GetEmailAddress()
        {
            return String.IsNullOrEmpty(EmailAddress) ? EmailAddress_Default : EmailAddress;
        }
    }
}
