using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SteamQuickSwitch
{
    public partial class MainForm
    {
        bool settingsLoaded = false;
        
        /// <summary>
        /// Saves all settings displayed in the settingsPanel, except LoginInfo & ManagerPassword
        /// </summary>
        void SaveSettings(object _sender, EventArgs e)
        {
            Properties.Settings.Default.FirstRun = false;

            int maxX = Screen.PrimaryScreen.Bounds.Width - formSize.Width;
            int maxY = Screen.PrimaryScreen.Bounds.Height - formSize.Height;

            // If textBoxes are not ints
            if (settingCustomStartingPos.Checked && !FixIntTextbox(textBoxSettingPosX) || !FixIntTextbox(textBoxSettingPosY))
            {
                MessageBox.Show("Make sure the 'Start at: coordinates' are valid number.", "Steam Quick Switch", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                return;
            }
            else if (!FixIntTextbox(textBoxSettingAnimPosX) || !FixIntTextbox(textBoxSettingAnimPosY))
            {
                MessageBox.Show("Make sure the 'Finish at: coordinates' are valid number.", "Steam Quick Switch", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                return;
            }
            else if (settingCustomStartingPos.Checked && !settingAnimStartingPos.Checked && CoordinateIsOutOfScreen(int.Parse(textBoxSettingPosX.Text), int.Parse(textBoxSettingPosY.Text)))
            {
                MessageBox.Show("The start position needs to be within" + "\n" +
                    "X: 0 - " + maxX.ToString() + "\n" +
                    "Y: 0 - " + maxY.ToString(), "Steam Quick Switch", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                return;
            }
            else if (settingAnimStartingPos.Checked && CoordinateIsOutOfScreen(int.Parse(textBoxSettingAnimPosX.Text), int.Parse(textBoxSettingAnimPosY.Text)))
            {
                MessageBox.Show("The animations finish - position needs to be within" + "\n" +
                    "X: 0 - " + maxX.ToString() + "\n" +
                    "Y: 0 - " + maxY.ToString(), "Steam Quick Switch", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                return;
            }

            // Settings Tab
            if (setting1.Checked)
            {
                if (rk.GetValue("Steam Quick Switch") == null)
                {
                    rk.SetValue("Steam Quick Switch", Application.ExecutablePath.ToString());
                }
            }
            else
            {
                if (rk.GetValue("Steam Quick Switch") != null)
                    rk.DeleteValue("Steam Quick Switch", false);
            }

            Properties.Settings.Default.CloseAfterAccountChange = setting2.Checked;
            Properties.Settings.Default.SteamPath = textBoxSettingSteamPath.Text;

            // Update Pos-textBoxes (if CustomStartingPos == false)
            if (!settingCustomStartingPos.Checked)
            {
                textBoxSettingPosX.Text = GetCenterPos().X.ToString();
                textBoxSettingPosY.Text = GetCenterPos().Y.ToString();
            }

            // SQS Position:
            Properties.Settings.Default.CustomStartingPositions = settingCustomStartingPos.Checked;
            Properties.Settings.Default.AnimateStartingPosition = settingAnimStartingPos.Checked;
            Properties.Settings.Default.StartingPosX = Convert.ToInt32(textBoxSettingPosX.Text);
            Properties.Settings.Default.StartingPosY = Convert.ToInt32(textBoxSettingPosY.Text);
            Properties.Settings.Default.AnimatePosX = Convert.ToInt32(textBoxSettingAnimPosX.Text);
            Properties.Settings.Default.AnimatePosY = Convert.ToInt32(textBoxSettingAnimPosY.Text);
            // Speed:
            Properties.Settings.Default.AnimationSpeed = trackBarSettingAnimSpeed.Value;
            
            // Color Scheme
            Properties.Settings.Default.ColorScheme = comboBoxSettingColorScheme.SelectedIndex;
            
            // Write settings to disk
            WriteSettingsToDisk();
            
            // Change SQS-color & Move window to default location
            ChangeSQSColor();
            MoveSQSToPos();
        }
        
        void LoadSavedSettings()
        {
            #region Settings Tab

            // CheckBox(Start with Windows)
            if (rk.GetValue("Steam Quick Switch") != null)
                setting1.Checked = true;
            else
                setting1.Checked = false;

            // CheckBox(Close after changing account)
            setting2.Checked = Properties.Settings.Default.CloseAfterAccountChange;

            // ComboBox(Color Scheme)
            comboBoxSettingColorScheme.SelectedIndex = Properties.Settings.Default.ColorScheme;

            textBoxSettingSteamPath.Text = Properties.Settings.Default.SteamPath;

            #region StartingPosition

            // Assign CheckBoxes
            settingCustomStartingPos.Checked = Properties.Settings.Default.CustomStartingPositions;
            settingAnimStartingPos.Checked = Properties.Settings.Default.AnimateStartingPosition;

            // Set text fields
            textBoxSettingPosX.Text = Properties.Settings.Default.StartingPosX.ToString();
            textBoxSettingPosY.Text = Properties.Settings.Default.StartingPosY.ToString();
            textBoxSettingAnimPosX.Text = Properties.Settings.Default.AnimatePosX.ToString();
            textBoxSettingAnimPosY.Text = Properties.Settings.Default.AnimatePosY.ToString();
            if (Properties.Settings.Default.FirstRun)
            {
                textBoxSettingPosX.Text = GetCenterPos().X.ToString();
                textBoxSettingPosY.Text = GetCenterPos().Y.ToString();
            }

            // AnimationSpeed TrackBar
            trackBarSettingAnimSpeed.Value = Properties.Settings.Default.AnimationSpeed;

            // Assign label.Enabled values
            labelSettingStartAt.Enabled = Properties.Settings.Default.CustomStartingPositions;
            labelSettingStartX.Enabled = Properties.Settings.Default.CustomStartingPositions;
            labelSettingStartY.Enabled = Properties.Settings.Default.CustomStartingPositions;

            labelSettingFinishAt.Enabled = Properties.Settings.Default.AnimateStartingPosition;
            labelSettingFinishX.Enabled = Properties.Settings.Default.AnimateStartingPosition;
            labelSettingFinishY.Enabled = Properties.Settings.Default.AnimateStartingPosition;
            labelSettingAnimSpeed.Enabled = Properties.Settings.Default.AnimateStartingPosition;
            
            #endregion

            #endregion
            
            // Process-exceptions Tab
            textBoxExceptionsList.Text = Properties.Settings.Default.ProcessExceptionsList;
            
            settingsLoaded = true;

            // Go to the Home Tab
            buttonHome_Click(null, null);

            // Change SQS-color & Move window to default location
            ChangeSQSColor();
            MoveSQSToPos();
            
        }

        void FillListViewLogins()
        {
            for (int i = 0; i < 18; i++)
            {
                if (sds.ReadLine("Data", sdsIDUsernames + i) != "")
                {
                    ListViewItem lvi = new ListViewItem(sds.ReadLine("Data", sdsIDUsernames + i));
                    lvi.SubItems.Add(sds.ReadLine("Data", sdsIDPasswords + i));
                    listViewLogins.Items.Add(lvi);
                }
            }
        }

        void EmptyListViewLogins()
        {
            foreach (ListViewItem lvi in listViewLogins.Items) lvi.Remove();
        }

        void EmptyListViewLoginsPrompt()
        {
            if (MessageBox.Show("Are you sure you want to remove all items?", "Steam Quick Switch", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (listViewLogins.Items.Count != 0)
                {
                    EmptyListViewLogins();
                }
            }
        }

        void SaveLoginInfo()
        {
            for (int i = 0; i < 18; i++)
            {
                if (listViewLogins.Items.Count >= (i + 1))
                {
                    sds.WriteLine("Data", sdsIDUsernames + i, listViewLogins.Items[i].Text);
                    sds.WriteLine("Data", sdsIDPasswords + i, listViewLogins.Items[i].SubItems[1].Text);
                }
                else
                {
                    sds.WriteLine("Data", sdsIDUsernames + i, "");
                    sds.WriteLine("Data", sdsIDPasswords + i, "");
                }
            }
        }
        
        void WriteSettingsToDisk()
        {
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

        /// <summary>
        /// Checks if passed in TextBox.Text can be parsed to an int, etc.
        /// </summary>
        /// <returns>true = TextBox.Text can be parsed to an int</returns>
        bool FixIntTextbox(TextBox _textBox)
        {
            //If _textBox is not an int, return false
            if (!int.TryParse(_textBox.Text, out int parsedInt) || _textBox.Text.Contains(" "))
                return false;

            if (_textBox.Text.Contains("+"))
            {
                _textBox.Text = Math.Abs(parsedInt).ToString();
            }
            else if (_textBox.Text.Contains("-") && Math.Abs(parsedInt) == 0)
            {
                _textBox.Text = "0";
            }

            return true;
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
                    colors[0] = Color.FromArgb(15, 15, 15);
                    colors[1] = Color.FromArgb(20, 20, 20);
                    colors[2] = Color.FromArgb(25, 25, 25);
                    colors[3] = Color.FromArgb(25, 25, 25);
                    colors[4] = Color.FromArgb(30, 30, 30);
                    colors[5] = Color.FromArgb(35, 35, 35);
                    colors[6] = Color.FromArgb(26, 26, 26);
                    colors[7] = Color.FromArgb(50, 50, 50);
                    colors[8] = Color.FromArgb(55, 55, 55);
                    colors[9] = Color.FromArgb(60, 60, 60);
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
                    colors[0] = Color.FromArgb(41, 0, 41);
                    colors[1] = Color.FromArgb(46, 0, 46);
                    colors[2] = Color.FromArgb(51, 0, 51);
                    colors[3] = Color.FromArgb(51, 0, 51);
                    colors[4] = Color.FromArgb(56, 0, 56);
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
        
        bool UsingManagerPassword()
        {
            return (sds.ReadLine("Data", sdsIDManagerPassword).Length > 0) ? true : false;
        }

    }
}
