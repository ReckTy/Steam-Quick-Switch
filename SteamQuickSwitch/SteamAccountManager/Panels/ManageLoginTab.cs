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

        private void textBoxManagerPasswordLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                buttonManagerLogin_Click(null, null);
                e.Handled = true;
            }
        }

        private void buttonManagerLogin_Click(object sender, EventArgs e)
        {
            if (textBoxManagerPasswordLogin.Text == sds.ReadLine("Data", sdsIDManagerPassword))
            {
                ChangePanel(1);
                textBoxUsername.Focus();
            }
            textBoxManagerPasswordLogin.Text = "";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            focusLabel.Focus();

            DialogResult result = MessageBox.Show("You can reset your password in the settings tab." + "\n" + "Do you want to change it?",
                "Steam Quick Switch",
                MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);

            if (result == DialogResult.Yes)
            {
                ChangePanel(2);
                textBoxManagerPasswordOld.Focus();
            }
        }
        
    }
}
