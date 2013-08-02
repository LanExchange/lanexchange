﻿using System;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

//using System.Xml.Serialization;

namespace LanExchange.Plugin.Network
{
    [Serializable]
    public class ServerInfo : IComparable<ServerInfo>
    {
        private string sv101_name;
        private uint sv101_platform_id;
        private uint sv101_version_major;
        private uint sv101_version_minor;
        private uint sv101_type;
        private string sv101_comment;

        private DateTime m_UtcUpdated;

        /// <summary>
        /// Constructor without params is required for XML-serialization.
        /// </summary>
        //public ServerInfo()
        //{
        //}

        public static ServerInfo FromNetApi32(NetApi32.SERVER_INFO_101 info)
        {
            var result = new ServerInfo();
            result.sv101_platform_id = info.sv101_platform_id;
            result.sv101_name = info.sv101_name;
            result.sv101_version_major = info.sv101_version_major;
            result.sv101_version_minor = info.sv101_version_minor;
            result.sv101_type = info.sv101_type;
            result.sv101_comment = info.sv101_comment;
            return result;
        }

        public string Name
        {
            get { return sv101_name; }
            set { sv101_name = value; }
        }

        public string Comment
        {
            get { return sv101_comment; }
            set { sv101_comment = value; }
        }

        public uint Type
        {
            get { return sv101_type; }
            set { sv101_type = value; }
        }

        public uint PlatformID
        {
            get { return sv101_platform_id; }
            set { sv101_platform_id = value; }
        }

        public uint VersionMajor
        {
            get { return sv101_version_major; }
            set { sv101_version_major = value; }
        }

        public uint VersionMinor
        {
            get { return sv101_version_minor; }
            set { sv101_version_minor = value; }
        }

        public DateTime UtcUpdated
        {
            get { return m_UtcUpdated; }
            set { m_UtcUpdated = value; }
        }

