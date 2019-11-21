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
using PRoCon.Core.Players;
using PRoCon.Core.Remote;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PRoCon.Controls
{
    public partial class uscChatPanel : uscPage
    {

        private uscServerConnection m_uscParent;
        private CLocalization m_clocLanguage;

        private PRoConClient m_prcClient;

        //private FileStream m_stmChatFile;
        //private StreamWriter m_stwChatFile;

        //private Dictionary<string, Color> m_dicChatTextColours;

        private LinkedList<string> m_llChatHistory;
        private LinkedListNode<string> m_llChatHistoryCurrentNode;

        //public delegate void SendCommandDelegate(List<string> lstCommand);
        //public event SendCommandDelegate SendCommand;

        string m_strAllPlayers = "All Players";

        private int m_iChat_MaxLength = 100;
        private int m_iCanLongMsg = 0;
        private int m_iYellDuration = 0;

        Regex m_regRemoveCaretCodes;


        private void SendCommand(List<string> lstWords)
        {
            if (lstWords.Count > 0)
            {
                if (String.Compare(lstWords[0], "admin.yell", true) == 0)
                {
                    if (lstWords.Count >= 5)
                    {
                        this.m_prcClient.SendProconAdminYell(lstWords[1], lstWords[2], lstWords[3], lstWords[4]);
                    }
                    else
                    {
                        this.m_prcClient.SendProconAdminYell(lstWords[1], lstWords[2], lstWords[3], String.Empty);
                    }
                }
                else if (String.Compare(lstWords[0], "admin.say", true) == 0)
                {
                    if (lstWords.Count >= 4)
                    {
                        this.m_prcClient.SendProconAdminSay(lstWords[1], lstWords[2], lstWords[3]);
                    }
                    else
                    {
                        this.m_prcClient.SendProconAdminSay(lstWords[1], lstWords[2], String.Empty);
                    }
                }
            }
        }


        public uscChatPanel()
        {
            InitializeComponent();

            //this.m_stmChatFile = null;
            //this.m_stwChatFile = null;
            //this.m_blChatLogging = false;
            this.m_clocLanguage = null;

            this.m_llChatHistory = new LinkedList<string>();

            this.cboDisplayChatTime.Items.Clear();
            this.cboDisplayChatTime.Items.Add(2000);
            this.cboDisplayChatTime.Items.Add(4000);
            this.cboDisplayChatTime.Items.Add(6000);
            this.cboDisplayChatTime.Items.Add(8000);
            this.cboDisplayChatTime.Items.Add(10000);
            this.cboDisplayChatTime.Items.Add(15000);
            this.cboDisplayChatTime.Items.Add(20000);
            this.cboDisplayChatTime.Items.Add(25000);
            this.cboDisplayChatTime.Items.Add(30000);
            this.cboDisplayChatTime.Items.Add(35000);
            this.cboDisplayChatTime.Items.Add(40000);
            this.cboDisplayChatTime.Items.Add(45000);
            this.cboDisplayChatTime.Items.Add(50000);
            this.cboDisplayChatTime.Items.Add(55000);
            this.cboDisplayChatTime.Items.Add(59999);

            this.cboDisplayChatTime.SelectedIndex = 4;
            this.cboDisplayList.SelectedIndex = 0;

            this.rtbChatBox.Flushed += new Action<object, EventArgs>(rtbChatBox_Flushed);

            this.m_regRemoveCaretCodes = new Regex(@"\^[0-9]|\^b|\^i|\^n", RegexOptions.Compiled);
        }

        private void uscChatPanel_Load(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {

                // Setting fires events which neatens the code a little elsewhere.
                this.m_prcClient.ChatConsole.LogJoinLeaving = this.m_prcClient.ChatConsole.LogJoinLeaving;
                this.m_prcClient.ChatConsole.LogKills = this.m_prcClient.ChatConsole.LogKills;
                this.m_prcClient.ChatConsole.Scrolling = this.m_prcClient.ChatConsole.Scrolling;
                this.m_prcClient.ChatConsole.LogComRoseMessages = this.m_prcClient.ChatConsole.LogComRoseMessages;
                this.m_prcClient.ChatConsole.LogPlayerDisconnected = this.m_prcClient.ChatConsole.LogPlayerDisconnected;
                this.m_prcClient.ChatConsole.DisplayTypeIndex = this.m_prcClient.ChatConsole.DisplayTypeIndex;
                this.m_prcClient.ChatConsole.DisplayTimeIndex = this.m_prcClient.ChatConsole.DisplayTimeIndex;

                if (System.String.Compare(this.m_prcClient.GameType, "BF4", System.StringComparison.OrdinalIgnoreCase) != 0)
                {
                    this.chkDisplayComRoseMsg.Enabled = false;
                    this.chkDisplayComRoseMsg.Visible = false;

                    this.chkDisplayPlayerDisconnected.Enabled = false;
                    this.chkDisplayPlayerDisconnected.Visible = false;
                }
            }
        }

        public override void SetConnection(PRoConClient prcClient)
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
                // update max length
                this.chatUpdTxtLength();
            }
        }

        private void m_prcClient_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                this.m_prcClient.ChatConsole.WriteConsole += new PRoCon.Core.Logging.Loggable.WriteConsoleHandler(ChatConsole_WriteConsole);
                this.m_prcClient.ChatConsole.LogJoinLeavingChanged += new PRoCon.Core.Consoles.ChatConsole.IsEnabledHandler(ChatConsole_LogJoinLeavingChanged);
                this.m_prcClient.ChatConsole.LogKillsChanged += new PRoCon.Core.Consoles.ChatConsole.IsEnabledHandler(ChatConsole_LogKillsChanged);
                this.m_prcClient.ChatConsole.ScrollingChanged += new PRoCon.Core.Consoles.ChatConsole.IsEnabledHandler(ChatConsole_ScrollingChanged);
                this.m_prcClient.ChatConsole.LogComRoseMessagesChanged += new PRoCon.Core.Consoles.ChatConsole.IsEnabledHandler(ChatConsole_LogComRoseMessagesChanged);
                this.m_prcClient.ChatConsole.LogPlayerDisconnectedChanged += new PRoCon.Core.Consoles.ChatConsole.IsEnabledHandler(ChatConsole_LogPlayerDisconnectedChanged);

                this.m_prcClient.ChatConsole.DisplayTimeChanged += new PRoCon.Core.Consoles.ChatConsole.IndexChangedHandler(ChatConsole_DisplayTimeChanged);
                this.m_prcClient.ChatConsole.DisplayTypeChanged += new PRoCon.Core.Consoles.ChatConsole.IndexChangedHandler(ChatConsole_DisplayTypeChanged);

                this.m_prcClient.Game.ListPlayers += new FrostbiteClient.ListPlayersHandler(m_prcClient_ListPlayers);
                this.m_prcClient.Game.ServerInfo += new FrostbiteClient.ServerInfoHandler(m_prcClient_ServerInfo);

                if (sender.Game is MoHClient)
                {
                    this.cboDisplayList.Items.RemoveAt(1);
                }
                if (sender.Game is MOHWClient)
                {
                    this.lblDisplayFor.Visible = false;
                    this.cboDisplayChatTime.Visible = false;
                }
                if (sender.Game is BFHLClient || sender.Game is BF4Client || sender.Game is BF3Client || sender.Game is MOHWClient)
                {
                    this.m_iCanLongMsg = 1;
                    this.m_iChat_MaxLength = 128;
                }
            });
        }

        public void Initialize(uscServerConnection uscParent)
        {
            this.m_uscParent = uscParent;

            this.cboPlayerList.Items.Add(new CPlayerInfo("", String.Empty, -10, -10));
            this.cboPlayerList.SelectedIndex = 0;
        }

        public void SetColour(string strVariable, string strValue)
        {
            this.rtbChatBox.SetColour(strVariable, strValue);
        }

        public override void SetLocalization(CLocalization clocLanguage)
        {
            this.m_clocLanguage = clocLanguage;

            this.btnChatSend.Text = this.m_clocLanguage.GetLocalized("uscChatPanel.btnChatSend", null);
            this.m_strAllPlayers = this.m_clocLanguage.GetLocalized("uscChatPanel.cboPlayerList.AllPlayers", null);
            this.lblAudience.Text = this.m_clocLanguage.GetLocalized("uscChatPanel.lblAudience", null);
            this.lblDisplayFor.Text = this.m_clocLanguage.GetLocalized("uscChatPanel.lblDisplayFor", null);

            this.lblDisplay.Text = this.m_clocLanguage.GetLocalized("uscChatPanel.lblDisplay", null);

            this.cboDisplayList.Items[0] = this.m_clocLanguage.GetLocalized("uscChatPanel.cboDisplayList.Say", null);

            if (this.cboDisplayList.Items.Count > 1)
            {
                this.cboDisplayList.Items[1] = this.m_clocLanguage.GetLocalized("uscChatPanel.cboDisplayList.Yell", null);
            }

            this.chkDisplayOnJoinLeaveEvents.Text = this.m_clocLanguage.GetLocalized("uscChatPanel.chkDisplayOnJoinLeaveEvents", null);
            this.chkDisplayOnKilledEvents.Text = this.m_clocLanguage.GetLocalized("uscChatPanel.chkDisplayOnKilledEvents", null);
            this.chkDisplayScrollingEvents.Text = this.m_clocLanguage.GetLocalized("uscChatPanel.chkDisplayScrolling", null);
            this.chkDisplayComRoseMsg.Text = this.m_clocLanguage.GetLocalized("uscChatPanel.chkDisplayComRoseMsg", null);
            this.chkDisplayPlayerDisconnected.Text = this.m_clocLanguage.GetLocalized("uscChatPanel.chkDisplayPlayerDisconnected", null);
            this.btnclearchat.Text = this.m_clocLanguage.GetLocalized("uscChatPanel.btnclearchat", null);
            this.cboDisplayChatTime.Refresh();
        }

        private void cboDisplayChatTime_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                int iDrawItemData = ((int)cboDisplayChatTime.Items[e.Index]);

                e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
                if (iDrawItemData == 10000)
                {
                    e.Graphics.DrawString(String.Format("{0} {1}", iDrawItemData / 1000, this.m_clocLanguage.GetLocalized("global.Seconds.Plural", null).ToLower()), new Font("Calibri", 10, FontStyle.Bold), SystemBrushes.WindowText, e.Bounds.Left, e.Bounds.Top, StringFormat.GenericDefault);
                }
                else
                {
                    e.Graphics.DrawString(String.Format("{0} {1}", Math.Ceiling((double)iDrawItemData / 1000), this.m_clocLanguage.GetLocalized("global.Seconds.Plural", null).ToLower()), this.Font, SystemBrushes.WindowText, e.Bounds.Left, e.Bounds.Top, StringFormat.GenericDefault);
                }
            }
        }

        private bool isInComboList(CPlayerInfo cpiPlayer)
        {

            bool blFound = false;

            foreach (CPlayerInfo cpiInfo in this.cboPlayerList.Items)
            {
                if (String.Compare(cpiInfo.SoldierName, cpiPlayer.SoldierName) == 0)
                {
                    blFound = true;
                    break;
                }
            }

            return blFound;
        }

        private void m_prcClient_ListPlayers(FrostbiteClient sender, List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset)
        {
            this.InvokeIfRequired(() =>
            {
                if (cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.All)
                {

                    lstPlayers.Sort((x, y) => String.Compare(x.SoldierName, y.SoldierName));

                    CPlayerInfo objSelected = (CPlayerInfo)this.cboPlayerList.SelectedItem;

                    this.cboPlayerList.BeginUpdate();

                    //MoHW R-6 can't address individual players
                    if (sender.GameType != "MOHW")
                    {
                        // org.
                        for (int i = 0; i < this.cboPlayerList.Items.Count; i++)
                        {
                            CPlayerInfo cpiInfo = (CPlayerInfo)this.cboPlayerList.Items[i];

                            if (cpiInfo.SquadID != -10 && cpiInfo.TeamID != -10)
                            {
                                this.cboPlayerList.Items.RemoveAt(i);
                                i--;
                            }
                        }

                        foreach (CPlayerInfo cpiPlayer in lstPlayers)
                        {
                            if (this.isInComboList(cpiPlayer) == false)
                            {
                                this.cboPlayerList.Items.Add(cpiPlayer);
                            }
                        }

                        this.cboPlayerList.SelectedIndex = 0;
                        for (int i = 0; i < this.cboPlayerList.Items.Count; i++)
                        {
                            CPlayerInfo cpiInfo = (CPlayerInfo)this.cboPlayerList.Items[i];
                            if (String.Compare(cpiInfo.SoldierName, objSelected.SoldierName) == 0)
                            {
                                this.cboPlayerList.SelectedIndex = i;
                                break;
                            }
                        }
                    } // end hack

                    this.cboPlayerList.EndUpdate();

                }
            });
        }

        // Quick R3 hack to stop the chat getting spammed out..
        //private string m_strPreviousAddition = String.Empty;

        private void ChatConsole_WriteConsole(DateTime dtLoggedTime, string strLoggedText)
        {
            this.InvokeIfRequired(() => this.rtbChatBox.AppendText(String.Format("^n[{0}] {1}{2}", dtLoggedTime.ToString("HH:mm:ss"), strLoggedText, "\n")));
        }

        private void rtbChatBox_Flushed(object arg1, EventArgs arg2)
        {
            if (this.m_prcClient.ChatConsole.Scrolling == true)
            {
                this.rtbChatBox.ScrollToCaret();
            }

            this.rtbChatBox.TrimLines(this.m_prcClient.Variables.GetVariable("MAX_CHAT_LINES", 75));
        }

        private void txtChat_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyData == Keys.Enter)
            {

                this.btnChatSend_Click(this, null);

                this.txtChat.Clear();
                this.txtChat.Focus();
                e.SuppressKeyPress = true;

                // update max length
                this.chatUpdTxtLength();
            }

            if (e.KeyData == Keys.Up)
            {
                e.SuppressKeyPress = true;

                if (this.m_llChatHistoryCurrentNode == null && this.m_llChatHistory.First != null)
                {
                    this.m_llChatHistoryCurrentNode = this.m_llChatHistory.First;
                    this.txtChat.Text = this.m_llChatHistoryCurrentNode.Value;

                    this.txtChat.Select(this.txtChat.Text.Length, 0);
                }
                else if (this.m_llChatHistoryCurrentNode != null && this.m_llChatHistoryCurrentNode.Next != null)
                {
                    this.m_llChatHistoryCurrentNode = this.m_llChatHistoryCurrentNode.Next;
                    this.txtChat.Text = this.m_llChatHistoryCurrentNode.Value;

                    this.txtChat.Select(this.txtChat.Text.Length, 0);
                }
            }
            else if (e.KeyData == Keys.Down)
            {

                if (this.m_llChatHistoryCurrentNode != null && this.m_llChatHistoryCurrentNode.Previous != null)
                {
                    this.m_llChatHistoryCurrentNode = this.m_llChatHistoryCurrentNode.Previous;
                    this.txtChat.Text = this.m_llChatHistoryCurrentNode.Value;

                    this.txtChat.Select(this.txtChat.Text.Length, 0);
                }

                e.SuppressKeyPress = true;
            }
        }

        private void btnChatSend_Click(object sender, EventArgs e)
        {

            this.m_llChatHistory.AddFirst(this.txtChat.Text);
            if (this.m_llChatHistory.Count > 20)
            {
                this.m_llChatHistory.RemoveLast();
            }
            this.m_llChatHistoryCurrentNode = null;

            CPlayerInfo objSelected = (CPlayerInfo)this.cboPlayerList.SelectedItem;

            if (objSelected != null && this.txtChat.Text.Length > 0)
            {

                if (this.cboDisplayList.SelectedIndex == 0)
                {

                    string sayOutput = String.Empty;
                    // PK
                    if (this.txtChat.Text.Length > 0 && this.txtChat.Text[0] == '/')
                    {
                        sayOutput = this.txtChat.Text;
                    }
                    else
                    {
                        if (Program.ProconApplication.OptionsSettings.ChatDisplayAdminName)
                        {
                            sayOutput = String.Format("{0}: {1}", this.m_prcClient.Username.Length > 0 ? this.m_prcClient.Username : "Admin", this.txtChat.Text);
                        }
                        else
                        {
                            sayOutput = this.txtChat.Text;
                        }
                    }

                    if (objSelected.SquadID == -10 && objSelected.TeamID == -10)
                    {

                        this.SendCommand(new List<string> { "admin.say", sayOutput, "all" });
                    }
                    else if (objSelected.SquadID == -10 && objSelected.TeamID > 0)
                    {
                        this.SendCommand(new List<string> { "admin.say", sayOutput, "team", objSelected.TeamID.ToString() });
                    }
                    else
                    {
                        this.SendCommand(new List<string> { "admin.say", sayOutput, "player", objSelected.SoldierName });
                    }
                }
                else if (this.cboDisplayList.SelectedIndex == 1)
                {
                    this.m_iYellDuration = (int)cboDisplayChatTime.SelectedItem;
                    if (this.m_prcClient.Game is BFHLClient || this.m_prcClient.Game is BF4Client || this.m_prcClient.Game is BF3Client || this.m_prcClient.Game is MOHWClient)
                    {
                        this.m_iYellDuration = (int)cboDisplayChatTime.SelectedItem / 1000;
                    }

                    if (objSelected.SquadID == -10 && objSelected.TeamID == -10)
                    {
                        this.SendCommand(new List<string> { "admin.yell", this.txtChat.Text, ((int)this.m_iYellDuration).ToString(), "all" });
                    }
                    else if (objSelected.SquadID == -10 && objSelected.TeamID > 0)
                    {
                        this.SendCommand(new List<string> { "admin.yell", this.txtChat.Text, ((int)this.m_iYellDuration).ToString(), "team", objSelected.TeamID.ToString() });
                    }
                    else
                    {
                        this.SendCommand(new List<string> { "admin.yell", this.txtChat.Text, ((int)this.m_iYellDuration).ToString(), "player", objSelected.SoldierName });
                    }
                }
            }

            this.txtChat.Clear();
            this.txtChat.Focus();
            // update max length
            this.chatUpdTxtLength();
        }

        private void btnclearchat_Click(object sender, EventArgs e)
        {
            this.rtbChatBox.Clear();
            // update max length
            this.chatUpdTxtLength();
            this.txtChat.Clear();
        }

        private int ListContainsTeam(int iTeamID)
        {
            int iReturnTeamIndex = -1;

            for (int i = 0; i < this.cboPlayerList.Items.Count; i++)
            {
                if (((CPlayerInfo)cboPlayerList.Items[i]).SquadID == -10 && ((CPlayerInfo)cboPlayerList.Items[i]).TeamID == iTeamID)
                {
                    iReturnTeamIndex = i;
                    break;
                }
            }

            return iReturnTeamIndex;
        }

        List<string> m_lstTeamNames = new List<string>(17);
        string m_strCurrentMapFileName = String.Empty;

        private void m_prcClient_ServerInfo(FrostbiteClient sender, CServerInfo csiServerInfo)
        {
            this.InvokeIfRequired(() =>
            {
                this.cboPlayerList.BeginUpdate();

                int iTotalTeams = this.m_prcClient.GetLocalizedTeamNameCount(csiServerInfo.Map, csiServerInfo.GameMode);
                this.m_strCurrentMapFileName = csiServerInfo.Map;

                // Add all the teams.
                for (int i = 1; i < iTotalTeams; i++)
                {

                    int iTeamIndex = -1;

                    if ((iTeamIndex = this.ListContainsTeam(i)) == -1)
                    {
                        this.cboPlayerList.Items.Insert(1, new CPlayerInfo(this.m_prcClient.GetLocalizedTeamName(i, csiServerInfo.Map, csiServerInfo.GameMode), String.Empty, i, -10));
                    }
                    else if (iTeamIndex >= 0 && iTeamIndex < this.cboPlayerList.Items.Count)
                    {
                        this.cboPlayerList.Items[iTeamIndex] = new CPlayerInfo(this.m_prcClient.GetLocalizedTeamName(i, csiServerInfo.Map, csiServerInfo.GameMode), String.Empty, i, -10);
                    }
                }

                // Remove any excess teams (change gamemode)
                for (int i = 0; i < this.cboPlayerList.Items.Count; i++)
                {
                    if (((CPlayerInfo)cboPlayerList.Items[i]).SquadID == -10 && ((CPlayerInfo)cboPlayerList.Items[i]).TeamID > iTotalTeams)
                    {
                        cboPlayerList.Items.RemoveAt(i);
                        i--;
                    }
                }

                this.cboPlayerList.EndUpdate();
            });
        }
        /*
        public void OnServerInfo(CServerInfo csiInfo) {

        }
        */
        private void cboPlayerList_DrawItem(object sender, DrawItemEventArgs e)
        {

            if (e.Index != -1)
            {
                //e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                CPlayerInfo cpiDraw = ((CPlayerInfo)cboPlayerList.Items[e.Index]);

                e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);

                if (cpiDraw.SquadID == -10 && cpiDraw.TeamID == -10)
                {
                    e.Graphics.DrawString(this.m_strAllPlayers, new Font("Calibri", 10, FontStyle.Bold), SystemBrushes.WindowText, e.Bounds.Left + 5, e.Bounds.Top, StringFormat.GenericDefault);
                }
                else if (cpiDraw.SquadID == -10 && cpiDraw.TeamID > 0)
                {
                    e.Graphics.DrawString(cpiDraw.SoldierName, new Font("Calibri", 10, FontStyle.Bold), SystemBrushes.WindowText, e.Bounds.Left + 5, e.Bounds.Top, StringFormat.GenericDefault);
                }
                else
                {
                    e.Graphics.DrawString(String.Format("{0} {1}", cpiDraw.ClanTag, cpiDraw.SoldierName), this.Font, SystemBrushes.WindowText, e.Bounds.Left + 5, e.Bounds.Top, StringFormat.GenericDefault);
                }
            }
        }

        public void PlayerSelectionChange(string strSoldierName)
        {
            foreach (CPlayerInfo cpiInfo in this.cboPlayerList.Items)
            {
                if (String.Compare(cpiInfo.SoldierName, strSoldierName) == 0)
                {
                    this.cboPlayerList.SelectedItem = cpiInfo;
                    break;
                }
            }
        }

        private void cboDisplayList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.m_prcClient.ChatConsole.DisplayTypeIndex = this.cboDisplayList.SelectedIndex;
            }
        }

        private void ChatConsole_DisplayTypeChanged(int index)
        {
            this.InvokeIfRequired(() =>
            {
                if (index >= 0 && index < this.cboDisplayList.Items.Count)
                {
                    this.cboDisplayList.SelectedIndex = index;
                }

                if (this.cboDisplayList.SelectedIndex == 0)
                {
                    this.lblDisplayFor.Enabled = false;
                    this.cboDisplayChatTime.Enabled = false;
                    this.m_iChat_MaxLength = 100;
                    if (this.m_iCanLongMsg == 1)
                    {
                        this.m_iChat_MaxLength = 128;
                        this.txtChat.Clear();
                    }
                    this.chatUpdTxtLength();
                }
                else if (this.cboDisplayList.SelectedIndex == 1)
                {
                    this.lblDisplayFor.Enabled = true;
                    this.cboDisplayChatTime.Enabled = true;
                    this.m_iChat_MaxLength = 100;
                    if (this.m_iCanLongMsg == 1)
                    {
                        this.m_iChat_MaxLength = 255;
                    }
                    this.chatUpdTxtLength();
                }
            });
        }

        private void chkDisplayOnJoinLeaveEvents_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.m_prcClient.ChatConsole.LogJoinLeaving = this.chkDisplayOnJoinLeaveEvents.Checked;
            }
        }

        private void chkDisplayOnKilledEvents_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.m_prcClient.ChatConsole.LogKills = this.chkDisplayOnKilledEvents.Checked;
            }
        }

        private void chkDisplayScrollingEvents_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.m_prcClient.ChatConsole.Scrolling = this.chkDisplayScrollingEvents.Checked;
            }
        }

        private void chkDisplayComRoseMsg_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.m_prcClient.ChatConsole.LogComRoseMessages = this.chkDisplayComRoseMsg.Checked;
            }
        }

        private void chkDisplayPlayerDisconnected_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.m_prcClient.ChatConsole.LogPlayerDisconnected = this.chkDisplayPlayerDisconnected.Checked;
            }
        }

        private void ChatConsole_ScrollingChanged(bool isEnabled)
        {
            this.chkDisplayScrollingEvents.Checked = isEnabled;
            this.chatUpdTxtLength();
            this.txtChat.Clear();
        }

        private void ChatConsole_LogKillsChanged(bool isEnabled)
        {
            this.chkDisplayOnKilledEvents.Checked = isEnabled;
        }

        private void ChatConsole_LogJoinLeavingChanged(bool isEnabled)
        {
            this.chkDisplayOnJoinLeaveEvents.Checked = isEnabled;
        }

        private void ChatConsole_LogComRoseMessagesChanged(bool isEnabled)
        {
            this.chkDisplayComRoseMsg.Checked = isEnabled;
        }

        private void ChatConsole_LogPlayerDisconnectedChanged(bool isEnabled)
        {
            this.chkDisplayPlayerDisconnected.Checked = isEnabled;
        }

        private void ChatConsole_DisplayTimeChanged(int index)
        {
            if (index >= 0 && index < this.cboDisplayChatTime.Items.Count)
            {
                this.cboDisplayChatTime.SelectedIndex = index;
            }
        }

        private void cboDisplayChatTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.m_prcClient.ChatConsole.DisplayTimeIndex = this.cboDisplayChatTime.SelectedIndex;
            }
        }

        private void chatUpdTxtLength()
        {
            // update max length
            if (Program.ProconApplication.OptionsSettings.ChatDisplayAdminName)
            {
                if (this.m_prcClient.Username.Length > 0)
                {
                    this.txtChat.MaxLength = this.m_iChat_MaxLength - (this.m_prcClient.Username.Length + 2);
                }
                else
                {
                    this.txtChat.MaxLength = this.m_iChat_MaxLength - 7; // "Admin: "
                }
            }
            else
            {
                this.txtChat.MaxLength = this.m_iChat_MaxLength;
            }
            //
        }
    }
}
