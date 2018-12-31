using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SteamQuickSwitch
{
    public partial class Form1
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
            Properties.Settings.Default.SteamConsole = settingSteamConsole.Checked;

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

            settingSteamConsole.Checked = Properties.Settings.Default.SteamConsole;

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
                    listViewLogins.Items.Insert(i, lvi);
                }
            }
        }

        void EmptyListViewLogins()
        {
            foreach (ListViewItem lvi in listViewLogins.Items) lvi.Remove();
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
                    if (i != 1)
                    {
                        sds.WriteLine("Data", sdsIDUsernames + i, "");
                        sds.WriteLine("Data", sdsIDPasswords + i, "");

                    }
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

    }
}
