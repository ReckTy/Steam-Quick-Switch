using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;

namespace SteamQuickSwitch
{
    public partial class Form1
    {
        bool fadeIP = false;
        bool fadeIn = false;
        
        System.Windows.Forms.Timer fadeTimer = new System.Windows.Forms.Timer() { Interval = 20 };

        static Thread animationThread;
        static int currentTimerTick = 1;
        static int tickAmount;

        static float totalX;
        static float totalY;

        static float AmountToMoveX, AmountToMoveY;

        static float currentPosX = Properties.Settings.Default.StartingPosX;
        static float currentPosY = Properties.Settings.Default.StartingPosY;

        static bool animationInProgess = false;
        static int animationSpeed = 3;

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        #region SQS-Hotkey

        private void hotkeyTimer_Tick(object sender, EventArgs e)
        {
            hotkeyTimer.Interval = 20;

            // ALT + Q Hotkey
            if ((GetAsyncKeyState(0x12) < 0 && GetAsyncKeyState(0x51) < 0)) // (0x12) = ALT & (0x51) = Q
            {
                List<string> exceptionProcesses = null;

                if (textBoxExceptionsList.Text != "")
                    exceptionProcesses = textBoxExceptionsList.Lines.ToList();
                else
                    exceptionProcesses = new List<string> { "skrrt" }; // <===(Random String)

                foreach (string _string in exceptionProcesses)
                {
                    if (Process.GetProcessesByName(_string).Length < 1)
                    {
                        ToggleWindow();

                        hotkeyTimer.Interval = 500;
                        break;
                    }
                }
            }
        }

        #endregion

        void MoveSQSToPos()
        {
            this.StartPosition = FormStartPosition.Manual;

            if (Properties.Settings.Default.CustomStartingPositions)
            {
                if (settingAnimStartingPos.Checked)
                {
                    currentPosX = Properties.Settings.Default.StartingPosX;
                    currentPosY = Properties.Settings.Default.StartingPosY;

                    int startX = Properties.Settings.Default.StartingPosX;
                    int startY = Properties.Settings.Default.StartingPosY;
                    int stopX = Properties.Settings.Default.AnimatePosX;
                    int stopY = Properties.Settings.Default.AnimatePosY;

                    totalX = stopX - startX;
                    totalY = stopY - startY;

                    currentTimerTick = 1;
                    animationSpeed = trackBarSettingAnimSpeed.Value;

                    float tickAmtX = Math.Abs(totalX);
                    float tickAmtY = Math.Abs(totalY);
                    
                    tickAmount = ((int)(tickAmtX > (int)(tickAmtY) ? (int)tickAmtX : (int)tickAmtY));
                    
                    if (tickAmtX > tickAmtY)
                    {
                        AmountToMoveX = 1;
                        AmountToMoveY = (totalY / totalX);

                        if ((int)totalX == 0)
                        {
                            AmountToMoveX = 0;
                            AmountToMoveY = (totalY / totalY);
                        }
                    }
                    else if (tickAmtY > tickAmtX)
                    {
                        AmountToMoveY = 1;
                        AmountToMoveX = (totalX / totalY);

                        if ((int)totalY == 0)
                        {
                            AmountToMoveY = 0;
                            AmountToMoveX = (totalX / totalX);
                        }
                    }
                    else
                    {
                        AmountToMoveY = 1;
                        AmountToMoveX = 1;
                    }

                    // Abort any existing animationThreads
                    if (animationInProgess) animationThread.Abort();

                    // Start new animationThread
                    animationThread = new Thread(() => Animate(this, new Point(Properties.Settings.Default.AnimatePosX, Properties.Settings.Default.AnimatePosY)));
                    animationThread.Start();
                    
                    return;
                }

                this.Location = new Point(Properties.Settings.Default.StartingPosX, Properties.Settings.Default.StartingPosY);
            }

            if (Properties.Settings.Default.FirstRun)
            {
                Properties.Settings.Default.StartingPosX = GetCenterPos().X;
                Properties.Settings.Default.StartingPosY = GetCenterPos().Y;
            }

            this.Location = new Point(Properties.Settings.Default.StartingPosX, Properties.Settings.Default.StartingPosY);
            FadeSQS(true);
        }

        static void Animate(Form _formToMove, Point _endPos)
        {
            animationInProgess = true;
            _formToMove.Location = new Point(Properties.Settings.Default.StartingPosX, Properties.Settings.Default.StartingPosY);
            _formToMove.Opacity = 1;
            
            while (currentTimerTick < tickAmount)
            {
                float movedX = (totalX < 0 && AmountToMoveX >= 0) ? (AmountToMoveX * currentTimerTick) * -1 : AmountToMoveX * currentTimerTick;
                float movedY = (totalY < 0 && AmountToMoveY >= 0) ? (AmountToMoveY * currentTimerTick) * -1 : AmountToMoveY * currentTimerTick;
                
                _formToMove.Location = new Point((int)currentPosX + (int)movedX, (int)currentPosY + (int)movedY);

                currentTimerTick += animationSpeed;

                Thread.Sleep(1);
            }
            _formToMove.Location = _endPos;

            animationInProgess = false;
            animationThread.Abort();
        }

        void ToggleWindow()
        {
            if (GetCurrentPanelIndex() == 5)
            {
                MessageBox.Show("You can't use the SQS-hotkey while editing hotkey-exceptions!", "Steam Quick Switch", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            if (this.Visible)
            {
                focusLabel.Focus();

                FadeSQS(false);
            }
            else
            {
                buttonHome_Click(null, null);

                this.Visible = true;
                this.Focus();
                this.BringToFront();

                MoveSQSToPos();
            }
        }

        void FadeSQS(bool _fadeIn)
        {
            if (fadeIP) return;

            fadeIP = true;
            fadeIn = _fadeIn;

            if (fadeIn) Opacity = 0;

            fadeTimer.Tick += new EventHandler(fadeTimer_Tick);
            fadeTimer.Enabled = true;
        }

        void fadeTimer_Tick(Object source, EventArgs e)
        {
            if ((fadeIn && Opacity < 1) || (!fadeIn && Opacity > 0))
            {
                Opacity += (fadeIn) ? 0.25f : -0.25f;
                WindowState = FormWindowState.Normal;
            }
            else
            {
                fadeIP = false;

                if (!fadeIn) this.Visible = false;

                fadeTimer.Enabled = false;
                fadeTimer.Tick -= new EventHandler(fadeTimer_Tick);
            }
        }

        Point GetCenterPos()
        {
            return new Point((Screen.PrimaryScreen.Bounds.Width - formSize.Width) / 2,
                (Screen.PrimaryScreen.Bounds.Height - formSize.Height) / 2);
        }
        
        bool CoordinateIsOutOfScreen(int X, int Y)
        {
            if (X < 0 || Y < 0 || X > (Screen.PrimaryScreen.Bounds.Width - formSize.Width) || Y > (Screen.PrimaryScreen.Bounds.Height - formSize.Height))
                return true;
            return false;
        }

    }
}
