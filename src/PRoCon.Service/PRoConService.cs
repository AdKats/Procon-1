/*  Copyright 2011 Christian 'XpKiller' Suhr & Geoffrey 'Phogue' Green

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
using System.ServiceProcess;

namespace PRoCon.Service
{
    using PRoCon.Core;
    using PRoCon.Core.Remote;
    public partial class PRoConService : ServiceBase
    {
        public PRoConApplication application = null;

        public PRoConService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //Setting the Evironment back to the Basedirectory
            //because Windows changes it to c://windows/system32 when running as service.
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            System.Console.WriteLine(Environment.CurrentDirectory);

            int iValue;
            if (args != null && args.Length >= 2)
            {
                for (int i = 0; i < args.Length; i = i + 2)
                {
                    if (String.Compare("-use_core", args[i], true) == 0 && int.TryParse(args[i + 1], out iValue) == true && iValue > 0)
                    {
                        System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = (System.IntPtr)iValue;
                    }
                }
            }

            if (PRoConApplication.IsProcessOpen() == false)
            {
                try
                {
                    application = new PRoConApplication(true, args);
                    // Note: The license states usage data must be enabled for procon.console.exe support
                    application.OptionsSettings.AllowAnonymousUsageData = true;
                    application.Execute();
                    GC.Collect();
                }
                catch (Exception e)
                {
                    FrostbiteConnection.LogError("PRoCon.Service.exe", "", e);
                }
            }
        }

        protected override void OnStop()
        {
            if (application != null)
            {
                application.Shutdown();
            }
        }

        protected override void OnShutdown()
        {
            if (application != null)
            {
                application.Shutdown();
            }
        }
    }
}
