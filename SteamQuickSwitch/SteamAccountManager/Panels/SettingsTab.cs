using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamQuickSwitch
{
    public partial class MainForm
    {
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

        private void buttonSettingBrowseSteamPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog() { Description = "Select your Steam-folder", ShowNewFolderButton = false })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (!dlg.SelectedPath.EndsWith("Steam"))
                    {
                        string[] pathSplit = dlg.SelectedPath.Split('\\');
                        string selectedFileName = pathSplit[pathSplit.Length - 1];

                        DialogResult result = MessageBox.Show("Are you sure you selected your Steam-folder?\n" +
                            "The folders name is usually 'Steam', not '" + selectedFileName + "'", "Steam Quick Switch",
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2);

                        if (result == DialogResult.No)
                        {
                            buttonSettingBrowseSteamPath_Click(null, null);
                            return;
                        }
                        else if (result == DialogResult.Cancel)
                        {
                            return;
                        }
                    }

                    textBoxSettingSteamPath.Text = dlg.SelectedPath;
                }
            }
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
            bool hadPassBefore = UsingManagerPassword();
            bool oldPassIsMatching = (textBoxManagerPasswordOld.Text == sds.ReadLine("Data", sdsIDManagerPassword)) ? true : false;
            
            if (hadPassBefore && !oldPassIsMatching)
            {
                if (MessageBox.Show("The old password is not matching the entered one.\nAre you sure you want to change it?\n" +
                    "This will delete all your saved accounts.", "Steam Quick Switch", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    sds.WriteLine("Data", sdsIDManagerPassword, textBoxManagerPasswordSet.Text);
                    textBoxManagerPasswordOld.Text = "";
                    textBoxManagerPasswordSet.Text = "";

                    EmptyListViewLogins();
                    SaveLoginInfo();
                    
                    PasswordChangeDisplayConformationMsg();
                }
            }
            else
            {
                sds.WriteLine("Data", sdsIDManagerPassword, textBoxManagerPasswordSet.Text);
                textBoxManagerPasswordOld.Text = "";
                textBoxManagerPasswordSet.Text = "";

                FillListViewLogins();
                SaveLoginInfo();
                EmptyListViewLogins();

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
        
        private void buttonSettingExtraFeatures_Click(object sender, EventArgs e)
        {
            ChangePanel(7); //panelExtraFeatures
        }
        
    }
}
