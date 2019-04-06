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

        private void textBoxEditUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                textBoxEditPassword.Focus();
                e.Handled = true;
            }
        }

        private void textBoxEditPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                buttonEditConfirm_Click(null, null);
                e.Handled = true;
            }
        }

        private void buttonEditCancel_Click(object sender, EventArgs e)
        {
            textBoxEditUsername.Text = "";
            textBoxEditPassword.Text = "";

            ChangePanel(1);
        }

        private void buttonEditConfirm_Click(object sender, EventArgs e)
        {
            if (textBoxEditUsername.Text != "" && textBoxEditPassword.Text != "")
            {
                sds.WriteLine("Data", sdsIDUsernames + latestSelectedLvi, textBoxEditUsername.Text);
                sds.WriteLine("Data", sdsIDPasswords + latestSelectedLvi, textBoxEditPassword.Text);
                buttonEditCancel_Click(null, null);
            }
            else
            {
                DialogResult result = MessageBox.Show("The account details have to contain both a name and a password.\nDo you want to delete the account from SQS?", "Steam Quick Switch", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    listViewLogins.Items[latestSelectedLvi].Remove();
                    buttonEditCancel_Click(null, null);
                }

            }
        }

    }
}
