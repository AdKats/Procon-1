/*
 * SdkTemplatePlugin — Chat Command Handling (Partial Class)
 *
 * This file handles all chat-based commands (!help, !info, etc.)
 * It's compiled together with SdkTemplatePlugin.cs automatically.
 *
 * NAMING RULE: The file must be named <ClassName>.<Anything>.cs
 *   SdkTemplatePlugin.Commands.cs  ← This file
 *   SdkTemplatePlugin.cs           ← Main file
 *
 * Both files share the same class, namespace, and all fields/methods.
 */

using System;
using System.Collections.Generic;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Players;

namespace PRoConEvents
{
    public partial class SdkTemplatePlugin
    {
        // =====================================================================
        // Chat event overrides — route to command handler
        // =====================================================================

        public override void OnGlobalChat(string speaker, string message)
        {
            if (!_isEnabled || string.IsNullOrEmpty(message)) return;
            HandleChatCommand(speaker, message);
        }

        public override void OnTeamChat(string speaker, string message, int teamId)
        {
            if (!_isEnabled || string.IsNullOrEmpty(message)) return;
            HandleChatCommand(speaker, message);
        }

        public override void OnSquadChat(string speaker, string message, int teamId, int squadId)
        {
            if (!_isEnabled || string.IsNullOrEmpty(message)) return;
            HandleChatCommand(speaker, message);
        }

        // =====================================================================
        // Command dispatcher
        // =====================================================================

        private void HandleChatCommand(string speaker, string message)
        {
            string cmd = message.Trim().ToLower();

            if (cmd == "!help")
            {
                SayToPlayer(speaker, "Available commands: !help, !info, !stats");
            }
            else if (cmd == "!info")
            {
                SayToPlayer(speaker,
                    string.Format("Server: {0}:{1} | PRoCon {2}", _hostName, _port, _proconVersion));
            }
            else if (cmd == "!stats")
            {
                // Example: query database for player stats
                // This calls into the Database partial class
                string stats = GetPlayerStats(speaker);
                SayToPlayer(speaker, stats ?? "No stats found.");
            }
        }

        // =====================================================================
        // Chat helpers
        // =====================================================================

        private void SayToPlayer(string soldierName, string message)
        {
            ExecuteCommand("procon.protected.send", "admin.say", message, "player", soldierName);
        }

        private void SayToAll(string message)
        {
            ExecuteCommand("procon.protected.send", "admin.say", message, "all");
        }

        private void YellToPlayer(string soldierName, string message, int durationSeconds = 5)
        {
            ExecuteCommand("procon.protected.send", "admin.yell", message,
                durationSeconds.ToString(), "player", soldierName);
        }
    }
}
