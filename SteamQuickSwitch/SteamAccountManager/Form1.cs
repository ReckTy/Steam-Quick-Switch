using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.Management;

namespace SteamQuickSwitch
{
    public partial class Form1 : Form
    {
        Size formSize = new Size(489, 302);
        
        bool allowAppExit = false;

        Panel[] panelArray;
        Button[] startButtons;
        
        RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        SensitiveDataStorage.SensitiveDataStorage sds = new SensitiveDataStorage.SensitiveDataStorage();
        int sdsIDManagerPassword = 0, sdsIDUsernames = 1, sdsIDPasswords = 19;
        
        public Form1()
        {
            if (!Application.ExecutablePath.EndsWith("SQS.exe"))
            {
                MessageBox.Show("Executable name has to be 'SQS' to prevent misconceptions.");
                allowAppExit = true;
                Application.Exit();
                return;
            }
            // Close identical apps
            Process[] procList = Process.GetProcessesByName("SQS");
            if (procList.Length > 1)
            {
                Process currentProcess = Process.GetCurrentProcess();
                foreach (Process proc in procList)
                    if (proc.Id != currentProcess.Id)
                        proc.Kill();
            }

            InitializeComponent();

            AssignArrays();

            // Set panel locations
            while (Size != formSize) Size = formSize;
            foreach (Panel panel in panelArray) panel.Location = new Point(5, 54);

            this.Visible = false;

            sds.EncryptionPassword = "ChangedForYourSafety";
            sds.CreateFile("Data");

            LoadSavedSettings();
        }
        
