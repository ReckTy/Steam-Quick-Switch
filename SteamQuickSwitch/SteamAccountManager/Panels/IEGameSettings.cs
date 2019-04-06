using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamQuickSwitch
{
    public partial class MainForm
    {
        private string[] steamNickname = new string[0];
        private string[] steamID3 = new string[0];

        private string[] steamAvailableGames = new string[0];
        private string[] steamAvailableGamesID = new string[0];

        private string importedSettingsSteamID3;
        private string importedSettingsAppID;

        private int selectedIAccount;
        private int selectedIGame;

        private bool loadingAccounts = false;
        private bool loadingAvailableGames = false;
        
        private void buttonIEUserdataBack_Click(object sender, EventArgs e)
        {
            ChangePanel(7); //panelExtraFeatures
        }

        private void FillAccounts()
        {
            if (backgroundWorkerFillAccounts.IsBusy) return;

            // Run BackgroundWorker
            backgroundWorkerFillAccounts.RunWorkerAsync();

            // Disable comboBoxes + buttons
            comboBoxIAccount.Enabled = false;
            comboBoxEAccount.Enabled = false;
            comboBoxIGame.Enabled = false;
            buttonImportGameSettings.Enabled = false;
            buttonExportGameSettings.Enabled = false;

            // Update comboBox
            comboBoxIAccount.Items.Clear();
            comboBoxEAccount.Items.Clear();
            comboBoxIGame.Items.Clear();
            comboBoxIAccount.Items.Add("Loading...");
            comboBoxEAccount.Items.Add("Loading...");
            comboBoxIAccount.SelectedIndex = 0;
            comboBoxEAccount.SelectedIndex = 0;

            // Update status-label
            labelIEGameSettingsStatus.Text = "Status: Loading saved userdata...";
        }

        private void comboBoxIAccount_SelectedValueChanged(object sender, EventArgs e)
        {
            if (loadingAccounts || loadingAvailableGames) return;

            // Clear current import
            ClearCurrentImport();

            // Set selectedIAccount
            selectedIAccount = comboBoxIAccount.SelectedIndex;

            // Disable comboBoxes + buttons
            comboBoxIAccount.Enabled = false;
            comboBoxIGame.Enabled = false;
            buttonImportGameSettings.Enabled = false;

            // Run BackgroundWorker
            backgroundWorkerFillGames.RunWorkerAsync();

            // Update comboBox
            comboBoxIGame.Items.Clear();
            comboBoxIGame.Items.Add("Loading...");
            comboBoxIGame.SelectedIndex = 0;
        }

        private void comboBoxIGame_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loadingAccounts || loadingAvailableGames) return;

            buttonImportGameSettings.Enabled = true;
        }

        private void buttonImportGameSettings_Click(object sender, EventArgs e)
        {
            selectedIGame = comboBoxIGame.SelectedIndex;

            // Update current import
            importedSettingsSteamID3 = steamID3[comboBoxIAccount.SelectedIndex];
            importedSettingsAppID = steamAvailableGamesID[comboBoxIGame.SelectedIndex];

            // Enable comboBoxes + buttons
            comboBoxEAccount.Enabled = true;
            buttonExportGameSettings.Enabled = true;

            // Update status-label
            labelIEGameSettingsStatus.Text = "Status: [" + steamNickname[comboBoxIAccount.SelectedIndex] + "/" + steamAvailableGames[selectedIGame] + "] Imported.";
        }

        private void buttonExportGameSettings_Click(object sender, EventArgs e)
        {
            // Safety-checks
            if (steamID3[comboBoxEAccount.SelectedIndex] == importedSettingsSteamID3)
            {
                MessageBox.Show("You can't export settings to the user you imported from!","Steam Quick Switch",MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            // Export settings
            DirectoryCopy(Path.Combine(Properties.Settings.Default.SteamPath, "userdata", importedSettingsSteamID3, importedSettingsAppID),
                Path.Combine(Properties.Settings.Default.SteamPath, "userdata", steamID3[comboBoxEAccount.SelectedIndex], importedSettingsAppID), true);

            // Clear current import
            ClearCurrentImport();

            // Enable comboBoxes + buttons
            comboBoxEAccount.Enabled = false;
            buttonExportGameSettings.Enabled = false;

            // Update status-label
            labelIEGameSettingsStatus.Text = "Status: Exported settings to [" + steamNickname[comboBoxEAccount.SelectedIndex] + "/" + steamAvailableGames[selectedIGame] + "],";
        }

        private void ClearCurrentImport()
        {
            importedSettingsAppID = null;
            importedSettingsSteamID3 = null;
        }

        private void DirectoryCopy(string sourceDirName, string destDirName, bool deletePreviousDestDir = false)
        {
            if (deletePreviousDestDir && Directory.Exists(destDirName))
                Directory.Delete(destDirName, true);

            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            foreach (FileInfo file in dir.GetFiles())
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            foreach (DirectoryInfo subDir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subDir.Name);
                DirectoryCopy(subDir.FullName, tempPath);
            }
        }

        #region backgroundWorkerFillAccounts
        private void backgroundWorkerFillAccounts_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            loadingAccounts = true;

            var dir = Directory.EnumerateDirectories(Properties.Settings.Default.SteamPath + "\\userdata\\");

            int fileCount = dir.Count();

            // Reset arrays
            steamNickname = new string[0];
            steamID3 = new string[0];

            foreach (string path in dir)
            {
                try
                {
                    string[] pathSplit = path.Split('\\');

                    string tempSteamID3 = pathSplit[pathSplit.Length - 1];
                    string tempNickname = SteamAPI.GetNicknameFromSteamID(tempSteamID3);

                    // Resize arrays
                    Array.Resize(ref steamNickname, steamNickname.Length + 1);
                    Array.Resize(ref steamID3, steamID3.Length + 1);
                    
                    // Add user-info to arrays
                    steamNickname[steamNickname.Length - 1] = tempNickname;
                    steamID3[steamID3.Length - 1] = tempSteamID3;
                }
                catch { }
            }
        }

        private void backgroundWorkerFillAccounts_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (steamNickname.Length == 0)
            {
                MessageBox.Show("No userdata could be found.\n" +
                    "Please make sure the Steam-path in Settings is correct.", "Steam Quick Switch", 
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

                // Update status-label
                labelIEGameSettingsStatus.Text = "Status: No userdata found.";

                loadingAccounts = false;
                return;
            }

            // Enable comboBoxes + buttons
            comboBoxIAccount.Enabled = true;
            buttonImportGameSettings.Enabled = false;

            // Update comboBoxes
            comboBoxIAccount.Items.Clear();
            comboBoxEAccount.Items.Clear();
            comboBoxIAccount.Items.AddRange(steamNickname);
            comboBoxEAccount.Items.AddRange(steamNickname);

            // Update status-label
            labelIEGameSettingsStatus.Text = "Status: Userdata loaded.";

            loadingAccounts = false;
        }
        #endregion

        #region backgroundWorkerFillGames
        private void backgroundWorkerFillGames_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            loadingAvailableGames = true;

            var dir = Directory.EnumerateDirectories(Properties.Settings.Default.SteamPath + "\\userdata\\" + steamID3[selectedIAccount] + "\\");

            int fileCount = dir.Count();

            // Reset arrays
            Array.Resize(ref steamAvailableGames, 0);
            Array.Resize(ref steamAvailableGamesID, 0);

            steamAvailableGames = new string[0];
            steamAvailableGamesID = new string[0];

            foreach (string path in dir)
            {
                try
                {
                    string[] pathSplit = path.Split('\\');

                    string tempGameID = pathSplit[pathSplit.Length - 1];
                    string tempGameName = SteamAPI.GetGameNameFromID(tempGameID);

                    // Resize arrays
                    Array.Resize(ref steamAvailableGames, steamAvailableGames.Length + 1);
                    Array.Resize(ref steamAvailableGamesID, steamAvailableGamesID.Length + 1);

                    // Add user-info to arrays
                    steamAvailableGames[steamAvailableGames.Length - 1] = tempGameName;
                    steamAvailableGamesID[steamAvailableGamesID.Length - 1] = tempGameID;
                }
                catch { }
            }
        }

        private void backgroundWorkerFillGames_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (steamAvailableGames.Length == 0)
            {
                MessageBox.Show("No saved game-settings could be found for this account.", "Steam Quick Switch", 
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

                // Update comboBoxes
                comboBoxIAccount.Enabled = true;
                comboBoxIGame.Items.Clear();

                // Update status-label
                labelIEGameSettingsStatus.Text = "Status: No saved game-settings found.";

                loadingAvailableGames = false;
                return;
            }

            // Update comboBoxes
            comboBoxIAccount.Enabled = true;
            comboBoxIGame.Enabled = true;
            comboBoxIGame.Items.Clear();
            comboBoxIGame.Items.AddRange(steamAvailableGames);
            
            // Update status-label
            labelIEGameSettingsStatus.Text = "Status: Loaded saved game-settings for [" + steamNickname[selectedIAccount] + "]";

            loadingAvailableGames = false;
        }
        #endregion
        
    }
}