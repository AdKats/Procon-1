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

using PRoCon.Controls.ControlsEx;
using PRoCon.Core;
using PRoCon.Core.Events;
using PRoCon.Core.Remote;
using PRoCon.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PRoCon.Controls
{
    public partial class uscEventsPanel : UserControl
    {

        private frmMain m_frmMain;
        private uscServerConnection m_uscConnectionPanel;
        private CLocalization m_clocLanguage;

        private PRoConClient m_prcClient;

        private Queue<ListViewItem> m_queListItems;

        private ListViewColumnSorter m_lvwColumnSorter;

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
                this.m_prcClient.EventsLogging.CapturedEvents.ItemAdded += new NotificationList<CapturableEvents>.ItemModifiedHandler(CapturedEvents_ItemAdded);
                this.m_prcClient.EventsLogging.CapturedEvents.ItemRemoved += new NotificationList<CapturableEvents>.ItemModifiedHandler(CapturedEvents_ItemRemoved);

                this.m_prcClient.EventsLogging.MaximumDisplayedEventsChange += new EventCaptures.MaximumDisplayedEventsChangeHandler(LoggedEvents_MaximumDisplayedEventsChange);
                this.m_prcClient.EventsLogging.OptionsVisibleChange += new EventCaptures.OptionsVisibleChangeHandler(LoggedEvents_OptionsHiddenChange);
                this.m_prcClient.EventsLogging.ScrollingEnabledChange += new EventCaptures.ScrollingEnabledChangeHandler(LoggedEvents_ScrollingEnabledChange);

                this.m_prcClient.EventsLogging.LoggedEvent += new EventCaptures.LoggedEventHandler(LoggedEvents_LoggedEvent);
            });
        }

        public void SetLocalization(CLocalization clocLanguage)
        {

            if ((this.m_clocLanguage = clocLanguage) != null)
            {

                this.colSource.Text = this.m_clocLanguage.GetLocalized("uscEvents.colSource", null);
                this.colTime.Text = this.m_clocLanguage.GetLocalized("uscEvents.colTime", null);
                this.colEvent.Text = this.m_clocLanguage.GetLocalized("uscEvents.colEvent", null);
                this.colAdmin.Text = this.m_clocLanguage.GetLocalized("uscEvents.colAdmin", null);
                this.colMessage.Text = this.m_clocLanguage.GetLocalized("uscEvents.colMessage", null);

                this.lnkShowHide.Text = this.m_clocLanguage.GetLocalized("uscEvents.lnkShowHide.Hide", null);

                this.gpbCaptures.Text = this.m_clocLanguage.GetLocalized("uscEvents.gpbCaptures", null);
                this.lblMaximumDisplayed.Text = this.m_clocLanguage.GetLocalized("uscEvents.gpbCaptures.lblMaximumDisplayed", null);
                this.lblEvents.Text = this.m_clocLanguage.GetLocalized("uscEvents.gpbCaptures.lblEvents", null);
                this.lblCapturingEvents.Text = this.m_clocLanguage.GetLocalized("uscEvents.gpbCaptures.lblCapturingEvents", null);
                this.lnkAddCapture.Text = this.m_clocLanguage.GetLocalized("uscEvents.gpbCaptures.lnkAddCapture", null);
                this.btncleareventbox.Text = this.m_clocLanguage.GetLocalized("uscEvents.gpbCaptures.btncleareventbox", null);
                this.chkEventsEnableScrolling.Text = this.m_clocLanguage.GetLocalized("uscEvents.gpbCaptures.chkEventsEnableScrolling", null);
            }
        }

        public uscEventsPanel()
        {
            InitializeComponent();
            this.m_queListItems = new Queue<ListViewItem>();

            this.m_lvwColumnSorter = new ListViewColumnSorter();
            this.lsvEvents.ListViewItemSorter = this.m_lvwColumnSorter;

            ListViewItem lviNewCapture = null;
            foreach (string strEvent in Enum.GetNames(typeof(CapturableEvents)))
            {
                this.cboEvents.Items.Add(strEvent);
                lviNewCapture = new ListViewItem(strEvent);
                lviNewCapture.Name = strEvent;
                //this.lsvCapturedEvents.Items.Add(lviNewCapture);
            }

            this.cboEvents.SelectedIndex = 0;
        }

        private bool isLoaded = false;
        private void uscEventsPanel_Load(object sender, EventArgs e)
        {

            this.spltEvents.SplitterDistance = (int)(this.spltEvents.Width * 0.8);

            if (isLoaded == false && this.m_prcClient != null && this.m_prcClient.EventsLogging != null)
            {

                // it just fires the events again, saves messy coding elsewhere.
                this.m_prcClient.EventsLogging.OptionsVisible = this.m_prcClient.EventsLogging.OptionsVisible;
                this.m_prcClient.EventsLogging.MaximumDisplayedEvents = this.m_prcClient.EventsLogging.MaximumDisplayedEvents;
                this.m_prcClient.EventsLogging.ScrollingEnabled = this.m_prcClient.EventsLogging.ScrollingEnabled;

                this.lsvCapturedEvents.Items.Clear();

                foreach (CapturableEvents ceCapturedEvents in this.m_prcClient.EventsLogging.CapturedEvents)
                {
                    ListViewItem lviNewCapture = new ListViewItem(ceCapturedEvents.ToString());
                    lviNewCapture.Name = ceCapturedEvents.ToString();
                    this.lsvCapturedEvents.Items.Add(lviNewCapture);
                }

                this.lsvCapturedEvents.BeginUpdate();

                // Clear out any of the lists / masked out, caused an empty list of events to capture
                //this.lsvCapturedEvents.Items.Clear();
                //this.m_queListItems.Clear();

                ListViewItem lastAddedItem = null;
                foreach (CapturedEvent ceEvent in this.m_prcClient.EventsLogging.LogEntries.ToArray())
                {
                    lastAddedItem = this.AddLoggedEvent(ceEvent);
                }

                if (lastAddedItem != null)
                {
                    this.CleanUpLoggedEvents(lastAddedItem);
                }

                this.lsvCapturedEvents.EndUpdate();

                isLoaded = true;
            }

            this.spltEvents.SplitterDistance = (int)(this.spltEvents.Width * 0.8);

            this.lsvCapturedEvents.SetBounds(this.lsvCapturedEvents.Bounds.X, this.lsvCapturedEvents.Bounds.Y, this.lsvCapturedEvents.Bounds.Width, btncleareventbox.Bounds.Y - this.lsvCapturedEvents.Bounds.Y - 10);
        }

        public void Initalize(frmMain frmMain, uscServerConnection uscConnectionPanel)
        {
            this.m_frmMain = frmMain;
            this.m_uscConnectionPanel = uscConnectionPanel;

            this.lsvEvents.SmallImageList = this.m_frmMain.iglIcons;

            this.btnRemoveCapture.ImageList = this.m_frmMain.iglIcons;
            this.btnRemoveCapture.ImageKey = "cross.png";

            this.btnAddCapture.ImageList = this.m_frmMain.iglIcons;
            this.btnAddCapture.ImageKey = "add.png";

            this.picOpenCloseCaptures.Image = this.m_frmMain.iglIcons.Images["arrow_right.png"];
        }

        private ListViewItem CreateLoggedEvent(CapturedEvent ceEvent)
        {

            ListViewItem lviNewEvent = new ListViewItem();

            if (ceEvent.EventType == EventType.Game)
            {
                lviNewEvent.ImageKey = "bfbc2server.png";
                lviNewEvent.Text = "Game";
            }
            else if (ceEvent.EventType == EventType.Plugins)
            {
                lviNewEvent.ImageKey = "plugin.png";
                lviNewEvent.Text = "Plugins";
            }
            else if (ceEvent.EventType == EventType.Connection)
            {
                lviNewEvent.ImageKey = "connect.png";
                lviNewEvent.Text = "Connection";
            }
            else if (ceEvent.EventType == EventType.Playerlist)
            {
                lviNewEvent.ImageKey = "mouse.png";
                lviNewEvent.Text = "Playerlist";
            }
            else if (ceEvent.EventType == EventType.Layer)
            {
                lviNewEvent.ImageKey = "layer.png";
                lviNewEvent.Text = "Layer";
            }

            ListViewItem.ListViewSubItem lvsiNewSubItem = new ListViewItem.ListViewSubItem(lviNewEvent, ceEvent.LoggedTime.ToString("MM/dd/yyyy HH:mm:ss"));
            lvsiNewSubItem.Name = "datetime";
            lviNewEvent.SubItems.Add(lvsiNewSubItem);

            lvsiNewSubItem = new ListViewItem.ListViewSubItem(lviNewEvent, ceEvent.InstigatingAdmin);
            lvsiNewSubItem.Name = "admin";
            lviNewEvent.SubItems.Add(lvsiNewSubItem);

            lvsiNewSubItem = new ListViewItem.ListViewSubItem(lviNewEvent, ceEvent.Event.ToString());
            lvsiNewSubItem.Name = "event";
            lviNewEvent.SubItems.Add(lvsiNewSubItem);

            //lviNewEvent.SubItems.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            //lviNewEvent.SubItems.Add(Event.ToString("G"));

            lvsiNewSubItem = new ListViewItem.ListViewSubItem(lviNewEvent, ceEvent.EventText);

            lvsiNewSubItem.Name = "message";
            lviNewEvent.SubItems.Add(lvsiNewSubItem);

            return lviNewEvent;
        }

        private ListViewItem AddLoggedEvent(CapturedEvent ceEvent)
        {

            ListViewItem lviNewEvent = this.CreateLoggedEvent(ceEvent);

            this.m_queListItems.Enqueue(lviNewEvent);
            this.lsvEvents.Items.Add(lviNewEvent);

            return lviNewEvent;
        }

        private void CleanUpLoggedEvents(ListViewItem newEventItem)
        {

            while (this.m_queListItems.Count > this.numMaximumEvents.Value)
            {
                this.lsvEvents.Items.Remove(this.m_queListItems.Dequeue());
            }

            this.lsvEvents.Sort();

            if (this.m_prcClient.EventsLogging.ScrollingEnabled == true)
            {
                newEventItem.EnsureVisible();
            }

            foreach (ColumnHeader ch in this.lsvEvents.Columns)
            {
                ch.Width = -2;
            }
        }

        private void LoggedEvents_LoggedEvent(CapturedEvent ceEvent)
        {
            this.InvokeIfRequired(() =>
            {
                this.lsvEvents.BeginUpdate();

                this.CleanUpLoggedEvents(this.AddLoggedEvent(ceEvent));

                this.lsvEvents.EndUpdate();
            });
        }

        private void CapturedEvents_ItemAdded(int iIndex, CapturableEvents item)
        {
            this.InvokeIfRequired(() =>
            {
                ListViewItem lviNewCapture = new ListViewItem(item.ToString());
                lviNewCapture.Name = item.ToString();
                this.lsvCapturedEvents.Items.Add(lviNewCapture);
            });
        }

        private void CapturedEvents_ItemRemoved(int iIndex, CapturableEvents item)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.lsvCapturedEvents.Items.ContainsKey(item.ToString()) == true)
                {
                    this.lsvCapturedEvents.Items.Remove(this.lsvCapturedEvents.Items[item.ToString()]);
                }
            });
        }

        private void LoggedEvents_OptionsHiddenChange(bool isVisible)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.m_frmMain != null)
                {

                    if (isVisible == true)
                    {
                        this.lnkShowHide.Text = this.lnkShowHide.Text = this.m_clocLanguage.GetLocalized("uscEvents.lnkShowHide.Hide", null);
                        this.picOpenCloseCaptures.Image = this.m_frmMain.iglIcons.Images["arrow_right.png"];
                        spltEvents.Panel2Collapsed = false;
                    }
                    else
                    {
                        this.lnkShowHide.Text = this.lnkShowHide.Text = this.m_clocLanguage.GetLocalized("uscEvents.lnkShowHide.Show", null);
                        this.picOpenCloseCaptures.Image = this.m_frmMain.iglIcons.Images["arrow_left.png"];
                        spltEvents.Panel2Collapsed = true;
                    }
                }
            });
        }

        private void LoggedEvents_MaximumDisplayedEventsChange(int maximumDisplayedEvents)
        {
            this.InvokeIfRequired(() =>
            {
                if (maximumDisplayedEvents >= this.numMaximumEvents.Minimum && maximumDisplayedEvents <= this.numMaximumEvents.Maximum)
                {
                    this.numMaximumEvents.Value = (decimal)maximumDisplayedEvents;

                    this.lsvEvents.BeginUpdate();

                    while (this.m_queListItems.Count > this.numMaximumEvents.Value)
                    {
                        this.lsvEvents.Items.Remove(this.m_queListItems.Dequeue());
                    }

                    this.lsvEvents.EndUpdate();
                }
            });
        }

        private void LoggedEvents_ScrollingEnabledChange(bool isEnabled)
        {
            this.InvokeIfRequired(() => { this.chkEventsEnableScrolling.Checked = isEnabled; });
        }

        private void picOpenCloseCaptures_Click(object sender, EventArgs e)
        {
            this.m_prcClient.EventsLogging.OptionsVisible = !this.m_prcClient.EventsLogging.OptionsVisible;
        }

        private void lnkShowHide_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.picOpenCloseCaptures_Click(sender, e);
        }

        private void cboEvents_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (this.m_prcClient != null)
            {
                if (this.m_prcClient.EventsLogging.CapturedEvents.Contains((CapturableEvents)Enum.Parse(typeof(CapturableEvents), (string)cboEvents.SelectedItem)) == false)
                {
                    this.btnAddCapture.Enabled = true;
                }
                else
                {
                    this.btnAddCapture.Enabled = false;
                }
            }
        }

        private void lsvCapturedEvents_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (this.lsvCapturedEvents.SelectedItems.Count > 0)
            {
                this.btnRemoveCapture.Enabled = true;
            }
            else if (this.lsvCapturedEvents.FocusedItem != null)
            {
                this.btnRemoveCapture.Enabled = false;
            }

        }

        private void btnRemoveCapture_Click(object sender, EventArgs e)
        {

            if (this.lsvCapturedEvents.SelectedItems.Count > 0)
            {

                if (Enum.IsDefined(typeof(CapturableEvents), this.lsvCapturedEvents.SelectedItems[0].Text) == true && this.m_prcClient.EventsLogging.CapturedEvents.Contains((CapturableEvents)Enum.Parse(typeof(CapturableEvents), this.lsvCapturedEvents.SelectedItems[0].Text)) == true)
                {

                    int iCurrentIndex = this.lsvCapturedEvents.SelectedItems[0].Index;
                    this.m_prcClient.EventsLogging.CapturedEvents.Remove((CapturableEvents)Enum.Parse(typeof(CapturableEvents), this.lsvCapturedEvents.SelectedItems[0].Text));

                    if (this.lsvCapturedEvents.Items.Count > 0)
                    {
                        this.lsvCapturedEvents.SelectedIndices.Clear();
                        this.lsvCapturedEvents.SelectedIndices.Add(Math.Min(iCurrentIndex, this.lsvCapturedEvents.Items.Count - 1));
                    }

                    if (this.lsvCapturedEvents.Items.ContainsKey((string)cboEvents.SelectedItem) == false)
                    {
                        this.btnAddCapture.Enabled = true;
                    }
                    else
                    {
                        this.btnAddCapture.Enabled = false;
                    }
                }
            }

        }

        private void numMaximumEvents_ValueChanged(object sender, EventArgs e)
        {
            this.m_prcClient.EventsLogging.MaximumDisplayedEvents = (int)this.numMaximumEvents.Value;
        }

        private void lsvEvents_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == this.m_lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (this.m_lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    this.m_lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    this.m_lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                this.m_lvwColumnSorter.SortColumn = e.Column;
                this.m_lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.lsvEvents.Sort();
        }

        private void btnAddCapture_Click(object sender, EventArgs e)
        {

            if (Enum.IsDefined(typeof(CapturableEvents), (string)cboEvents.SelectedItem) == true && this.m_prcClient.EventsLogging.CapturedEvents.Contains((CapturableEvents)Enum.Parse(typeof(CapturableEvents), (string)cboEvents.SelectedItem)) == false)
            {
                this.m_prcClient.EventsLogging.CapturedEvents.Add((CapturableEvents)Enum.Parse(typeof(CapturableEvents), (string)cboEvents.SelectedItem));

                cboEvents.Focus();

                this.btnAddCapture.Enabled = false;
            }

        }

        private void btncleareventbox_Click(object sender, EventArgs e)
        {
            this.lsvEvents.BeginUpdate();

            while (this.m_queListItems.Count > 0)
            {
                this.lsvEvents.Items.Remove(this.m_queListItems.Dequeue());
            }

            this.lsvEvents.EndUpdate();
        }

        private void chkEventsEnableScrolling_CheckedChanged(object sender, EventArgs e)
        {
            this.m_prcClient.EventsLogging.ScrollingEnabled = this.chkEventsEnableScrolling.Checked;
        }
    }
}
