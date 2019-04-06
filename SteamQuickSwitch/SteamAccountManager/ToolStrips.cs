using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamQuickSwitch
{
    public partial class MainForm
    {
        private void exitSQSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program.CloseApplicationPromt();
        }
    }
}
