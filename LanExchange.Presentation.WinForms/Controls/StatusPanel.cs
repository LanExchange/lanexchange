﻿using System;
using System.Windows.Forms;
using LanExchange.Presentation.Interfaces;

namespace LanExchange.Presentation.WinForms.Controls
{
    internal sealed partial class StatusPanel : UserControl, IStatusPanelView
    {
        private readonly IStatusPanelPresenter presenter;

        public StatusPanel(IStatusPanelPresenter presenter)
        {
            if (presenter == null) throw new ArgumentNullException(nameof(presenter));

            InitializeComponent();
            this.presenter = presenter;
            presenter.Initialize(this);
        }

        public string ComputerName
        {
            get { return lCompName.Text; }
            set { lCompName.Text = value; }
        }

        public int ComputerImageIndex
        {
            get { return lCompName.ImageIndex; }
            set { lCompName.ImageIndex = value; }
        }

        public string UserName
        {
            get { return lUserName.Text; }
            set { lUserName.Text = value; }
        }

        public int UserImageIndex
        {
            get { return lUserName.ImageIndex; }
            set { lUserName.ImageIndex = value; }
        }

        public void SetImageList(object imageList)
        {
            Status.ImageList = (ImageList)imageList;
        }

        private void LabelCompName_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                presenter.PerformComputerLeftClick();

            if (e.Button == MouseButtons.Right)
            {
                var control = (ModifierKeys & Keys.Control) != 0;
                var shift = (ModifierKeys & Keys.Shift) != 0;
                presenter.PerformComputerRightClick(control, shift);
            }
        }

        private void LabelUserName_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                presenter.PerformUserRightClick();
        }
    }
}