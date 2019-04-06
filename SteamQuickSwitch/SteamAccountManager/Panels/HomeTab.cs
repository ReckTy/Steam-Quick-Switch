using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamQuickSwitch
{
    public partial class MainForm
    {
        
        private void StartButton(object _sender, EventArgs _e)
        {
            focusLabel.Focus();

            string executablePath = Properties.Settings.Default.SteamPath + @"\" + "Steam.exe";

            // Return if 'steam.exe' can't be found
            if (!File.Exists(executablePath))
            {
                MessageBox.Show("Can't find 'Steam.exe'.\n" +
                                "Make sure the Steam path in Settings is assigned correctly.", "Steam Quick Switch",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int currentButtonID = GetButtonByObject(_sender);

            foreach (Process proc in Process.GetProcessesByName("steam")) proc.Kill();

            // Starts new SteamProcess with login details
            string steamStartArgs = "-login " + sds.ReadLine("Data", sdsIDUsernames + currentButtonID) + " " + sds.ReadLine("Data", sdsIDPasswords + currentButtonID);
            Process steamProcess = Process.Start(executablePath, steamStartArgs);

            if (setting2.Checked) ToggleWindow();
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

        void RefreshStartButtons()
        {
            bool isAnyBtnVisible = false;
            for (int i = 0; i < 18; i++)
            {
                if (sds.ReadLine("Data", sdsIDUsernames + i) != "")
                {
                    startButtons[i].Visible = true;
                    isAnyBtnVisible = true;

                    startButtons[i].Text = sds.ReadLine("Data", sdsIDUsernames + i); // Set button name
                }
                else
                    startButtons[i].Visible = false;
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

    }
}
