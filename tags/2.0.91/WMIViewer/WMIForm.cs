﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Management;
using System.ComponentModel;
using System.Reflection;

namespace WMIViewer
{
    public partial class WMIForm : Form, IWMIView
    {
        private readonly WMIPresenter m_Presenter;
        private readonly WMIArgs m_Args;
        private string m_CurrentWMIClass;

        public event EventHandler FocusedItemChanged;

        [Localizable(false)]
        public WMIForm(WMIPresenter presenter)
        {
            m_Presenter = presenter;
            m_Presenter.View = this;
            m_Args = m_Presenter.Args;
            InitializeComponent();
            // Enable double buffer for ListView
            var mi = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            mi.Invoke(lvInstances, new object[] { ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true });

            FocusedItemChanged += lvInstances_FocusedItemChanged;
            UpdateTitle();

            WMIClassesInclude = new List<string>();
            WMIClassesInclude.Add("Win32_Desktop");
            WMIClassesInclude.Add("Win32_DesktopMonitor");
            WMIClassesInclude.Add("Win32_DiskDrive");
            WMIClassesInclude.Add("Win32_BIOS");
            WMIClassesInclude.Add("Win32_Processor");
            WMIClassesInclude.Add("Win32_PhysicalMemory");

        }

        public IList<string> WMIClassesInclude { get; private set; }

        public void UpdateTitle()
        {
            if (string.IsNullOrEmpty(m_Args.Comment))
                Text = @"\\" + m_Args.Name;
            else
                Text = String.Format(@"\\{0} — {1}", m_Args.Name, m_Args.Comment);
        }

        public WMIPresenter GetPresenter()
        {
            return m_Presenter;
        }

        [Localizable(false)]
        private void lvInstances_FocusedItemChanged(object sender, EventArgs e)
        {
            if (m_Presenter.WMIClass == null) return;
            if (lvInstances.FocusedItem == null) return;
            m_Presenter.WMIObject = (ManagementObject)lvInstances.FocusedItem.Tag;
            if (m_Presenter.WMIObject == null) return;

            var dynObj = new DynamicObject();
            foreach (PropertyData Prop in m_Presenter.WMIObject.Properties)
            {
                // skip array of bytes
                if (Prop.Type == CimType.UInt8 && Prop.IsArray)
                    continue;

                PropertyData ClassProp = m_Presenter.WMIClass.Properties[Prop.Name];

                bool isCimKey = false;
                bool IsReadOnly = true;
                string Description = "";

                foreach (QualifierData qd in ClassProp.Qualifiers)
                {
                    if (qd.Name.Equals("CIM_Key"))
                        isCimKey = true;
                    if (qd.Name.Equals("write"))
                        IsReadOnly = false;
                    if (qd.Name.Equals("Description"))
                        Description = qd.Value.ToString();
                }
                if (isCimKey) continue;
                string Category = Prop.Type.ToString();
                switch (Prop.Type)
                {

                    //     A signed 16-bit integer. This value maps to the System.Int16 type.
                    case CimType.SInt16:
                        dynObj_AddProperty<Int16>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    //     A signed 32-bit integer. This value maps to the System.Int32 type.
                    case CimType.SInt32:
                        dynObj_AddProperty<Int32>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    //     A floating-point 32-bit number. This value maps to the System.Single type.
                    case CimType.Real32:
                        dynObj_AddProperty<Single>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    //     A floating point 64-bit number. This value maps to the System.Double type.
                    case CimType.Real64:
                        dynObj_AddProperty<Double>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    //     A string. This value maps to the System.String type.
                    case CimType.String:
                        dynObj_AddProperty<String>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    //     A Boolean. This value maps to the System.Boolean type.
                    case CimType.Boolean:
                        dynObj_AddProperty<Boolean>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    //     An embedded object. Note that embedded objects differ from references in
                    //     that the embedded object does not have a path and its lifetime is identical
                    //     to the lifetime of the containing object. This value maps to the System.Object
                    //     type.
                    case CimType.Object:
                        dynObj_AddProperty<Object>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    //     A signed 8-bit integer. This value maps to the System.SByte type.
                    case CimType.SInt8:
                        dynObj_AddProperty<SByte>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    //     An unsigned 8-bit integer. This value maps to the System.Byte type.
                    case CimType.UInt8:
                        dynObj_AddProperty<Byte>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    //     An unsigned 16-bit integer. This value maps to the System.UInt16 type.
                    case CimType.UInt16:
                        dynObj_AddProperty<UInt16>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    //     An unsigned 32-bit integer. This value maps to the System.UInt32 type.
                    case CimType.UInt32:
                        dynObj_AddProperty<UInt32>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    //     A signed 64-bit integer. This value maps to the System.Int64 type.
                    case CimType.SInt64:
                        dynObj_AddProperty<Int64>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    //     An unsigned 64-bit integer. This value maps to the System.UInt64 type.
                    case CimType.UInt64:
                        dynObj_AddProperty<UInt64>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    //     A date or time value, represented in a string in DMTF date/time format: yyyymmddHHMMSS.mmmmmmsUUU,
                    //     where yyyymmdd is the date in year/month/day; HHMMSS is the time in hours/minutes/seconds;
                    //     mmmmmm is the number of microseconds in 6 digits; and sUUU is a sign (+ or
                    //     -) and a 3-digit UTC offset. This value maps to the System.DateTime type.
                    case CimType.DateTime:
                        if (Prop.Value == null)
                            dynObj.AddPropertyNull<DateTime>(Prop.Name, Description, Category, IsReadOnly);
                        else
                            dynObj.AddProperty(Prop.Name, WMIUtils.ToDateTime(Prop.Value.ToString()), Description, Category, IsReadOnly);
                        break;
                    //     A reference to another object. This is represented by a string containing
                    //     the path to the referenced object. This value maps to the System.Int16 type.
                    case CimType.Reference:
                        dynObj_AddProperty<Int16>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    //     A 16-bit character. This value maps to the System.Char type.
                    case CimType.Char16:
                        dynObj_AddProperty<Char>(dynObj, Prop, Description, Category, IsReadOnly);
                        break;
                    default:
                        string Value = Prop.Value == null ? null : Prop.Value.ToString();
                        dynObj.AddProperty(String.Format("{0} : {1}", Prop.Name, Prop.Type), Value, Description, "Unknown", IsReadOnly);
                        break;
                }
            }
            PropGrid.SelectedObject = dynObj;
        }

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        public ListView LV
        {
            get { return lvInstances; }
        }