        public void ResetUtcUpdated()
        {
            m_UtcUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns name and version of operation system.
        /// </summary>
        /// <returns></returns>
        public string Version()
        {
            //return String.Format("{0}.{1}.{2}.{3}", platform_id, ver_major, ver_minor, type);
            bool bServer = IsServer();
            NetApi32.SV_101_PLATFORM platform = (NetApi32.SV_101_PLATFORM) sv101_platform_id;
            // OS2 same as NT
            if (platform == NetApi32.SV_101_PLATFORM.PLATFORM_ID_OS2)
                platform = NetApi32.SV_101_PLATFORM.PLATFORM_ID_NT;
            switch (platform)
            {
                case NetApi32.SV_101_PLATFORM.PLATFORM_ID_DOS:
                    return String.Format("MS-DOS {0}.{1}", sv101_version_major, sv101_version_minor);
                case NetApi32.SV_101_PLATFORM.PLATFORM_ID_NT:
                    if ((sv101_type & (uint)NetApi32.SV_101_TYPES.SV_TYPE_XENIX_SERVER) != 0)
                        return String.Format("Linux Server {0}.{1}", sv101_version_major, sv101_version_minor);
                    switch (sv101_version_major)
                    {
                        case 3:
                            return "Windows NT 3.51";
                        case 4:
                            switch (sv101_version_minor)
                            {
                                case 0:
                                    return "Windows 95";
                                case 10:
                                    return "Windows 98";
                                case 90:
                                    return "Windows ME";
                                default:
                                    return String.Format("Windows NT {0}.{1}", sv101_version_major, sv101_version_minor);
                            }
                        case 5:
                            switch (sv101_version_minor)
                            {
                                case 0:
                                    return bServer ? "Windows Server 2000" : "Windows 2000";
                                case 1:
                                    return "Windows XP";
                                case 2:
                                    return "Windows Server 2003 R2";
                                default:
                                    return String.Format("Windows NT {0}.{1}", sv101_version_major, sv101_version_minor);
                            }
                        case 6:
                            switch (sv101_version_minor)
                            {
                                case 0:
                                    return bServer ? "Windows Server 2008" : "Windows Vista";
                                case 1:
                                    return bServer ? "Windows Server 2008 R2" : "Windows 7";
                                case 2:
                                    return bServer ? "Windows 8 Server" : "Windows 8";
                                default:
                                    return String.Format("Windows NT {0}.{1}", sv101_version_major, sv101_version_minor);
                            }
                        default:
                            return String.Format("Windows NT {0}.{1}", sv101_version_major, sv101_version_minor);
                    }
                case NetApi32.SV_101_PLATFORM.PLATFORM_ID_OSF:
                    return String.Format("OSF {0}.{1}", sv101_version_major, sv101_version_minor);
                case NetApi32.SV_101_PLATFORM.PLATFORM_ID_VMS:
                    return String.Format("VMS {0}.{1}", sv101_version_major, sv101_version_minor);
                default:
                    return String.Format("{0} {1}.{2}", sv101_platform_id, sv101_version_major, sv101_version_minor);
            }
        }

        public bool IsDomainController()
        {
            const uint ctrl =
                (uint)NetApi32.SV_101_TYPES.SV_TYPE_DOMAIN_CTRL | (uint)NetApi32.SV_101_TYPES.SV_TYPE_DOMAIN_BAKCTRL;
            return (sv101_type & ctrl) != 0;
        }

        public bool IsServer()
        {
            const uint srv = (uint)NetApi32.SV_101_TYPES.SV_TYPE_SERVER;
            const uint ctrl =
                (uint)NetApi32.SV_101_TYPES.SV_TYPE_DOMAIN_CTRL | (uint)NetApi32.SV_101_TYPES.SV_TYPE_DOMAIN_BAKCTRL;
            const uint noctrl = (uint)NetApi32.SV_101_TYPES.SV_TYPE_SERVER_NT;
            return (sv101_type & srv) != 0 && (sv101_type & (ctrl | noctrl)) != 0;
        }

        public bool IsSQLServer()
        {
            return (sv101_type & (uint)NetApi32.SV_101_TYPES.SV_TYPE_SQLSERVER) != 0;
        }

        public bool IsTimeSource()
        {
            return (sv101_type & (uint)NetApi32.SV_101_TYPES.SV_TYPE_TIME_SOURCE) != 0;
        }

        public bool IsPrintServer()
        {
            return (sv101_type & (uint)NetApi32.SV_101_TYPES.SV_TYPE_PRINTQ_SERVER) != 0;
        }

        public bool IsDialInServer()
        {
            return (sv101_type & (uint)NetApi32.SV_101_TYPES.SV_TYPE_DIALIN_SERVER) != 0;
        }

        public bool IsPotentialBrowser()
        {
            return (sv101_type & (uint)NetApi32.SV_101_TYPES.SV_TYPE_POTENTIAL_BROWSER) != 0;
        }

        public bool IsBackupBrowser()
        {
            return (sv101_type & (uint)NetApi32.SV_101_TYPES.SV_TYPE_BACKUP_BROWSER) != 0;
        }

        public bool IsMasterBrowser()
        {
            return (sv101_type & (uint)NetApi32.SV_101_TYPES.SV_TYPE_MASTER_BROWSER) != 0;
        }

        public bool IsDFSRoot()
        {
            return (sv101_type & (uint)NetApi32.SV_101_TYPES.SV_TYPE_DFS) != 0;
        }

        public int CompareVersionTo(ServerInfo other)
        {
            if (other == null) return 1;
            uint u1 = sv101_platform_id;
            uint u2 = other.sv101_platform_id;
            if (u1 < u2) return -1;
            if (u1 > u2) return 1;
            bool s1 = IsServer();
            bool c1 = IsDomainController();
            bool s2 = other.IsServer();
            bool c2 = other.IsDomainController();
            if (!s1 && s2) return -1;
            if (s1 && !s2) return 1;
            if (!c1 && c2) return 1;
            if (c1 && !c2) return -1;
            u1 = sv101_version_major;
            u2 = other.sv101_version_major;
            if (u1 < u2) return -1;
            if (u1 > u2) return 1;
            u1 = sv101_version_minor;
            u2 = other.sv101_version_minor;
            if (u1 < u2) return -1;
            if (u1 > u2) return 1;
            return 0;
        }

        #region IComparable Members

        public int CompareTo(ServerInfo other)
        {
            return String.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        /// <summary>
        /// This method is virtual for unit-tests only.
        /// </summary>
        /// <returns></returns>
        public virtual TimeSpan GetTopicality()
        {
            return DateTime.UtcNow - UtcUpdated;
        }

        public string GetTopicalityText()
        {
            TimeSpan diff = GetTopicality();
            StringBuilder sb = new StringBuilder();
            bool showSeconds = true;
            if (diff.Days > 0)
            {
                sb.Append(diff.Days);
                sb.Append("d");
                showSeconds = false;
            }
            if (diff.Hours > 0)
            {
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(diff.Hours);
                sb.Append("h");
                showSeconds = false;
            }
            if (diff.Minutes > 0)
            {
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(diff.Minutes);
                sb.Append("m");
            }
            if (showSeconds && diff.Seconds > 0)
            {
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(diff.Seconds);
                sb.Append("s");
            }
            return sb.ToString();
        }
    }
}
