/*  Copyright 2010 Geoffrey 'Phogue' Green

    http://www.phogue.net
 
    This file is part of PRoCon Frostbite.

    PRoCon Frostbite is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PRoCon Frostbite is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.
 */

using PRoCon.Core;
using PRoCon.Core.Remote;
using PRoCon.Forms;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PRoCon.Controls
{
    public partial class uscConsolePanel : UserControl
    {

        private frmMain m_frmMainWindow;
        private uscServerConnection m_uscParent;

        private CLocalization m_clocLanguage;

        private PRoConClient m_prcClient;

        //private Dictionary<string, Color> m_dicConsoleTextColours;

        private LinkedList<string> m_llCommandsHistory;
        private LinkedListNode<string> m_llCommandsHistoryCurrentNode;

        private LinkedList<string> m_llPunkbusterCommandsHistory;
        private LinkedListNode<string> m_llPunkbusterCommandsHistoryCurrentNode;

        public delegate void SendCommandDelegate(string strCommand);
        public event SendCommandDelegate SendCommand;

        public delegate void SendListCommandDelegate(List<string> lstCommand);
        public event SendListCommandDelegate SendListCommand;

        //public delegate void DebugDelegate(bool blDebugEnabled);
        //public event DebugDelegate EnableConsoleDebug;

        private Regex m_regRemoveCaretCodes;

        public uscConsolePanel()
        {
            InitializeComponent();

            this.m_clocLanguage = null;

            this.m_llCommandsHistory = new LinkedList<string>();
            this.m_llPunkbusterCommandsHistory = new LinkedList<string>();

            this.m_regRemoveCaretCodes = new Regex(@"\^[0-9]|\^b|\^i|\^n", RegexOptions.Compiled);

            this.rtbConsoleBox.Flushed += new Action<object, EventArgs>(rtbConsoleBox_Flushed);
            this.rtbPunkbusterBox.Flushed += new Action<object, EventArgs>(rtbPunkbusterBox_Flushed);
        }

        private void uscConsolePanel_Load(object sender, EventArgs e)
        {
            if (this.m_prcClient != null && this.m_prcClient.Console != null)
            {
                this.m_prcClient.Console.DisplayConnection = this.m_prcClient.Console.DisplayConnection;
                this.m_prcClient.Console.DisplayPunkbuster = this.m_prcClient.Console.DisplayPunkbuster;
                this.m_prcClient.Console.LogDebugDetails = this.m_prcClient.Console.LogDebugDetails;
                this.m_prcClient.Console.LogEventsConnection = this.m_prcClient.Console.LogEventsConnection;
                this.m_prcClient.Console.ConScrolling = this.m_prcClient.Console.ConScrolling;
                this.m_prcClient.Console.PBScrolling = this.m_prcClient.Console.PBScrolling;
            }
        }

        public void Initialize(frmMain frmMainWindow, uscServerConnection uscParent)
        {
            this.m_uscParent = uscParent;
            this.m_frmMainWindow = frmMainWindow;

            this.tbcConsoles.ImageList = this.m_frmMainWindow.iglIcons;
            this.tabConsole.ImageKey = "application_xp_terminal.png";
            this.tabPunkbuster.ImageKey = "punkbuster.png";

        }

        public void SetConnection(PRoConClient prcClient)
        {
            if ((this.m_prcClient = prcClient) != null)
            {
                if (this.m_prcClient.Game != null)
                {
                    this.m_prcClient_GameTypeDiscovered(prcClient);
                }
                else
                {
                    this.m_prcClient.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(m_prcClient_GameTypeDiscovered);
                }
            }
        }

        private void m_prcClient_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                this.m_prcClient.Console.WriteConsole += new PRoCon.Core.Logging.Loggable.WriteConsoleHandler(Console_WriteConsole);
                this.m_prcClient.Console.LogEventsConnectionChanged += new PRoCon.Core.Consoles.ConnectionConsole.IsEnabledHandler(Console_LogEventsConnectionChanged);
                this.m_prcClient.Console.DisplayConnectionChanged += new PRoCon.Core.Consoles.ConnectionConsole.IsEnabledHandler(Console_DisplayConnectionChanged);
                this.m_prcClient.Console.DisplayPunkbusterChanged += new PRoCon.Core.Consoles.ConnectionConsole.IsEnabledHandler(Console_DisplayPunkbusterChanged);

                this.m_prcClient.Console.LogDebugDetailsChanged += new PRoCon.Core.Consoles.ConnectionConsole.IsEnabledHandler(Console_LogDebugDetailsChanged);

                this.m_prcClient.PunkbusterConsole.WriteConsole += new PRoCon.Core.Logging.Loggable.WriteConsoleHandler(PunkbusterConsole_WriteConsole);

                this.m_prcClient.Console.ConScrollingChanged += new PRoCon.Core.Consoles.ConnectionConsole.IsEnabledHandler(Console_ConEnableScrollingChanged);
                this.m_prcClient.Console.PBScrollingChanged += new PRoCon.Core.Consoles.ConnectionConsole.IsEnabledHandler(Console_PBEnableScrollingChanged);

                this.m_prcClient.Game.Help += new FrostbiteClient.HelpHandler(Game_Help);
            });
        }

        void Game_Help(FrostbiteClient sender, List<string> lstCommands)
        {
            this.InvokeIfRequired(() =>
            {
                this.txtConsoleCommand.AutoCompleteCustomSource.Clear();
                this.txtConsoleCommand.AutoCompleteCustomSource.AddRange(lstCommands.ToArray());
            });
        }

        public void SetColour(string strVariable, string strValue)
        {
            this.rtbConsoleBox.SetColour(strVariable, strValue);
        }

        public void SetLocalization(CLocalization clocLanguage)
        {
            this.m_clocLanguage = clocLanguage;

            this.tabConsole.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.tabConsole", null);
            this.btnConsoleSend.Text = this.m_clocLanguage.GetLocalized("uscConsolePanel.btnConsoleSend", null);
            this.chkEnableOutput.Text = this.m_clocLanguage.GetLocalized("uscConsolePanel.chkEnableOutput", null);
            this.chkEvents.Text = this.m_clocLanguage.GetLocalized("uscConsolePanel.chkEvents", null);
            this.chkDebug.Text = this.m_clocLanguage.GetLocalized("uscConsolePanel.chkDebug", null);

            this.btnPunkbusterSend.Text = this.m_clocLanguage.GetLocalized("uscConsolePanel.btnConsoleSend", null);
            this.chkEnablePunkbusterOutput.Text = this.m_clocLanguage.GetLocalized("uscConsolePanel.chkEnableOutput", null);
            this.chkConEnableScrolling.Text = this.m_clocLanguage.GetLocalized("uscChatPanel.chkDisplayScrolling", null);
            this.chkPBEnableScrolling.Text = this.m_clocLanguage.GetLocalized("uscChatPanel.chkDisplayScrolling", null);
        }

        public event uscServerConnection.OnTabChangeDelegate OnTabChange;

        private void tbcConsoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.OnTabChange != null)
            {
                Stack<string> stkTabIndexes = new Stack<string>();
                stkTabIndexes.Push(tbcConsoles.SelectedTab.Name);

                this.OnTabChange(this, stkTabIndexes);
            }
        }

        public void SetTabIndexes(Stack<string> stkTabIndexes)
        {
            if (tbcConsoles.TabPages.ContainsKey(stkTabIndexes.Peek()) == true)
            {
                this.tbcConsoles.SelectedTab = tbcConsoles.TabPages[stkTabIndexes.Pop()];
            }
        }

        private void Console_WriteConsole(DateTime dtLoggedTime, string strLoggedText)
        {
            if (this.chkEnableOutput.Checked == true)
            {
                this.rtbConsoleBox.AppendText(String.Format("[{0}] {1}{2}", dtLoggedTime.ToString("HH:mm:ss"), strLoggedText, "\n"));
            }
        }

        private void rtbConsoleBox_Flushed(object sender, EventArgs args)
        {
            if (this.rtbConsoleBox.Focused == false)
            {
                if (this.m_prcClient.Console.ConScrolling == true)
                {
                    this.rtbConsoleBox.ScrollToCaret();
                }
            }

            this.rtbConsoleBox.TrimLines(this.m_prcClient.Variables.GetVariable<int>("MAX_CONSOLE_LINES", 75));
        }

        private void PunkbusterConsole_WriteConsole(DateTime dtLoggedTime, string strLoggedText)
        {
            if (this.chkEnablePunkbusterOutput.Checked == true)
            {
                this.rtbPunkbusterBox.AppendText(String.Format("{0}{1}", strLoggedText, "\n"));
            }
        }

        private void rtbPunkbusterBox_Flushed(object sender, EventArgs args)
        {
            if (this.rtbPunkbusterBox.Focused == false)
            {
                if (this.m_prcClient.Console.PBScrolling == true)
                {
                    this.rtbPunkbusterBox.ScrollToCaret();
                }
            }

            this.rtbPunkbusterBox.TrimLines(this.m_prcClient.Variables.GetVariable<int>("MAX_PUNKBUSTERCONSOLE_LINES", 200));
        }

        private void btnPunkbusterSend_Click(object sender, EventArgs e)
        {
            if (this.SendListCommand != null)
            {
                this.m_llPunkbusterCommandsHistory.AddFirst(this.txtPunkbusterCommand.Text);
                if (this.m_llPunkbusterCommandsHistory.Count > 20)
                {
                    this.m_llPunkbusterCommandsHistory.RemoveLast();
                }
                this.m_llPunkbusterCommandsHistoryCurrentNode = null;

                this.SendListCommand(new List<string>() { "punkBuster.pb_sv_command", this.txtPunkbusterCommand.Text });

                this.txtPunkbusterCommand.Clear();
                this.txtPunkbusterCommand.Focus();
            }
        }

        private void txtPunkbusterCommand_KeyDown(object sender, KeyEventArgs e)
        {

            if (this.SendListCommand != null && e.KeyData == Keys.Enter)
            {

                this.SendListCommand(new List<string>() { "punkBuster.pb_sv_command", this.txtPunkbusterCommand.Text });

                this.m_llPunkbusterCommandsHistory.AddFirst(this.txtPunkbusterCommand.Text);
                if (this.m_llPunkbusterCommandsHistory.Count > 20)
                {
                    this.m_llPunkbusterCommandsHistory.RemoveLast();
                }
                this.m_llPunkbusterCommandsHistoryCurrentNode = null;

                this.txtPunkbusterCommand.Clear();
                this.txtPunkbusterCommand.Focus();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == Keys.Up)
            {
                e.SuppressKeyPress = true;

                if (this.m_llPunkbusterCommandsHistoryCurrentNode == null && this.m_llPunkbusterCommandsHistory.First != null)
                {
                    this.m_llPunkbusterCommandsHistoryCurrentNode = this.m_llPunkbusterCommandsHistory.First;
                    this.txtPunkbusterCommand.Text = this.m_llPunkbusterCommandsHistoryCurrentNode.Value;

                    this.txtPunkbusterCommand.Select(this.txtPunkbusterCommand.Text.Length, 0);
                }
                else if (this.m_llPunkbusterCommandsHistoryCurrentNode != null && this.m_llPunkbusterCommandsHistoryCurrentNode.Next != null)
                {
                    this.m_llPunkbusterCommandsHistoryCurrentNode = this.m_llPunkbusterCommandsHistoryCurrentNode.Next;
                    this.txtPunkbusterCommand.Text = this.m_llPunkbusterCommandsHistoryCurrentNode.Value;

                    this.txtPunkbusterCommand.Select(this.txtConsoleCommand.Text.Length, 0);
                }
            }
            else if (e.KeyData == Keys.Down)
            {

                if (this.m_llPunkbusterCommandsHistoryCurrentNode != null && this.m_llPunkbusterCommandsHistoryCurrentNode.Previous != null)
                {
                    this.m_llPunkbusterCommandsHistoryCurrentNode = this.m_llPunkbusterCommandsHistoryCurrentNode.Previous;
                    this.txtPunkbusterCommand.Text = this.m_llPunkbusterCommandsHistoryCurrentNode.Value;

                    this.txtPunkbusterCommand.Select(this.txtPunkbusterCommand.Text.Length, 0);
                }

                e.SuppressKeyPress = true;
            }

        }


        private void btnConsoleSend_Click(object sender, EventArgs e)
        {
            if (this.SendCommand != null)
            {
                this.m_llCommandsHistory.AddFirst(this.txtConsoleCommand.Text);
                if (this.m_llCommandsHistory.Count > 20)
                {
                    this.m_llCommandsHistory.RemoveLast();
                }
                this.m_llCommandsHistoryCurrentNode = null;

                this.SendCommand(this.txtConsoleCommand.Text);

                this.txtConsoleCommand.Clear();
                this.txtConsoleCommand.Focus();
            }
        }

        private void txtConsoleCommand_KeyDown(object sender, KeyEventArgs e)
        {

            if (this.SendCommand != null && e.KeyData == Keys.Enter)
            {

                this.SendCommand(this.txtConsoleCommand.Text);

                this.m_llCommandsHistory.AddFirst(this.txtConsoleCommand.Text);
                if (this.m_llCommandsHistory.Count > 20)
                {
                    this.m_llCommandsHistory.RemoveLast();
                }
                this.m_llCommandsHistoryCurrentNode = null;

                this.txtConsoleCommand.Clear();
                this.txtConsoleCommand.Focus();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == Keys.Up)
            {
                e.SuppressKeyPress = true;

                if (this.m_llCommandsHistoryCurrentNode == null && this.m_llCommandsHistory.First != null)
                {
                    this.m_llCommandsHistoryCurrentNode = this.m_llCommandsHistory.First;
                    this.txtConsoleCommand.Text = this.m_llCommandsHistoryCurrentNode.Value;

                    this.txtConsoleCommand.Select(this.txtConsoleCommand.Text.Length, 0);
                }
                else if (this.m_llCommandsHistoryCurrentNode != null && this.m_llCommandsHistoryCurrentNode.Next != null)
                {
                    this.m_llCommandsHistoryCurrentNode = this.m_llCommandsHistoryCurrentNode.Next;
                    this.txtConsoleCommand.Text = this.m_llCommandsHistoryCurrentNode.Value;

                    this.txtConsoleCommand.Select(this.txtConsoleCommand.Text.Length, 0);
                }
            }
            else if (e.KeyData == Keys.Down)
            {

                if (this.m_llCommandsHistoryCurrentNode != null && this.m_llCommandsHistoryCurrentNode.Previous != null)
                {
                    this.m_llCommandsHistoryCurrentNode = this.m_llCommandsHistoryCurrentNode.Previous;
                    this.txtConsoleCommand.Text = this.m_llCommandsHistoryCurrentNode.Value;

                    this.txtConsoleCommand.Select(this.txtConsoleCommand.Text.Length, 0);
                }

                e.SuppressKeyPress = true;
            }
        }

        private void chkEnableOutput_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.m_prcClient.Console.DisplayConnection = this.chkEnableOutput.Checked;
            }
        }

        private void Console_DisplayConnectionChanged(bool isEnabled)
        {
            this.InvokeIfRequired(() => { this.chkEnableOutput.Checked = isEnabled; });
        }


        private void chkEnablePunkbusterOutput_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.m_prcClient.Console.DisplayPunkbuster = this.chkEnablePunkbusterOutput.Checked;
            }
        }

        void Console_DisplayPunkbusterChanged(bool isEnabled)
        {
            this.InvokeIfRequired(() => { this.chkEnablePunkbusterOutput.Checked = isEnabled; });
        }

        private void chkConEnableScrolling_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.m_prcClient.Console.ConScrolling = this.chkConEnableScrolling.Checked;
            }
        }

        void Console_ConEnableScrollingChanged(bool isEnabled)
        {
            this.InvokeIfRequired(() => { this.chkConEnableScrolling.Checked = isEnabled; });
        }

        private void chkPBEnableScrolling_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.m_prcClient.Console.PBScrolling = this.chkPBEnableScrolling.Checked;
            }
        }

        void Console_PBEnableScrollingChanged(bool isEnabled)
        {
            this.InvokeIfRequired(() => { this.chkPBEnableScrolling.Checked = isEnabled; });
        }

        private void chkDebug_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.m_prcClient.Console.LogDebugDetails = this.chkDebug.Checked;
            }
        }

        private void Console_LogDebugDetailsChanged(bool isEnabled)
        {
            this.InvokeIfRequired(() => { this.chkDebug.Checked = isEnabled; });
        }

        private void chkEvents_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.m_prcClient.Console.LogEventsConnection = this.chkEvents.Checked;
            }
        }

        private void Console_LogEventsConnectionChanged(bool isEnabled)
        {
            this.InvokeIfRequired(() => { this.chkEvents.Checked = isEnabled; });
        }
    }
}
