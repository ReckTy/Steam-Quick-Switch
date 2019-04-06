using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using SimpleLogger;
using System.Diagnostics;

namespace SteamQuickSwitch
{
    static class Program
    {
        static Form mainForm = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Set SimpleLog properties
            SimpleLog.SetLogFile(logDir: ".\\Log", prefix: "SQS-Log_", writeText: true);
            
            // Close identical apps
            CloseIdenticalApps();

            // Upgrade settings if needed
            if (Properties.Settings.Default.UpgradeSettings)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeSettings = false;

                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();
            }

            // Display update notification
            DisplayUpdateNotification();

            // Add methods to events
            Application.ApplicationExit += Application_ApplicationExit;

            // Check for available updates
            SquirrelHandler.CheckForUpdatesAsync();

            // Start Application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            mainForm = new MainForm();
            Application.Run(mainForm);
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

        async static public void CloseApplicationPromt()
        {
            if (mainForm == null) return;

            // Promt user
            if (MessageBox.Show("Are you sure you want to quit SQS?",
                "Steam Quick Switch", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            // Close visuals
            mainForm.Hide();

            // Await Squirrel
            await SquirrelHandler.WaitForUpdatesOnShutdown();

            // Exit Application
            Application.Exit();

        }
        
        private static void CloseIdenticalApps()
        {
            // Assuming executable name is "SQS"
            Process[] procArray = Process.GetProcessesByName("SQS");
            int thisProcessID = Process.GetCurrentProcess().Id;

            foreach (Process proc in procArray)
            {
                try
                {
                    if (proc.Id != thisProcessID) proc.Kill();
                }
                catch (Exception ex)
                {
                    SimpleLog.Error(ex.Message);
                }
            }
        }

        public static void DisplayUpdateNotification()
        {
            string currentAppVersion = GetVersionInfo().FileVersion;

            // (If previous version is assigned, but not current version)
            if (Properties.Settings.Default.PrevVersion != "" && Properties.Settings.Default.PrevVersion != currentAppVersion)
            {
                MessageBox.Show($"Steam Quick Switch has been updated to v.{ currentAppVersion }!\nVisit the GitHub repository for further information.", 
                    $"Steam Quick Switch v.{ currentAppVersion }", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Set 'PrevVersion' to the current
            Properties.Settings.Default.PrevVersion = currentAppVersion;
            
            // Save settings
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

        public static FileVersionInfo GetVersionInfo()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SteamQuickSwitch.SensitiveDataStorage.dll"))
            {
                byte[] assemblyData = new byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        }

    }
}
