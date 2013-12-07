﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using LanExchange.Intf;
using LanExchange.Misc.Action;
using LanExchange.Model;
using LanExchange.Model.Settings;
using System.Drawing;
using System.ComponentModel;
using System.Security.Permissions;
using LanExchange.Properties;
using LanExchange.SDK;
using LanExchange.Utils;
using Settings = LanExchange.Model.Settings.Settings;

namespace LanExchange.UI
{
    public partial class MainForm : RunMinimizedForm, IMainView
    {
        public const int WAIT_FOR_KEYUP_MS = 500;

        private readonly GlobalHotkeys m_Hotkeys;

        public PagesView Pages;

        public MainForm()
        {
            InitializeComponent();
            // App.MainView must be set before panel view will be created
            App.MainView = this;
            // load settings from cfg-file
            Settings.Instance.Changed += SettingsOnChanged;
            //Settings.Instance.Load();
            SetRunMinimized(Settings.Instance.RunMinimized);
            // init Pages presenter
            Pages = (PagesView)App.Resolve<IPagesView>();
            Pages.Dock = DockStyle.Fill;
            Controls.Add(Pages);
            Pages.BringToFront();
            App.MainPages = Pages.Presenter;
            App.MainPages.PanelViewFocusedItemChanged += Pages_PanelViewFocusedItemChanged;
            App.MainPages.LoadSettings();
            // init main form
            SetupActions();
            SetupForm();
            // setup images
            //ClearToolTip(Pages.Pages);
            App.Images.SetImagesTo(Pages.Pages);
            App.Images.SetImagesTo(Status);
            // set hotkey for activate: Ctrl+Win+X
            m_Hotkeys = new GlobalHotkeys();
            m_Hotkeys.RegisterGlobalHotKey((int)Keys.X, GlobalHotkeys.MOD_CONTROL + GlobalHotkeys.MOD_WIN, Handle);
            // set lazy events
            App.Threads.DataReady += OnDataReady;
#if DEBUG
            App.Threads.NumThreadsChanged += OnNumThreadsChanged;
#endif
        }

        [Localizable(false)]
        private void SettingsOnChanged(object sender, SettingsChangedArgs e)
        {
            if (e.Name.Equals("ShowMainMenu"))
            {
                MainMenu.Visible = (bool) e.Value;
                return;
            }
            if (e.Name.Equals("NumInfoLines"))
            {
                var value = (int) e.Value;
                if (value < 2)
                    value = 2;
                if (App.PanelColumns != null)
                {
                    var maxColumns = Math.Max(3, App.PanelColumns.MaxColumns);
                    if (value > maxColumns)
                        value = maxColumns;
                }
                e.NewValue = value;
                pInfo.CountLines = value;
                App.MainPages.DoPanelViewFocusedItemChanged(Pages.ActivePanelView, EventArgs.Empty);
                //return;
            }
        }

        #region Global actions
        private IAction m_AboutAction;
        private IAction m_ReReadAction;
        private IAction m_ShortcutKeysAction;

        public void SetupActions()
        {
            m_AboutAction = new AboutAction();
            m_ReReadAction = new ReReadAction();
            m_ShortcutKeysAction = new ShortcutKeysAction();
        }
        #endregion

        [Localizable(false)]
        private void SetupForm()
        {
            // set mainform bounds
            var rect = App.Presenter.SettingsGetBounds();
            SetBounds(rect.Left, rect.Top, rect.Width, rect.Height);
            // set mainform title
            var aboutModel = App.Resolve<IAboutModel>();
            Text = String.Format("{0} {1}", aboutModel.Product, aboutModel.VersionShort);
            // show tray
            TrayIcon.Text = Text;
            TrayIcon.Visible = true;
            // show computer name
            lCompName.Text = SystemInformation.ComputerName;
            lCompName.ImageIndex = App.Images.IndexOf(PanelImageNames.ComputerNormal);
            // show current user
            lUserName.Text = SystemInformation.UserName;
            lUserName.ImageIndex = App.Images.IndexOf(PanelImageNames.UserNormal);
        }

