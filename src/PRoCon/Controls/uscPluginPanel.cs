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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using PRoCon.Controls.ControlsEx;
using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Remote;
using PRoCon.Forms;

namespace PRoCon.Controls {
    public partial class uscPluginPanel : UserControl {

        private uscServerConnection m_uscParent;
        private frmMain m_frmMain;
        private CLocalization m_clocLanguage;

        private PRoConClient m_prcClient;

        private CustomClass m_cscPluginVariables;

        public delegate void EventDelegate();
        public event EventDelegate ReloadPlugins;

        public delegate void PluginEventDelegate(PluginDetails spdPlugin);
        public event PluginEventDelegate PluginLoaded;
        public event PluginEventDelegate PluginVariablesAltered;

        public delegate PluginDetails GetPluginDetailsDelegate(string strClassName);
        public event GetPluginDetailsDelegate GetPluginDetails;

        public delegate void SetPluginVariableDelegate(string strClassName, string strVariable, string strValue);
        public event SetPluginVariableDelegate SetPluginVariable;

        public delegate void PluginEnabledDelegate(string strClassName, bool blEnabled);
        public event PluginEnabledDelegate PluginEnabled;
        
        public ListViewNF.ListViewItemCollection LoadedPlugins {
            get {
                return this.lsvLoadedPlugins.Items;
            }
        }


