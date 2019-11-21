using PRoCon.Core;
using PRoCon.Core.Remote;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;

namespace PRoCon.Controls
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public partial class uscPage : UserControl
    {

        protected Dictionary<string, AsyncStyleSetting> AsyncSettingControls
        {
            get;
            private set;
        }

        public Image SettingLoading
        {
            get;
            set;
        }

        public Image SettingFail
        {
            get;
            set;
        }

        public Image SettingSuccess
        {
            get;
            set;
        }

        public uscPage()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);

            this.AsyncSettingControls = new Dictionary<string, AsyncStyleSetting>();
        }

        public virtual void SetLocalization(CLocalization clocLanguage)
        {

        }

        public virtual void SetConnection(PRoConClient prcClient)
        {

        }

        #region Settings Animator

        private void SetControlValue(Control ctrlTarget, object objValue)
        {

            if (objValue != null)
            {
                if (ctrlTarget is TextBox)
                {
                    ((TextBox)ctrlTarget).Text = (string)objValue;
                }
                else if (ctrlTarget is CheckBox)
                {
                    ((CheckBox)ctrlTarget).Checked = (bool)objValue;
                }
                else if (ctrlTarget is NumericUpDown)
                {

                    if (((NumericUpDown)ctrlTarget).Minimum > (decimal)objValue)
                    {
                        ((NumericUpDown)ctrlTarget).Value = ((NumericUpDown)ctrlTarget).Minimum;
                    }
                    else if (((NumericUpDown)ctrlTarget).Maximum < (decimal)objValue)
                    {
                        ((NumericUpDown)ctrlTarget).Value = ((NumericUpDown)ctrlTarget).Maximum;
                    }
                    else
                    {
                        ((NumericUpDown)ctrlTarget).Value = (decimal)objValue;
                    }
                }
                else if (ctrlTarget is Label)
                {
                    ((Label)ctrlTarget).Text = (string)objValue;
                }
            }
        }

        protected void WaitForSettingResponse(string strResponseCommand, object objOriginalValue)
        {

            if (this.AsyncSettingControls.ContainsKey(strResponseCommand) == true)
            {
                this.AsyncSettingControls[strResponseCommand].m_objOriginalValue = objOriginalValue;

                this.AsyncSettingControls[strResponseCommand].m_picStatus.Image = this.SettingLoading;
                //this.m_dicAsyncSettingControls[strResponseCommand].m_iImageIndex = CAsyncSetting.INT_ICON_ANIMATEDSETTING_START;
                this.AsyncSettingControls[strResponseCommand].m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_TIMEOUT_TICKS;

                this.tmrTimeoutCheck.Enabled = true;

                foreach (Control ctrlEnable in this.AsyncSettingControls[strResponseCommand].ma_ctrlEnabledInputs)
                {
                    if (ctrlEnable is TextBox)
                    {
                        ((TextBox)ctrlEnable).ReadOnly = true;
                    }
                    else if (ctrlEnable is NumericUpDown)
                    {
                        ((NumericUpDown)ctrlEnable).ReadOnly = true;
                    }
                    else
                    {
                        ctrlEnable.Enabled = false;
                    }
                }
            }
        }

        protected void WaitForSettingResponse(string strResponseCommand)
        {

            if (this.AsyncSettingControls.ContainsKey(strResponseCommand) == true)
            {
                //this.m_dicAsyncSettingControls[strResponseCommand].m_objOriginalValue = String.Empty;
                this.AsyncSettingControls[strResponseCommand].m_picStatus.Image = this.SettingLoading;
                this.AsyncSettingControls[strResponseCommand].m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_TIMEOUT_TICKS;

                this.tmrTimeoutCheck.Enabled = true;

                foreach (Control ctrlEnable in this.AsyncSettingControls[strResponseCommand].ma_ctrlEnabledInputs)
                {
                    if (ctrlEnable is TextBox)
                    {
                        ((TextBox)ctrlEnable).ReadOnly = true;
                    }
                    else
                    {
                        ctrlEnable.Enabled = false;
                    }
                }
            }
        }

        public void OnSettingResponse(string strResponseCommand, bool blSuccess)
        {

            if (this.AsyncSettingControls.ContainsKey(strResponseCommand) == true)
            {

                if (this.AsyncSettingControls[strResponseCommand].m_blReEnableControls == true)
                {
                    foreach (Control ctrlEnable in this.AsyncSettingControls[strResponseCommand].ma_ctrlEnabledInputs)
                    {
                        if (ctrlEnable is TextBox)
                        {
                            ((TextBox)ctrlEnable).ReadOnly = false;
                        }
                        else
                        {
                            ctrlEnable.Enabled = true;
                        }
                    }
                }

                this.AsyncSettingControls[strResponseCommand].IgnoreEvent = true;

                if (blSuccess == true)
                {
                    this.AsyncSettingControls[strResponseCommand].m_picStatus.Image = this.SettingSuccess;
                    this.AsyncSettingControls[strResponseCommand].m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_SHOWRESULT_TICKS;
                    this.AsyncSettingControls[strResponseCommand].m_blSuccess = true;
                }
                else
                {
                    this.AsyncSettingControls[strResponseCommand].m_picStatus.Image = this.SettingFail;
                    this.AsyncSettingControls[strResponseCommand].m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_SHOWRESULT_TICKS;
                    this.AsyncSettingControls[strResponseCommand].m_blSuccess = false;
                }

                this.tmrTimeoutCheck.Enabled = true;

                this.AsyncSettingControls[strResponseCommand].IgnoreEvent = false;
            }
        }

        public void OnSettingResponse(string strResponseCommand, object objValue, bool blSuccess)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.AsyncSettingControls.ContainsKey(strResponseCommand) == true)
                {

                    foreach (Control ctrlEnable in this.AsyncSettingControls[strResponseCommand].ma_ctrlEnabledInputs)
                    {
                        if (ctrlEnable is TextBox)
                        {
                            ((TextBox)ctrlEnable).ReadOnly = false;
                        }
                        else if (ctrlEnable is NumericUpDown)
                        {
                            ((NumericUpDown)ctrlEnable).ReadOnly = false;
                        }
                        else
                        {
                            ctrlEnable.Enabled = true;
                        }
                    }

                    this.AsyncSettingControls[strResponseCommand].IgnoreEvent = true;

                    if (blSuccess == true)
                    {
                        this.SetControlValue(this.AsyncSettingControls[strResponseCommand].m_ctrlResponseTarget, objValue);
                        this.AsyncSettingControls[strResponseCommand].m_picStatus.Image = this.SettingSuccess;
                        //this.m_dicAsyncSettingControls[strResponseCommand].m_iImageIndex = CAsyncSetting.INT_ICON_ANIMATEDSETTING_SET_SUCCESS;
                        this.AsyncSettingControls[strResponseCommand].m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_SHOWRESULT_TICKS;

                        this.AsyncSettingControls[strResponseCommand].m_blSuccess = true;
                    }
                    else
                    {
                        this.SetControlValue(this.AsyncSettingControls[strResponseCommand].m_ctrlResponseTarget, this.AsyncSettingControls[strResponseCommand].m_objOriginalValue);
                        this.AsyncSettingControls[strResponseCommand].m_picStatus.Image = this.SettingFail;
                        //this.m_dicAsyncSettingControls[strResponseCommand].m_iImageIndex = CAsyncSetting.INT_ICON_ANIMATEDSETTING_SET_FAILURE;
                        this.AsyncSettingControls[strResponseCommand].m_blSuccess = false;
                        if (objValue != null)
                        {
                            this.AsyncSettingControls[strResponseCommand].m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_SHOWRESULT_TICKS;
                        }
                        else
                        {
                            // TO DO: objValue will hold the error recieved from BFBC2 server
                        }
                    }

                    this.tmrTimeoutCheck.Enabled = true;

                    this.AsyncSettingControls[strResponseCommand].IgnoreEvent = false;
                }
            });
        }

        private int CountTicking()
        {
            int i = 0;

            foreach (KeyValuePair<string, AsyncStyleSetting> kvpAsync in this.AsyncSettingControls)
            {
                if (kvpAsync.Value.m_iTimeout >= 0)
                {
                    i++;
                }
            }

            return i;
        }

        private void tmrSettingsAnimator_Tick(object sender, EventArgs e)
        {
            //if (((from o in this.m_dicAsyncSettingControls where o.Value.m_iTimeout >= 0 select o).Count()) > 0) {
            if (this.CountTicking() > 0)
            {
                foreach (KeyValuePair<string, AsyncStyleSetting> kvpAsyncSetting in this.AsyncSettingControls)
                {

                    kvpAsyncSetting.Value.m_iTimeout--;
                    if (kvpAsyncSetting.Value.m_iTimeout == 0 && kvpAsyncSetting.Value.m_blSuccess == false)
                    {
                        kvpAsyncSetting.Value.m_picStatus.Image = this.SettingFail;
                        kvpAsyncSetting.Value.m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_SHOWRESULT_TICKS;

                        kvpAsyncSetting.Value.m_blSuccess = true;
                    }
                    else if (kvpAsyncSetting.Value.m_iTimeout == 0 && kvpAsyncSetting.Value.m_blSuccess == true)
                    {
                        kvpAsyncSetting.Value.m_picStatus.Image = null;

                        if (kvpAsyncSetting.Value.m_blReEnableControls == true)
                        {
                            foreach (Control ctrlEnable in kvpAsyncSetting.Value.ma_ctrlEnabledInputs)
                            {
                                if (ctrlEnable is TextBox)
                                {
                                    ((TextBox)ctrlEnable).ReadOnly = false;
                                }
                                else
                                {
                                    ctrlEnable.Enabled = true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                this.tmrTimeoutCheck.Enabled = false;
            }
        }

        #endregion
    }
}
