using System;
using System.Collections.Generic;

namespace PRoCon.UI.Models
{
    public class RconCommandDef
    {
        public string Name;
        public string Signature; // display: "command <param1> <param2> [optional]"
        public int MinParams;    // minimum required params (excluding command name)
        public int MaxParams;    // max params (-1 = unlimited)
        public string Games;     // null = all games, otherwise comma-separated: "BF4,BF3,BFHL"
    }

    public static class RconCommandDatabase
    {
        public static readonly RconCommandDef[] Commands = {
            // Auth & Misc
            new RconCommandDef { Name = "login.plainText", Signature = "login.plainText <password>", MinParams = 1, MaxParams = 1 },
            new RconCommandDef { Name = "login.hashed", Signature = "login.hashed [passwordHash]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "logout", Signature = "logout", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "quit", Signature = "quit", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "version", Signature = "version", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "serverInfo", Signature = "serverInfo", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "currentLevel", Signature = "currentLevel", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "listPlayers", Signature = "listPlayers <all|team <teamId>|player <name>>", MinParams = 1, MaxParams = 4 },
            // Admin
            new RconCommandDef { Name = "admin.eventsEnabled", Signature = "admin.eventsEnabled [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "admin.help", Signature = "admin.help", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "admin.kickPlayer", Signature = "admin.kickPlayer <soldierName> [reason]", MinParams = 1, MaxParams = 2 },
            new RconCommandDef { Name = "admin.killPlayer", Signature = "admin.killPlayer <soldierName>", MinParams = 1, MaxParams = 1 },
            new RconCommandDef { Name = "admin.listPlayers", Signature = "admin.listPlayers <all|team <teamId>|squad <teamId> <squadId>>", MinParams = 1, MaxParams = 4 },
            new RconCommandDef { Name = "admin.movePlayer", Signature = "admin.movePlayer <soldierName> <teamId> <squadId> <forceKill>", MinParams = 4, MaxParams = 4 },
            new RconCommandDef { Name = "admin.password", Signature = "admin.password [password]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "admin.say", Signature = "admin.say <message> <all|team <teamId>|player <name>>", MinParams = 2, MaxParams = 4 },
            new RconCommandDef { Name = "admin.shutDown", Signature = "admin.shutDown", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "admin.yell", Signature = "admin.yell <message> [duration] [all|team <teamId>|player <name>]", MinParams = 1, MaxParams = 5 },
            new RconCommandDef { Name = "admin.effectiveMaxPlayers", Signature = "admin.effectiveMaxPlayers", MinParams = 0, MaxParams = 0, Games = "BF3" },
            // Ban List
            new RconCommandDef { Name = "banList.add", Signature = "banList.add <name|ip|guid> <id> <perm|rounds <n>|seconds <n>> [reason]", MinParams = 3, MaxParams = 5 },
            new RconCommandDef { Name = "banList.remove", Signature = "banList.remove <name|ip|guid> <id>", MinParams = 2, MaxParams = 2 },
            new RconCommandDef { Name = "banList.clear", Signature = "banList.clear", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "banList.list", Signature = "banList.list [startIndex]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "banList.load", Signature = "banList.load", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "banList.save", Signature = "banList.save", MinParams = 0, MaxParams = 0 },
            // Map List
            new RconCommandDef { Name = "mapList.add", Signature = "mapList.add <mapName> <gamemode> <rounds> [index]", MinParams = 3, MaxParams = 4 },
            new RconCommandDef { Name = "mapList.remove", Signature = "mapList.remove <index>", MinParams = 1, MaxParams = 1 },
            new RconCommandDef { Name = "mapList.clear", Signature = "mapList.clear", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.list", Signature = "mapList.list [startIndex]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "mapList.load", Signature = "mapList.load", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.save", Signature = "mapList.save", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.setNextMapIndex", Signature = "mapList.setNextMapIndex <index>", MinParams = 1, MaxParams = 1 },
            new RconCommandDef { Name = "mapList.getMapIndices", Signature = "mapList.getMapIndices", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.getRounds", Signature = "mapList.getRounds", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.availableMaps", Signature = "mapList.availableMaps", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.restartRound", Signature = "mapList.restartRound", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.runNextRound", Signature = "mapList.runNextRound", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.endRound", Signature = "mapList.endRound <winningTeamId>", MinParams = 1, MaxParams = 1 },
            // Reserved Slots
            new RconCommandDef { Name = "reservedSlotsList.add", Signature = "reservedSlotsList.add <soldierName>", MinParams = 1, MaxParams = 1 },
            new RconCommandDef { Name = "reservedSlotsList.remove", Signature = "reservedSlotsList.remove <soldierName>", MinParams = 1, MaxParams = 1 },
            new RconCommandDef { Name = "reservedSlotsList.clear", Signature = "reservedSlotsList.clear", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "reservedSlotsList.list", Signature = "reservedSlotsList.list", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "reservedSlotsList.load", Signature = "reservedSlotsList.load", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "reservedSlotsList.save", Signature = "reservedSlotsList.save", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "reservedSlotsList.aggressiveJoin", Signature = "reservedSlotsList.aggressiveJoin [true|false]", MinParams = 0, MaxParams = 1 },
            // Player queries (BF4 only)
            new RconCommandDef { Name = "player.idleDuration", Signature = "player.idleDuration <soldierName>", MinParams = 1, MaxParams = 1, Games = "BF4" },
            new RconCommandDef { Name = "player.isAlive", Signature = "player.isAlive <soldierName>", MinParams = 1, MaxParams = 1, Games = "BF4" },
            new RconCommandDef { Name = "player.ping", Signature = "player.ping <soldierName>", MinParams = 1, MaxParams = 1, Games = "BF4" },
            // PunkBuster
            new RconCommandDef { Name = "punkBuster.activate", Signature = "punkBuster.activate", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "punkBuster.deactivate", Signature = "punkBuster.deactivate", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "punkBuster.isActive", Signature = "punkBuster.isActive", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "punkBuster.pb_sv_command", Signature = "punkBuster.pb_sv_command <command>", MinParams = 1, MaxParams = -1 },
            // FairFight (BF4 only)
            new RconCommandDef { Name = "fairFight.activate", Signature = "fairFight.activate", MinParams = 0, MaxParams = 0, Games = "BF4" },
            new RconCommandDef { Name = "fairFight.deactivate", Signature = "fairFight.deactivate", MinParams = 0, MaxParams = 0, Games = "BF4" },
            new RconCommandDef { Name = "fairFight.isActive", Signature = "fairFight.isActive", MinParams = 0, MaxParams = 0, Games = "BF4" },
            // Vars - boolean
            new RconCommandDef { Name = "vars.3dSpotting", Signature = "vars.3dSpotting [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.3pCam", Signature = "vars.3pCam [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.alwaysAllowSpectators", Signature = "vars.alwaysAllowSpectators [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.autoBalance", Signature = "vars.autoBalance [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.commander", Signature = "vars.commander [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.crossHair", Signature = "vars.crossHair [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.forceReloadWholeMags", Signature = "vars.forceReloadWholeMags [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.friendlyFire", Signature = "vars.friendlyFire [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.hitIndicatorsEnabled", Signature = "vars.hitIndicatorsEnabled [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.hud", Signature = "vars.hud [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.killCam", Signature = "vars.killCam [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.miniMap", Signature = "vars.miniMap [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.miniMapSpotting", Signature = "vars.miniMapSpotting [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.nameTag", Signature = "vars.nameTag [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.onlySquadLeaderSpawn", Signature = "vars.onlySquadLeaderSpawn [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.regenerateHealth", Signature = "vars.regenerateHealth [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.vehicleSpawnAllowed", Signature = "vars.vehicleSpawnAllowed [true|false]", MinParams = 0, MaxParams = 1 },
            // Vars - integer
            new RconCommandDef { Name = "vars.bulletDamage", Signature = "vars.bulletDamage [modifier: 0-300]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.gameModeCounter", Signature = "vars.gameModeCounter [modifier: 0-500]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.idleBanRounds", Signature = "vars.idleBanRounds [rounds]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.idleTimeout", Signature = "vars.idleTimeout [seconds]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.maxPlayers", Signature = "vars.maxPlayers [playerLimit]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.maxSpectators", Signature = "vars.maxSpectators [count]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.playerRespawnTime", Signature = "vars.playerRespawnTime [modifier: 0-300]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.roundLockdownCountdown", Signature = "vars.roundLockdownCountdown [seconds]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.roundRestartPlayerCount", Signature = "vars.roundRestartPlayerCount [count]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.roundStartPlayerCount", Signature = "vars.roundStartPlayerCount [count]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.roundTimeLimit", Signature = "vars.roundTimeLimit [modifier: 0-300]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.roundWarmupTimeout", Signature = "vars.roundWarmupTimeout [seconds]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.soldierHealth", Signature = "vars.soldierHealth [modifier: 0-300]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.teamKillCountForKick", Signature = "vars.teamKillCountForKick [count]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.teamKillKickForBan", Signature = "vars.teamKillKickForBan [count]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.teamKillValueDecreasePerSecond", Signature = "vars.teamKillValueDecreasePerSecond [value]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.teamKillValueForKick", Signature = "vars.teamKillValueForKick [value]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.teamKillValueIncrease", Signature = "vars.teamKillValueIncrease [value]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.ticketBleedRate", Signature = "vars.ticketBleedRate [modifier: 0-500]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.vehicleSpawnDelay", Signature = "vars.vehicleSpawnDelay [modifier: 0-300]", MinParams = 0, MaxParams = 1 },
            // Vars - string
            new RconCommandDef { Name = "vars.gamePassword", Signature = "vars.gamePassword [password]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.serverDescription", Signature = "vars.serverDescription [description]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.serverMessage", Signature = "vars.serverMessage [message]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.serverName", Signature = "vars.serverName [name]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.serverType", Signature = "vars.serverType [Official|Ranked|Unranked|Private]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.unlockMode", Signature = "vars.unlockMode [mode]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.preset", Signature = "vars.preset [Normal|Hardcore|Infantry|Custom] [lockPresetSetting]", MinParams = 0, MaxParams = 2 },
            new RconCommandDef { Name = "vars.mpExperience", Signature = "vars.mpExperience [experience]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.teamFactionOverride", Signature = "vars.teamFactionOverride <teamId> [factionId]", MinParams = 1, MaxParams = 2 },
        };

        public static readonly Dictionary<string, RconCommandDef> Lookup;

        static RconCommandDatabase()
        {
            Lookup = new Dictionary<string, RconCommandDef>(StringComparer.OrdinalIgnoreCase);
            foreach (var cmd in Commands)
                Lookup[cmd.Name] = cmd;
        }
    }
}