        void AssignArrays()
        {
            panelArray = new Panel[] 
            {
                panelHome, panelManage, panelSettings, panelManageLogin, 
                panelManageEditItem, panelSettingsExceptions, panelExtras
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

            LoadLoginInfo();
            
            switch (GetCurrentPanelIndex())
            {
                // panelManage
                case 1:
                    // Clear textboxes
                    textBoxUsername.Clear();
                    textBoxPassword.Clear();

                    SaveLoginInfo();
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

                    //settingSteamConsole
                    settingSteamConsole.Checked = Properties.Settings.Default.SteamConsole;

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
        
        int[] GetSelectedItemCount()
        {
            int selectedItemCount = 0, selectedItemIndex = 0;

            foreach (ListViewItem lvi in listViewLogins.Items)
            {
                if (lvi.Checked)
                {
                    selectedItemCount++;
                    selectedItemIndex = lvi.Index;
                }
            }
            return new int[2] { selectedItemCount, selectedItemIndex };
        }
        
        void RefreshStartButtons()
        {
            bool isAnyBtnVisible = false;
            for (int i = 0; i < 18; i++)
            {
                if (listViewLogins.Items.Count >= i + 1)
                {
                    startButtons[i].Visible = true;
                    isAnyBtnVisible = true;

                    startButtons[i].Text = listViewLogins.Items[i].Text; // Set button name
                    // Maybe possible to add something more here
                }
                else
                {
                    startButtons[i].Visible = false;
                }
            }

            if (!isAnyBtnVisible)
            {
                pictureBoxManageArrow.Visible = true;
                labelHomeNoAccounts.Visible = true;
            }
            else
            {
                pictureBoxManageArrow.Visible = false;
                labelHomeNoAccounts.Visible = false;
            }
        }

        int GetButtonByObject(object _obj)
        {
            for (int i = 0; i < startButtons.Length + 1; i++)
            {
                if (startButtons[i] == _obj)
                    return i;
            }
            return -1; // If the button can't be found
        }

        bool CoordinateIsOutOfScreen(int X, int Y)
        {
            if (X < 0 || Y < 0 || X > (Screen.PrimaryScreen.Bounds.Width - formSize.Width) || Y > (Screen.PrimaryScreen.Bounds.Height - formSize.Height))
                return true;
            return false;
        }
        
        void ChangeSQSColor()
        {
            // Assign Arrays
            List<Panel> panels = new List<Panel>() { panelTopBar, panelHome, panelManage, panelManageEditItem, panelManageLogin, panelSettingsExceptions,
                                                     panelSettings, panelExtras, panel1 };
            for (int i = 0; i < panelArray.Length; i++)
            {
                if (!panels.Contains(panelArray[i]))
                    panels.Add(panelArray[i]);
            }

            // Assign color variables
            Color[] colors = new Color[18];
            switch (Properties.Settings.Default.ColorScheme)
            {
                // Defalut
                case 0:
                    colors[0]  = Color.FromArgb(15, 15, 15);
                    colors[1]  = Color.FromArgb(20, 20, 20);
                    colors[2]  = Color.FromArgb(25, 25, 25);
                    colors[3]  = Color.FromArgb(25, 25, 25);
                    colors[4]  = Color.FromArgb(30, 30, 30);
                    colors[5]  = Color.FromArgb(35, 35, 35);
                    colors[6]  = Color.FromArgb(26, 26, 26);
                    colors[7]  = Color.FromArgb(50, 50, 50);
                    colors[8]  = Color.FromArgb(55, 55, 55);
                    colors[9]  = Color.FromArgb(60, 60, 60);
                    colors[10] = Color.FromArgb(255, 255, 255);
                    colors[11] = Color.FromArgb(200, 200, 200);
                    colors[12] = Color.FromArgb(80, 80, 80);
                    colors[13] = Color.FromArgb(220, 220, 0);
                    colors[14] = Color.FromArgb(255, 196, 57);
                    colors[15] = Color.FromArgb(255, 206, 60);
                    colors[16] = Color.FromArgb(255, 226, 65);
                    colors[17] = Color.FromArgb(20, 20, 20);
                    break;

                // Purple
                case 1:
                    colors[0]  = Color.FromArgb(41, 0, 41);
                    colors[1] = Color.FromArgb(46, 0, 46);
                    colors[2] = Color.FromArgb(51, 0, 51);
                    colors[3] = Color.FromArgb(51, 0, 51);
                    colors[4]  = Color.FromArgb(56, 0, 56);
                    colors[5] = Color.FromArgb(61, 0, 61);
                    colors[6] = Color.FromArgb(48, 0, 48);
                    colors[7] = Color.FromArgb(70, 0, 70);
                    colors[8] = Color.FromArgb(75, 0, 75);
                    colors[9] = Color.FromArgb(80, 0, 80);
                    colors[10] = Color.FromArgb(255, 255, 255);
                    colors[11] = Color.FromArgb(153, 0, 153);
                    colors[12] = Color.FromArgb(102, 0, 102);
                    colors[13] = Color.FromArgb(220, 220, 0);
                    colors[14] = Color.FromArgb(255, 196, 57);
                    colors[15] = Color.FromArgb(255, 206, 60);
                    colors[16] = Color.FromArgb(255, 226, 65);
                    colors[17] = Color.FromArgb(20, 20, 20);
                    break;

                // Pink
                case 2:
                    colors[0] = Color.FromArgb(137, 0, 137);
                    colors[1] = Color.FromArgb(167, 0, 167);
                    colors[2] = Color.FromArgb(255, 0, 255);
                    colors[3] = Color.FromArgb(255, 0, 255);
                    colors[4] = Color.FromArgb(184, 0, 184);
                    colors[5] = Color.FromArgb(173, 0, 173);
                    colors[6] = Color.FromArgb(175, 0, 175);
                    colors[7] = Color.FromArgb(130, 0, 130);
                    colors[8] = Color.FromArgb(120, 0, 120);
                    colors[9] = Color.FromArgb(110, 0, 110);
                    colors[10] = Color.FromArgb(255, 255, 255);
                    colors[11] = Color.FromArgb(255, 204, 255);
                    colors[12] = Color.FromArgb(255, 193, 255);
                    colors[13] = Color.FromArgb(220, 220, 0);
                    colors[14] = Color.FromArgb(255, 196, 57);
                    colors[15] = Color.FromArgb(255, 206, 60);
                    colors[16] = Color.FromArgb(255, 226, 65);
                    colors[17] = Color.FromArgb(20, 20, 20);
                    break;

                // Red
                case 3:
                    colors[0] = Color.FromArgb(137, 0, 0);
                    colors[1] = Color.FromArgb(167, 0, 0);
                    colors[2] = Color.FromArgb(255, 0, 0);
                    colors[3] = Color.FromArgb(255, 0, 0);
                    colors[4] = Color.FromArgb(184, 0, 0);
                    colors[5] = Color.FromArgb(173, 0, 0);
                    colors[6] = Color.FromArgb(175, 0, 0);
                    colors[7] = Color.FromArgb(130, 0, 0);
                    colors[8] = Color.FromArgb(120, 0, 0);
                    colors[9] = Color.FromArgb(110, 0, 0);
                    colors[10] = Color.FromArgb(255, 255, 255);
                    colors[11] = Color.FromArgb(255, 204, 204);
                    colors[12] = Color.FromArgb(255, 153, 153);
                    colors[13] = Color.FromArgb(220, 220, 0);
                    colors[14] = Color.FromArgb(255, 196, 57);
                    colors[15] = Color.FromArgb(255, 206, 60);
                    colors[16] = Color.FromArgb(255, 226, 65);
                    colors[17] = Color.FromArgb(20, 20, 20);
                    break;

                // Orange
                case 4:
                    colors[0] = Color.FromArgb(157, 85, 0);
                    colors[1] = Color.FromArgb(172, 95, 0);
                    colors[2] = Color.FromArgb(255, 125, 0);
                    colors[3] = Color.FromArgb(255, 125, 0);
                    colors[4] = Color.FromArgb(204, 115, 0);
                    colors[5] = Color.FromArgb(194, 105, 0);
                    colors[6] = Color.FromArgb(195, 109, 0);
                    colors[7] = Color.FromArgb(150, 81, 0);
                    colors[8] = Color.FromArgb(140, 81, 0);
                    colors[9] = Color.FromArgb(130, 81, 0);
                    colors[10] = Color.FromArgb(255, 255, 255);
                    colors[11] = Color.FromArgb(255, 229, 204);
                    colors[12] = Color.FromArgb(255, 202, 153);
                    colors[13] = Color.FromArgb(220, 220, 0);
                    colors[14] = Color.FromArgb(255, 196, 57);
                    colors[15] = Color.FromArgb(255, 206, 60);
                    colors[16] = Color.FromArgb(255, 226, 65);
                    colors[17] = Color.FromArgb(20, 20, 20);
                    break;

                // Yellow
                case 5:
                    colors[0] = Color.FromArgb(147, 147, 0);
                    colors[1] = Color.FromArgb(173, 173, 0);
                    colors[2] = Color.FromArgb(200, 200, 50);
                    colors[3] = Color.FromArgb(200, 200, 50);
                    colors[4] = Color.FromArgb(184, 184, 0);
                    colors[5] = Color.FromArgb(173, 173, 0);
                    colors[6] = Color.FromArgb(185, 185, 0);
                    colors[7] = Color.FromArgb(130, 130, 0);
                    colors[8] = Color.FromArgb(120, 120, 0);
                    colors[9] = Color.FromArgb(110, 110, 0);
                    colors[10] = Color.FromArgb(255, 255, 255);
                    colors[11] = Color.FromArgb(255, 255, 204);
                    colors[12] = Color.FromArgb(255, 255, 204);
                    colors[13] = Color.FromArgb(50, 50, 200);
                    colors[14] = Color.FromArgb(255, 255, 57);
                    colors[15] = Color.FromArgb(255, 255, 60);
                    colors[16] = Color.FromArgb(255, 255, 65);
                    colors[17] = Color.FromArgb(20, 20, 20);
                    break;

                // Green
                case 6:
                    colors[0] = Color.FromArgb(0, 100, 0);
                    colors[1] = Color.FromArgb(0, 114, 0);
                    colors[2] = Color.FromArgb(0, 128, 0);
                    colors[3] = Color.FromArgb(0, 128, 0);
                    colors[4] = Color.FromArgb(0, 118, 0);
                    colors[5] = Color.FromArgb(0, 108, 0);
                    colors[6] = Color.FromArgb(0, 118, 0);
                    colors[7] = Color.FromArgb(0, 84, 0);
                    colors[8] = Color.FromArgb(0, 64, 0);
                    colors[9] = Color.FromArgb(0, 74, 0);
                    colors[10] = Color.FromArgb(255, 255, 255);
                    colors[11] = Color.FromArgb(0, 58, 0);
                    colors[12] = Color.FromArgb(0, 78, 0);
                    colors[13] = Color.FromArgb(220, 220, 0);
                    colors[14] = Color.FromArgb(255, 196, 57);
                    colors[15] = Color.FromArgb(255, 206, 60);
                    colors[16] = Color.FromArgb(255, 226, 65);
                    colors[17] = Color.FromArgb(20, 20, 20);
                    break;

                // Cyan
                case 7:
                    colors[0] = Color.FromArgb(0, 147, 147);
                    colors[1] = Color.FromArgb(0, 173, 173);
                    colors[2] = Color.FromArgb(0, 200, 200);
                    colors[3] = Color.FromArgb(0, 200, 200);
                    colors[4] = Color.FromArgb(0, 184, 184);
                    colors[5] = Color.FromArgb(0, 173, 173);
                    colors[6] = Color.FromArgb(0, 185, 185);
                    colors[7] = Color.FromArgb(0, 130, 130);
                    colors[8] = Color.FromArgb(0, 120, 120);
                    colors[9] = Color.FromArgb(0, 110, 110);
                    colors[10] = Color.FromArgb(255, 255, 255);
                    colors[11] = Color.FromArgb(204, 255, 255);
                    colors[12] = Color.FromArgb(153, 255, 255);
                    colors[13] = Color.FromArgb(255, 255, 0);
                    colors[14] = Color.FromArgb(255, 196, 57);
                    colors[15] = Color.FromArgb(255, 206, 60);
                    colors[16] = Color.FromArgb(255, 226, 65);
                    colors[17] = Color.FromArgb(20, 20, 20);
                    break;

                // Blue
                case 8:
                    colors[0] = Color.FromArgb(0, 0, 137);
                    colors[1] = Color.FromArgb(0, 0, 167);
                    colors[2] = Color.FromArgb(0, 0, 255);
                    colors[3] = Color.FromArgb(0, 0, 255);
                    colors[4] = Color.FromArgb(0, 0, 184);
                    colors[5] = Color.FromArgb(0, 0, 173);
                    colors[6] = Color.FromArgb(0, 0, 175);
                    colors[7] = Color.FromArgb(0, 0, 130);
                    colors[8] = Color.FromArgb(0, 0, 120);
                    colors[9] = Color.FromArgb(0, 0, 110);
                    colors[10] = Color.FromArgb(255, 255, 255);
                    colors[11] = Color.FromArgb(204, 204, 255);
                    colors[12] = Color.FromArgb(153, 153, 255);
                    colors[13] = Color.FromArgb(220, 220, 0);
                    colors[14] = Color.FromArgb(255, 196, 57);
                    colors[15] = Color.FromArgb(255, 206, 60);
                    colors[16] = Color.FromArgb(255, 226, 65);
                    colors[17] = Color.FromArgb(20, 20, 20);
                    break;

            }
            
            // Assign Control Variables
            Panel[] panelColor0 = new Panel[] { panelTopBar };
            Label[] labelColor9 = new Label[] { labelVersionDisplay };
            Label[] labelColor10 = new Label[] { labelInfo, labelInfo1 };
            Label[] labelColor11 = new Label[] { labelImportant };
            Button[] buttonColor0 = new Button[] { buttonHome, buttonManage, buttonAltQ, buttonSettings };
            Button[] buttonColor3 = new Button[] { buttonProfile1, buttonProfile2, buttonProfile3, buttonProfile4, buttonProfile5, buttonProfile6,
                                                   buttonProfile7, buttonProfile8, buttonProfile9, buttonProfile10, buttonProfile11, buttonProfile12,
                                                   buttonProfile13, buttonProfile14, buttonProfile15,buttonProfile16, buttonProfile17, buttonProfile18 };
            Button[] buttonColor14 = new Button[] { buttonDonate };
            
            // Apply Colors
            this.BackColor = colors[3];
            foreach (Panel p in panels)
            {
                if (p == null)
                    continue;

                p.BackColor = colors[3];

                if (panelColor0.Contains(p))
                    p.BackColor = colors[0];

                //Continue
                foreach (Control c in p.Controls)
                {
                    switch (c.GetType().Name)
                    {
                        case "Panel":
                            if (panelColor0.Contains(c))
                                c.BackColor = colors[0];
                            else
                                c.BackColor = colors[3];
                            break;

                        case "Label":
                            if (labelColor9.Contains(c))
                                c.ForeColor = colors[11];
                            else if (labelColor10.Contains(c))
                                c.ForeColor = colors[12];
                            else if (labelColor11.Contains(c))
                                c.ForeColor = colors[13];
                            else
                                c.ForeColor = colors[10];
                            break;

                        case "Button":
                            foreach (Button b in p.Controls.OfType<Button>())
                            {
                                if (b == c)
                                {
                                    if (buttonColor0.Contains(c))
                                    {
                                        b.BackColor = colors[0];
                                        b.FlatAppearance.MouseOverBackColor = colors[1];
                                        b.FlatAppearance.MouseDownBackColor = colors[2];
                                    }
                                    else if (buttonColor3.Contains(c))
                                    {
                                        b.BackColor = colors[3];
                                        b.FlatAppearance.MouseOverBackColor = colors[4];
                                        b.FlatAppearance.MouseDownBackColor = colors[5];
                                        b.ForeColor = colors[10];
                                    }
                                    else if (buttonColor14.Contains(c))
                                    {
                                        b.BackColor = colors[14];
                                        b.FlatAppearance.MouseOverBackColor = colors[15];
                                        b.FlatAppearance.MouseDownBackColor = colors[16];

                                        b.ForeColor = colors[17];
                                    }
                                    else
                                    {
                                        b.BackColor = colors[7];
                                        b.FlatAppearance.MouseOverBackColor = colors[8];
                                        b.FlatAppearance.MouseDownBackColor = colors[9];
                                    }
                                    break;
                                }
                            }
                            buttonPasswordHelp.BackColor = colors[3];
                            buttonPasswordHelp.FlatAppearance.MouseOverBackColor = colors[3];
                            buttonPasswordHelp.FlatAppearance.MouseDownBackColor = colors[3];
                            break;

                        case "TextBox":
                            c.BackColor = colors[6];
                            c.ForeColor = colors[10];
                            break;
                            
                        case "CheckBox":
                            c.ForeColor = colors[10];
                            break;

                        case "ComboBox":
                            c.BackColor = colors[6];
                            c.ForeColor = colors[10];
                            break;
                    }
                }
            }
            
        }
        
        void PasswordChangeDisplayConformationMsg()
        {
            if (UsingManagerPassword())
            {
                MessageBox.Show("Manager password changed to '" + sds.ReadLine("Data", sdsIDManagerPassword) + "'",
                    "Steam Quick Switch", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Manager password removed.",
                    "Steam Quick Switch", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        bool UsingManagerPassword()
        {
            if (sds.ReadLine("Data", sdsIDManagerPassword).Length > 0) return true;
            return false;
        }
        
    }
}
 