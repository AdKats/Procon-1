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

using System;
using System.Windows.Forms;

namespace PRoCon.Controls.TextChatModeration
{
    using Core;
    using Core.Remote;
    using Core.TextChatModeration;
    using PRoCon.Controls.ControlsEx;
    using PRoCon.Forms;
    public partial class uscTextChatModerationListcs : uscPage
    {

        private PRoConClient m_client;
        private ListViewColumnSorter m_columnSorter;

        public uscTextChatModerationListcs()
        {
            InitializeComponent();

            this.m_columnSorter = new ListViewColumnSorter();
            this.lsvTextChatModerationList.ListViewItemSorter = this.m_columnSorter;

            this.cboTextChatModerationLevels.SelectedIndex = 0;
        }

        public void Initialize(frmMain frmMainWindow)
        {
            this.btnTextChatModerationRemoveSoldier.ImageList = frmMainWindow.iglIcons;
            this.btnTextChatModerationRemoveSoldier.ImageKey = "cross.png";
        }

        public override void SetLocalization(CLocalization clocLanguage)
        {
            base.SetLocalization(clocLanguage);

            this.lblTextChatModerationCurrent.Text = clocLanguage.GetLocalized("uscTextChatModerationList.lblTextChatModerationCurrent");
            this.lblTextChatModerationAddSoldierName.Text = clocLanguage.GetLocalized("uscTextChatModerationList.lblTextChatModerationAddSoldierName");
            this.lnkTextChatModerationAddSoldierName.Text = clocLanguage.GetLocalized("uscTextChatModerationList.lnkTextChatModerationAddSoldierName");
        }

        public override void SetConnection(Core.Remote.PRoConClient prcClient)
        {
            base.SetConnection(prcClient);

            if ((this.m_client = prcClient) != null)
            {
                if (this.m_client.Game != null)
                {
                    this.m_prcClient_GameTypeDiscovered(prcClient);
                }
                else
                {
                    this.m_client.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(m_prcClient_GameTypeDiscovered);
                }
            }
        }

        private void m_prcClient_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                this.m_client.ProconPrivileges += new PRoConClient.ProconPrivilegesHandler(m_client_ProconPrivileges);
                this.m_client.FullTextChatModerationListList += new PRoConClient.FullTextChatModerationListListHandler(m_client_FullTextChatModerationListList);

                this.m_client.Game.TextChatModerationListAddPlayer += new FrostbiteClient.TextChatModerationListAddPlayerHandler(Game_TextChatModerationListAddPlayer);
                this.m_client.Game.TextChatModerationListRemovePlayer += new FrostbiteClient.TextChatModerationListRemovePlayerHandler(Game_TextChatModerationListRemovePlayer);
            });
        }

        private void m_client_ProconPrivileges(PRoConClient sender, CPrivileges spPrivs)
        {
            this.InvokeIfRequired(() => { this.Enabled = spPrivs.CanEditTextChatModerationList; });
        }

        private void Game_TextChatModerationListAddPlayer(FrostbiteClient sender, TextChatModerationEntry playerEntry)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.lsvTextChatModerationList.Items.ContainsKey(playerEntry.SoldierName.ToLower()) == false)
                {

                    ListViewItem lsvNewSoldier = new ListViewItem(playerEntry.SoldierName);
                    lsvNewSoldier.Group = this.lsvTextChatModerationList.Groups[playerEntry.PlayerModerationLevel.ToString().ToLower()];
                    lsvNewSoldier.Name = playerEntry.SoldierName.ToLower();

                    this.lsvTextChatModerationList.Items.Add(lsvNewSoldier);
                }
                else
                {
                    this.lsvTextChatModerationList.Items[playerEntry.SoldierName.ToLower()].Group = this.lsvTextChatModerationList.Groups[playerEntry.PlayerModerationLevel.ToString().ToLower()];
                }
            });
        }

        private void m_client_FullTextChatModerationListList(PRoConClient sender, TextChatModerationDictionary moderationList)
        {
            this.InvokeIfRequired(() =>
            {
                foreach (TextChatModerationEntry playerEntry in moderationList)
                {

                    this.Game_TextChatModerationListAddPlayer(null, playerEntry);

                    foreach (ColumnHeader column in this.lsvTextChatModerationList.Columns)
                    {
                        column.Width = -2;
                    }
                }
            });
        }

        private void Game_TextChatModerationListRemovePlayer(FrostbiteClient sender, TextChatModerationEntry playerEntry)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.lsvTextChatModerationList.Items.ContainsKey(playerEntry.SoldierName.ToLower()) == true)
                {
                    this.lsvTextChatModerationList.Items.RemoveByKey(playerEntry.SoldierName.ToLower());
                }
            });
        }

        private void lsvTextChatModerationList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == this.m_columnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (this.m_columnSorter.Order == SortOrder.Ascending)
                {
                    this.m_columnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    this.m_columnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                this.m_columnSorter.SortColumn = e.Column;
                this.m_columnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.lsvTextChatModerationList.Sort();
        }

        private void lnkTextChatModerationAddSoldierName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            if (this.cboTextChatModerationLevels.SelectedItem != null)
            {
                if (this.m_client != null && this.m_client.Game != null)
                {

                    this.m_client.Game.SendTextChatModerationListAddPacket(new TextChatModerationEntry(TextChatModerationEntry.GetPlayerModerationLevelType((string)this.cboTextChatModerationLevels.SelectedItem), this.txtTextChatModerationAddSoldierName.Text));

                    this.m_client.Game.SendTextChatModerationListSavePacket();
                }
            }
        }

        private void btnTextChatModerationRemoveSoldier_Click(object sender, EventArgs e)
        {

            if (this.lsvTextChatModerationList.SelectedItems.Count > 0)
            {
                if (this.m_client != null && this.m_client.Game != null)
                {
                    this.m_client.Game.SendTextChatModerationListRemovePacket(this.lsvTextChatModerationList.SelectedItems[0].Text);

                    this.m_client.Game.SendTextChatModerationListSavePacket();
                }
            }
        }

        private void lsvTextChatModerationList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.lsvTextChatModerationList.SelectedItems.Count > 0)
            {
                this.btnTextChatModerationRemoveSoldier.Enabled = true;
                this.txtTextChatModerationAddSoldierName.Text = this.lsvTextChatModerationList.SelectedItems[0].Text;
            }
            else
            {
                this.btnTextChatModerationRemoveSoldier.Enabled = false;
            }
        }

    }
}
