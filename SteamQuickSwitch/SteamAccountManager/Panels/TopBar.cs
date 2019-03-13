using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamQuickSwitch
{
    public partial class Form1
    {

        #region Make window dragable

        bool TogMove;
        int MvalX, MvalY;

        private void panelTopBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !animationInProgess)
            {
                TogMove = true;
                MvalX = e.X;
                MvalY = e.Y;
            }
        }

        private void panelTopBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                TogMove = false;
        }

        private void panelTopBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (TogMove)
                this.Location = new Point(MousePosition.X - MvalX, MousePosition.Y - MvalY);
        }

        #endregion

        private void buttonHome_Click(object sender, EventArgs e)
        {
            ChangePanel(0);
            RefreshStartButtons();
        }

        private void buttonManage_Click(object sender, EventArgs e)
        {
            if (UsingManagerPassword()) // Login page
            {
                ChangePanel(3);
                textBoxManagerPasswordLogin.Focus();
            }
            else // Skip Login page
            {
                ChangePanel(1);
                textBoxUsername.Focus();
            }
        }

        private void buttonAltQ_Click(object sender, EventArgs e)
        {
            ToggleWindow();
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            ChangePanel(2);
        }
        
    }
}
