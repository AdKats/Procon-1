/*  Copyright 2011 Christian 'XpKiller' Suhr & Geoffrey 'Phogue' Green

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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace PRoCon.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            // Set working directory to application base directory
            // (Windows Services default to C:\Windows\System32)
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<PRoConHostedService>();
                });

            // Auto-detect platform and configure the appropriate service lifetime
            if (OperatingSystem.IsWindows())
            {
                builder.UseWindowsService();
            }
            else
            {
                builder.UseSystemd();
            }

            builder.Build().Run();
        }
    }
}
