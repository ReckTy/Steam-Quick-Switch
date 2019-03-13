using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamQuickSwitch
{
    public partial class Form1
    {

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

    }
}
