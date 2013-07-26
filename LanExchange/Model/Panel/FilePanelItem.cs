﻿using System;
using System.IO;
using LanExchange.SDK;

namespace LanExchange.Model.Panel
{
    public class FilePanelItem : PanelItemBase//, IComparable<FilePanelItem>, IComparable
    {

        private readonly string m_FileName;
        private bool m_Directory;
        private bool m_Exists;

        public FilePanelItem(PanelItemBase parent, string fileName) : base(parent)
        {
            m_FileName = fileName;
            Name = Path.GetFileName(m_FileName);
            m_Directory = Directory.Exists(m_FileName);
            if (m_Directory)
                m_Exists = true;
            else
                m_Exists = File.Exists(m_FileName);
        }

        public override sealed string Name { get; set; }

        public string FileName
        {
            get { return m_FileName; }
        }

        public bool IsDirectory
        {
            get { return m_Directory; }
        }

        public bool IsExists
        {
            get { return m_Exists; }
        }

        public override int CountColumns
        {
            get { return 2; }
        }

        public override IComparable this[int index]
        {
            get
            {
                if (index == 0)
                    return Name;
                if (index == 1)
                    return IsDirectory ? "Папка с файлами" : "Файл";
                throw new ArgumentOutOfRangeException();
            }
        }

        public override string ImageName
        {
            get
            {
                if (Name == s_DoubleDot)
                    return PanelImageNames.DoubleDot;
                return IsDirectory ? PanelImageNames.ShareNormal : String.Empty;
            }
        }

        public override PanelColumnHeader CreateColumnHeader(int index)
        {
            throw new NotImplementedException();
        }

        //public int CompareTo(FilePanelItem other)
        //{
        //    if (IsDirectory)
        //    {
        //        if (!other.IsDirectory)
        //            return 1;
        //    }
        //    else if (other.IsDirectory)
        //        return -1;
        //    return String.Compare(Name, other.Name, StringComparison.Ordinal);
        //}

        //public int CompareTo(object other)
        //{
        //    return CompareTo(other as FilePanelItem);
        //}
    }
}
