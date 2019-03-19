using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Squirrel;

namespace SteamQuickSwitch
{
    public static class SquirrelHandler
    {
        private static readonly string repoURL = "https://github.com/ReckTy/Steam-Quick-Switch";

        public static async void CheckForUpdatesAsync()
        {
            await Task.Run(() => UpdateIfAvailable());
        }

        public static async Task UpdateIfAvailable()
        {
            updateInProgress = RealUpdateIfAvailable();
            await updateInProgress;
        }

        public static async Task WaitForUpdatesOnShutdown()
        {
            await updateInProgress.ContinueWith(ex => { });
        }

        static Task updateInProgress = Task.FromResult(true);
        private static async Task RealUpdateIfAvailable()
        {
            //lastUpdateCheck = DateTime.Now;
            //_logger.Debug("Checking remote server for update.");
            try
            {
                using (var mgr = UpdateManager.GitHubUpdateManager(repoURL))
                {
                    var updateInfo = await mgr.Result.CheckForUpdate();
                    
                    if (updateInfo.ReleasesToApply.Any())
                    {
                        await mgr.Result.UpdateApp();

                        string msg = new StringBuilder().AppendLine("Steam Quick Switch has updated!").
                            AppendLine("Changes will not take affect until SQS is restarted.").ToString();

                        MessageBox.Show(msg, "Steam Quick Switch - Update", MessageBoxButtons.OK,
                            MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                    }

                    mgr.Result.Dispose();
                }
            }
            catch /*(Exception ex)*/
            {
                //_logger.Debug(ex.Message);
            }
        }
        
    }
}
