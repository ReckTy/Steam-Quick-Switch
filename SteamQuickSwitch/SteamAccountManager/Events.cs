using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamQuickSwitch
{
    public partial class Form1
    {
        int latestSelectedLvi;

        bool forceItemRemoval = false;
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Checks if user really wants to exit
            if (e.CloseReason == CloseReason.ApplicationExitCall && MessageBox.Show("Are you sure you want to quit SQS?", 
                "Steam Quick Switch", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                e.Cancel = true;
            else
                return;

            // SaveLoginInfo if CurrentPanel is panelManage
            if (GetCurrentPanelIndex() == 1)
                SaveLoginInfo();

            // Abort any existing animationThreads
            if (animationThread != null) animationThread.Abort();
        }
        
        #region Form

        #region panelTopBar

        #region Make window dragable

        bool TogMove;
        int MvalX, MvalY;

        private void panelTopBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !animationInProgess)
            {
                TogMove = true;
                MvalX = e.X;
                MvalY = e.Y;
            }
        }

        private void panelTopBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                TogMove = false;
        }

        private void panelTopBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (TogMove)
                this.Location = new Point(MousePosition.X - MvalX, MousePosition.Y - MvalY);
        }

        #endregion

        private void buttonHome_Click(object sender, EventArgs e)
        {
            ChangePanel(0);
            RefreshStartButtons();
        }

        private void buttonManage_Click(object sender, EventArgs e)
        {
            if (UsingManagerPassword()) // Login page
            {
                ChangePanel(3);
                textBoxManagerPasswordLogin.Focus();
            }
            else // Skip Login page
            {
                ChangePanel(1);
                textBoxUsername.Focus();
            }
        }

        private void buttonAltQ_Click(object sender, EventArgs e)
        {
            ToggleWindow();
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            ChangePanel(2);
        }

        #endregion

        #region panelHome

        private void StartButton(object _sender, EventArgs _e)
        {
            focusLabel.Focus();

            int currentButtonID = GetButtonByObject(_sender);

            foreach (Process proc in Process.GetProcessesByName("steam")) proc.Kill();

            // Starts new SteamProcess with login details
            string steamStartArgs = "-login " + sds.ReadLine("Data", sdsIDUsernames + currentButtonID) + " " + sds.ReadLine("Data", sdsIDPasswords + currentButtonID);
            //string steamStartArgs = "-login " + listViewLogins.Items[currentButtonID].Text + " " + listViewLogins.Items[currentButtonID].SubItems[1].Text;
            if (settingSteamConsole.Checked) steamStartArgs += " -console";
            Process steamProcess = Process.Start(@"C:\Program Files (x86)\Steam\Steam.exe", steamStartArgs);

            if (setting2.Checked) ToggleWindow();
        }

        #endregion

        #region panelManage

        private void textBoxUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBoxPassword.Focus();
                e.SuppressKeyPress = true;
            }
        }

        private void textBoxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonAddLogin_Click(null, null);
                e.SuppressKeyPress = true;
            }
        }

        private void buttonAddLogin_Click(object sender, EventArgs e)
        {
            if (listViewLogins.Items.Count < 18)
            {
                if (textBoxUsername.Text != "" && textBoxPassword.Text != "")
                {
                    ListViewItem lvi = new ListViewItem(textBoxUsername.Text);
                    lvi.SubItems.Add(textBoxPassword.Text);
                    listViewLogins.Items.Add(lvi);
                    textBoxUsername.Clear();
                    textBoxPassword.Clear();

                    textBoxUsername.Focus();
                }
            }
            else
                MessageBox.Show("You already have the maximum amount of accounts inserted."
                    , "Steam Quick Switch", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void buttonEditSelectedItem_Click(object sender, EventArgs e)
        {
            int selectedItemCount = GetSelectedItemCount()[0];
            int selectedItemIndex = GetSelectedItemCount()[1];

            if (selectedItemCount == 1)
            {
                ChangePanel(4);
                textBoxEditUsername.Text = sds.ReadLine("Data", sdsIDUsernames + selectedItemIndex);
                textBoxEditPassword.Text = sds.ReadLine("Data", sdsIDPasswords + selectedItemIndex);

                latestSelectedLvi = selectedItemIndex;
            }
            else if (selectedItemCount > 1)
            {
                MessageBox.Show("You can't edit multiple items at once.", "Steam Quick Switch", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                ShowMsgBoxNoItemSelected();
            }
        }

        private void buttonRemoveSelectedItems_Click(object sender, EventArgs e)
        {
            int selectedItemCount = GetSelectedItemCount()[0];
            if (selectedItemCount == 0)
            {
                ShowMsgBoxNoItemSelected();
                return;
            }

            if (MessageBox.Show("Are you sure you want to remove the selected items?", "Steam Quick Switch", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (ListViewItem lvi in listViewLogins.Items)
                {
                    if (lvi.Checked) lvi.Remove();
                }
            }
        }
        
        #region listViewLogins

        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int selectedItemCount = GetSelectedItemCount()[0];
            int selectedItemID = GetSelectedItemCount()[1];
            
            if (selectedItemCount == 0)
            {
                ShowMsgBoxNoItemSelected();
                return;
            }
            else if (selectedItemCount > 1)
            {
                ShowMsgBoxCantMoveMultipleItems();
                return;
            }

            ListViewItem lvi = new ListViewItem(listViewLogins.Items[selectedItemID].Text);
            lvi.SubItems.Add(listViewLogins.Items[selectedItemID].SubItems[1].Text);
            lvi.Checked = true;

            listViewLogins.Items[selectedItemID].Remove();
            listViewLogins.Items.Insert(selectedItemID - 1, lvi);
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int selectedItemCount = GetSelectedItemCount()[0];
            int selectedItemID = GetSelectedItemCount()[1];

            if (selectedItemCount == 0)
            {
                ShowMsgBoxNoItemSelected();
                return;
            }
            else if (selectedItemCount > 1)
            {
                ShowMsgBoxCantMoveMultipleItems();
                return;
            }

            ListViewItem lvi = new ListViewItem(listViewLogins.Items[selectedItemID].Text);
            lvi.SubItems.Add(listViewLogins.Items[selectedItemID].SubItems[1].Text);
            lvi.Checked = true;

            listViewLogins.Items[selectedItemID].Remove();
            listViewLogins.Items.Insert(selectedItemID + 1, lvi);

        }

        private void removeAllItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (forceItemRemoval || MessageBox.Show("Are you sure you want to remove all items?", "Steam Quick Switch", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (listViewLogins.Items.Count != 0)
                {
                    foreach (ListViewItem lvi in listViewLogins.Items)
                    {
                        lvi.Remove();
                    }
                }
            }
        }

        #endregion

        #endregion

        #region panelManageLogin

        private void textBoxManagerPasswordLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                buttonManagerLogin_Click(null, null);
                e.Handled = true;
            }
        }

        private void buttonManagerLogin_Click(object sender, EventArgs e)
        {
            if (textBoxManagerPasswordLogin.Text == sds.ReadLine("Data", sdsIDManagerPassword))
            {
                ChangePanel(1);
                textBoxUsername.Focus();
            }
            textBoxManagerPasswordLogin.Text = "";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            focusLabel.Focus();

            DialogResult result = MessageBox.Show("You can reset your password in the settings tab." + "\n" + "Do you want to change it?",
                "Steam Quick Switch",
                MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);

            if (result == DialogResult.Yes)
            {
                ChangePanel(2);
                textBoxManagerPasswordOld.Focus();
            }
        }

        #endregion

        #region panelManageEditItem

        private void textBoxEditUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                textBoxEditPassword.Focus();
                e.Handled = true;
            }
        }

        private void textBoxEditPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                buttonEditConfirm_Click(null, null);
                e.Handled = true;
            }
        }

        private void buttonEditCancel_Click(object sender, EventArgs e)
        {
            textBoxEditUsername.Text = "";
            textBoxEditPassword.Text = "";

            ChangePanel(1);
        }

        private void buttonEditConfirm_Click(object sender, EventArgs e)
        {
            if (textBoxEditUsername.Text != "" && textBoxEditPassword.Text != "")
            {
                sds.WriteLine("Data", sdsIDUsernames + latestSelectedLvi, textBoxEditUsername.Text);
                sds.WriteLine("Data", sdsIDPasswords + latestSelectedLvi, textBoxEditPassword.Text);
                buttonEditCancel_Click(null, null);
            }
            else
            {
                DialogResult result = MessageBox.Show("The account details have to contain both a name and a password.\nDo you want to delete the account from SQS?", "Steam Quick Switch", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    listViewLogins.Items[latestSelectedLvi].Remove();
                    buttonEditCancel_Click(null, null);
                }

            }
        }

        #endregion

        #region panelSettings
        
        private void settingCustomStartingPos_CheckedChanged(object sender, EventArgs e)
        {
            if (!settingCustomStartingPos.Checked)
                settingAnimStartingPos.Checked = false;

            // Assign labels:
            labelSettingStartAt.Enabled = settingCustomStartingPos.Checked;
            labelSettingStartX.Enabled = settingCustomStartingPos.Checked;
            labelSettingStartY.Enabled = settingCustomStartingPos.Checked;

            // Assign textBoxes:
            textBoxSettingPosX.Enabled = settingCustomStartingPos.Checked;
            textBoxSettingPosY.Enabled = settingCustomStartingPos.Checked;
        }

        private void settingAnimStartingPos_CheckedChanged(object sender, EventArgs e)
        {
            if (settingAnimStartingPos.Checked)
                settingCustomStartingPos.Checked = true;

            if (settingsLoaded)
            {
                string posHolderX = textBoxSettingAnimPosX.Text;
                string posHolderY = textBoxSettingAnimPosY.Text;

                textBoxSettingAnimPosX.Text = textBoxSettingPosX.Text;
                textBoxSettingAnimPosY.Text = textBoxSettingPosY.Text;

                textBoxSettingPosX.Text = posHolderX;
                textBoxSettingPosY.Text = posHolderY;
            }


            // Assign labels:
            labelSettingFinishAt.Enabled = settingAnimStartingPos.Checked;
            labelSettingFinishX.Enabled = settingAnimStartingPos.Checked;
            labelSettingFinishY.Enabled = settingAnimStartingPos.Checked;
            labelSettingAnimSpeed.Enabled = settingAnimStartingPos.Checked;

            // Assign textBoxes:
            textBoxSettingAnimPosX.Enabled = settingAnimStartingPos.Checked;
            textBoxSettingAnimPosY.Enabled = settingAnimStartingPos.Checked;

            // Assign trackBar:
            trackBarSettingAnimSpeed.Enabled = settingAnimStartingPos.Checked;
        }

        private void comboBoxSettingColorScheme_SelectionChangeCommitted(object sender, EventArgs e)
        {
            focusLabel.Focus();
        }

        private void trackBarSettingAnimSpeed_ValueChanged(object sender, EventArgs e)
        {
            animationSpeed = trackBarSettingAnimSpeed.Value;
        }

        #region panelSetManagerPassword

        private void textBoxManagerPasswordOld_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                textBoxManagerPasswordSet.Focus();
            }
        }

        private void textBoxManagerPasswordSet_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                buttonSetManagerPassword_Click(null, null);
                e.Handled = true;
            }
        }

        private void buttonSetManagerPassword_Click(object sender, EventArgs e)
        {
            bool hadPassBefore = false;
            bool oldPassIsMatching = false;
            if (UsingManagerPassword()) hadPassBefore = true;
            if (textBoxManagerPasswordOld.Text == sds.ReadLine("Data", sdsIDManagerPassword)) oldPassIsMatching = true;

            if (hadPassBefore && !oldPassIsMatching)
            {
                if (MessageBox.Show("The old password is not matching the entered one.\nAre you sure you want to change it?\nThis will delete all your saved accounts.", "Steam Quick Switch", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    sds.WriteLine("Data", sdsIDManagerPassword, textBoxManagerPasswordSet.Text);
                    textBoxManagerPasswordOld.Text = "";
                    textBoxManagerPasswordSet.Text = "";
                    forceItemRemoval = true;
                    removeAllItemsToolStripMenuItem_Click(null, null);
                    forceItemRemoval = false;
                    
                    SaveLoginInfo();
                    PasswordChangeDisplayConformationMsg();
                }
            }
            else
            {
                sds.WriteLine("Data", sdsIDManagerPassword, textBoxManagerPasswordSet.Text);
                textBoxManagerPasswordOld.Text = "";
                textBoxManagerPasswordSet.Text = "";
                
                SaveLoginInfo();
                PasswordChangeDisplayConformationMsg();
            }
        }

        #region buttonPasswordHelp

        private void buttonPasswordHelp_MouseEnter(object sender, EventArgs e)
        {
            buttonPasswordHelp.Size = new Size(26, 26);
            buttonPasswordHelp.Location = new Point(buttonPasswordHelp.Location.X - 3, buttonPasswordHelp.Location.Y - 3);
        }

        private void buttonPasswordHelp_MouseLeave(object sender, EventArgs e)
        {
            buttonPasswordHelp.Size = new Size(20, 20);
            buttonPasswordHelp.Location = new Point(buttonPasswordHelp.Location.X + 3, buttonPasswordHelp.Location.Y + 3);
        }

        private void buttonPasswordHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("If you want to remove your password, you just click 'Set' without entering anything.\n\nIf you don't know the previous password, you don't need it.\nHowever, all saved accounts will get deleted if you don't enter it.", "Steam Quick Switch", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        #endregion

        #endregion

        private void buttonExtras_Click(object sender, EventArgs e)
        {
            ChangePanel(6); // panelExtras
        }

        private void buttonManageExceptions_Click(object sender, EventArgs e)
        {
            ChangePanel(5);
            textBoxExceptionsList.Focus();
        }

        #endregion

        #region panelSettingsExceptions

        private void buttonDiscardExceptions_Click(object sender, EventArgs e)
        {
            textBoxExceptionsList.Text = Properties.Settings.Default.ProcessExceptionsList;

            ChangePanel(2);
        }
        private void buttonSaveExceptions_Click(object sender, EventArgs e)
        {
            if (!textBoxExceptionsList.Text.Contains(" "))
            {
                Properties.Settings.Default.ProcessExceptionsList = textBoxExceptionsList.Text;

                WriteSettingsToDisk();

                ChangePanel(2);
            }
            else
                MessageBox.Show("The text currently entered contains spaces.\n" +
                    "Try removing all spaces and save again.", "Steam Quick Switch", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion

        #region panelExtras

        private void buttonDonate_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.paypal.me/MattiasAldhagen");
            ToggleWindow();
        }

        private void buttonExtrasBack_Click(object sender, EventArgs e)
        {
            ChangePanel(2); // panelSettings
        }

        #endregion

        #region ToolStrips

        private void exitSQSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion

        #region Other

        void ShowMsgBoxNoItemSelected()
        {
            MessageBox.Show("No item selected.", "Steam Quick Switch", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        void ShowMsgBoxCantMoveMultipleItems()
        {
            MessageBox.Show("Can't move multiple items at once.", "Steam Quick Switch", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        #endregion

        #endregion

    }
}
