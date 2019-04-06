using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Squirrel;
using SimpleLogger;

namespace SteamQuickSwitch
{
    public static class SquirrelHandler
    {
        private static readonly string repoURL = "https://github.com/ReckTy/Steam-Quick-Switch";

        private static Task<UpdateManager> mgr = null;

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
            // Wait for update to end
            await updateInProgress.ContinueWith(ex => { });

            // Dispose UpdateManager
            mgr.Result.Dispose();
        }

        static Task updateInProgress = Task.FromResult(true);
        private static async Task RealUpdateIfAvailable()
        {
            SimpleLog.Info("Checking remote server for update.");

            try
            {
                mgr = UpdateManager.GitHubUpdateManager(repoURL);
                await mgr.Result.UpdateApp();
            }
            catch (Exception ex)
            {
                SimpleLog.Error(ex.Message);
            }
        }
    }
}

