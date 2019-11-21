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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PRoCon.Controls.ServerSettings
{
    using Core;
    using Core.Remote;
    public partial class uscServerSettingsLevelVariables : uscServerSettings
    {

        public uscServerSettingsLevelVariables()
        {
            InitializeComponent();

            this.AsyncSettingControls.Add("levelvars.clear", new AsyncStyleSetting(this.picClearLevelSettings, this.btnClearLevelSettings, new Control[] { this.btnClearLevelSettings }, true));

            this.AsyncSettingControls.Add("levelvars.set tickets", new AsyncStyleSetting(this.picLevelTickets, this.numLevelTickets, new Control[] { this.numLevelTickets, this.lnkLevelTickets, this.lblLevelTickets }, true));
            this.AsyncSettingControls.Add("levelvars.set ticketbleedspeed", new AsyncStyleSetting(this.picLevelTicketBleedSpeed, this.numLevelTicketBleedSpeed, new Control[] { this.numLevelTicketBleedSpeed, this.lnkLevelTicketBleedSpeed, this.lblLevelTicketBleedSpeed }, true));
            this.AsyncSettingControls.Add("levelvars.set vehiclespawnrate", new AsyncStyleSetting(this.picLevelVehicleSpawnRate, this.numLevelVehicleSpawnRate, new Control[] { this.numLevelVehicleSpawnRate, this.lnkLevelVehicleSpawnRate, this.lblLevelVehicleSpawnRate }, true));
            this.AsyncSettingControls.Add("levelvars.set vehiclesdisabled", new AsyncStyleSetting(this.picLevelVehiclesDisabled, this.chkLevelVehiclesDisabled, new Control[] { this.chkLevelVehiclesDisabled }, true));
            this.AsyncSettingControls.Add("levelvars.set startdelay", new AsyncStyleSetting(this.picLevelStartDelay, this.numLevelStartDelay, new Control[] { this.numLevelStartDelay, this.lnkLevelStartDelay, this.lblLevelStartDelay }, true));
            this.AsyncSettingControls.Add("levelvars.set respawndelay", new AsyncStyleSetting(this.picLevelRespawnDelay, this.numLevelRespawnDelay, new Control[] { this.numLevelRespawnDelay, this.lnkLevelRespawnDelay, this.lblLevelRespawnDelay }, true));

            this.rdoSettingsLevelContextAll.Checked = true;
        }

        public override void SetLocalization(CLocalization clocLanguage)
        {
            base.SetLocalization(clocLanguage);

            // Level variables
            this.lblSettingsLevelContext.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsLevelContext");
            this.rdoSettingsLevelContextAll.Text = this.Language.GetLocalized("uscServerSettingsPanel.rdoSettingsLevelContextAll");
            this.rdoSettingsLevelContextGamemode.Text = this.Language.GetLocalized("uscServerSettingsPanel.rdoSettingsLevelContextGamemode");
            this.rdoSettingsLevelContextLevel.Text = this.Language.GetLocalized("uscServerSettingsPanel.rdoSettingsLevelContextLevel");

            this.btnClearLevelSettings.Text = this.Language.GetLocalized("uscServerSettingsPanel.btnClearLevelSettings");

            this.lblLevelTickets.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblLevelTickets");
            this.lnkLevelTickets.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkLevelTickets");

            this.lblLevelTicketBleedSpeed.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblLevelTicketBleedSpeed");
            this.lnkLevelTicketBleedSpeed.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkLevelTicketBleedSpeed");

            this.lblLevelVehicleSpawnRate.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblLevelVehicleSpawnRate");
            this.lnkLevelVehicleSpawnRate.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkLevelVehicleSpawnRate");

            this.lblLevelStartDelay.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblLevelStartDelay");
            this.lnkLevelStartDelay.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkLevelStartDelay");

            this.lblLevelRespawnDelay.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblLevelRespawnDelay");
            this.lnkLevelRespawnDelay.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkLevelRespawnDelay");

            this.chkLevelVehiclesDisabled.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkLevelVehiclesDisabled");

            this.lblLevelEvaluatedEffects.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblLevelEvaluatedEffects");

            this.context.Text = this.Language.GetLocalized("uscServerSettingsPanel.lsvEvaluatedEffect.context");
            this.tickets.Text = this.Language.GetLocalized("uscServerSettingsPanel.lsvEvaluatedEffect.tickets");
            this.ticketBleedSpeed.Text = this.Language.GetLocalized("uscServerSettingsPanel.lsvEvaluatedEffect.ticketBleedSpeed");
            this.vehicleSpawnRate.Text = this.Language.GetLocalized("uscServerSettingsPanel.lsvEvaluatedEffect.vehicleSpawnRate");
            this.vehiclesDisabled.Text = this.Language.GetLocalized("uscServerSettingsPanel.lsvEvaluatedEffect.vehiclesDisabled");
            this.startDelay.Text = this.Language.GetLocalized("uscServerSettingsPanel.lsvEvaluatedEffect.startDelay");
            this.respawnDelay.Text = this.Language.GetLocalized("uscServerSettingsPanel.lsvEvaluatedEffect.respawnDelay");

            this.DisplayName = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsLevelVariables");
        }

        public override void SetConnection(Core.Remote.PRoConClient prcClient)
        {
            base.SetConnection(prcClient);

            if (this.Client != null)
            {
                if (this.Client.Game != null)
                {
                    this.m_prcClient_GameTypeDiscovered(prcClient);
                }
                else
                {
                    this.Client.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(m_prcClient_GameTypeDiscovered);
                }
            }
        }


        private void m_prcClient_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                this.Client.Login += new PRoConClient.EmptyParamterHandler(m_prcClient_CommandLogin);

                this.Client.Game.LevelVariablesList += new FrostbiteClient.LevelVariableListHandler(m_prcClient_LevelVariablesList);
                this.Client.Game.LevelVariablesClear += new FrostbiteClient.LevelVariableHandler(m_prcClient_LevelVariablesClear);
                this.Client.Game.LevelVariablesSet += new FrostbiteClient.LevelVariableHandler(m_prcClient_LevelVariablesSet);

                foreach (CMap gamemodeMap in this.Client.GetGamemodeList())
                {
                    this.cboSettingsGamemodes.Items.Add(gamemodeMap);
                }

                foreach (CMap map in this.Client.MapListPool)
                {
                    this.cboSettingsLevels.Items.Add(map);
                }
            });
        }

        private void m_prcClient_CommandLogin(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                this.GetSelectedLevelVariables();

                if (this.cboSettingsGamemodes.Items.Count > 0)
                {
                    this.cboSettingsGamemodes.SelectedIndex = 0;
                }

                if (this.cboSettingsLevels.Items.Count > 0)
                {
                    this.cboSettingsLevels.SelectedIndex = 0;
                }
            });
        }

        #region Level Variable Settings

        #region Helper Level Settings Methods

        private LevelVariableContext GetSelectedContext()
        {

            LevelVariableContextType contextType = LevelVariableContextType.None;
            string contextTarget = String.Empty;

            if (this.rdoSettingsLevelContextAll.Checked == true)
            {
                contextType = LevelVariableContextType.All;
            }
            else if (this.rdoSettingsLevelContextGamemode.Checked == true)
            {
                contextType = LevelVariableContextType.GameMode;
                if (this.cboSettingsGamemodes.SelectedItem != null)
                {
                    contextTarget = ((CMap)this.cboSettingsGamemodes.SelectedItem).PlayList;
                }
            }
            else if (this.rdoSettingsLevelContextLevel.Checked == true)
            {
                contextType = LevelVariableContextType.Level;

                if (this.cboSettingsGamemodes.SelectedItem != null)
                {
                    contextTarget = ((CMap)this.cboSettingsLevels.SelectedItem).FileName;
                }
            }

            return new LevelVariableContext(contextType, contextTarget);
        }

        private void GetLevelVariablesByContext(LevelVariableContext context)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.Client.Game.SendLevelVarsListPacket(context);
            }

            //if (context.ContextTarget.Length > 0) {
            //    this.SendCommand("levelVars.list", context.ContextType.ToString().ToLower(), context.ContextTarget);
            //}
            //else {
            //    this.SendCommand("levelVars.list", context.ContextType.ToString().ToLower());
            //}
        }

        private void SetLevelVariablesByContext(LevelVariableContext context, string variable, string value)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.Client.Game.SendLevelVarsSetPacket(context, variable, value);
            }

            //if (context.ContextTarget.Length > 0) {
            //    this.SendCommand("levelVars.set", context.ContextType.ToString().ToLower(), context.ContextTarget, variable, value);
            //}
            //else {
            //    this.SendCommand("levelVars.set", context.ContextType.ToString().ToLower(), variable, value);
            //}
        }

        private void GetSelectedLevelVariables()
        {
            this.InvokeIfRequired(() =>
            {
                LevelVariableContext selectedContext = this.GetSelectedContext();

                this.lsvEvaluatedEffect.Items.Clear();

                // If we just got a gamemode or level
                if (selectedContext.ContextType == LevelVariableContextType.GameMode || selectedContext.ContextType == LevelVariableContextType.Level)
                {
                    // Then we need the context of "all" to evaluate the effect.
                    this.GetLevelVariablesByContext(new LevelVariableContext(LevelVariableContextType.All, String.Empty));

                    // If we just got a level
                    if (selectedContext.ContextType == LevelVariableContextType.Level && this.cboSettingsLevels.SelectedItem != null)
                    {
                        // Then we need the context of the levels gamemode to evaluate the effect.
                        this.GetLevelVariablesByContext(new LevelVariableContext(LevelVariableContextType.GameMode, ((CMap)this.cboSettingsLevels.SelectedItem).PlayList));
                    }
                }

                this.GetLevelVariablesByContext(selectedContext);
            });
        }

        #endregion

        private void rdoSettingsLevelContextAll_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdoSettingsLevelContextAll.Checked == true)
            {
                this.rdoSettingsLevelContextAll.Font = new Font(this.Font, FontStyle.Bold);

                this.GetSelectedLevelVariables();
            }
            else
            {
                this.rdoSettingsLevelContextAll.Font = new Font(this.Font, FontStyle.Regular);
            }
        }

        private void rdoSettingsLevelContextGamemode_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdoSettingsLevelContextGamemode.Checked == true)
            {
                this.rdoSettingsLevelContextGamemode.Font = new Font(this.Font, FontStyle.Bold);

                this.GetSelectedLevelVariables();
            }
            else
            {
                this.rdoSettingsLevelContextGamemode.Font = new Font(this.Font, FontStyle.Regular);
            }

            this.cboSettingsGamemodes.Enabled = this.rdoSettingsLevelContextGamemode.Checked;
        }

        private void rdoSettingsLevelContextLevel_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdoSettingsLevelContextLevel.Checked == true)
            {
                this.rdoSettingsLevelContextLevel.Font = new Font(this.Font, FontStyle.Bold);

                this.GetSelectedLevelVariables();
            }
            else
            {
                this.rdoSettingsLevelContextLevel.Font = new Font(this.Font, FontStyle.Regular);
            }

            this.cboSettingsLevels.Enabled = this.rdoSettingsLevelContextLevel.Checked;
        }

        private void cboSettingsGamemodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.GetSelectedLevelVariables();
        }

        private void cboSettingsLevels_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.GetSelectedLevelVariables();
        }

        private void cboSettingsGamemodes_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {

                CMap mapDraw = ((CMap)cboSettingsGamemodes.Items[e.Index]);

                e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
                e.DrawBackground();
                e.DrawFocusRectangle();

                e.Graphics.DrawString(String.Format("{0}", mapDraw.GameMode), this.Font, SystemBrushes.WindowText, e.Bounds.Left + 5, e.Bounds.Top);
            }
        }

        private void cboSettingsLevels_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {

                CMap mapDraw = ((CMap)cboSettingsLevels.Items[e.Index]);

                e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
                e.DrawBackground();
                e.DrawFocusRectangle();

                e.Graphics.DrawString(String.Format("{0} - {1}", mapDraw.GameMode, mapDraw.PublicLevelName), this.Font, SystemBrushes.WindowText, e.Bounds.Left + 5, e.Bounds.Top);
            }
        }

        private void SetLevelVariable(LevelVariable levelVar)
        {
            this.InvokeIfRequired(() =>
            {
                object value = null;

                if (String.Compare(levelVar.VariableName, "vehiclesdisabled", true) == 0)
                {
                    value = levelVar.GetValue<bool>(false);
                }
                else
                {
                    value = levelVar.GetValue<decimal>(100);
                }

                string responseKey = String.Format("levelvars.set {0}", levelVar.VariableName.ToLower());

                this.OnSettingResponse(responseKey, value, true);

                if (this.AsyncSettingControls.ContainsKey(responseKey) == true)
                {
                    foreach (Control enableControl in this.AsyncSettingControls[responseKey].ma_ctrlEnabledInputs)
                    {
                        if ((enableControl is Label && !(enableControl is LinkLabel)) || enableControl is CheckBox)
                        {
                            enableControl.ForeColor = Color.LightSeaGreen;
                            enableControl.Font = new Font(this.Font, FontStyle.Bold);
                        }
                    }
                }
            });
        }

        #region Evaluated effects

        private void SetCurrentSelectedLevelVariables(List<LevelVariable> lstReturnedValues)
        {
            this.InvokeIfRequired(() =>
            {
                this.lblLevelTicketBleedSpeed.Font = this.lblLevelTickets.Font = this.lblLevelVehicleSpawnRate.Font = this.lblLevelStartDelay.Font = this.lblLevelRespawnDelay.Font = this.chkLevelVehiclesDisabled.Font = new Font(this.Font, FontStyle.Regular);

                this.lblLevelTicketBleedSpeed.ForeColor = this.lblLevelTickets.ForeColor = this.lblLevelVehicleSpawnRate.ForeColor = this.lblLevelStartDelay.ForeColor = this.lblLevelRespawnDelay.ForeColor = this.chkLevelVehiclesDisabled.ForeColor = SystemColors.WindowText;

                // Set all to default values.  This might be expanded on in the future to be context sensitive (e.g more spawn delay for SQDM)
                this.IgnoreEvents = true;
                this.numLevelTickets.Value = 100;
                this.numLevelTicketBleedSpeed.Value = 100;
                this.numLevelVehicleSpawnRate.Value = 100;
                this.numLevelStartDelay.Value = 10;
                this.numLevelRespawnDelay.Value = 20;
                this.chkLevelVehiclesDisabled.Checked = false;
                this.IgnoreEvents = false;

                foreach (LevelVariable levelVar in lstReturnedValues)
                {
                    this.SetLevelVariable(levelVar);
                }
            });
        }

        private void UpdateTotalEffects()
        {

            if (this.lsvEvaluatedEffect.Items.ContainsKey("totalEvaluatedEffects") == true)
            {
                ListViewItem effectsItem = this.lsvEvaluatedEffect.Items["totalEvaluatedEffects"];

                for (int iSubItem = 1; iSubItem < effectsItem.SubItems.Count; iSubItem++)
                {
                    for (int iContext = effectsItem.Index - 1; iContext >= 0; iContext--)
                    {

                        if (iContext < this.lsvEvaluatedEffect.Items[iContext].SubItems.Count)
                        {
                            if (String.Compare(this.lsvEvaluatedEffect.Items[iContext].SubItems[iSubItem].Text, "-") != 0)
                            {
                                effectsItem.SubItems[iSubItem].Text = this.lsvEvaluatedEffect.Items[iContext].SubItems[iSubItem].Text;
                                break;
                            }
                            else
                            {
                                effectsItem.SubItems[iSubItem].Text = "Default";
                            }
                        }
                    }
                }
            }
        }

        private void SetLevelVariablesToEffects(LevelVariable lvRequestedContext, List<LevelVariable> lstReturnedValues)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.lsvEvaluatedEffect.Items.ContainsKey(lvRequestedContext.Context.ToString()) == true)
                {
                    ListViewItem item = this.lsvEvaluatedEffect.Items[lvRequestedContext.Context.ToString()];
                    foreach (LevelVariable variable in lstReturnedValues)
                    {
                        if (item.SubItems.ContainsKey(variable.VariableName) == true)
                        {
                            item.SubItems[variable.VariableName].Text = variable.RawValue;
                        }
                    }

                    foreach (ColumnHeader col in this.lsvEvaluatedEffect.Columns)
                    {
                        col.Width = -2;
                    }

                    this.UpdateTotalEffects();
                }
            });
        }

        private ListViewItem CreateVariableEffectItem(string name, string text, Font font)
        {

            ListViewItem newItem;

            if (this.lsvEvaluatedEffect.Items.ContainsKey(name) == false)
            {

                newItem = new ListViewItem();
                newItem.Name = name;
                newItem.Text = text;
                newItem.UseItemStyleForSubItems = true;
                newItem.Font = font;

                ListViewItem.ListViewSubItem newSubItem = new ListViewItem.ListViewSubItem(newItem, "-");
                newSubItem.Name = "tickets";
                newItem.SubItems.Add(newSubItem);

                newSubItem = new ListViewItem.ListViewSubItem(newItem, "-");
                newSubItem.Name = "ticketBleedSpeed";
                newItem.SubItems.Add(newSubItem);

                newSubItem = new ListViewItem.ListViewSubItem(newItem, "-");
                newSubItem.Name = "vehicleSpawnRate";
                newItem.SubItems.Add(newSubItem);

                newSubItem = new ListViewItem.ListViewSubItem(newItem, "-");
                newSubItem.Name = "vehiclesDisabled";
                newItem.SubItems.Add(newSubItem);

                newSubItem = new ListViewItem.ListViewSubItem(newItem, "-");
                newSubItem.Name = "startDelay";
                newItem.SubItems.Add(newSubItem);

                newSubItem = new ListViewItem.ListViewSubItem(newItem, "-");
                newSubItem.Name = "respawnDelay";
                newItem.SubItems.Add(newSubItem);
            }
            else
            {
                newItem = this.lsvEvaluatedEffect.Items[name];
            }

            return newItem;
        }

        private void AddLevelVariablesToEffects(LevelVariable lvRequestedContext, List<LevelVariable> lstReturnedValues)
        {

            string strFriendlyContextName = lvRequestedContext.Context.ToString();

            if (lvRequestedContext.Context.ContextType == LevelVariableContextType.All)
            {
                strFriendlyContextName = this.Language.GetLocalized("uscServerSettingsPanel.lsvEvaluatedEffect.items.all");
            }
            else if (lvRequestedContext.Context.ContextType == LevelVariableContextType.GameMode)
            {
                strFriendlyContextName = this.Client.GetFriendlyGamemode(lvRequestedContext.Context.ContextTarget);
            }
            else if (lvRequestedContext.Context.ContextType == LevelVariableContextType.Level)
            {
                strFriendlyContextName = this.Client.GetFriendlyMapname(lvRequestedContext.Context.ContextTarget);
            }

            ListViewItem newItem = this.CreateVariableEffectItem(lvRequestedContext.Context.ToString(), strFriendlyContextName, this.Font);

            if (this.lsvEvaluatedEffect.Items.ContainsKey(newItem.Name) == false)
            {
                this.lsvEvaluatedEffect.Items.Add(newItem);
            }

            this.SetLevelVariablesToEffects(lvRequestedContext, lstReturnedValues);
        }

        private bool isApplicableContext(LevelVariable lvRequestedContext)
        {
            LevelVariableContext selectedContext = this.GetSelectedContext();

            bool returnIsApplicableContext = false;

            if (lvRequestedContext.Context.CompareTo(selectedContext) == 0)
            {
                returnIsApplicableContext = true;
            }
            else if (lvRequestedContext.Context.ContextType == LevelVariableContextType.All)
            {
                returnIsApplicableContext = true;
            }
            else if (lvRequestedContext.Context.ContextType == LevelVariableContextType.GameMode &&
                selectedContext.ContextType == LevelVariableContextType.Level)
            {

                string targetPlaylist = this.Client.GetPlaylistByMapname(selectedContext.ContextTarget);

                if (String.Compare(lvRequestedContext.Context.ContextTarget, targetPlaylist, true) == 0)
                {
                    returnIsApplicableContext = true;
                }
            }

            return returnIsApplicableContext;
        }

        private void ValidateAddLevelVariablesToEffects(LevelVariable lvRequestedContext, List<LevelVariable> lstReturnedValues)
        {
            this.InvokeIfRequired(() =>
            {
                LevelVariableContext selectedContext = this.GetSelectedContext();

                if (this.isApplicableContext(lvRequestedContext) == true)
                {
                    this.AddLevelVariablesToEffects(lvRequestedContext, lstReturnedValues);

                    if (lvRequestedContext.Context.CompareTo(selectedContext) == 0)
                    {

                        if (this.lsvEvaluatedEffect.Items.ContainsKey("totalEvaluatedEffects") == false)
                        {
                            this.lsvEvaluatedEffect.Items.Add(this.CreateVariableEffectItem("totalEvaluatedEffects", this.Language.GetLocalized("uscServerSettingsPanel.lsvEvaluatedEffect.items.totalEvaluatedEffects"), new Font(this.Font, FontStyle.Bold)));
                        }

                        this.UpdateTotalEffects();
                    }
                }

                foreach (ColumnHeader col in this.lsvEvaluatedEffect.Columns)
                {
                    col.Width = -2;
                }
            });
        }

        #endregion

        private void m_prcClient_LevelVariablesList(FrostbiteClient sender, LevelVariable lvRequestedContext, List<LevelVariable> lstReturnedValues)
        {
            LevelVariableContext selectedContext = this.GetSelectedContext();

            if (selectedContext.CompareTo(lvRequestedContext.Context) == 0)
            {

                this.SetCurrentSelectedLevelVariables(lstReturnedValues);
            }

            this.ValidateAddLevelVariablesToEffects(lvRequestedContext, lstReturnedValues);
        }

        #region Clear Level Settings

        private void m_prcClient_LevelVariablesClear(FrostbiteClient sender, LevelVariable lvRequestedContext)
        {

            LevelVariableContext selectedContext = this.GetSelectedContext();

            if (selectedContext.CompareTo(lvRequestedContext.Context) == 0)
            {
                this.GetSelectedLevelVariables();

                this.OnSettingResponse("levelvars.clear", true, true);
            }
        }

        private void btnClearLevelSettings_Click(object sender, EventArgs e)
        {
            this.WaitForSettingResponse("levelvars.clear", null);

            LevelVariableContext selectedContext = this.GetSelectedContext();

            if (this.Client != null && this.Client.Game != null)
            {
                this.Client.Game.SendLevelVarsClearPacket(selectedContext);

                //if (selectedContext.ContextTarget.Length > 0) {
                //    this.SendCommand("levelVars.clear", selectedContext.ContextType.ToString().ToLower(), selectedContext.ContextTarget);
                //}
                //else {
                //    this.SendCommand("levelVars.clear", selectedContext.ContextType.ToString().ToLower());
                //}
            }
        }

        #endregion

        #region Set level variables

        private void m_prcClient_LevelVariablesSet(FrostbiteClient sender, LevelVariable lvRequestedContext)
        {
            LevelVariableContext selectedContext = this.GetSelectedContext();

            // If they are an exact match (we or another account is editing what we are editing)
            if (selectedContext.CompareTo(lvRequestedContext.Context) == 0)
            {
                this.SetLevelVariable(lvRequestedContext);
            }

            // If the variable applies to our currently editing context.
            if (this.isApplicableContext(lvRequestedContext) == true)
            {
                this.SetLevelVariablesToEffects(lvRequestedContext, new List<LevelVariable>() { lvRequestedContext });
            }
        }

        private void lnkLevelTickets_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.numLevelTickets.Focus();
            this.WaitForSettingResponse("levelvars.set tickets", (decimal)100);

            this.SetLevelVariablesByContext(this.GetSelectedContext(), "tickets", this.numLevelTickets.Value.ToString());
        }

        private void lnkLevelTicketBleedSpeed_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.numLevelTicketBleedSpeed.Focus();
            this.WaitForSettingResponse("levelvars.set ticketbleedspeed", (decimal)100);

            this.SetLevelVariablesByContext(this.GetSelectedContext(), "ticketBleedSpeed", this.numLevelTicketBleedSpeed.Value.ToString());
        }

        private void lnkLevelVehicleSpawnRate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.numLevelVehicleSpawnRate.Focus();
            this.WaitForSettingResponse("levelvars.set vehiclespawnrate", (decimal)100);

            this.SetLevelVariablesByContext(this.GetSelectedContext(), "vehicleSpawnRate", this.numLevelVehicleSpawnRate.Value.ToString());
        }

        private void lnkLevelStartDelay_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.numLevelStartDelay.Focus();
            this.WaitForSettingResponse("levelvars.set startdelay", (decimal)100);

            this.SetLevelVariablesByContext(this.GetSelectedContext(), "startDelay", this.numLevelStartDelay.Value.ToString());
        }

        private void lnkLevelRespawnDelay_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.numLevelRespawnDelay.Focus();
            this.WaitForSettingResponse("levelvars.set respawndelay", (decimal)100);

            this.SetLevelVariablesByContext(this.GetSelectedContext(), "respawnDelay", this.numLevelRespawnDelay.Value.ToString());
        }

        private void chkLevelVehiclesDisabled_CheckedChanged(object sender, EventArgs e)
        {
            if (this.IgnoreEvents == false && this.AsyncSettingControls["levelvars.set vehiclesdisabled"].IgnoreEvent == false)
            {
                this.WaitForSettingResponse("levelvars.set vehiclesdisabled", !this.chkLevelVehiclesDisabled.Checked);

                this.SetLevelVariablesByContext(this.GetSelectedContext(), "vehiclesDisabled", Packet.Bltos(this.chkLevelVehiclesDisabled.Checked));

            }
        }

        #endregion

        #endregion
    }
}