        private bool m_EscDown;
        private DateTime m_EscTime;

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                var pv = Pages.ActivePanelView;
                e.Handled = true;
                if (pv != null && pv.Filter.IsVisible)
                    pv.Filter.SetFilterText(string.Empty);
                else
                {
                    var parent = pv == null || pv.Presenter.Objects.CurrentPath.IsEmptyOrRoot
                                     ? null
                                     : pv.Presenter.Objects.CurrentPath.Peek();
                    if ((parent == null) || App.PanelItemTypes.DefaultRoots.Contains(parent))
                        Hide();
                    else if (!m_EscDown)
                    {
                        m_EscTime = DateTime.UtcNow;
                        m_EscDown = true;
                    }
                    else
                    {
                        TimeSpan diff = DateTime.UtcNow - m_EscTime;
                        if (diff.TotalMilliseconds >= WAIT_FOR_KEYUP_MS)
                        {
                            Hide();
                            m_EscDown = false;
                        }
                    }
                }
            }
            // F9 - Show/Hide main menu
            if (e.KeyCode == Keys.F9)
            {
                Settings.Instance.SetBoolValue("ShowMainMenu", !Settings.Instance.GetBoolValue("ShowMainMenu"));
                e.Handled = true;
            }
            // Ctrl+Up/Ctrl+Down - change number of info lines
            if (e.Control && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
            {
                int value = Settings.Instance.GetIntValue("NumInfoLines");
                if (e.KeyCode == Keys.Down)
                    Settings.Instance.SetIntValue("NumInfoLines", value + 1);
                else
                    Settings.Instance.SetIntValue("NumInfoLines", value - 1);
                e.Handled = true;
            }
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (m_EscDown)
                {
                    TimeSpan diff = DateTime.UtcNow - m_EscTime;
                    var pv = Pages.ActivePanelView;
                    var presenter = pv.Presenter;
                    if (pv != null && !presenter.Objects.CurrentPath.IsEmptyOrRoot)
                    {
                        if (diff.TotalMilliseconds < WAIT_FOR_KEYUP_MS)
                            presenter.CommandLevelUp();
                        else
                            Hide();
                    }
                    m_EscDown = false;
                }
                e.Handled = true;
            }
        }

        private void popTop_Opening(object sender, CancelEventArgs e)
        {
            var pv = Pages.ActivePanelView as PanelView;
            if (pv == null)
            {
                e.Cancel = true;
                return;
            }
            if (pInfo.CurrentItem == null)
            {
                e.Cancel = true;
                return;
            }
            e.Cancel = !App.Addons.BuildMenuForPanelItemType(popTop, pInfo.CurrentItem.GetType().Name);
        }

        private void tipComps_Popup(object sender, PopupEventArgs e)
        {
            var tooltip = (sender as ToolTip);
            if (tooltip == null) return;
            if (e.AssociatedControl == pInfo.Picture)
            {
                tooltip.ToolTipTitle = Resources.MainForm_Legend;
                return;
            }
            if (e.AssociatedControl is TabControl && e.AssociatedControl == Pages.Pages)
            {
                var tab = Pages.GetTabPageByPoint(e.AssociatedControl.PointToClient(MousePosition));
                if (tab != null)
                    tooltip.ToolTipTitle = tab.Text;
                else
                    e.Cancel = true;
                return;
            }
            tooltip.ToolTipTitle = string.Empty;
        }

        private void mAbout_Click(object sender, EventArgs e)
        {
            m_AboutAction.Execute();
        }
        
        private void lItemsCount_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var point = Status.PointToScreen(e.Location);
                popTray.Show(point);
            }
        }

        /// <summary>
        /// This event fires when focused item of PanelView has been changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pages_PanelViewFocusedItemChanged(object sender, EventArgs e)
        {
            // get focused item from current PanelView
            var pv = sender as PanelView;
            if (pv == null) return;
            var panelItem = pv.Presenter.GetFocusedPanelItem(false, true);
            // check if parent item more informative than current panel item
            while (panelItem != null &&
                   panelItem.Parent != PanelItemRoot.ROOT_OF_USERITEMS &&
                   !App.PanelItemTypes.DefaultRoots.Contains(panelItem) &&
                   !App.PanelItemTypes.DefaultRoots.Contains(panelItem.Parent))
                panelItem = panelItem.Parent;
            if (panelItem == null) return;
            pInfo.CurrentItem = panelItem;
            // update info panel at top of the form
            pInfo.Picture.Image = App.Images.GetLargeImage(panelItem.ImageName);
            SetToolTip(pInfo.Picture, panelItem.ImageLegendText);
            var helper = new PanelModelCopyHelper(null);
            helper.CurrentItem = panelItem;
            int index = 0;
            foreach (var column in helper.Columns)
            {
                pInfo.SetLine(index, helper.GetColumnValue(column.Index));
                ++index;
                if (index >= pInfo.CountLines) break;
            }
            for (int i = index; i < pInfo.CountLines; i++)
                pInfo.SetLine(i, string.Empty);
        }

        private void popTray_Opening(object sender, CancelEventArgs e)
        {
            mOpen.Text = Visible ? Resources.MainForm_Close : Resources.MainForm_Open;
        }

        private void mOpen_Click(object sender, EventArgs e)
        {
            ToggleVisible();
        }

        private void TrayIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ToggleVisible();
        }

        private void mExit_Click(object sender, EventArgs e)
        {
            ApplicationExit();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case GlobalHotkeys.WM_HOTKEY:
                    if ((short)m.WParam == m_Hotkeys.HotkeyID)
                    {
                        ToggleVisible();
                    }
                    break;
                case NativeMethods.WM_QUERYENDSESSION:
                    m.Result = new IntPtr(1);
                    break;
                case NativeMethods.WM_ENDSESSION:
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            App.Presenter.SettingsSetBounds(Bounds);
            //Settings.SaveIfModified();
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            Pages.FocusPanelView();
        }

        private void rereadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_ReReadAction.Execute();
            popTop.Tag = null;
        }

        private void lCompName_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var scm = new ShellContextMenu();
                scm.ShowContextMenuForCSIDL(Handle, ShellAPI.CSIDL.DRIVES, Cursor.Position);
            }
        }

        private void Status_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Process.Start("explorer.exe", "/n,::{20D04FE0-3AEA-1069-A2D8-08002B30309D}");
        }

        private void UpdateViewTypeMenu()
        {
            mPanelLarge.Checked = false;
            mPanelSmall.Checked = false;
            mPanelList.Checked = false;
            mPanelDetails.Checked = false;
            var pv = Pages.ActivePanelView;
            var enabled = pv != null;
            mPanelLarge.Enabled = enabled;
            mPanelSmall.Enabled = enabled;
            mPanelList.Enabled = enabled;
            mPanelDetails.Enabled = enabled;
            if (pv != null)
                switch (pv.ViewMode)
                {
                    case PanelViewMode.LargeIcon:
                        mPanelLarge.Checked = true;
                        break;
                    case PanelViewMode.SmallIcon:
                        mPanelSmall.Checked = true;
                        break;
                    case PanelViewMode.List:
                        mPanelList.Checked = true;
                        break;
                    case PanelViewMode.Details:
                        mPanelDetails.Checked = true;
                        break;
                }
        }

        private void mPanel_DropDownOpening(object sender, EventArgs e)
        {
            UpdateViewTypeMenu();
        }

        private void mPanelLarge_Click(object sender, EventArgs e)
        {
            var pv = Pages.ActivePanelView;
            if (pv == null) return;
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem == null) return;
            int tag;
            if (int.TryParse(menuItem.Tag.ToString(), out tag))
                pv.ViewMode = (PanelViewMode)tag;
        }

        private void mWebPage_Click(object sender, EventArgs e)
        {
            var presenter = App.Resolve<IAboutPresenter>();
            presenter.OpenWebLink();
        }

        private void mHelpKeys_Click(object sender, EventArgs e)
        {
            m_ShortcutKeysAction.Execute();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            App.Threads.Dispose();
        }

        public void OnDataReady(object sender, DataReadyArgs args)
        {
            BeginInvoke(new WaitCallback(MainForm_RefreshItem), new object[1] { args.Item });
        }

