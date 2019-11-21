namespace PRoCon.Controls.ServerSettings
{
    using Core;
    using Core.Remote;
    public partial class uscServerSettings : uscPage
    {

        public string DisplayName
        {
            get;
            protected set;
        }

        protected CLocalization Language
        {
            get;
            private set;
        }

        protected PRoConClient Client
        {
            get;
            private set;
        }

        protected bool IgnoreEvents
        {
            get;
            set;
        }

        public uscServerSettings()
        {
            InitializeComponent();
        }

        public override void SetLocalization(CLocalization clocLanguage)
        {
            this.Language = clocLanguage;
        }

        public override void SetConnection(Core.Remote.PRoConClient prcClient)
        {
            this.Client = prcClient;

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
            this.InvokeIfRequired(() => { this.Client.Game.ResponseError += new FrostbiteClient.ResponseErrorHandler(m_prcClient_ResponseError); });
        }

        private void m_prcClient_ResponseError(FrostbiteClient sender, Packet originalRequest, string errorMessage)
        {
            if (originalRequest.Words.Count >= 1)
            {
                this.OnSettingResponse(originalRequest.Words[0].ToLower(), null, false);
            }
        }
    }
}