        public ContextMenuStrip MENU
        {
            get { return menuCommands; }
        }

        public string CurrentWMIClass
        {
            get
            {
                return m_CurrentWMIClass;
            }
            set
            {
                m_CurrentWMIClass = value;
                lDescription.Text = WMIClassList.Instance.GetClassDescription(m_Presenter.Namespace, value);
                lClassName.Text = value;
                m_Presenter.EnumObjects(value);
                m_Presenter.BuildContextMenu();
                if (lvInstances.Items.Count == 0)
                    PropGrid.SelectedObject = null;
                else
                {
                    lvInstances.FocusedItem = lvInstances.Items[0];
                    lvInstances.FocusedItem.Selected = true;
                    lvInstances_FocusedItemChanged(lvInstances, EventArgs.Empty);
                    lvInstances.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                }
                lStatus.Text = String.Format("Elements: {0}", lvInstances.Items.Count);
            }
        }

        [Localizable(false)]
        private void WMIForm_Load(object sender, EventArgs e)
        {
            CurrentWMIClass = "Win32_OperatingSystem";
            ActiveControl = lvInstances;
        }

        public void ShowStat(int classCount, int propCount, int methodCount)
        {
            lClasses.Text = String.Format("Classes: {0}", classCount);
            lProps.Text = String.Format("Properties: {0}", propCount);
            lMethods.Text = String.Format("Methods: {0}", methodCount);
        }

        public static void dynObj_AddProperty<T>(DynamicObject dynObj, PropertyData prop, string description, string category, bool isReadOnly)
        {
            if (prop.Value == null)
                dynObj.AddPropertyNull<T>(prop.Name, description, category, isReadOnly);
            else
                if (prop.IsArray)
                    dynObj.AddProperty(prop.Name, (T[])prop.Value, description, category, isReadOnly);
                else
                    dynObj.AddProperty(prop.Name, (T)prop.Value, description, category, isReadOnly);
        }

        public void menuClasses_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
                CurrentWMIClass = menuItem.Text;
        }

        //private void mSetup_Click(object sender, EventArgs e)
        //{
        //    using (var form = new WMISetupForm())
        //    {
        //        form.PrepareForm();
        //        form.ShowDialog();
        //    }
        //}