#if DEBUG
        public void OnNumThreadsChanged(object sender, EventArgs eventArgs)
        {
            BeginInvoke(new WaitCallback(MainForm_RefreshNumThreads), new object[1] { sender });
        }
#endif

        private void MainForm_RefreshItem(object item)
        {
            var pv = Pages.ActivePanelView;
            if (pv != null)
            {
                var index = pv.Presenter.Objects.IndexOf(item as PanelItemBase);
                if (index >= 0)
                    pv.RedrawItem(index);
            }
        }

 #if DEBUG
        [Localizable(false)]
        private void MainForm_RefreshNumThreads(object sender)
        {
            int count = 0;
            foreach (var column in App.PanelColumns.EnumAllColumns())
                if (column.Callback != null)
                    count += column.LazyDict.Count;
            var aboutModel = App.Resolve<IAboutModel>();
            Text = String.Format("{0} {1} [Threads: {2}, Dict: {3}]", aboutModel.Product, aboutModel.VersionFull, App.Threads.NumThreads, count);
        }
 #endif

        private void lCompName_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Clicks == 1)
            {
                var label = (sender as ToolStripStatusLabel);
                if (label != null)
                {
                    var obj = new DataObject();
                    obj.SetText(label.Text, TextDataFormat.UnicodeText);
                    Status.DoDragDrop(obj, DragDropEffects.Copy);
                }
            }
        }

        private void mPanelNewTab_Click(object sender, EventArgs e)
        {
            App.MainPages.CommandNewTab();
        }

        private void pInfo_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (!e.Data.GetDataPresent(typeof(PanelModelCopyHelper))) return;
            var helper = (PanelModelCopyHelper) e.Data.GetData(typeof (PanelModelCopyHelper));
            if (helper != null && helper.IndexesCount > 0)
                e.Effect = DragDropEffects.Copy;
        }

        private void pInfo_DragDrop(object sender, DragEventArgs e)
        {
            App.MainPages.CommandSendToNewTab();
        }

        public void SetToolTip(object control, string tipText)
        {
            if (control is Control)
                tipComps.SetToolTip(control as Control, tipText);
        }

        public void ShowStatusText(string format, params object[] args)
        {
            lItemsCount.Text = String.Format(format, args);
        }
    }
}