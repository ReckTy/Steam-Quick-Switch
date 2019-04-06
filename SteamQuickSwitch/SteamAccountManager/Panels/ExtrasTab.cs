using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamQuickSwitch
{
    public partial class MainForm
    {

        private void buttonDonate_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.paypal.me/MattiasAldhagen");
            ToggleWindow();
        }

        private void buttonExtrasBack_Click(object sender, EventArgs e)
        {
            ChangePanel(2); // panelSettings
        }

    }
}
