﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LanExchange.Model.Panel
{
    public class FilePanelItem : AbstractPanelItem
    {

        private readonly string m_FileName;
        private bool m_Directory;
        private bool m_Exists;

        public FilePanelItem(AbstractPanelItem parent, string fileName) : base(parent)
        {
            m_FileName = fileName;
            Name = Path.GetFileName(m_FileName);
            m_Directory = Directory.Exists(m_FileName);
            if (m_Directory)
                m_Exists = true;
            else
                m_Exists = File.Exists(m_FileName);
        }

        public string Name { get; set; }

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
                    return "QQQ";
                throw new ArgumentOutOfRangeException();
            }
        }

        public override string ToolTipText
        {
            get { return String.Empty; }
        }

        public override IPanelColumnHeader CreateColumnHeader(int index)
        {
            throw new NotImplementedException();
        }
    }
}