        #region css string
        private readonly string m_cssDescription = @"

/* RESET */

html,body,div,span,applet,object,iframe,h1,h2,h3,h4,h5,h6,p,blockquote,pre,a,abbr,acronym,address,big,cite,code,del,dfn,em,font,img,ins,kbd,q,s,samp,small,strike,strong,sub,sup,tt,var,dl,dt,dd,fieldset,form,label,legend,table,caption,tbody,tfoot,thead,tr,th,td{border:0;outline:0;vertical-align:baseline;background:transparent;margin:0;padding:0;}



/* BASIC */

*:focus{outline:none;}

.clear{clear:both;}

body{font-family:Tahoma;font-size:11px;color:#2C2C29;}

p{font-size:1.2em;padding:2px;margin:1px 0 15px;}

a{color:#807D7A;}

h1{background-color:#FFF;border-bottom:1px solid #DCDCDB;letter-spacing:-1px;font-size:24px;padding-bottom:3px;font-weight:400;margin:10px 0 3px 0;font-family:Vera, Helvetica, Georgia;}

h2{background-color:#FFF;border-bottom:1px solid #DCDCDB;letter-spacing:-1px;font-size:20px;padding-bottom:3px;font-weight:400;margin:10px 0 3px 0;font-family:Vera, Helvetica, Georgia;color:#3366ff;}

h2 a{font-weight:700;border:0;text-decoration:none;color:#3366ff;display:block;}

h3{font-size:1.3em;color:#3366ff;}

h4{font-size:1.1em}

h5{font-size:0.9em}

h6{font-size:0.7em}

hr{color:#DCDCDB;background-color:#DCDCDB;height:1px;border:0px;}

pre{width:100%; white-space:pre-wrap;}

ul li{list-style:circle;margin-bottom:4px;}

blockquote{margin:20px 10px 10px 5px;border-left:4px solid #DDD;padding:0 5px 0 5px;font-size:11px;text-align:justify;}

table{margin:.5em 0 1em;font-size:11px;}

table td,table th{text-align:left;border-right:1px solid #fff;padding:.4em .8em;}

table th{background-color:#5e5e5e;color:#fff;text-transform:uppercase;font-weight:bold;border-bottom:1px solid #e8e1c8;}

table td{background-color:#eee;}

table th a{color:#d6f325;}

table th a:hover{color:#fff;}

table tr.even td{background-color:#ddd;}

table tr:hover td{background-color:#fff;}

table.nostyle td,table.nostyle th,table.nostyle tr.even td,table.nostyle tr:hover td{border:0;background:none;background-color:transparent;}
";
        #endregion

        private bool m_blLocalPlugins;
        [Category("PRoCon Settings"), Description("The control is used for local plugins or remote")]
        public bool LocalPlugins {
            set {
                //this.spltPlugins.Panel2Collapsed = value;
                this.lnkReloadPlugins.Visible = !value;

                this.m_blLocalPlugins = value;
            }
            get {
                return this.m_blLocalPlugins;
            }
        }

        private AssemblyBuilder m_asmBuilder;
        private ModuleBuilder m_modBuilder;
        private Dictionary<string, Enum> m_dicGeneratedEnums;

        public uscPluginPanel() {
            InitializeComponent();

            this.m_frmMain = null;
            this.m_uscParent = null;

            if (this.webDescription.Document == null) {
                this.webDescription.Navigate("about:blank");
                this.webDescription.Document.Window.Name = "hi";
            }
            this.webDescription.Navigating += new WebBrowserNavigatingEventHandler(webDescription_Navigating);

            this.m_asmBuilder = Thread.GetDomain().DefineDynamicAssembly(new AssemblyName("PRoConPluginEnumAssembly"), AssemblyBuilderAccess.Run);
            this.m_modBuilder = this.m_asmBuilder.DefineDynamicModule("PRoConPluginEnumModule");
            this.m_dicGeneratedEnums = new Dictionary<string, Enum>();

            this.m_cscPluginVariables = new CustomClass();

            this.m_blLocalPlugins = true;

            this.rtbScriptConsole.Flushed += new Action<object, EventArgs>(rtbScriptConsole_Flushed);

            this.lsvLoadedPlugins.CreateGraphics();
        }

        void webDescription_Navigating(object sender, WebBrowserNavigatingEventArgs e) {
            e.Cancel = true;
        }

        public void Initialize(frmMain frmMainWindow, uscServerConnection uscParent) {

            this.m_frmMain = frmMainWindow;
            this.m_uscParent = uscParent;

            this.tbcPluginDetails.ImageList = this.m_frmMain.iglIcons;
            this.lsvLoadedPlugins.SmallImageList = this.m_frmMain.iglIcons;

            this.tabPluginDetails.ImageKey = "information.png";
            this.tabPluginSettings.ImageKey = "plugin_edit.png";
        }

        public void SetConnection(PRoConClient prcClient) {
            if ((this.m_prcClient = prcClient) != null) {

            }
        }

        public void SetColour(string strVariable, string strValue) {
            this.rtbScriptConsole.SetColour(strVariable, strValue);
        }

        public void SetLocalization(CLocalization clocLanguage) {
            this.m_clocLanguage = clocLanguage;

            //this.tbpPlugins.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.tbpPlugins.Title", null);

            this.lblLoadedPlugins.Text = this.m_clocLanguage.GetLocalized("uscPluginPanel.lblLoadedPlugins", null);
            this.lnkReloadPlugins.Text = this.m_clocLanguage.GetLocalized("uscPluginPanel.lnkReloadPlugins", null);
            //this.lnkReloadPlugins.LinkArea = new LinkArea(0, this.lnkReloadPlugins.Text.Length);
            this.lnkMorePlugins.Text = this.m_clocLanguage.GetLocalized("uscPluginPanel.lnkMorePlugins", null);

            this.tabPluginDetails.Text = this.m_clocLanguage.GetLocalized("uscPluginPanel.tabPluginDetails", null);
            //this.lblPluginName.Text = this.m_clocLanguage.GetLocalized("uscPluginPanel.tabPluginDetails.lblPluginName", null);
            //this.lblPluginAuthor.Text = this.m_clocLanguage.GetLocalized("uscPluginPanel.tabPluginDetails.lblPluginAuthor", null);
            //this.lblPluginDescription.Text = this.m_clocLanguage.GetLocalized("uscPluginPanel.tabPluginDetails.lblPluginDescription", null);
            //this.lblPluginVersion.Text = this.m_clocLanguage.GetLocalized("uscPluginPanel.tabPluginDetails.lblPluginVersion", null);
            //this.lblPluginWebsite.Text = this.m_clocLanguage.GetLocalized("uscPluginPanel.tabPluginDetails.lblPluginWebsite", null);

            this.tabPluginSettings.Text = this.m_clocLanguage.GetLocalized("uscPluginPanel.tabPluginSettings", null);
        }

        public event uscServerConnection.OnTabChangeDelegate OnTabChange;

        private bool m_blSettingTab = false;
        private void tbcPluginDetails_SelectedIndexChanged(object sender, EventArgs e) {
            if (this.m_blSettingTab == false && this.OnTabChange != null) {
                Stack<string> stkTabIndexes = new Stack<string>();

                if (lsvLoadedPlugins.SelectedItems.Count > 0) {
                    stkTabIndexes.Push(lsvLoadedPlugins.SelectedItems[0].Name);
                }
                else {
                    stkTabIndexes.Push("");
                }

                stkTabIndexes.Push(tbcPluginDetails.SelectedTab.Name);

                this.OnTabChange(this, stkTabIndexes);
                this.m_blSettingTab = false;
            }
        }

        public void SetTabIndexes(Stack<string> stkTabIndexes) {

            this.m_blSettingTab = true;

            if (stkTabIndexes.Count > 0 && tbcPluginDetails.TabPages.ContainsKey(stkTabIndexes.Peek()) == true) {
                this.tbcPluginDetails.SelectedTab = tbcPluginDetails.TabPages[stkTabIndexes.Pop()];
            }

            if (stkTabIndexes.Count > 0 && lsvLoadedPlugins.Items.ContainsKey(stkTabIndexes.Peek()) == true) {
                lsvLoadedPlugins.Items[stkTabIndexes.Pop()].Selected = true;
            }

        }

        public void Write(DateTime dtLoggedTime, string strPluginConsoleOutput) {
            this.rtbScriptConsole.AppendText(String.Format("[{0}] {1}{2}", dtLoggedTime.ToString("HH:mm:ss ff"), strPluginConsoleOutput, "\n"));
        }

        private void rtbScriptConsole_Flushed(object arg1, EventArgs arg2) {
            this.rtbScriptConsole.ScrollToCaret();

            this.rtbScriptConsole.TrimLines(this.m_prcClient.Variables.GetVariable<int>("MAX_PLUGINCONSOLE_LINES", 75));
        }

        public ListViewItem IsLoadedPlugin(string strClassName) {
            ListViewItem lviLoadedPlugin = null;

            foreach (ListViewItem lviPlugin in this.lsvLoadedPlugins.Items) {
                if (lviPlugin.Tag != null && ((PluginDetails)lviPlugin.Tag).ClassName.CompareTo(strClassName) == 0) {
                    lviLoadedPlugin = lviPlugin;
                }
            }

            return lviLoadedPlugin;
        }

        private bool m_blSupressDisabledEvent = false;

        public void SetEnabledPlugins(List<string> lstClassNames) {

            ListViewItem lviPlugin = null;

            foreach (string strClassName in lstClassNames) {
                if ((lviPlugin = IsLoadedPlugin(strClassName)) != null) {
                    lviPlugin.Checked = true;
                }
            }

        }

        public void SetLoadedPlugins(List<string> lstClassNames) {

            ListViewItem lviPlugin = null;

            foreach (string strClassName in new List<string>(lstClassNames)) {

                PluginDetails spdDetails = this.GetPluginDetails(strClassName);

                if ((lviPlugin = IsLoadedPlugin(strClassName)) == null) {

                    ListViewItem lviNewItem = new ListViewItem(spdDetails.Name);
                    lviNewItem.Tag = spdDetails;
                    lviNewItem.Name = strClassName;
                    lviNewItem.ImageKey = "plugin_disabled.png";

                    //this.m_uscParent.ThrowEvent(this, uscEventsPanel.CapturableEvents.PluginLoaded, new string[] { strClassName });
                    this.m_blSupressDisabledEvent = true;

                    lsvLoadedPlugins.Items.Add(lviNewItem);

                    
                }
                else {
                    lviPlugin.Text = spdDetails.Name;
                    lviPlugin.Tag = spdDetails;
                }

                if (this.PluginLoaded != null) {
                    this.PluginLoaded(spdDetails);
                }

                this.lsvLoadedPlugins_SelectedIndexChanged(this, null);

                //foreach (KeyValuePair<string, CPRoConLayerClient> kvpConnection in this.m_dicLayerClients) {
                //    kvpConnection.Value.OnPluginLoaded(spdDetails);
                //}

            }

            foreach (ColumnHeader column in this.lsvLoadedPlugins.Columns) {
                column.Width = -2;
            }

        }

        private Enum GenerateEnum(string enumName, string[] literals) {

            Enum returnEnum = null;

            try {

                if (this.m_dicGeneratedEnums.ContainsKey(enumName) == false) {

                    EnumBuilder enumBuilder = m_modBuilder.DefineEnum(enumName, TypeAttributes.Public, typeof(System.Int32));
                    //string[] al = { "en-US", "en-UK", "ar-SA", "da-DK", "French", "Cantonese" };
                    for (int i = 0; i < literals.Length; i++) {
                        // here al is an array list with a list of string values
                        if (literals[i].ToString().Length > 0) {
                            enumBuilder.DefineLiteral(literals[i].ToString(), i);
                        }
                    }

                    Type enumType = enumBuilder.CreateType();
                    returnEnum = (Enum)Activator.CreateInstance(enumType);

                    this.m_dicGeneratedEnums.Add(enumName, returnEnum);
                }
                else {
                    returnEnum = this.m_dicGeneratedEnums[enumName];
                }
            }
            catch (Exception e) {
                FrostbiteConnection.LogError("uscPluginPanel.GenerateEnum", enumName + " " + String.Join("|", literals), e);
            }

            return returnEnum;
        }

        private void SetPluginsVariables(string strClassName, string strPluginName, List<CPluginVariable> lstVariables) {

            if (lstVariables != null) {
                foreach (CPluginVariable cpvVariable in lstVariables) {

                    string strCategoryName = strPluginName;
                    string strVariableName = cpvVariable.Name;
                    bool blVariableReadOnly = cpvVariable.ReadOnly;

                    string[] a_strVariable = cpvVariable.Name.Split(new char[] { '|' }, 2);
                    if (a_strVariable.Length == 2) {
                        strCategoryName = a_strVariable[0];
                        strVariableName = a_strVariable[1];
                    }

                    Enum generatedEnum = null;
                    Match isGeneratedEnum;
                    if ((isGeneratedEnum = Regex.Match(cpvVariable.Type, @"enum.(?<enumname>.*?)\((?<literals>.*)\)")).Success == true) {
                        if ((generatedEnum = this.GenerateEnum(isGeneratedEnum.Groups["enumname"].Value, isGeneratedEnum.Groups["literals"].Value.Split('|'))) != null) {
                            string variableValue = cpvVariable.Value;

                            if (Enum.IsDefined(generatedEnum.GetType(), variableValue) == false) {
                                string[] a_Names = Enum.GetNames(generatedEnum.GetType());

                                if (a_Names.Length > 0) {
                                    variableValue = a_Names[0];
                                }
                            }

                            if (Enum.IsDefined(generatedEnum.GetType(), variableValue) == true) {
                                if (this.m_cscPluginVariables.ContainsKey(cpvVariable.Name) == false) {
                                    this.m_cscPluginVariables.Add(new CustomProperty(strVariableName, strCategoryName, strClassName, Enum.Parse(generatedEnum.GetType(), variableValue), generatedEnum.GetType(), blVariableReadOnly, true));
                                }
                                else {
                                    this.m_cscPluginVariables[cpvVariable.Name].Value = Enum.Parse(generatedEnum.GetType(), variableValue);
                                }
                            }
                            
                        }

                    }
                    else {

                        switch (cpvVariable.Type) {
                            case "bool":
                                bool blTryBool;
                                if (bool.TryParse(cpvVariable.Value, out blTryBool) == true) {
                                    if (this.m_cscPluginVariables.ContainsKey(cpvVariable.Name) == false) {
                                        this.m_cscPluginVariables.Add(new CustomProperty(strVariableName, strCategoryName, strClassName, blTryBool, typeof(bool), blVariableReadOnly, true));
                                    }
                                    else {
                                        this.m_cscPluginVariables[cpvVariable.Name].Value = blTryBool;
                                    }
                                }
                                break;
                            case "onoff":
                                if (Enum.IsDefined(typeof(enumBoolOnOff), cpvVariable.Value) == true) {

                                    if (this.m_cscPluginVariables.ContainsKey(cpvVariable.Name) == false) {
                                        this.m_cscPluginVariables.Add(new CustomProperty(strVariableName, strCategoryName, strClassName, Enum.Parse(typeof(enumBoolOnOff), cpvVariable.Value), typeof(enumBoolOnOff), blVariableReadOnly, true));
                                    }
                                    else {
                                        this.m_cscPluginVariables[cpvVariable.Name].Value = Enum.Parse(typeof(enumBoolOnOff), cpvVariable.Value);
                                    }
                                }
                                break;
                            case "yesno":
                                if (Enum.IsDefined(typeof(enumBoolYesNo), cpvVariable.Value) == true) {
                                    if (this.m_cscPluginVariables.ContainsKey(cpvVariable.Name) == false) {
                                        this.m_cscPluginVariables.Add(new CustomProperty(strVariableName, strCategoryName, strClassName, Enum.Parse(typeof(enumBoolYesNo), cpvVariable.Value), typeof(enumBoolYesNo), blVariableReadOnly, true));
                                    }
                                    else {
                                        this.m_cscPluginVariables[cpvVariable.Name].Value = Enum.Parse(typeof(enumBoolYesNo), cpvVariable.Value);
                                    }
                                }
                                break;
                            case "int":
                                int iTryInt;
                                if (int.TryParse(cpvVariable.Value, out iTryInt) == true) {
                                    if (this.m_cscPluginVariables.ContainsKey(cpvVariable.Name) == false) {
                                        this.m_cscPluginVariables.Add(new CustomProperty(strVariableName, strCategoryName, strClassName, iTryInt, typeof(int), blVariableReadOnly, true));
                                    }
                                    else {
                                        this.m_cscPluginVariables[cpvVariable.Name].Value = iTryInt;
                                    }
                                }
                                break;
                            case "double":
                                double dblTryDouble;
                                if (double.TryParse(cpvVariable.Value, out dblTryDouble) == true) {
                                    if (this.m_cscPluginVariables.ContainsKey(cpvVariable.Name) == false) {
                                        this.m_cscPluginVariables.Add(new CustomProperty(strVariableName, strCategoryName, strClassName, dblTryDouble, typeof(double), blVariableReadOnly, true));
                                    }
                                    else {
                                        this.m_cscPluginVariables[cpvVariable.Name].Value = dblTryDouble;
                                    }
                                }
                                break;
                            case "string":
                                if (this.m_cscPluginVariables.ContainsKey(cpvVariable.Name) == false) {
                                    this.m_cscPluginVariables.Add(new CustomProperty(strVariableName, strCategoryName, strClassName, CPluginVariable.Decode(cpvVariable.Value), typeof(String), blVariableReadOnly, true));
                                }
                                else {

                                    this.m_cscPluginVariables[cpvVariable.Name].Value = cpvVariable.Value;
                                }
                                break;
                            case "multiline":
                                if (this.m_cscPluginVariables.ContainsKey(cpvVariable.Name) == false) {
                                    CustomProperty cptNewProperty = new CustomProperty(strVariableName, strCategoryName, strClassName, CPluginVariable.Decode(cpvVariable.Value), typeof(String), blVariableReadOnly, true);

                                    cptNewProperty.Attributes = new AttributeCollection( new EditorAttribute(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor)), new TypeConverterAttribute(typeof(System.ComponentModel.Design.MultilineStringEditor)) );
                                    this.m_cscPluginVariables.Add(cptNewProperty);
                                }
                                else {
                                    this.m_cscPluginVariables[cpvVariable.Name].Value = cpvVariable.Value;
                                }
                                break;
                            case "stringarray":
                                if (this.m_cscPluginVariables.ContainsKey(cpvVariable.Name) == false) {
                                    this.m_cscPluginVariables.Add(new CustomProperty(strVariableName, strCategoryName, strClassName, CPluginVariable.DecodeStringArray(cpvVariable.Value), typeof(string[]), blVariableReadOnly, true));

                                    //this.m_cscPluginVariables.Add(new CustomProperty(cpvVariable.Name, strPluginName, strClassName, "Alaska", typeof(StatesList), false, true));
                                }
                                else {
                                    this.m_cscPluginVariables[cpvVariable.Name].Value = CPluginVariable.DecodeStringArray(cpvVariable.Value);
                                }

                                break;
                        }
                        //                }
                    }
                }
            }

            this.ppgScriptSettings.Refresh();
        }


        private void ppgScriptSettings_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) {

            if (this.m_cscPluginVariables.ContainsKey(e.ChangedItem.Label) == true) {

                string strValue = e.ChangedItem.Value.ToString();

                if (this.m_cscPluginVariables[e.ChangedItem.Label].Type == typeof(bool)) {
                    strValue = strValue.ToLower();
                }
                //               else if (this.m_cscPluginVariables[e.ChangedItem.Label].Type == typeof(string)) {
                //                   strValue = strValue;
                //               }
                else if (this.m_cscPluginVariables[e.ChangedItem.Label].Type == typeof(string[])) {
                    strValue = CPluginVariable.EncodeStringArray((string[])e.ChangedItem.Value);

                }

                // TO DO: Set the script variable.
                //this.m_frmMainParent.SetSingleSv(this.m_cscPluginVariables[e.ChangedItem.Label].ClassName, e.ChangedItem.Label, strValue);

                this.SetPluginVariable(this.m_cscPluginVariables[e.ChangedItem.Label].ClassName, e.ChangedItem.Label, strValue);

                //this.lsvLoadedPlugins_SelectedIndexChanged(this, null);
                this.RefreshSelectedPlugin();

                if (this.m_cscPluginVariables.ContainsKey(e.ChangedItem.Label) == true) {

                    PluginDetails spdUpdatedDetails = this.GetPluginDetails(this.m_cscPluginVariables[e.ChangedItem.Label].ClassName);
                    if (this.PluginVariablesAltered != null) {
                        this.PluginVariablesAltered(spdUpdatedDetails);
                    }

                }

                //foreach (KeyValuePair<string, CPRoConLayerClient> kvpConnection in this.m_dicLayerClients) {
                //    kvpConnection.Value.OnPluginVariablesAltered(spdUpdatedDetails);
                //}

            }
        }

        public void RefreshPlugin() {
            //this.lsvLoadedPlugins_SelectedIndexChanged(this, null);
            this.RefreshSelectedPlugin();
        }

        private void RefreshSelectedPlugin() {
            if (this.lsvLoadedPlugins.SelectedItems.Count > 0) {
                //this.tbcPluginDetails.Enabled = true;

                PluginDetails spdDetails = this.GetPluginDetails(((PluginDetails)lsvLoadedPlugins.SelectedItems[0].Tag).ClassName);

                this.lsvLoadedPlugins.SelectedItems[0].Tag = spdDetails;

                //this.txtPluginName.Text = spdDetails.m_strName;
                //this.txtPluginAuthor.Text = spdDetails.m_strAuthor;
                //this.txtPluginVersion.Text = spdDetails.m_strVersion;
                //this.txtPluginDescription.Text = spdDetails.m_strDescription;

                HtmlDocument document = this.webDescription.Document.OpenNew(true);

                string html = String.Format(@"<html><head><style>{0}</style></head><body><h1>{1} - {2}</h1>{3}: <a href=""http://{4}"" target=""_blank"">{5}</a><br><br>{6}</body></html>", this.m_cssDescription, spdDetails.Name, spdDetails.Version, this.m_clocLanguage.GetLocalized("uscPluginPanel.tabPluginDetails.lblPluginAuthor"), spdDetails.Website, spdDetails.Author, spdDetails.Description != null ? spdDetails.Description : String.Empty); // .Replace(Environment.NewLine, @"<br>")
                document.Write(html);

                //this.lklPluginWebsite.Text = spdDetails.m_strWebsite;
                //this.lklPluginWebsite.Tag = spdDetails.m_strWebsite;
                //this.lklPluginWebsite.LinkArea = new LinkArea(0, spdDetails.m_strWebsite.Length);

                this.m_cscPluginVariables.Clear();
                this.SetPluginsVariables(spdDetails.ClassName, spdDetails.Name, spdDetails.DisplayPluginVariables);
            }
            else if (this.lsvLoadedPlugins.FocusedItem != null) {
                //this.tbcPluginDetails.Enabled = false;

                //this.txtPluginName.Text = String.Empty;
                //this.txtPluginAuthor.Text = String.Empty;
                //this.txtPluginVersion.Text = String.Empty;
                //this.txtPluginDescription.Text = String.Empty;

                //this.lklPluginWebsite.Text = String.Empty;

                this.m_cscPluginVariables.Clear();
                this.ppgScriptSettings.Refresh();
            }
        }

        private void lsvLoadedPlugins_SelectedIndexChanged(object sender, EventArgs e) {

            // Start up optimization, takes 100ms at startup to assign this is ctor so now
            // it's set here when the user first selects a plugin to display.  They won't notice it at all there.
            if (this.ppgScriptSettings.SelectedObject == null ) {
                this.ppgScriptSettings.SelectedObject = this.m_cscPluginVariables;
            }

            //if (this.m_prcClient != null && this.m_prcClient.Plugins != null) {
                this.RefreshSelectedPlugin();
            //}
            /*
            if (this.lsvLoadedPlugins.SelectedItems.Count > 0) {
                //this.tbcPluginDetails.Enabled = true;

                SPluginDetails spdDetails = this.GetPluginDetails(((SPluginDetails)lsvLoadedPlugins.SelectedItems[0].Tag).m_strClassName);

                this.lsvLoadedPlugins.SelectedItems[0].Tag = spdDetails;

                this.txtPluginName.Text = spdDetails.m_strName;
                this.txtPluginAuthor.Text = spdDetails.m_strAuthor;
                this.txtPluginVersion.Text = spdDetails.m_strVersion;
                this.txtPluginDescription.Text = spdDetails.m_strDescription;

                this.lklPluginWebsite.Text = spdDetails.m_strWebsite;
                this.lklPluginWebsite.Tag = spdDetails.m_strWebsite;
                this.lklPluginWebsite.LinkArea = new LinkArea(0, spdDetails.m_strWebsite.Length);

                this.m_cscPluginVariables.Clear();
                this.SetPluginsVariables(spdDetails.m_strClassName, spdDetails.m_strName, spdDetails.m_lstDisplayPluginVariables);
            }
            else if (this.lsvLoadedPlugins.FocusedItem != null) {
                //this.tbcPluginDetails.Enabled = false;

                this.txtPluginName.Text = String.Empty;
                this.txtPluginAuthor.Text = String.Empty;
                this.txtPluginVersion.Text = String.Empty;
                this.txtPluginDescription.Text = String.Empty;

                this.lklPluginWebsite.Text = String.Empty;

                this.m_cscPluginVariables.Clear();
                this.ppgScriptSettings.Refresh();
            }
            */
            if (this.lsvLoadedPlugins.FocusedItem != null) {
                this.m_blSettingTab = false;
                this.tbcPluginDetails_SelectedIndexChanged(null, null);
            }
        }

        //private void lklPluginWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {

        //    if (this.lklPluginWebsite.Tag != null) {

        //        string strLink = this.lklPluginWebsite.Tag.ToString();

        //        if (Regex.Match(strLink, "^http://.*?$").Success == false) {
        //            strLink = "http://" + strLink;
        //        }

        //        System.Diagnostics.Process.Start(strLink);
        //    }
        //}

        private void lsvLoadedPlugins_ItemChecked(object sender, ItemCheckedEventArgs e) {
            if (e.Item.Tag != null && this.m_blSupressDisabledEvent == false) {
                if (e.Item.Checked == true) {

                    if (this.PluginEnabled != null) {
                        this.PluginEnabled(((PluginDetails)e.Item.Tag).ClassName, true);
                        e.Item.ImageKey = "plugin.png";
                    }

                    /*
                    this.m_prcConnection.EnablePlugin(((SPluginDetails)e.Item.Tag).m_strClassName);

                    foreach (KeyValuePair<string, CPRoConLayerClient> kvpConnection in this.m_dicLayerClients) {
                        kvpConnection.Value.OnPluginEnabled(((SPluginDetails)e.Item.Tag).m_strClassName, true);
                    }
                    */
                }
                else {

                    if (this.PluginEnabled != null) {
                        this.PluginEnabled(((PluginDetails)e.Item.Tag).ClassName, false);
                        
                        e.Item.ImageKey = "plugin_disabled.png";
                    }

                    /*
                    this.m_prcConnection.DisablePlugin(((SPluginDetails)e.Item.Tag).m_strClassName);

                    foreach (KeyValuePair<string, CPRoConLayerClient> kvpConnection in this.m_dicLayerClients) {
                        kvpConnection.Value.OnPluginEnabled(((SPluginDetails)e.Item.Tag).m_strClassName, false);
                    }
                    */
                }
            }

            this.m_blSupressDisabledEvent = false;
        }

        private void lnkReloadPlugins_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {

            if (this.ReloadPlugins != null) {
                this.rtbScriptConsole.Text = String.Empty;

                this.ReloadPlugins();
            }

            //this.m_prcConnection.CompilePlugins();
        }

        private void lnkMorePlugins_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://myrcon.net/forum/9-plugins/");
        }

        private void uscPluginPanel_Resize(object sender, EventArgs e) {
            Rectangle tabBounds = this.tbcPluginDetails.Bounds;
            tabBounds.Width = this.Bounds.Width - tabBounds.X - 5;
            this.tbcPluginDetails.SetBounds(tabBounds.X, tabBounds.Y, tabBounds.Width, tabBounds.Height);

            this.lsvLoadedPlugins.Height = this.spltPlugins.Panel1.Height - 50;

            try {
                this.spltPlugins.SplitterDistance = (int)(this.spltPlugins.Bounds.Height * 0.8);
            }
            catch (Exception) { }
        }

    }
}
