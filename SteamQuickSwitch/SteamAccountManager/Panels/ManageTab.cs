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
        int latestSelectedLvi;

        private void textBoxUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBoxPassword.Focus();
                e.SuppressKeyPress = true;
            }
        }

        private void textBoxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonAddLogin_Click(null, null);
                e.SuppressKeyPress = true;
            }
        }

        private void buttonAddLogin_Click(object sender, EventArgs e)
        {
            if (listViewLogins.Items.Count < 18)
            {
                if (textBoxUsername.Text != "" && textBoxPassword.Text != "")
                {
                    ListViewItem lvi = new ListViewItem(textBoxUsername.Text);
                    lvi.SubItems.Add(textBoxPassword.Text);
                    listViewLogins.Items.Add(lvi);
                    textBoxUsername.Clear();
                    textBoxPassword.Clear();

                    textBoxUsername.Focus();
                }
            }
            else
                MessageBox.Show("You already have the maximum amount of accounts inserted."
                    , "Steam Quick Switch", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void buttonEditSelectedItem_Click(object sender, EventArgs e)
        {
            int selectedItemCount = GetSelectedItemCount()[0];
            int selectedItemIndex = GetSelectedItemCount()[1];

            if (selectedItemCount == 1)
            {
                ChangePanel(4);
                textBoxEditUsername.Text = sds.ReadLine("Data", sdsIDUsernames + selectedItemIndex);
                textBoxEditPassword.Text = sds.ReadLine("Data", sdsIDPasswords + selectedItemIndex);

                latestSelectedLvi = selectedItemIndex;
            }
            else if (selectedItemCount > 1)
            {
                MessageBox.Show("You can't edit multiple items at once.", "Steam Quick Switch", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                ShowMsgBoxNoItemSelected();
            }
        }

        private void buttonRemoveSelectedItems_Click(object sender, EventArgs e)
        {
            int selectedItemCount = GetSelectedItemCount()[0];
            if (selectedItemCount == 0)
            {
                ShowMsgBoxNoItemSelected();
                return;
            }

            if (MessageBox.Show("Are you sure you want to remove the selected items?", "Steam Quick Switch", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (ListViewItem lvi in listViewLogins.Items)
                {
                    if (lvi.Checked) lvi.Remove();
                }
            }
        }
        
        #region listViewLogins

        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int selectedItemCount = GetSelectedItemCount()[0];
            int selectedItemID = GetSelectedItemCount()[1];
            
            if (selectedItemCount == 0)
            {
                ShowMsgBoxNoItemSelected();
                return;
            }
            else if (selectedItemCount > 1)
            {
                ShowMsgBoxCantMoveMultipleItems();
                return;
            }

            ListViewItem lvi = new ListViewItem(listViewLogins.Items[selectedItemID].Text);
            lvi.SubItems.Add(listViewLogins.Items[selectedItemID].SubItems[1].Text);
            lvi.Checked = true;

            listViewLogins.Items[selectedItemID].Remove();
            listViewLogins.Items.Insert(selectedItemID - 1, lvi);
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int selectedItemCount = GetSelectedItemCount()[0];
            int selectedItemID = GetSelectedItemCount()[1];

            if (selectedItemCount == 0)
            {
                ShowMsgBoxNoItemSelected();
                return;
            }
            else if (selectedItemCount > 1)
            {
                ShowMsgBoxCantMoveMultipleItems();
                return;
            }

            ListViewItem lvi = new ListViewItem(listViewLogins.Items[selectedItemID].Text);
            lvi.SubItems.Add(listViewLogins.Items[selectedItemID].SubItems[1].Text);
            lvi.Checked = true;

            listViewLogins.Items[selectedItemID].Remove();
            listViewLogins.Items.Insert(selectedItemID + 1, lvi);

        }

        private void removeAllItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove all items?", "Steam Quick Switch", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (listViewLogins.Items.Count != 0)
                {
                    foreach (ListViewItem lvi in listViewLogins.Items)
                    {
                        lvi.Remove();
                    }
                }
            }
        }

        #endregion
        


        int[] GetSelectedItemCount()
        {
            int selectedItemCount = 0, selectedItemIndex = 0;

            foreach (ListViewItem lvi in listViewLogins.Items)
            {
                if (lvi.Checked)
                {
                    selectedItemCount++;
                    selectedItemIndex = lvi.Index;
                }
            }
            return new int[2] { selectedItemCount, selectedItemIndex };
        }

        private void ShowMsgBoxNoItemSelected()
        {
            MessageBox.Show("No item selected.", "Steam Quick Switch", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        void ShowMsgBoxCantMoveMultipleItems()
        {
            MessageBox.Show("Can't move multiple items at once.", "Steam Quick Switch", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }


    }
}
