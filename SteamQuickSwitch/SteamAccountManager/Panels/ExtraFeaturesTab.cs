using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamQuickSwitch
{
    public partial class Form1
    {

        private void buttonExtraFeaturesBack_Click(object sender, EventArgs e)
        {
            ChangePanel(2); //panelSettings
        }

        private void buttonExtraFeaturesIEGameSettings_Click(object sender, EventArgs e)
        {
            ChangePanel(8); //panelIEGameSettings

            FillAccounts();
        }

    }
}
