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

using System;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PRoCon.Core;
using PRoCon.Core.Remote;
using Task = System.Threading.Tasks.Task;

namespace PRoCon.Service
{
    public class PRoConHostedService : BackgroundService
    {
        private readonly ILogger<PRoConHostedService> _logger;
        private PRoConApplication _application;
        private bool _shutdownRequested;

        public PRoConHostedService(ILogger<PRoConHostedService> logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PRoCon Service starting...");

            if (PRoConApplication.IsProcessOpen())
            {
                _logger.LogWarning("PRoCon is already running. Service will not start a second instance.");
                return Task.CompletedTask;
            }

            try
            {
                _application = new PRoConApplication(true, Environment.GetCommandLineArgs());
                _application.Execute();
                _logger.LogInformation("PRoCon Application started successfully.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to start PRoCon Application.");
                FrostbiteConnection.LogError("PRoCon.Service", "", e);
            }

            // Keep the service running until cancellation is requested
            stoppingToken.Register(() =>
            {
                _logger.LogInformation("PRoCon Service stopping...");
                if (!_shutdownRequested)
                {
                    _shutdownRequested = true;
                    _application?.Shutdown();
                }
            });

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("PRoCon Service shutting down...");
            if (!_shutdownRequested)
            {
                _shutdownRequested = true;
                _application?.Shutdown();
            }
            return base.StopAsync(cancellationToken);
        }
    }
}
