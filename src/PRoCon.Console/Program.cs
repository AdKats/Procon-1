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
using System.Threading;

namespace PRoCon.Console
{
    using Core;
    using Core.Remote;
    class Program
    {

        static void Main(string[] args)
        {

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

            PRoConApplication application = null;

            if (PRoConApplication.IsProcessOpen() == false)
            {
                try
                {
                    application = new PRoConApplication(true, args);

                    System.Console.WriteLine("Procon Frostbite");
                    System.Console.WriteLine("================");
                    System.Console.WriteLine("By executing this application you agree to the license available at:");
                    System.Console.WriteLine("\thttps://myrcon.net/licenses/myrcon.pdf");
                    System.Console.WriteLine("If you do not agree you must immediately exit this application.");
                    System.Console.WriteLine("================");
                    System.Console.WriteLine("This is a cut down version of PRoCon.exe to be used by GSPs and PRoCon Hosts.");
                    System.Console.WriteLine("Executing this file is the same as \"PRoCon.exe -console 1\"");
                    System.Console.WriteLine("No output is given.  This is as cut down as we're gunno get..");
                    System.Console.WriteLine("\nExecuting procon...");
                    application.Execute();

                    GC.Collect();
                    System.Console.WriteLine("Running... (Press any key to shutdown)");
                    System.Console.ReadKey();
                }
                catch (Exception e)
                {
                    FrostbiteConnection.LogError("PRoCon.Console.exe", "", e);
                }
                finally
                {
                    if (application != null)
                    {
                        application.Shutdown();
                    }
                }

            }
            else
            {
                // Possible prevention of a cpu consumption bug I can see at the time of writing.
                // TCAdmin: Start procon.exe
                // procon.exe has an update to install
                // procon.exe loads proconupdater.exe
                // procon.exe unloads
                // proconupdater.exe begins update
                // TCAdmin detects procon.exe shutdown - attempts to reload
                System.Console.WriteLine("Already running - shutting down");
                Thread.Sleep(50);
            }

        }
    }
}