        public void UpdateClassesMenu()
        {
            menuClasses.Items.Clear();
            int Count1 = WMIClassList.Instance.Classes.Count;
            int Count2 = WMIClassList.Instance.ReadOnlyClasses.Count;
            foreach(string str in WMIClassList.Instance.Classes)
            {
                ToolStripMenuItem MI = new ToolStripMenuItem { Text = str };
                MI.Click += menuClasses_Click;
                menuClasses.Items.Add(MI);
            }
            if (Count1 > 0 && Count2 > 0)
                menuClasses.Items.Add(new ToolStripSeparator());
            foreach (string str in WMIClassList.Instance.ReadOnlyClasses)
            {
                ToolStripMenuItem MI = new ToolStripMenuItem { Text = str };
                MI.Click += menuClasses_Click;
                menuClasses.Items.Add(MI);
            }
            // TODO uncomment setup wmi-classes
            //if (Count1 + Count2 > 0)
            //    menuClasses.Items.Add(new ToolStripSeparator());
            //ToolStripMenuItem mSetup = new ToolStripMenuItem { Text = "Настроить..."};
            //mSetup.Click += mSetup_Click;
            //menuClasses.Items.Add(mSetup);
        }

        private bool m_MenuUpdated;

        private void menuClasses_Opening(object sender, CancelEventArgs e)
        {
            if (!WMIClassList.Instance.Loaded)
            {
                e.Cancel = true;
                return;
            }
            if (!m_MenuUpdated)
            {
                UpdateClassesMenu();
                ShowStat(WMIClassList.Instance.ClassCount, WMIClassList.Instance.PropCount, WMIClassList.Instance.MethodCount);
                m_MenuUpdated = true;
            }
            foreach (var MI in menuClasses.Items)
            {
                var mi = MI as ToolStripMenuItem;
                if (mi != null)
                    mi.Checked = mi.Text.Equals(CurrentWMIClass);
            }
        }

        [Localizable(false)]
        private void PropGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            string PropName = e.ChangedItem.Label;
            if (PropName == null) return;
            object PropValue = e.ChangedItem.Value;
            string Caption = String.Format("Editing property {0}", PropName);
            string Message = String.Format("Computer name: \\{0}\n\nOld value: «{1}»\nNew value: «{2}»", m_Args.Name, e.OldValue, PropValue);
            try
            {
                // trying to change wmi property
                m_Presenter.WMIObject[PropName] = PropValue;
                m_Presenter.WMIObject.Put();

                // update computer comment if we changes Win32_OperatingSystme.Description
                if (CurrentWMIClass.Equals("Win32_OperatingSystem") && PropName.Equals("Description"))
                {
                    m_Args.Comment = PropValue.ToString();
                    UpdateTitle();
                }

                // property has been changed
                Message += String.Format("\n\nProperty {0} successfully changed.", PropName);
                MessageBox.Show(Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                // property not changed
                var dynObj = PropGrid.SelectedObject as DynamicObject;
                if (dynObj != null)
                    dynObj[PropName] = e.OldValue;
                Message += "\n\n" + ex.Message;
                if (ex.InnerException != null)
                    Message += "\n\n" + ex.InnerException.Message;
                MessageBox.Show(Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DoFocusedItemChanged()
        {
            if (FocusedItemChanged != null)
                FocusedItemChanged(this, EventArgs.Empty);
        }

        public void lvComps_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
                DoFocusedItemChanged();
        }

        private void menuCommands_Opening(object sender, CancelEventArgs e)
        {
            if (LV.FocusedItem != null)
                if (LV.FocusedItem.Selected)
                    DoFocusedItemChanged();
        }

        private void lvInstances_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                e.Handled = true;
            }
        }

        private void WMIForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Esc
            if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.Handled = true;
            }
            // F9 - Show/Hide menu
            if (e.KeyCode == Keys.F9)
            {
                menuMAIN.Visible = !menuMAIN.Visible;
                e.Handled = true;
            }
            // Ctrl+Left
            if (e.Control && e.KeyCode == Keys.Left)
            {
                PropGrid.Dock = DockStyle.Left;
                TheSplitter.Dock = DockStyle.Left;
                e.Handled = true;
            }
            // Ctrl+Right
            if (e.Control && e.KeyCode == Keys.Right)
            {
                PropGrid.Dock = DockStyle.Right;
                TheSplitter.Dock = DockStyle.Right;
                e.Handled = true;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }

}