namespace PRoCon.Forms {
    partial class frmAbout {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAbout));
            this.okButton = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabAbout = new System.Windows.Forms.TabPage();
            this.lnkVisitForum = new System.Windows.Forms.LinkLabel();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblProductName = new System.Windows.Forms.Label();
            this.tabCopyright = new System.Windows.Forms.TabPage();
            this.pnlCopyright = new System.Windows.Forms.Panel();
            this.lnkMySQLconnector = new System.Windows.Forms.LinkLabel();
            this.lblMySQLconnector = new System.Windows.Forms.Label();
            this.lnlDotNetLibLibrary = new System.Windows.Forms.LinkLabel();
            this.lblDotNetZipLibrary = new System.Windows.Forms.Label();
            this.lnkMaxMind = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.discordToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.ezscaleTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ezscaleLogo = new System.Windows.Forms.PictureBox();
            this.discordIcon = new System.Windows.Forms.PictureBox();
            this.picMaxMind = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl.SuspendLayout();
            this.tabAbout.SuspendLayout();
            this.tabCopyright.SuspendLayout();
            this.pnlCopyright.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ezscaleLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.discordIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMaxMind)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Location = new System.Drawing.Point(485, 248);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(87, 27);
            this.okButton.TabIndex = 24;
            this.okButton.Text = "Close";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabAbout);
            this.tabControl.Controls.Add(this.tabCopyright);
            this.tabControl.Location = new System.Drawing.Point(90, 14);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(482, 227);
            this.tabControl.TabIndex = 25;
            // 
            // tabAbout
            // 
            this.tabAbout.Controls.Add(this.label2);
            this.tabAbout.Controls.Add(this.label1);
            this.tabAbout.Controls.Add(this.ezscaleLogo);
            this.tabAbout.Controls.Add(this.discordIcon);
            this.tabAbout.Controls.Add(this.lnkVisitForum);
            this.tabAbout.Controls.Add(this.lblVersion);
            this.tabAbout.Controls.Add(this.lblProductName);
            this.tabAbout.Location = new System.Drawing.Point(4, 24);
            this.tabAbout.Name = "tabAbout";
            this.tabAbout.Padding = new System.Windows.Forms.Padding(3);
            this.tabAbout.Size = new System.Drawing.Size(474, 199);
            this.tabAbout.TabIndex = 0;
            this.tabAbout.Text = "About";
            this.tabAbout.UseVisualStyleBackColor = true;
            // 
            // lnkVisitForum
            // 
            this.lnkVisitForum.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkVisitForum.LinkArea = new System.Windows.Forms.LinkArea(0, 48);
            this.lnkVisitForum.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkVisitForum.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkVisitForum.Location = new System.Drawing.Point(9, 97);
            this.lnkVisitForum.Name = "lnkVisitForum";
            this.lnkVisitForum.Size = new System.Drawing.Size(303, 21);
            this.lnkVisitForum.TabIndex = 2;
            this.lnkVisitForum.TabStop = true;
            this.lnkVisitForum.Text = "Visit the forums for support and bug submissions";
            this.lnkVisitForum.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(9, 73);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(45, 15);
            this.lblVersion.TabIndex = 1;
            this.lblVersion.Text = "Version";
            // 
            // lblProductName
            // 
            this.lblProductName.AutoSize = true;
            this.lblProductName.Font = new System.Drawing.Font("Arial Black", 15.75F, System.Drawing.FontStyle.Bold);
            this.lblProductName.Location = new System.Drawing.Point(7, 28);
            this.lblProductName.Name = "lblProductName";
            this.lblProductName.Size = new System.Drawing.Size(210, 30);
            this.lblProductName.TabIndex = 0;
            this.lblProductName.Text = "PRoCon Frostbite";
            // 
            // tabCopyright
            // 
            this.tabCopyright.Controls.Add(this.pnlCopyright);
            this.tabCopyright.Location = new System.Drawing.Point(4, 24);
            this.tabCopyright.Name = "tabCopyright";
            this.tabCopyright.Padding = new System.Windows.Forms.Padding(3);
            this.tabCopyright.Size = new System.Drawing.Size(474, 199);
            this.tabCopyright.TabIndex = 2;
            this.tabCopyright.Text = "Copyright";
            this.tabCopyright.UseVisualStyleBackColor = true;
            // 
            // pnlCopyright
            // 
            this.pnlCopyright.AutoScroll = true;
            this.pnlCopyright.Controls.Add(this.lnkMySQLconnector);
            this.pnlCopyright.Controls.Add(this.lblMySQLconnector);
            this.pnlCopyright.Controls.Add(this.lnlDotNetLibLibrary);
            this.pnlCopyright.Controls.Add(this.lblDotNetZipLibrary);
            this.pnlCopyright.Controls.Add(this.lnkMaxMind);
            this.pnlCopyright.Controls.Add(this.picMaxMind);
            this.pnlCopyright.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCopyright.Location = new System.Drawing.Point(3, 3);
            this.pnlCopyright.Name = "pnlCopyright";
            this.pnlCopyright.Padding = new System.Windows.Forms.Padding(5);
            this.pnlCopyright.Size = new System.Drawing.Size(468, 193);
            this.pnlCopyright.TabIndex = 0;
            // 
            // lnkMySQLconnector
            // 
            this.lnkMySQLconnector.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkMySQLconnector.LinkArea = new System.Windows.Forms.LinkArea(40, 14);
            this.lnkMySQLconnector.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkMySQLconnector.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkMySQLconnector.Location = new System.Drawing.Point(8, 154);
            this.lnkMySQLconnector.Name = "lnkMySQLconnector";
            this.lnkMySQLconnector.Size = new System.Drawing.Size(346, 26);
            this.lnkMySQLconnector.TabIndex = 5;
            this.lnkMySQLconnector.TabStop = true;
            this.lnkMySQLconnector.Text = "PRoCon utilises the MySQL Connector/Net available here.";
            this.lnkMySQLconnector.UseCompatibleTextRendering = true;
            this.lnkMySQLconnector.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkMySQLconnector_LinkClicked);
            // 
            // lblMySQLconnector
            // 
            this.lblMySQLconnector.AutoSize = true;
            this.lblMySQLconnector.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMySQLconnector.Location = new System.Drawing.Point(5, 133);
            this.lblMySQLconnector.Name = "lblMySQLconnector";
            this.lblMySQLconnector.Size = new System.Drawing.Size(178, 18);
            this.lblMySQLconnector.TabIndex = 4;
            this.lblMySQLconnector.Text = "MySQL Connector/Net";
            // 
            // lnlDotNetLibLibrary
            // 
            this.lnlDotNetLibLibrary.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnlDotNetLibLibrary.LinkArea = new System.Windows.Forms.LinkArea(44, 14);
            this.lnlDotNetLibLibrary.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnlDotNetLibLibrary.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnlDotNetLibLibrary.Location = new System.Drawing.Point(8, 107);
            this.lnlDotNetLibLibrary.Name = "lnlDotNetLibLibrary";
            this.lnlDotNetLibLibrary.Size = new System.Drawing.Size(346, 26);
            this.lnlDotNetLibLibrary.TabIndex = 3;
            this.lnlDotNetLibLibrary.TabStop = true;
            this.lnlDotNetLibLibrary.Text = "PRoConUpdate utilises the DotNetLib Library available here.";
            this.lnlDotNetLibLibrary.UseCompatibleTextRendering = true;
            this.lnlDotNetLibLibrary.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked_1);
            // 
            // lblDotNetZipLibrary
            // 
            this.lblDotNetZipLibrary.AutoSize = true;
            this.lblDotNetZipLibrary.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDotNetZipLibrary.Location = new System.Drawing.Point(5, 86);
            this.lblDotNetZipLibrary.Name = "lblDotNetZipLibrary";
            this.lblDotNetZipLibrary.Size = new System.Drawing.Size(140, 18);
            this.lblDotNetZipLibrary.TabIndex = 2;
            this.lblDotNetZipLibrary.Text = "DotNetZip Library";
            // 
            // lnkMaxMind
            // 
            this.lnkMaxMind.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkMaxMind.LinkArea = new System.Windows.Forms.LinkArea(70, 19);
            this.lnkMaxMind.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkMaxMind.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkMaxMind.Location = new System.Drawing.Point(8, 41);
            this.lnkMaxMind.Name = "lnkMaxMind";
            this.lnkMaxMind.Size = new System.Drawing.Size(346, 36);
            this.lnkMaxMind.TabIndex = 1;
            this.lnkMaxMind.TabStop = true;
            this.lnkMaxMind.Text = "This product includes GeoLite data created by MaxMind, available from http://maxm" +
    "ind.com/";
            this.lnkMaxMind.UseCompatibleTextRendering = true;
            this.lnkMaxMind.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkMaxMind_LinkClicked);
            // 
            // linkLabel1
            // 
            this.linkLabel1.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkArea = new System.Windows.Forms.LinkArea(17, 20);
            this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabel1.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.linkLabel1.Location = new System.Drawing.Point(94, 254);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(148, 21);
            this.linkLabel1.TabIndex = 2;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Copyright © 2022 Myrcon";
            this.linkLabel1.UseCompatibleTextRendering = true;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(220, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(247, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "Click the logo above to join MyRCON discord";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(13, 38);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(71, 200);
            this.pictureBox1.TabIndex = 26;
            this.pictureBox1.TabStop = false;
            // 
            // ezscaleLogo
            // 
            this.ezscaleLogo.Image = global::PRoCon.Properties.Resources.ezscale_logo;
            this.ezscaleLogo.Location = new System.Drawing.Point(242, 121);
            this.ezscaleLogo.Name = "ezscaleLogo";
            this.ezscaleLogo.Size = new System.Drawing.Size(215, 50);
            this.ezscaleLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.ezscaleLogo.TabIndex = 4;
            this.ezscaleLogo.TabStop = false;
            this.ezscaleTooltip.SetToolTip(this.ezscaleLogo, "Click to join EZSCALE Discord Server");
            this.ezscaleLogo.Click += new System.EventHandler(this.ezscaleLogo_Click);
            // 
            // discordIcon
            // 
            this.discordIcon.Image = global::PRoCon.Properties.Resources.Discord_Logo_Wordmark_Color__1_;
            this.discordIcon.Location = new System.Drawing.Point(223, 6);
            this.discordIcon.Name = "discordIcon";
            this.discordIcon.Size = new System.Drawing.Size(244, 52);
            this.discordIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.discordIcon.TabIndex = 3;
            this.discordIcon.TabStop = false;
            this.discordToolTip.SetToolTip(this.discordIcon, "Click to join the MyRCON Discord.\r\n");
            this.discordIcon.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // picMaxMind
            // 
            this.picMaxMind.Image = ((System.Drawing.Image)(resources.GetObject("picMaxMind.Image")));
            this.picMaxMind.Location = new System.Drawing.Point(8, 3);
            this.picMaxMind.Name = "picMaxMind";
            this.picMaxMind.Size = new System.Drawing.Size(117, 35);
            this.picMaxMind.TabIndex = 0;
            this.picMaxMind.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(210, 174);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(252, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Click the logo above to join EZSCALE\'s discord";
            // 
            // frmAbout
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 289);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.tabControl);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAbout";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "frmAbout";
            this.tabControl.ResumeLayout(false);
            this.tabAbout.ResumeLayout(false);
            this.tabAbout.PerformLayout();
            this.tabCopyright.ResumeLayout(false);
            this.pnlCopyright.ResumeLayout(false);
            this.pnlCopyright.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ezscaleLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.discordIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMaxMind)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabAbout;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblProductName;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.LinkLabel lnkVisitForum;
        private System.Windows.Forms.TabPage tabCopyright;
        private System.Windows.Forms.Panel pnlCopyright;
        private System.Windows.Forms.LinkLabel lnkMaxMind;
        private System.Windows.Forms.PictureBox picMaxMind;
        private System.Windows.Forms.LinkLabel lnlDotNetLibLibrary;
        private System.Windows.Forms.Label lblDotNetZipLibrary;
        private System.Windows.Forms.Label lblMySQLconnector;
        private System.Windows.Forms.LinkLabel lnkMySQLconnector;
        private System.Windows.Forms.PictureBox discordIcon;
        private System.Windows.Forms.ToolTip discordToolTip;
        private System.Windows.Forms.PictureBox ezscaleLogo;
        private System.Windows.Forms.ToolTip ezscaleTooltip;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}
