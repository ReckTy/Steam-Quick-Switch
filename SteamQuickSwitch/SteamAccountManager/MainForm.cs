﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;

namespace SteamQuickSwitch
{
    public partial class MainForm : Form
    {
        private readonly Size formSize = new Size(489, 302);
        
        private Panel[] panelArray;
        private Button[] startButtons;
        
        private RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        private SensitiveDataStorage.SensitiveDataStorage sds = new SensitiveDataStorage.SensitiveDataStorage() { EncryptionPassword = PrivateInfo.Data.EncryptionPassword };
        private readonly int sdsIDManagerPassword = 0, sdsIDUsernames = 1, sdsIDPasswords = 19;

        private KeyboardHook hook = new KeyboardHook();

        public MainForm()
        {
            InitializeComponent();
            
            sds.CreateFile("Data");

            // Register SQS-Hotkey
            hook.KeyPressed += Hook_KeyPressed;
            hook.RegisterHotKey(SteamQuickSwitch.ModifierKeys.Alt, Keys.Q);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Visible = false;

            // Assign version-number
            AssignVersionNumber();

            // Assign necessary arrays
            AssignArrays();
            
            // Set panel locations/sizes
            while (Size != formSize) Size = formSize;
            foreach (Panel panel in panelArray) panel.Location = new Point(5, 54);

            LoadSavedSettings();
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // SaveLoginInfo if CurrentPanel is panelManage
            if (GetCurrentPanelIndex() == 1)
                SaveLoginInfo();

            // Abort any existing animationThreads
            if (animationThread != null) animationThread.Abort();
        }
        
        private void AssignVersionNumber()
        {
            labelVersionDisplay.Text = $"Steam Quick Switch v.{ Program.GetVersionInfo().FileVersion }";
        }

        private void AssignArrays()
        {
            panelArray = new Panel[] 
            {
                panelHome, panelManage, panelSettings,
                panelManageLogin, panelManageEditItem,
                panelSettingsExceptions, panelExtras,
                panelExtraFeatures, panelIEGameSettings
            };
            
            startButtons = new Button[]
            {
                buttonProfile1, buttonProfile2, buttonProfile3, buttonProfile4,
                buttonProfile5, buttonProfile6, buttonProfile7, buttonProfile8,
                buttonProfile9, buttonProfile10, buttonProfile11, buttonProfile12,
                buttonProfile13, buttonProfile14, buttonProfile15, buttonProfile16,
                buttonProfile17, buttonProfile18
            };
        }
        
        int GetCurrentPanelIndex()
        {
            int _currentPanelOpen = 0;
            int _panelCount = 0;

            for (int i = 0; i < panelArray.Length; i++)
            {
                if (panelArray[i].Enabled)
                {
                    _currentPanelOpen = i;
                }
            }

            if (_panelCount <= 1)
            {
                return _currentPanelOpen;
            }
            return 0;
        }
        
        void ChangePanel(int desiredPanel)
        {
            focusLabel.Focus();
            
            switch (GetCurrentPanelIndex())
            {
                // panelManage
                case 1:
                    // Clear textboxes
                    textBoxUsername.Clear();
                    textBoxPassword.Clear();

                    SaveLoginInfo();
                    EmptyListViewLogins();
                    break;

                // panelSettings
                case 2:
                    // Clear textboxes
                    textBoxManagerPasswordOld.Clear();
                    textBoxManagerPasswordSet.Clear();

                    // Reset unsaved settings
                    //setting1
                    if (rk.GetValue("Steam Quick Switch") == null)
                        setting1.Checked = false;
                    else
                        if (rk.GetValue("Steam Quick Switch") != null)
                        setting1.Checked = true;

                    //setting2
                    setting2.Checked = Properties.Settings.Default.CloseAfterAccountChange;

                    //settingCustomStartingPos
                    settingCustomStartingPos.Checked = Properties.Settings.Default.CustomStartingPositions;

                    //settingAnimStartingPos
                    settingAnimStartingPos.Checked = Properties.Settings.Default.AnimateStartingPosition;

                    //settingSteamPath
                    textBoxSettingSteamPath.Text = Properties.Settings.Default.SteamPath;

                    // Assign label values
                    labelSettingStartAt.Enabled = Properties.Settings.Default.CustomStartingPositions;
                    labelSettingStartX.Enabled = Properties.Settings.Default.CustomStartingPositions;
                    labelSettingStartY.Enabled = Properties.Settings.Default.CustomStartingPositions;
                    
                    // AnimationSpeed-TrackBar
                    trackBarSettingAnimSpeed.Value = Properties.Settings.Default.AnimationSpeed;

                    labelSettingFinishAt.Enabled = Properties.Settings.Default.AnimateStartingPosition;
                    labelSettingFinishX.Enabled = Properties.Settings.Default.AnimateStartingPosition;
                    labelSettingFinishY.Enabled = Properties.Settings.Default.AnimateStartingPosition;
                    labelSettingAnimSpeed.Enabled = Properties.Settings.Default.AnimateStartingPosition;

                    break;

                // panelSettingsExceptions
                case 5:
                    if (textBoxExceptionsList.Text != Properties.Settings.Default.ProcessExceptionsList)
                    {
                        DialogResult result = MessageBox.Show("There's unsaved changes to the Hotkey-exceptions tab." + "\n" +
                            "Do you want to discard the changes?", "Steam Quick Switch", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                        if (result == DialogResult.Yes)
                        {
                            textBoxExceptionsList.Text = Properties.Settings.Default.ProcessExceptionsList;
                        }
                        else
                            return;
                    }
                    break;
            }
            
            switch (desiredPanel)
            {
                case 0:
                    RefreshStartButtons();
                    break;
                case 1:
                    FillListViewLogins();
                    break;
            }

            ClearPanels();
            panelArray[desiredPanel].Visible = true;
            panelArray[desiredPanel].Enabled = true;
        }

        void ClearPanels()
        {
            foreach (Panel panel in panelArray)
            {
                panel.Enabled = false;
                panel.Visible = false;
            }
        }

        private void ExitSQS(object sender = null, EventArgs e = null)
        {
            Program.CloseApplicationPromt();
        }

        private void ExitSteam(object sender = null, EventArgs e = null)
        {
            foreach (Process proc in Process.GetProcessesByName("steam")) proc.Kill();
        }

    }
}
 