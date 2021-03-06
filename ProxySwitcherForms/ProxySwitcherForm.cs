﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using ProxySwitcher;
using ProxySwitcherForms.Properties;
using ProxySwitcher.Triggers;
using System.Runtime.CompilerServices;
using static System.Console;

namespace ProxySwitcherForms
{
    public partial class ProxySwitcherForm : Form
    {
        private ProfileModel model = ProfileModel.Instance;
        private ProxyController ctrl = ProxyController.Instance;
        private TriggerListener triggers = new TriggerListener();
        
        private StartupController startupController = new StartupController();
        public bool VisibilityAllowed = true;

        public ProxySwitcherForm()
        {
            InitializeComponent();

            model.Proxies.ListChanged += ProxiesOnListChanged;
            RefreshListBoxes();
            RefreshTrayMenu();
            RefreshEnabled();
            RefreshSettings();


            triggers.OnProxyTriggered += Triggers_OnProxyTriggered;
            this.SizeChanged += ProxySwitcherForm_SizeEventHandler;
        }

        private void ProxySwitcherForm_SizeEventHandler(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
            }
        }

        private void Triggers_OnProxyTriggered(Profile profile, string reason)
        {
            // Run updates on UI thread
            Invoke(new MethodInvoker(
            delegate
            {
                toolStripStatusLabel1.Text = "Trigger activated " + profile.Title + " " + reason;
                RefreshEnabled();
            }));
        }

        private void ProxiesOnListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            RefreshTrayMenu();
        }

        private void RefreshTrayMenu()
        {

        }

        private void Set_Click(object sender, EventArgs e)
        {
            Profile selected = profilesListBox.SelectedItem as Profile;
            ctrl.SetProxy(selected.Proxy);
            RefreshListBoxes();
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            ProfileForm profileForm = new ProfileForm();
            DialogResult addStatus = profileForm.ShowDialog();

            if (addStatus == DialogResult.OK)
            {
                Profile profile = profileForm.GetProfile();
                model.Proxies.Add(profile);
                //RefreshListBox();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            ProfileForm profileForm = new ProfileForm(profilesListBox.SelectedItem as Profile);
            profileForm.ShowDialog();
            //profilesListBox.Refresh();
            RefreshListBoxes();
            // todo changed?
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            Profile selected = profilesListBox.SelectedItem as Profile;
            model.Proxies.Remove(selected);
        }

        private void triggersAddBtn_Click(object sender, EventArgs e)
        {
            var triggerForm = new TriggerForm();

            if (triggerForm.ShowDialog() == DialogResult.OK)
            {
                WriteLine("OK exit " + triggerForm.Trigger);
                TriggerModel.Instance.Triggers.Add(triggerForm.Trigger);


            }
        }

        private void triggersEditBtn_Click(object sender, EventArgs e)
        {
            Trigger selected = triggersListBox.SelectedItem as Trigger;

            var triggerForm = new TriggerForm(selected);
            triggerForm.ShowDialog();
            RefreshListBoxes();
        }

        private void triggersRemoveBtn_Click(object sender, EventArgs e)
        {
            Trigger t = triggersListBox.SelectedItem as Trigger;
            TriggerModel.Instance.Triggers.Remove(t);
        }

        private void ToggleEnabled(object sender, EventArgs e)
        {
            ctrl.SetEnabled(!ctrl.IsEnabled());
            RefreshEnabled();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void RefreshEnabled()
        {
            string label;
            Icon icon;
            if (ctrl.IsEnabled())
            {
                label = "Disable";
                icon = Resources.world;
            }
            else
            {
                label = "Enable";
                icon = Resources.world_delete;
            }
            proxyEnableBtn.Text = label;
            toolStripMenuItemEnabled.Text = label;
            trayIcon.Icon = icon;
        }

        private void RefreshListBoxes()
        {
            profilesListBox.DataSource = null;
            profilesListBox.DataSource = model.Proxies;
            profilesListBox.DisplayMember = "DisplayView";

            triggersListBox.DataSource = null;
            triggersListBox.DataSource = TriggerModel.Instance.Triggers;
            triggersListBox.DisplayMember = "Title";
        }

        private void ProxySwitcherForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteLine("saving settings");
            model.SaveProfiles();

            TriggerModel.Instance.SaveTriggers();
            Settings.Default.Save();
        }


        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            
            var me = e as MouseEventArgs;
            if (me != null && me.Button == MouseButtons.Left)
            {
                if (this.Visible)
                {
                    this.Hide();
                }
                else
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    Activate();
                }
            }
        }

        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void RefreshSettings()
        {
            startupButton.Text = startupController.CheckStartupEntry() ? "Deregister startup" : "Register startup";
        }

        private void startupButton_Click(object sender, EventArgs e)
        {
            if (startupController.CheckStartupEntry()) startupController.RemoveStartupEntry();
            else startupController.CreateStartupEntry();
            RefreshSettings();
        }
        
        /// <summary>
        /// Intercept showing window once and allow subsequent visibility changes
        /// </summary>
        /// <param name="value"></param>
        protected override void SetVisibleCore(bool value)
        {
            if (!VisibilityAllowed)
            {
                value = false;
                if (!this.IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
            // Allow visibility from now on
            VisibilityAllowed = true;
        }

        private void trayRightClickMenu_Opening(object sender, CancelEventArgs e)
        {

        }

        private void ProxySwitcherForm_Load(object sender, EventArgs e)
        {

        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }
    }
}
