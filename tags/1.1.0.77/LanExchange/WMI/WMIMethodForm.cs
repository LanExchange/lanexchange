﻿using System;
using System.Management;
using System.Windows.Forms;
using System.Collections.Generic;
using NLog;
using System.ComponentModel;
using System.Drawing;

namespace LanExchange.WMI
{
    public partial class WMIMethodForm : Form
    {
        private readonly static Logger logger = LogManager.GetCurrentClassLogger();

        private bool m_OutParamPresent;

        public WMIMethodForm()
        {
            InitializeComponent();
        }

        public ManagementClass WMIClass { get; set; }
        public ManagementObject WMIObject { get; set; }
        public MethodData WMIMethod { get; set; }

        private string FormatQualifierValue(object value)
        {
            if (value is string[])
            {
                string[] list = value as string[];
                string S = "";
                for (int i = 0; i < list.Length; i++)
                {
                    if (i > 0) S += ", ";
                    S += list[i];
                }
                return S;
            }
            return value.ToString();
        }

        private string m_ReturnValueName;

        public void PrepareForm()
        {
            Text = String.Format(Text, WMIClass.Path.ClassName, WMIMethod.Name);
            lMethodName.Text = String.Format("{0}()", WMIMethod.Name);
            foreach (var qd in WMIMethod.Qualifiers)
            {
                if (qd.Name.Equals("Description"))
                {
                    lDescription.Text = qd.Value.ToString();
                }
            }
            // output method qualifiers
            string str = String.Format("Class={0}, Method={1}", WMIClass.Path.ClassName, WMIMethod.Name);
            LB.Items.Add(str);
            logger.Info("WMI: {0}", str);
            foreach (var qd in WMIMethod.Qualifiers)
            {
                if (qd.Name.Equals("Description") || qd.Name.Equals("Implemented"))
                    continue;
                str = String.Format("{0}={1}", qd.Name, FormatQualifierValue(qd.Value));
                LB.Items.Add(str);
                logger.Info("WMI: {0}", str);
            }
            LB.Items.Add(String.Empty);
            // prepare properties list and sort it by ID
            var propList = new List<PropertyDataEx>();
            if (WMIMethod.InParameters != null)
                foreach (PropertyData pd in WMIMethod.InParameters.Properties)
                    propList.Add(new PropertyDataEx(pd));
            if (WMIMethod.OutParameters != null)
                foreach (PropertyData pd in WMIMethod.OutParameters.Properties)
                    propList.Add(new PropertyDataEx(pd));
            propList.Sort();
            // output properties
            var excludeList = new List<string>();
            excludeList.Add("CIMTYPE");
            excludeList.Add("ID");
            excludeList.Add("In");
            excludeList.Add("Out");
            excludeList.Add("in");
            excludeList.Add("out");
            foreach (PropertyDataEx prop in propList)
            {
                // detect return value parameter name
                if (prop.ParamType == WMIParamType.RETURN)
                    m_ReturnValueName = prop.Name;
                if (prop.ParamType == WMIParamType.OUT)
                    m_OutParamPresent = true;
                str = String.Format("{0} {1} {2} : {3}", prop.ID, prop.ParamType, prop.Name, prop.Type);
                LB.Items.Add(str);
                logger.Info("WMI: {0}", str);
                foreach (var qd in prop.Qualifiers)
                    if (!excludeList.Contains(qd.Name))
                    {
                        str = String.Format("    {0}={1}", qd.Name, FormatQualifierValue(qd.Value));
                        LB.Items.Add(str);
                        if (!qd.Name.Equals("Description"))
                            logger.Info("WMI: {0}", str);
                    }
            }
        }

        private void WMIMethodForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                e.Handled = true;
            }
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                RunTheMethod();
                e.Handled = true;
            }
            if (e.Control && e.Alt && e.KeyCode == Keys.C)
            {
                LB.Visible = !LB.Visible;
                e.Handled = true;
            }
        }

        private void bRun_Click(object sender, EventArgs e)
        {
            RunTheMethod();
        }

        private void ShowOK(string message)
        {
            lResult.BackColor = Color.Green;
            lResult.Text = message;
        }

        private void ShowFAIL(string message)
        {
            lResult.BackColor = Color.Red;
            lResult.Text = message;
        }

        private void RunTheMethod()
        {
            if (WMIObject == null || WMIMethod == null) return;
            //var watcher = new ManagementOperationObserver();
            var inParams = WMIMethod.InParameters;
            var options = new InvokeMethodOptions();
            ManagementBaseObject result = null;
            bRun.Enabled = false;
            try
            {
                logger.Info("WMI: {0}.{1}()", WMIClass.Path.ClassName, WMIMethod.Name);
                result = WMIObject.InvokeMethod(WMIMethod.Name, inParams, options);
            }
            catch (Exception E)
            {
                logger.Error("WMI: {0}", E.Message);
                ShowFAIL(E.Message);
            }
            bRun.Enabled = true;
            if (result == null) return;
            LB.Items.Add(String.Empty);
            string str;
            foreach (PropertyData pd in result.Properties)
            {
                str = String.Format("{0} = {1}", pd.Name, pd.Value);
                LB.Items.Add(str);
                logger.Info("WMI: {0}", str);
            }
            PropertyDataEx resultProp = new PropertyDataEx(result.Properties[m_ReturnValueName]);
            if (resultProp.Value != null)
            {
                int value = (int) ((UInt32) resultProp.Value);
                var message = new Win32Exception(value).Message;
                str = String.Format("[{0}] {1}", value, message);
                LB.Items.Add(str);
                logger.Info("WMI: {0}", str);
                if (value == 0)
                {
                    ShowOK("[0] Success");
                    if (!m_OutParamPresent)
                        timerOK.Enabled = true;
                }
                else
                {
                    ShowFAIL(String.Format("[{0}] {1}", value, message));
                }
            }
        }

        private void timerOK_Tick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}