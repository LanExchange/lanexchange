﻿namespace LanExchange.Misc.Impl
{
    public interface IFolderManager
    {
        string CurrentPath { get; }
        string ConfigFileName { get; }
        string ExeFileName { get; }
        string TabsConfigFileName { get; }
        string SystemAddonsPath { get; }
        string UserAddonsPath { get; }
        string[] GetAddonsFiles();
        string GetAddonFileName(bool isSystem, string addonName);
    }
}