using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PRoCon.Forms {
    using PRoCon.Core;
    public partial class LicenseAgreement : Form {

        PRoConApplication m_app = null;
        string m_agreementRevision = "";

        public LicenseAgreement(PRoConApplication app, string agreementRevision) {
            this.m_app = app;
            this.m_agreementRevision = agreementRevision;
            InitializeComponent();

            this.rtbLicense.ReadOnly = false;
            this.rtbLicense.Rtf = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033\deflangfe2052{\fonttbl{\f0\fswiss\fprq2\fcharset0 Arial;}}
{\colortbl ;\red0\green0\blue0;}
{\*\generator Riched20 6.3.9600}{\*\mmathPr\mdispDef1\mwrapIndent1440 }\viewkind4\uc1 
\pard\nowidctlpar\sl276\slmult1\cf1\b\f0\fs22 Procon Frostbite 1.1 and subsequent releases - End User License Agreement\b0\par
Revised: April 7, 2015\par
\par
THIS IS A LEGAL AGREEMENT between 'you', the individual, company, or organisation utilising Procon Frostbite Software (hereinafter Procon Software) and Myrcon, an Adelaide, Australia based company.\par
\par
USE OF PROCON SOFTWARE INDICATES YOUR ACCEPTANCE OF THESE TERMS.\par
As used in this Agreement, the term 'Procon Software' means Procon Frostbite 1.1 and subsequent releases of the software, both client and server, as made available from myrcon.com together with any and all enhancements, upgrades, or updates that may be provided to you through myrcon.com. Including, but not limited to, all files and their content including all data and source code provided through myrcon.com or transmitted by Myrcon or an authorised agent thereof.\par
\par
\b\fs24 1. APPLICABLE LAW\par
\b0\fs22 All terms in this Agreement relating to ownership, distribution, prohibited conduct, or upgrades to Procon Software, will be handled by Myrcon or an authorised agent under the laws of South Australia, Australia.\par
\par
As such, all terms in this Agreement relating to Procon Software sales, billing, compliance with licensing, including related issues such as piracy or banning of servers, will be handled by Myrcon, or an authorised agent thereof, in accordance with the laws of South Australia, Australia.\par
\par
\b\fs24 2. OWNERSHIP\par
\b0\fs22 Ownership of Procon Software and any accompanying documentation shall at all times remain with Myrcon. This Agreement does not constitute the sale of Procon Software or any accompanying documentation, or any portion thereof. Without limiting the generality of the foregoing, you do not receive any rights to any patents, copyrights, trade secrets, trademarks or other intellectual property rights relating to Procon Software or any accompanying documentation. All rights not expressly granted to you under this Agreement are reserved by Myrcon.\par
\par
\b\fs24 3. DEFINITIONS\par
\fs22 3.1 Procon Client and Server\par
\b0 Procon Software consists of both a Procon Client and Procon Server application. The Procon Server is the application which acts as a host and allows two or more client connections to communicate with one another and issue commands over a network to a game server. The Procon Client is the application which connects to the Procon Server and contains end-user functionality which includes initiating a data stream for communication and/or control with another client connection or server.\par
\b\par
3.2 Procon Software Development Kit (Procon SDK)\par
\b0 Procon software may also consist of a Software Development Kit or SDK. The Procon SDK is a set of development tools and documentation which allows software engineers to create customized or integrated applications typically as part of an existing product or service. The Procon SDK includes API information, sample code, tools, documentation, and other related items.\par
\par
\b 3.3 Procon Virtual Server\par
\b0 A Procon Virtual Server is any instance within the Procon Server application (binary executable) which allows the Procon Client application to connect. A single executed Procon Server application (binary executable) will by default create a single Procon Virtual Server. However, the Procon Server application is capable of creating and hosting multiple Procon Virtual Servers within any single running binary executable, where each server contains its own configuration properties which to the end-user may appear to act as a stand-alone server.\par
\par
\b 3.4 Procon Server Layer\par
\b0 A Procon Server layer (or just 'layer') is utilised when Procon Software initiates a connection with a game server for purposes of communicating and/or controlling the game server. This does not include any client connections that may be made to the Procon Server Layer and not directly to a game server.\par
\par
\b 3.5 Commercial Entity\par
\b0 A commercial entity is an individual, company, or organisation which demonstrates (typically via but not limited to a website) that it is in business to turn a profit of any kind; be it monetary, from direct sales or rental fees, advertising profit, or through the privileged use of intangible goods and services.\par
\par
\b 3.6 Virtual Machine\par
\b0 A Virtual Machine is a virtualised computing resource of a physical machine or machines. Virtual machines are typically hosted on clustered physical machines or cloud environments. Multiple virtual machines can reside on one physical machine but are completely independant of any other virtual machine and the host.\par
\par
\ul Example of a Commercial Entity\ulnone :\ul\par
\ulnone A hosting company or organisation which charges a monthly fee for the use of a server running Procon Software OR a hosting company or organisation which does NOT charge a monthly fee for the use of a server running Procon Software but earns substantial profit from advertising, or from other products or services of any kind.\par
\par
\ul Example of a Commercial Entity profiting from advertising\ulnone :\ul\par
\ulnone An organisation advertising for products or services offered by a hosting company in exchange for the use of a server running Procon Software means the hosting company will be considered to be a commercial entity, even if they choose not to charge anything at all for the use of any of their servers running Procon Software. This situation is commonly referred to as a clan or guild 'sponsorship'.\par
\par
\ul Example of a Commercial Entity profiting from intangible goods\ulnone :\ul\par
\ulnone A 'payment' is made to an individual or hosting company using virtual currency (gold, etc.) within a popular massively multiplayer online game (MMOG) in exchange for the use of a server running Procon Software means the individual or hosting company will be considered to be a commercial entity.\par
\par
\b 3.6 Non-Profit Entity\par
\b0 A non-profit entity is an individual or organisation which does NOT utilise Procon Software for profit of any kind; be it monetary, from direct sales or rental fees, advertising profit, or intangible goods and services.\par
\par
\ul Example 1\ulnone : A clan or guild hosting a server running Procon Software for their own private use while complying with all terms and conditions set forth in Section 5.1 of this Agreement. \ul\par
\ulnone\par
\ul Example 2\ulnone : An individual hosting a server running Procon Software for private use while complying with all terms and conditions set forth in Section 5.1 of this Agreement.\ul\par
\ulnone\par
\b\fs24 4. LICENSE FEES\par
\b0\fs22 Based on the definitions above, license fees may be applicable to entities utilizing Procon Software. License fees are NOT applicable to the Procon Client application that do not directly connect to and/or issue commands to a game server. All Commercial Entities using the Procon Software for any reason must pay a license fee, regardless of whether or not they choose to charge fees for the use of their servers. Non-Profit Entities using the Procon Software do not need to pay a license fee; however, these entities must comply with the terms and conditions set forth in the License Types applicable to Non-Profit Entities below. If you are uncertain as to whether you qualify as a Non-Profit Entity you must contact a Myrcon representative via licensing@myrcon.com\par
\par
\b\fs24 5. LICENSE TYPES\par
\fs22 5.1. Non-Profit License\par
\b0 This license type is for an individual or organisation which is non-profit in nature, and does not require registration on our website nor a license key. An individual or organisation operating under this license may install and use Procon Software on one or more physical or virtual machines, alter or adapt files and source code and otherwise utilise Procon Software as the individual or organisation may desire without paying a license fee, provided that the following conditions are met:\par
\par
\ul a\ulnone . The individual or organisation must be non-profit in nature. Myrcon, and authorised agents thereof, reserve the right to assess and determine if any individual or organisation is non-profit in nature. If you are uncertain as to whether you qualify as a Non-Profit Entity you must contact a Myrcon representative via licensing@myrcon.com\par
\par
\ul b\ulnone . Any software, code, application, or other work product that includes any portion of Procon Software as defined by this license, must acknowledge the use of Procon Software. The acknowledgement must be conspicuous and include the following phrase: 'Procon Software by myrcon.com'. The determination of conspicuous placement of the aforementioned phrase will be in the sole discretion of Myrcon and any authorised agent of Myrcon. \ul\par
\ulnone\par
\ul c\ulnone . Any software, source code, application, or other work product that includes any portion of Procon Software as defined by this license, must include a copy of this license and will be bound by the same terms included herein.\ul\par
\ulnone\par
\ul d\ulnone . Any software, source code, application, or other work product that includes any portion of Procon Software as defined and bound by this license and is utilised by a commercial entity will be subject to the conditions, duties, and obligations of a Commercial License. (See Section 5.2)\ul\par
\ulnone\par
\b 5.2. Commercial License for APHPs (Authorised Procon Host Providers)\b0 : \b\par
\b0 An Authorised Procon Host Provider License or APHP License is a license requiring recurring monthly fees. APHP Licenses are issued to Commercial Entities (an individual, company, or organisation) which host servers running Procon Software to others for profit of any kind; be it monetary, from direct sales or rental fees, advertising profit, or through the privileged use of intangible goods and services. APHPs are Commercial Entities which typically charge their customers a monthly fee for the use of a Procon Virtual Server or include the Virtual Server as part of other services or offerings to their customers free of charge. Commercial Entities operating under the Authorised Procon Host Provider License may install and use Procon software on one or more physical or virtual machines, and must adhere to the following conditions:\par
\par
\ul a\ulnone . APHPs must register for an account by emailing a Myrcon representative via licensing@myrcon.com.\par
\par
\ul b\ulnone . APHPs are subject to recurring, monthly licensing fees based on the average Procon Server Layer count configured on each Virtual Server hosted by the APHP during the previous month (e.g. - if a Virtual Server reports being configured for 50 Layers during 15 out of 30 days of the previous month, the Virtual Server will be billed at 25 Layers). These licensing fees are completely indifferent to whether or not an APHP's customer, client, or user makes use of their Procon Server Layer in any way.\ul\par
\ulnone\par
\ul c\ulnone . APHPs are billed monthly, in arrears, by digital invoice delivered via myrcon.harvestapp.com. All invoices are typically sent on the 1st or 2nd day of every month via email and are also posted to the APHP's client dashboard account which is linked to via each electronic invoice.\ul\par
\ulnone\par
\ul d\ulnone . Payments are due 15 days after any invoice is generated (NET 15). Payment must be made through the APHPs myrcon.harvestapp.com client dashboard or via paypal.com to billing@myrcon.com. It is the APHP's responsibility to ensure that their invoice is received; whether by the primary email address registered to the APHP's online account or by a representative of the APHP ensuring that the APHP's online account is logged into or checked each month for new invoices.\ul\par
\ulnone\par
\ul e\ulnone . APHPs who become 30 or more days past due on their invoice may have their Procon Software License suspended for non payment.\ul\par
\ulnone\par
\ul f\ulnone . APHPs who consistently fail to pay their invoices on time are subject to having their account or license suspended or revoked.\ul\par
\ulnone\par
\ul g\ulnone . All license fees are listed in United States Dollars. All invoices for license fee\rquote s will be calculated in (USD) and payments should be made through the APHPs myrcon.harvestapp.com client dashboard or paypal in (USD).\ul\par
\ulnone\par
h. New APHPs acknowledge that Myrcon requires a onetime \ldblquote Processing Fee\rdblquote  of $10.00, paid in advance, for the creation of a new APHP account with myrcon.com. Payment of this fee is consideration and acknowledgment of the receipt, review, and acceptance of all terms and conditions set forth in this License Agreement.\par
\par
\ul i\ulnone . New APHPs acknowledge that there will be a minimum license fee of $10.00, per month, and includes ten (10) Procon layers, per month. \ul\par
\ulnone\par
\ul j\ulnone . License Fees are subject to bulk discounts. \ul\par
\ulnone Tier 1. The first 200 Procon layers used by a APHP are $1.00 each, per month.\par
Tier 2. The next additional 200 Procon layers used by an APHP are $.80 each, per month.\par
Tier 3. The next additional 100 Procon layers used by an APHP are $.60 each, per month.\par
Tier 4. Procon layers over 500 used by an APHP are $.50 each, per month.\par
For example, an APHP that has hosted an average of (210) layers per month would be subject to a ($208) licensing fee; or an average of (600) layers would be subject to a ($470).\par
\par
\ul k\ulnone . License Fees are subject to change at any time. However, all current, APHPs will be notified at least one full billing period before they are subject to any change in the license fee. Please check the latest EULA for current licensing fees.\ul\par
\ulnone\par
\ul l\ulnone . APHPs acknowledge that invoices may occasionally reflect inaccurate data due to incorrectly configured layer counts on licensed Virtual Servers (e.g. - test servers accidentally created with high layer counts, or duplicate data reported back to myrcon.com during data center migrations, etc.). As such, invoices are subject to review by Myrcon and their authorised agents at billing@myrcon.com. Every effort will be made by Myrcon to determine the best course of action when correcting or modifying an invoice.\ul\par
\ulnone\par
\ul m\ulnone . APHPs acknowledge that layer count data for each Virtual Server hosted by the APHP is reported daily to Procon 's tracking server located at myrcon.com for the purpose of tracking and billing the APHP accordingly.\ul\par
\ulnone\par
\ul n\ulnone . APHPs must add command line arguments, or a text file, into their instances of Procon Software upon request to facilitate tracking and identification of APHP license holders.\ul\par
\ulnone\par
\ul o\ulnone . APHPs may not utilise firewalls or any other tools to prevent communication from their licensed Virtual Servers to Procon 's tracking server located at myrcon.com. All outbound traffic, both TCP and UDP, must be made available to the tracking server AND the organisation must ensure that DNS is functioning properly and is able to resolve the hostname myrcon.com at all times on all physical or virtual machines where Procon Virtual Servers are being hosted.\ul\par
\ulnone\par
\ul p\ulnone . APHPs may allow resellers to sell their Procon Virtual Servers; however, the APHP must ensure that all of their Virtual Server IPs are licensed at all times. Resellers are not required to register and purchase a separate APHP license for themselves as long as all Virtual Servers sold by the reseller are licensed through the APHP.\ul\par
\ulnone\par
\ul q\ulnone . APHPs must update all of their Procon Servers within a reasonable time frame following the release of an updated version of Procon Software. All APHPs will be notified of available updates through the email used to register the APHP account. It is the APHPs responsibility to regularly check that email account for such notices and apply updates. Failure to apply updates to Procon Software may result in the suspension of a APHP account and/or license. \ul\par
\ulnone\par
\b\fs24 6. DISTRIBUTION VIA THE INTERNET\par
\b0\fs22 The preferred method of distribution of Procon Software over the Internet is via Procon 's official website at myrcon.com. You may not distribute Procon Software otherwise over the Internet, unless you obtain prior written consent from Myrcon or its authorised agent.\par
\par
\b\fs24 7. THIRD PARTY DISTRIBUTION PROHIBITED\par
\b0\fs22 Distribution of Procon Software by you to third parties (e.g. - publishers, magazines, third party products, etc.) is also hereby expressly prohibited unless you obtain prior written consent from Myrcon or its authorised agent.\par
\par
\b\fs24 8. TERMINATION\par
\b0\fs22 Myrcon reserves the right to terminate your license for Procon Software at any time or for any reason. Your license may also be terminated if you are in breach of any of the terms or conditions set forth in this Agreement. Upon termination, you shall immediately discontinue using Procon Software and destroy all copies and related intellectual property in your possession, custody or control.\par
\par
\par
\b\fs24 9. BILLING/LICENSING\par
\b0\fs22 All billing matters for Commercial Entities should be forwarded to billing@myrcon.com. Any inquiries relating to licensing must be e-mailed to licensing@myrcon.com.\par
\par
\b\fs24 10. PRICING\par
\b0\fs22 Procon software pricing information for Commercial Entities can be found in the latest version of this EULA on the Myrcon\rquote s website. All prices are listed in United States Dollars (USD).\par
\par
\b\fs24 11. PROHIBITED CONDUCT\par
\b0\fs22 You represent and warrant that you will not violate any of the terms and conditions set forth in this Agreement and that:\par
\par
\ul a\ulnone . You will not: (I) reverse engineer, decompile, disassemble, derive the source code of, modify, or create derivative works from Procon software; or (II) use, copy, modify, alter, or transfer, electronically or otherwise, Procon Software or any of the accompanying documentation except as expressly permitted in this Agreement; or (III) redistribute, sell, rent, lease, sublicense, or otherwise transfer rights to Procon Software whether in a stand-alone configuration or as incorporated with other software code written by any party except as expressly permitted in this Agreement.\ul\par
\ulnone\par
\ul b\ulnone . You will not use Procon Software to engage in or allow others to engage in any illegal activity.\ul\par
\ulnone\par
\ul c\ulnone . You will not engage in use of Procon Software that will interfere with or damage the operation of the services of third parties by overburdening or disabling network resources through automated queries, excessive usage or similar conduct.\ul\par
\ulnone\par
\ul d\ulnone . You will not use Procon Software to engage in any activity that will violate the rights of third parties, including, without limitation, through the use, public display, public performance, reproduction, distribution, or modification of communications or materials that infringe copyrights, trademarks, publicity rights, privacy rights, other proprietary rights, or rights against defamation of third parties.\ul\par
\ulnone\par
\ul e\ulnone . You will not transfer Procon Software or utilise Procon Software in combination with third party software authored by you or others to create an integrated software program which you transfer to unrelated third parties unless you obtain prior written consent from MYRCON or an authorised agent of MYRCON.\ul\par
\ulnone\b\fs24\par
12. UPGRADES, UPDATES AND ENHANCEMENTS\par
\b0\fs22 All upgrades, updates or enhancements of Procon Software shall be deemed to be part of Procon Software and will be subject to this Agreement.\par
\par
\par
\b\fs24 13. LEGENDS AND NOTICES\par
\b0\fs22 You agree that you will not remove or alter any trademark, logo, copyright or other proprietary notices, legends, symbols or labels in Procon Software or any accompanying documentation.\par
\par
\b\fs24 14. TERM AND TERMINATION\par
\b0\fs22 This Agreement is effective upon your acceptance as provided herein and will remain in force until terminated. Non-Profit Entities may terminate the licenses granted in this Agreement at any time by destroying Procon Software and any accompanying documentation, together with any and all copies thereof. Commercial Entities may terminate the licenses granted in this Agreement at any time by contacting MYRCON or an authorised agent of MYRCON via licensing@myrcon.com. The licenses granted in this Agreement will terminate automatically if you breach any of its terms or conditions or any of the terms or conditions of any other agreement between you and MYRCON or an authorised agent of MYRCON.\par
\par
\b\fs24 15. SOFTWARE SUGGESTIONS\par
\b0\fs22 Procon welcomes suggestions for enhancing Procon Software and any accompanying documentation that may result in computer programs, reports, presentations, documents, ideas or inventions relating or useful to Myrcon or Procon Software. You acknowledge that all title, ownership rights, and intellectual property rights concerning such suggestions shall become the exclusive property of Myrcon and may be used for its business purposes in its sole discretion without any payment or accounting to you.\par
\par
\b\fs24 16. MISCELLANEOUS\par
\b0\fs22 This Agreement constitutes the entire agreement between the parties concerning Procon Software, but is subject to change by Myrcon or any authorised agent of Myrcon. If any provision in this Agreement should be held illegal or unenforceable by a court of competent jurisdiction, such provision shall be modified to the extent necessary to render it enforceable without losing its intent, or severed from this Agreement if no such modification is possible, and other provisions of this Agreement shall remain in full force and effect. A waiver by either party of any term or condition of this Agreement or any breach thereof, in any one instance, shall not waive such term or condition or any subsequent breach thereof. Any waiver of any term of this agreement must be in writing.\par
\par
\b\fs24 17. DISCLAIMER OF WARRANTY\par
\b0\fs22 PROCON SOFTWARE IS PROVIDED ON AN 'AS IS' BASIS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING, WITHOUT LIMITATION, THE WARRANTIES THAT IT IS FREE OF DEFECTS, VIRUS FREE, ABLE TO OPERATE ON AN UNINTERRUPTED BASIS, MERCHANTABLE, FIT FOR A PARTICULAR PURPOSE OR NON-INFRINGING. THIS DISCLAIMER OF WARRANTY CONSTITUTES AN ESSENTIAL PART OF THIS LICENSE AND AGREEMENT. NO USE OF PROCON SOFTWARE IS Authorised HEREUNDER EXCEPT UNDER THIS DISCLAIMER.\par
\par
\b\fs24 18. LIMITATION OF LIABILITY\par
\b0\fs22 TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, IN NO EVENT WILL MYRCON, OR ANY AGENT OF MYRCON, BE LIABLE FOR ANY INDIRECT, SPECIAL, INCIDENTAL OR CONSEQUENTIAL DAMAGES ARISING OUT OF THE USE OF OR INABILITY TO USE PROCON SOFTWARE, INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOST PROFITS, LOSS OF GOODWILL, WORK STOPPAGE, COMPUTER FAILURE OR MALFUNCTION, OR ANY AND ALL OTHER COMMERCIAL DAMAGES OR LOSSES, EVEN IF ADVISED OF THE POSSIBILITY THEREOF, AND REGARDLESS OF THE LEGAL OR EQUITABLE THEORY (CONTRACT, TORT OR OTHERWISE) UPON WHICH THE CLAIM IS BASED. IN ANY CASE, MYRCON, OR ANY AGENT OF , MYRCON, COLLECTIVE LIABILITY UNDER ANY PROVISION OF THIS LICENSE SHALL NOT EXCEED IN THE AGGREGATE THE SUM OF THE FEES (IF ANY) YOU PAID FOR THIS LICENSE.\par
\par
}
";

            this.rtbLicense.ReadOnly = true;

        }

        private void btnCancel_Click(object sender, EventArgs e) {
            // No messing about.. gtfo. Now.
            Environment.Exit(0);
        }
        
        private void btnAgree_Click(object sender, EventArgs e) {
            this.m_app.LicenseAgreements.Add(this.m_agreementRevision);
            this.m_app.SaveGspSettings();
            this.m_app.OptionsSettings.AllowAnonymousUsageData = this.chkAgreeUsageReports.Checked;

            this.Close();
        }

        private void lnkDownloadPdf_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://myrcon.net/licenses/myrcon.pdf");
        }
    }
}
