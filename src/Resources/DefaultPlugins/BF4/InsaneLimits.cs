/*
 * Copyright 2011 Miguel Mendoza - miguel@micovery.com, PapaCharlie9, Singh400, EBastard, Hedius
 *
 * Insane Balancer is free software: you can redistribute it and/or modify it under the terms of the
 * GNU General Public License as published by the Free Software Foundation, either version 3 of the License,
 * or (at your option) any later version. Insane Balancer is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. You should have received a copy of the
 * GNU General Public License along with Insane Balancer. If not, see http://www.gnu.org/licenses/.
 *
 */

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Data;
using System.Linq;

using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom;
using System.IO.Compression;
using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;



using System.Security.Cryptography;



namespace PRoConEvents
{

    //Aliases
    using EventType = PRoCon.Core.Events.EventType;
    using CapturableEvent = PRoCon.Core.Events.CapturableEvents;


    // bitwise safe enums

    public enum HTTPMethod
    {
        POST = 0x01,
        GET = 0x02,
        PUT = 0x04
    };

    public enum EABanDuration
    {
        Temporary = 0x01,
        Permanent = 0x02,
        Round = 0x04
    };

    public enum PBBanDuration
    {
        Temporary = 0x01,
        Permanent = 0x02
    };

    public enum MessageAudience
    {
        All = 0x01,
        Team = 0x02,
        Squad = 0x04,
        Player = 0x08
    };

    public enum EABanType
    {
        EA_GUID = 0x01,
        IPAddress = 0x02,
        Name = 0x04
    };

    public enum PBBanType { PB_GUID = 0x01 };

    public enum StatSource
    {
        Web = 0x01,
        Round = 0x02,
        Total = 0x04
    };

    public enum BaseEvent
    {
        None = 0x000,
        Kill = 0x001,
        Suicide = 0x002,
        TeamKill = 0x004,
        Spawn = 0x008,
        GlobalChat = 0x010,
        TeamChat = 0x020,
        SquadChat = 0x040,
        RoundOver = 0x080,
        RoundStart = 0x100,
        TeamChange = 0x200,
        Leave = 0x400
    };



    public enum Actions
    {
        None = 0x0000,
        Kick = 0x0001,
        Kill = 0x0002,
        PBBan = 0x0004,
        EABan = 0x0010,
        Say = 0x0020,
        Log = 0x0040,
        TaskbarNotify = 0x0080,
        Mail = 0x0100,
        SMS = 0x0200,
        Tweet = 0x0400,
        PBCommand = 0x0800,
        ServerCommand = 0x1000,
        PRoConEvent = 0x2000,
        PRoConChat = 0x4000,
        SoundNotify = 0x8000,
        Yell = 0x10000
    }

    public enum TrueFalse
    {
        False = 0x01,
        True = 0x02
    };

    public enum AcceptDeny
    {
        Accept = 0x01,
        Deny = 0x02
    }

    public enum LimitChoice
    {
        All = 0x01,
        NotCompiled = 0x02
    };

    public enum ShowHide
    {
        Show = 0x01,
        Hide = 0x02
    }

    public interface DataDictionaryInterface
    {
        /* String Data */
        String setString(String key, String value);
        String getString(String key);
        String unsetString(String key);
        bool issetString(String key);
        List<String> getStringKeys();

        /* Boolean Data */
        bool setBool(String key, bool value);
        bool getBool(String key);
        bool unsetBool(String key);
        bool issetBool(String key);
        List<String> getBoolKeys();

        /* Double Data */
        double setDouble(String key, double value);
        double getDouble(String key);
        double unsetDouble(String key);
        bool issetDouble(String key);
        List<String> getDoubleKeys();

        /* Int Data */
        int setInt(String key, int value);
        int getInt(String key);
        int unsetInt(String key);
        bool issetInt(String key);
        List<String> getIntKeys();

        /* Object Data */
        object setObject(String key, object value);
        object getObject(String key);
        object unsetObject(String key);
        bool issetObject(String key);
        List<String> getObjectKeys();

        /* Generic set/get methods */
        Object set(Type type, String key, Object value);
        Object get(Type type, String key);
        Object unset(Type type, String key);
        bool isset(Type type, String key);
        List<String> getKeys(Type type);

        /* Other methods */
        void Clear();  /* clear/unset all data from repository */

    }

    public interface TeamInfoInterface
    {
        List<PlayerInfoInterface> players { get; }

        double KillsRound { get; }
        double DeathsRound { get; }
        double SuicidesRound { get; }
        double TeamKillsRound { get; }
        double TeamDeathsRound { get; }
        double HeadshotsRound { get; }
        double ScoreRound { get; }

        int TeamId { get; }
        double TicketsRound { get; } /* deprecated */
        double Tickets { get; }
        double RemainTickets { get; }
        double RemainTicketsPercent { get; }
        double StartTickets { get; }

        // BF4
        int Faction { get; }
    }

    public interface LimitInfoInterface
    {


        //Number of times the limit has been activated, (Current round)
        double Activations(String PlayerName);
        double Activations(int TeamId, int SquadId);
        double Activations(int TeamId);

        // Number of times player has activated this limit (Current round) in the given TimeSpan, e.g. last 10 seconds, etc
        double Activations(String PlayerName, TimeSpan time);
        double Activations();

        //Number of times the limit has been activated (All rounds)
        double ActivationsTotal(String PlayerName);
        double ActivationsTotal(int TeamId, int SquadId);
        double ActivationsTotal(int TeamId);
        double ActivationsTotal();

        // Number of times this limit has been activated by player
        /*
         * Kill, TeamKill: Spree value is reset when player dies
         * Death, TeamDeath, and Suicide: Spree value is reset whe player makes a kill
         *
         * Spawn, Join, Interval: Spree value is never reset, you may reset it manually.
         */

        double Spree(String PlayerName);


        // manually resets the Spree value for the player, (only for power-users)
        void ResetSpree(String PlayerName);

        /* Data Repository set/get custom data */

        DataDictionaryInterface Data { get; }        //this dictionary is user-managed
        DataDictionaryInterface RoundData { get; }   //this dictionary is automatically cleared OnRoundStart
        DataDictionaryInterface DataRound { get; }   //this dictionary is automatically cleared OnRoundStart

        /* Other methods */
        String LogFile { get; }

    }

    public interface BattlelogWeaponStatsInterface
    {
        double Accuracy { get; }
        double Kills { get; }
        double Headshots { get; }
        double ShotsFired { get; }
        double ShotsHit { get; }
        String Name { get; }
        String Slug { get; }
        String Category { get; }
        String Code { get; }
        double TimeEquipped { get; }
    }

    public interface WeaponStatsInterface
    {

        double KillsRound { get; }
        double DeathsRound { get; }
        double SuicidesRound { get; }
        double TeamKillsRound { get; }
        double TeamDeathsRound { get; }
        double HeadshotsRound { get; }

        double KillsTotal { get; }
        double DeathsTotal { get; }
        double SuicidesTotal { get; }
        double TeamKillsTotal { get; }
        double TeamDeathsTotal { get; }
        double HeadshotsTotal { get; }
    }

    public interface ClanStatsInterface
    {

        double KillsRound { get; }
        double DeathsRound { get; }
        double SuicidesRound { get; }
        double TeamKillsRound { get; }
        double TeamDeathsRound { get; }
        double HeadshotsRound { get; }

        double KillsTotal { get; }
        double DeathsTotal { get; }
        double SuicidesTotal { get; }
        double TeamKillsTotal { get; }
        double TeamDeathsTotal { get; }
        double HeadshotsTotal { get; }
    }


    public interface ServerInfoInterface
    {
        /* Server State */
        int CurrentRound { get; }
        int TotalRounds { get; }
        int PlayerCount { get; }
        int MaxPlayers { get; }

        /* Current Map Data */
        int MapIndex { get; }
        String MapFileName { get; }
        String Gamemode { get; }
        double GameModeCounter { get; }
        double CTFRoundTimeModifier { get; }

        /* Next Map Data */
        int NextMapIndex { get; }
        String NextMapFileName { get; }
        String NextGamemode { get; }
        
        /* Map Rotation */
        List<String> MapFileNameRotation { get; }
        List<String> GamemodeRotation { get; }
        List<int> LevelRoundsRotation { get; }

        /* All players, Current Round, Stats */
        double KillsRound { get; }
        double DeathsRound { get; }   // kind of useless, should be same as KillsTotal (suices not counted as death)
        double HeadshotsRound { get; }
        double SuicidesRound { get; }
        double TeamKillsRound { get; }


        /* All players, All rounds, Stats */

        double KillsTotal { get; }
        double DeathsTotal { get; }  // kind of useless, should be same s KillsTotal (suicides not counted as death)
        double HeadshotsTotal { get; }
        double SuicidesTotal { get; }
        double TeamKillsTotal { get; }


        /* Weapon Stats, Current Round, All Rounds (Total)*/
        WeaponStatsInterface this[String WeaponName] { get; }



        /* Other data */
        double TimeRound { get; }                // Time since round started
        double TimeTotal { get; }                // Time since plugin enabled
        double TimeUp { get; }                   // Time since last server restart
        double RoundsTotal { get; }              // Round played since plugin enabled

        /* Meta Data */
        String Port { get; }                     // Layer/Server port number
        String Host { get; }                     // Layer/Server Host
        String Name { get; }
        String Description { get; }
        String GameVersion { get; } // BF3 or BF4

        /* var.* value that is updated every update_interval seconds */
        int BulletDamage { get; }
        bool FriendlyFire { get; }
        int GunMasterWeaponsPreset { get; }
        double IdleTimeout { get; } // seconds
        int SoldierHealth { get; }
        bool VehicleSpawnAllowed { get; }
        int VehicleSpawnDelay { get; }
        // BF4
        bool Commander { get; }
        int MaxSpectators { get; }
        String ServerType { get; }
        int GetFaction(int TeamId);


        /* Team data */
        double Tickets(int TeamId);              // tickets for the specified team
        double RemainTickets(int TeamId);        // tickets remaining on specified team
        double RemainTicketsPercent(int TeamId); // tickets remaining on specified team (as percent)

        double StartTickets(int TeamId);         // tickets at the begining of round for specified team
        double TargetTickets { get; }            // tickets needed to win


        int OppositeTeamId(int TeamId);
        int WinTeamId { get; } //id of the team that won previous round

        /* Data Repository set/get custom data */

        DataDictionaryInterface Data { get; }        //this dictionary is user-managed
        DataDictionaryInterface RoundData { get; }   //this dictionary is automatically cleared OnRoundStart
        DataDictionaryInterface DataRound { get; }   //this dictionary is automatically cleared OnRoundStart
    }

    public interface KillInfoInterface
    {
        DateTime Time { get; }
        String Weapon { get; }
        bool Headshot { get; }
        String Category { get; } // ToString of DamageTypes
    }

    public interface KillReasonInterface
    {
        String Name { get; } // weapon name or reason, like "Suicide"
        String Detail { get; } // BF4: ammo or attachment
        String AttachedTo { get; } // BF4: main weapon when Name is a secondary attachment, like M320
        String VehicleName { get; } // BF4: if Name is "Death", this is the vehicle's name
        String VehicleDetail { get; } // BF4: if Name is "Death", this is the vehicles detail (stuff after final slash)
    }

    public interface PlayerInfoInterface
    {
        /* Online statistics (battlelog.battlefield.com) */
        double Rank { get; }
        double Kdr { get; }
        double Time { get; }
        double Kills { get; }
        double Wins { get; }
        double Skill { get; }
        double Spm { get; }
        double Score { get; }
        double Deaths { get; }
        double Losses { get; }
        double Repairs { get; }
        double Revives { get; }
        double Accuracy { get; }
        double Ressuplies { get; }
        double QuitPercent { get; }
        double ScoreTeam { get; }
        double ScoreCombat { get; }
        double ScoreVehicle { get; }
        double ScoreObjective { get; }
        double VehiclesKilled { get; }
        double KillStreakBonus { get; }
        double Kpm { get; }

        double KillAssists { get; }
        double ResetDeaths { get; }
        double ResetKills { get; }
        double ResetLosses { get; }
        double ResetWins { get; }
        double ResetScore { get; }
        double ResetShotsFired { get; }
        double ResetShotsHit { get; }
        double ResetTime { get; }

        double ReconTime { get; }
        double EngineerTime { get; }
        double AssaultTime { get; }
        double SupportTime { get; }
        double VehicleTime { get; }
        double ReconPercent { get; }
        double EngineerPercent { get; }
        double AssaultPercent { get; }
        double SupportPercent { get; }
        double VehiclePercent { get; }

        /* Player data */
        String Name { get; }
        String FullName { get; } // name including clan-tag
        String Tag { get; }
        String IPAddress { get; }
        String CountryCode { get; }
        String CountryName { get; }
        String PBGuid { get; }
        String EAGuid { get; }
        int TeamId { get; }
        int SquadId { get; }
        int Ping { get; }
        int MaxPing { get; }
        int MinPing { get; }
        int MedianPing { get; }
        int AveragePing { get; }
        int Role { get; } // BF4: 0 = PLAYER, 1 = SPECTATOR, 2 = COMMANDER, 3 = MOBILE COMMANDER

        /* Current round, Player Stats */
        double KdrRound { get; }
        double KpmRound { get; }
        double SpmRound { get; }
        double ScoreRound { get; }
        double KillsRound { get; }
        double DeathsRound { get; }
        double HeadshotsRound { get; }
        double TeamKillsRound { get; }
        double TeamDeathsRound { get; }
        double SuicidesRound { get; }
        double TimeRound { get; }

        /* All rounds, Player Stats */
        double KdrTotal { get; }
        double KpmTotal { get; }
        double SpmTotal { get; }
        double ScoreTotal { get; }
        double KillsTotal { get; }
        double DeathsTotal { get; }
        double HeadshotsTotal { get; }
        double TeamKillsTotal { get; }
        double TeamDeathsTotal { get; }
        double SuicidesTotal { get; }
        double TimeTotal { get; }
        double RoundsTotal { get; }

        /* Weapon Stats, Current Round, All Rounds (Total) */
        WeaponStatsInterface this[String WeaponName] { get; }
        
        /* Battlelog Weapon Stats function: use kill.Weapon for WeaponName */
        BattlelogWeaponStatsInterface GetBattlelog(String WeaponName);

        /* Other Data */
        DateTime JoinTime { get; }
        String LastChat { get; }   // text of the last chat sent by player
        bool Battlelog404 { get; } // True - Player has no PC Battlelog profile
        bool StatsError { get; }   // True - Error occurred while processing player stats


        /* Whitelist information */
        bool inClanWhitelist { get; }
        bool inPlayerWhitelist { get; }
        bool isInWhitelist { get; }


        /* Data Repository set/get custom data */

        DataDictionaryInterface Data { get; }        //this dictionary is user-managed
        DataDictionaryInterface RoundData { get; }   //this dictionary is automatically cleared OnRoundStart
        DataDictionaryInterface DataRound { get; }   //this dictionary is automatically cleared OnRoundStart

        /* Killer/Victim Data */

        Dictionary<String, List<KillInfoInterface>> TeamKillVictims { get; }
        Dictionary<String, List<KillInfoInterface>> TeamKillKillers { get; }
        Dictionary<String, List<KillInfoInterface>> Victims { get; }
        Dictionary<String, List<KillInfoInterface>> Killers { get; }

    }


    public interface PluginInterface
    {

        /*
         * Methods for sending messages
         */

        bool SendGlobalMessage(String message);
        bool SendTeamMessage(int teamId, String message);
        bool SendSquadMessage(int teamId, int squadId, String message);
        bool SendPlayerMessage(String name, String message);

        bool SendGlobalMessage(String message, int delay);
        bool SendTeamMessage(int teamId, String message, int delay);
        bool SendSquadMessage(int teamId, int squadId, String message, int delay);
        bool SendPlayerMessage(String name, String message, int delay);

        bool SendGlobalYell(String message, int duration);
        bool SendTeamYell(int teamId, String message, int duration);
        bool SendPlayerYell(String name, String message, int duration);

        bool SendMail(String address, String subject, String body);
        bool SendSMS(String country, String carrier, String number, String message);

        /*
         * Methods used for writing to the Plugin console
         */
        void ConsoleWrite(String text);
        void ConsoleWarn(String text);
        void ConsoleError(String text);
        void ConsoleException(String text);



        /*
         * Methods for getting whitelist information
         *
         */
        bool isInWhitelist(String PlayerName);
        bool isInPlayerWhitelist(String PlayerName);
        bool isInClanWhitelist(String PlayerName);
        bool isInWhiteList(String PlayerName, String list_name);

        /* Method for checking generic lists */
        bool isInList(String item, String list_name);

        List<String> GetReservedSlotsList();

        /*
         * Methods getting and setting the Plugin's variables
         */

        bool setPluginVarValue(String variable, String value);
        String getPluginVarValue(String variable);


        /*
         *  Method: R
         *
         *  Replaces tags like %p_n% (Player Name), %k_n% (Killer Name), %v_n% (Victim Name), etc
         */

        String R(String message);


        /*
         * Methods for actions
         */

        bool KickPlayerWithMessage(String name, String message);
        bool KillPlayer(String name);  /* deprecated, do not use */
        bool KillPlayer(String name, int delay);
        bool EABanPlayerWithMessage(EABanType type, EABanDuration duration, String name, int minutes, String message);
        bool PBBanPlayerWithMessage(PBBanDuration duration, String name, int minutes, String message);
        bool PBCommand(String text);
        bool MovePlayer(String name, int TeamId, int SquadId, bool force);
        bool SendTaskbarNotification(String title, String messagge);
        bool Log(String file, String message);
        bool Tweet(String status);
        bool PRoConChat(String text);
        bool PRoConEvent(String text, String player);
        bool SendSoundNotification(String soundfile, String soundfilerepeat);

        void ServerCommand(params String[] arguments);

        /*
         * Examples:
         *
         *           KickPlayerWithMessage(""micovery"" , ""Kicked you for team-killing!"");
         *           EABanPlayerWithMessage(EABanType.EA_GUID, EABanDuration.Temporary, ""micovery"", 10, ""You are banned for 10 minutes!"");
         *           PBBanPlayerWithMessage(PBBanDuration.Permanent, ""micovery"", 0, ""You are banned forever!"");
         *           ServerCommand(""admin.listPlayers"", ""all"");
         */


        /* Other Methods */

        String FriendlySpan(TimeSpan span);         //converts a TimeSpan into a friendly formatted string e.g. "2 hours, 20 minutes, 15 seconds"
        String BestPlayerMatch(String name);        //looks in the internal player's list, and finds the best match for the given player name

        bool IsInGameCommand(String text);          //checks if the given text start with one of these characters: !/@?
        bool IsCommand(String text);                //checks if the given text start with one of these characters: !/@?

        String ExtractInGameCommand(String text);   //if given text starts with one of these charactets !/@? it removes them
        String ExtractCommand(String text);         //if given text starts with one of these charactets !/@? it removes them

        String ExtractCommandPrefix(String text);   //if given text starts with one of these chracters !/@? it returns the character

        bool CheckAccount(String name, out bool canKill, out bool canKick, out bool canBan, out bool canMove, out bool canChangeLevel);
        
        double CheckPlayerIdle(String name); // -1 if unknown, otherwise idle time in seconds

        /* Updated every update_interval seconds */
        bool IsSquadLocked(int TeamId, int SquadId); // False if unknown or open, True if locked
        String GetSquadLeaderName(int TeamId, int SquadId); // null if unknown, player name otherwise

        /* This method looks in the internal player's list for player with matching name.
         * If fuzzy argument is set to true, it will find the player name that best matches the given name
         */
        PlayerInfoInterface GetPlayer(String name, bool fuzzy);

        /*
         * Creates a file in ProCOn's directory  (InsaneLimits.dump)
         * Detailed information about the exception.
         */
        void DumpException(Exception e);
        void DumpException(Exception e, String prefix);

        /* Data Repository set/get custom data */
        DataDictionaryInterface Data { get; }        //this dictionary is user-managed
        DataDictionaryInterface RoundData { get; }   //this dictionary is automatically cleared OnRoundStart
        DataDictionaryInterface DataRound { get; }   //this dictionary is automatically cleared OnRoundStart

        /* Friendly names */
        String FriendlyMapName(String mapFileName);  //example: "MP_001" -> "Grand Bazaar"
        String FriendlyModeName(String modeName);    //example: "TeamDeathMatch0" -> "TDM"
        KillReasonInterface FriendlyWeaponName(String killWeapon); 
            // BF3 example: "Weapons/XP2_L86/L86" => KillReason("L86", null, null)
            // BF4 example: "U_AK12_M320_HE" => KillReason("M320", "HE", "AK12")
            // BF4 vehicle example: "Gameplay/Vehicles/AH6/AH6_Littlebird" => KillReason("Death", null, null, "AH6", "AH6_Littlebird")

        /* External plugin support */
        bool IsOtherPluginEnabled(String className, String methodName);
        void CallOtherPlugin(String className, String methodName, Hashtable parms);
        DateTime GetLastPluginDataUpdate(); // return timestamp for the last time InsaneLimits.UpdatePluginData() was called
    }



    public class InsaneLimits : PRoConPluginAPI, IPRoConPluginInterface, PluginInterface
    {
        public String server_host = String.Empty;
        public String server_port = String.Empty;
        public String server_name = String.Empty;
        public String server_desc = String.Empty;

        public String[] Carriers = new string[]
        {
            /* Country, Carrier, Gateway */

            "Argentina", "Claro", "number@sms.ctimovil.com.ar",
            "Argentina", "Movistar", "number@sms.movistar.net.ar",
            "Argentina", "Nextel", "TwoWay.11number@nextel.net.ar",
            "Argentina", "Personal", "number@alertas.personal.com.ar",
            "Aruba", "Setar Mobile", "number@mas.aw",
            "Australia", "T-Mobile(Optus Zoo)", "0number@optusmobile.com.au",

            "Austria", "T-Mobile", "number@sms.t-mobile.at",
            "Brasil", "Claro", "number@clarotorpedo.com.br",
            "Brasil", "Vivo", "number@torpedoemail.com.br",
            "Bulgaria", "Globul", "35989number@sms.globul.bg",
            "Bulgaria", "Mobiltel", "35988number@sms.mtel.net",
            "Bulgaria", "Vivacom", "35987number@sms.vivacom.bg",
            "Canada", "Aliant", "number@sms.wirefree.informe.ca",
            "Canada", "Bell Mobility", "number@txt.bell.ca",
            "Canada", "Solo Mobile", "number@txt.bell.ca",
            "Canada", "Fido", "number@fido.ca",
            "Canada", "Koodo Mobile", "number@msg.telus.com",
            "Canada", "MTS Mobility", "number@text.mtsmobility.com",
            "Canada", "PC Telecom", "number@mobiletxt.ca",
            "Canada", "Rogers Wireless", "number@pcs.rogers.com",
            "Canada", "SaskTel", "number@sms.sasktel.com",
            "Canada", "Telus Mobility", "number@msg.telus.com",
            "Canada", "Virgin Mobile", "number@vmobile.ca",
            "China", "China Mobile", "number@139.com",
            "Colombia", "Comcel", "number@comcel.com.co",
            "Colombia", "Movistar", "number@movistar.com.co",
            "Colombia", "Tigo (Formerly Ola)", "number@sms.tigo.com.co",
            "Costa Rica", "ICE", "number@ice.cr",
            "Europe", "TellusTalk", "number@esms.nu",
            "France", "Bouygues Telecom", "number@mms.bouyguestelecom.fr",
            "Germany", "E-Plus", "0number@smsmail.eplus.de",
            "Germany", "O2", "0number@o2online.de",
            "Germany", "T-Mobile", "number@t-mobile-sms.de",
            "Germany", "Vodafone", "0number@vodafone-sms.de",
            "Hong Kong", "CSL", "number@mgw.mmsc1.hkcsl.com",
            "Hungary", "Ozeki", "number@ozekisms.com",
            "Iceland", "OgVodafone", "number@sms.is",
            "Iceland", "Siminn", "number@box.is",
            "India", "Aircel", "number@aircel.co.in",
            "India", "Andhra Pradesh Aircel", "number@airtelap.com",
            "India", "Karnataka Airtel", "number@airtelkk.com",
            "India", "Andhra Pradesh AirTel", "91number@airtelap.com",
            "India", "Andhra Pradesh Idea Cellular", "number@ideacellular.net",
            "India", "Chennai Skycell / Airtel", "919840number@airtelchennai.com",
            "India", "Chennai RPG Cellular", "9841number@rpgmail.net",
            "India", "Delhi Airtel", "919810number@airtelmail.com",
            "India", "Delhi Hutch", "9811number@delhi.hutch.co.in",
            "India", "Goa Airtel", "919890number@airtelmail.com",
            "India", "Goa Idea Cellular", "number@ideacellular.net",
            "India", "Goa BPL Mobile", "9823number@bplmobile.com",
            "India", "Gujarat Idea Cellular", "number@ideacellular.net",
            "India", "Gujarat Airtel", "919898number@airtelmail.com",
            "India", "Gujarat Celforce / Fascel", "9825number@celforce.com",
            "India", "Haryana Airtel", "919896number@airtelmail.com",
            "India", "Haryana Escotel", "9812number@escotelmobile.com",
            "India", "Himachai Pradesh Airtel", "919816number@airtelmail.com",
            "India", "Karnataka Airtel", "919845number@airtelkk.com",
            "India", "Kerala Airtel", "919895number@airtelkerala.com",
            "India", "Kerala BPL Mobile", "9846number@bplmobile.com",
            "India", "Kerala Escotel", "9847number@escotelmobile.com",
            "India", "Koltaka Airtel", "919831number@airtelkol.com",
            "India", "Madhya Pradesh Airtel", "919893number@airtelmail.com",
            "India", "Maharashtra Airtel", "919890number@airtelmail.com",
            "India", "Maharashtra BPL Mobile", "9823number@bplmobile.com",
            "India", "Maharashtra Idea Cellular", "number@ideacellular.net",
            "India", "Mumbai Airtel", "919892number@airtelmail.com",
            "India", "Mumbai BPL Mobile", "9821number@bplmobile.com",
            "India", "Pondicherry BPL Mobile", "9843number@bplmobile.com",
            "India", "Punjab Airtel", "919815number@airtelmail.com",
            "India", "Tamil Nadu Airtel", "919894number@airtelmobile.com",
            "India", "Tamil Nadu Aircel", "9842number@airsms.com",
            "India", "Tamil Nadu BPL Mobile", "919843number@bplmobile.com",
            "India", "Uttar Pradesh West Escotel", "9837number@escotelmobile.com",
            "India", "Loop (BPL Mobile), Mumbai", "number@loopmobile.co.in",
            "Ireland", "Meteor", "number@sms.mymeteor.ie",
            "Israel", "Spikko", "number@SpikkoSMS.com",
            "Italy", "TIM", "0number@timnet.com",
            "Japan", "AU, KDDI", "number@ezweb.ne.jp",
            "Japan", "NTT DoCoMo", "number@docomo.ne.jp",
            "Japan", "Vodafone, Chuugoku/Western", "number@n.vodafone.ne.jp",
            "Japan", "Vodafone, Hokkaido", "number@d.vodafone.ne.jp",
            "Japan", "Vodafone, Hokuriko/Central North", "number@r.vodafone.ne.jp",
            "Japan", "Vodafone, Kansai/West/Osaka", "number@k.vodafone.ne.jp",
            "Japan", "Vodafone, Kanto/Koushin/East", "number@t.vodafone.ne.jp",
            "Japan", "Vodafone, Kyuushu/Okinawa", "number@q.vodafone.ne.jp",
            "Japan", "Vodafone, Skikoku", "number@s.vodafone.ne.jp",
            "Japan", "Vodafone, Touhoku/Niigata/North", "number@h.vodafone.ne.jp",
            "Japan", "Willcom", "number@pdx.ne.jp",
            "Japan", "Willcom DJ", "number@dj.pdx.ne.jp",
            "Japan", "Willcom DI", "number@di.pdx.ne.jp",
            "Japan", "Willcom DK", "number@dk.pdx.ne.jp",
            "Mauritius", "Emtel", "number@emtelworld.net",
            "Mexico", "Nextel", "number@msgnextel.com.mx",
            "Nepal", "Mero Mobile", "977number@sms.spicenepal.com",
            "Netherlands", "Orange", "0number@sms.orange.nl",
            "Netherlands", "T-Mobile", "31number@gin.nl",
            "New Zealand", "Telecom New Zealand", "number@etxt.co.nz",
            "New Zealand", "Vodafone", "number@mtxt.co.nz",
            "Nicaragua", "Claro", "number@ideasclaro-ca.com",
            "Norway", "Sendega", "number@sendega.com",
            "Poland", "Orange Polska", "9digit@orange.pl",
            "Poland", "Plus", "+number@text.plusgsm.pl",
            "Puerto Rico", "Claro", "number@vtexto.com",
            "Singapore", "M1", "number@m1.com.sg",
            "South Africa", "MTN", "number@sms.co.za",
            "South Africa", "Vodacom", "number@voda.co.za",
            "South Korea", "Helio", "number@myhelio.com",
            "Spain", "Esendex", "number@esendex.net",
            "Spain", "Movistar", "0number@movistar.net",
            "Spain", "Vodafone", "0number@vodafone.es",
            "Singapore", "Starhub Enterprise Messaging Solution", "number@starhub-enterprisemessaging.com",
            "Sri Lanka", "Mobitel", "number@sms.mobitel.lk",
            "Sweden", "Tele2", "0number@sms.tele2.se",
            "Switzerland", "Sunrise Communications", "number@gsm.sunrise.ch",
            "United States", "Alaska Communications", "number@msg.acsalaska.com",
            "United States", "Alltel (Allied Wireless)", "number@sms.alltelwireless.com",
            "United States", "Verizon Wireless (Alltel Merger)", "number@text.wireless.alltel.com",
            "United States", "Ameritech", "number@paging.acswireless.com",
            "United States", "ATT Wireless", "number@txt.att.net",
            "United States", "ATT Enterprise Paging", "number@page.att.net",
            "United States", "ATT Global Smart Suite", "number@sms.smartmessagingsuite.com",
            "United States", "BellSouth", "number@bellsouth.cl",
            "United States", "Bluegrass Cellular", "number@sms.bluecell.com",
            "United States", "Bluesky Communications, Samoa", "number@psms.bluesky.as",
            "United States", "Boost Mobile", "number@myboostmobile.com",
            "United States", "Cellcom", "number@cellcom.quiktxt.com",
            "United States", "Cellular One", "number@mobile.celloneusa.com",
            "United States", "Cellular South", "number@csouth1.com",
            "United States", "Centenial Wireless", "number@cwemail.com",
            "United States", "Cariton Valley Wireless", "number@sms.cvalley.net",
            "United States", "Cincinnati Bell", "number@gocbw.com",
            "United States", "Cingular", "number@cingular.com",
            "United States", "Cingular (GoPhone)", "number@cingulartext.com",
            "United States", "Cleartalk Wireless", "number@sms.cleartalk.us",
            "United States", "Cricket", "number@sms.mycricket.com",
            "United States", "Edge Wireless", "number@sms.edgewireless.com",
            "United States", "Element Mobile", "number@SMS.elementmobile.net",
            "United States", "Esendex", "number@echoemail.net",
            "United States", "General Communications", "number@mobile.gci.net",
            "United States", "Golden State Cellular", "number@gscsms.com",
            "United States", "Hawaii Telcom Wireless", "number@hawaii.sprintpcs.com",
            "United States", "Helio", "number@myhelio.com",
            "United States", "Kajeet", "number@mobile.kajeet.net",
            "United States", "MetroPCS", "number@mymetropcs.com",
            "United States", "Nextel", "number@messaging.nextel.com",
            "United States", "O2", "number@mobile.celloneus.com",
            "United States", "Orange", "number@mobile.celloneus.com",
            "United States", "PagePlus Cellular", "number@vtext.com",
            "United States", "Pioneer Cellular", "number@zsend.com",
            "United States", "Pocket Wireless", "number@sms.pocket.com",
            "United States", "TracFone (prepaid)", "number@mmst5.tracfone.com",
            "United States", "Sprint (PCS)", "number@messaging.sprintpcs.com",
            "United States", "Nextel (Sprint)", "number@page.nextel.com",
            "United States", "Straight Talk", "number@vtext.com",
            "United States", "Syringa Wireless", "number@rinasms.com",
            "United States", "T-Mobile", "number@tmomail.net",
            "United States", "Teleflip", "number@teleflip.com",
            "United States", "Telus Mobility", "number@msg.telus.com",
            "United States", "Unicel", "number@utext.com",
            "United States", "US Cellular", "number@email.uscc.net",
            "United States", "US Mobility", "number@usmobility.net",
            "United States", "Verizon Wireless", "number@vtext.com",
            "United States", "Viaero", "number@viaerosms.com",
            "United States", "Virgin Mobile", "number@vmobl.com",
            "United States", "XIT Communications" , "number@sms.xit.net",
            "United States", "Qwest Wireless", "number@qwestmp.com",
            "United States", "Rogers Wireless", "number@pcs.rogers.com",
            "United States", "Simple Mobile", "number@smtext.com",
            "United States", "South Central Communications", "number@rinasms.com",
            "United Kingdom", "AQL", "number@text.aql.com",
            "United Kingdom", "Esendex","number@echoemail.net",
            "United Kingdom", "HSL", "number@sms.haysystems.com",
            "United Kingdom", "My-Cool-SMS", "number@my-cool-sms.com",
            "United Kingdom", "O2", "44number@mmail.co.uk",
            "United Kingdom", "Orange", "number@orange.net",
            "United Kingdom", "Txtlocal", "number@txtlocal.co.uk",
            "United Kingdom", "T-Mobile", "0n@t-mobile.uk.net",
            "United Kingdom", "UniMovil Corporation", "number@viawebsms.com",
            "United Kingdom", "Virgin Mobile", "number@vxtras.com",
            "Worldwide", "Panacea Mobile", "number@api.panaceamobile.com"

        };

        public String[] Replacements = new string[]
        {
            // Killer Replacements (Evaluations:  OnKill, OnDeath, OnTeamKills, and OnTeamDeath)
            /* k   - killer
             * n   - name
             * ct  - Clan-Tag
             * cn  - Country Name
             * cc  - Country Code
             * ip  - IPAddress
             * eg  - EA GUID
             * pg  - Punk Buster GUID
             * fn  - full name
             */
            "%k_n%",    "Killer name",
            "%k_ct%",   "Killer clan-Tag",
            "%k_cn%",   "Killer county-name",
            "%k_cc%",   "Killer county-code",
            "%k_ip%",   "Killer ip-address",
            "%k_eg%",   "Killer EA GUID",
            "%k_pg%",   "Killer Punk-Buster GUID",
            "%k_fn%",   "Killer full name, includes Clan-Tag (if any)",

            // Victim Replacements (Evaluations:  OnKill, OnDeath, OnTeamKills, and OnTeamDeath)

            /* Legend:
             * v   - victim
             */
            "%v_n%",    "Victim name",
            "%v_ct%",   "Victim clan-Tag",
            "%v_cn%",   "Victim county-name",
            "%v_cc%",   "Victim county-code",
            "%v_ip%",   "Victim ip-address",
            "%v_eg%",   "Victim EA GUID",
            "%v_pg%",   "Vitim Punk-Buster GUID",
            "%v_fn%",   "Victim full name, includes Clan-Tag (if any)",

            // Player Repalcements (Evaluations: OnJoin, OnSpawn, OnAnyChat, OnTeamChange, and OnSuicide)

            /* Legend:
             * p   - player
             * txt - Chat Text
             */
            "%p_n%",    "Player name",
            "%p_ct%",   "Player clan-Tag",
            "%p_cn%",   "Player county-name",
            "%p_cc%",   "Player county-code",
            "%p_ip%",   "Player ip-address",
            "%p_eg%",   "Player EA GUID",
            "%p_pg%",   "Player Punk-Buster GUID",
            "%p_fn%",   "Player full name, includes Clan-Tag (if any)",
            "%p_lc%",   "Player, Text of last chat",
            // Weapon Replacements (Evaluations: OnKill, OnDeath, OnTeamKill, OnTeamDeath, OnSuicide)

            /* Legend:
             * w   - weapon
             * n   - name
             * p   - player
             * a   - All (players)
             * x   - count
             */
            "%w_n%",    "Weapon name",
            "%w_p_x%",  "Weapon, number of times used by player in current round",
            "%w_a_x%",  "Weapon, number of times used by All players in current round",

            // Limit Replacements for Activations & Spree Counts (Evaluations: Any) ... (Current Round)

            /* Legend:
             * th  - ordinal count suffix e.g. 1st, 2nd, 3rd, 4th, etc
             * x   - count, 1, 2, 3, 4, etc
             * p   - player
             * s   - squad
             * t   - team
             * a   - All (players)
             * r   - SpRee
             */
            "%p_x_th%",  "Limit, ordinal number of times limit has been activated by the player",
            "%s_x_th%",  "Limit, ordinal number of times limit has been activated by the player's squad",
            "%t_x_th%",  "Limit, ordinal number of times limit has been activated by the player's team",
            "%a_x_th%",  "Limit, ordinal number of times limit has been activated by all players in the server",
            "%r_x_th%",  "Limit, ordinal number of times limit has been activated by player without Spree value being reset",
            "%p_x%",     "Limit, number of times limit has been activated by the player",
            "%s_x%",     "Limit, number of times limit has been activated by the player's squad",
            "%t_x%",     "Limit, number of times limit has been activated by the player's team",
            "%a_x%",     "Limit, number of times limit has been activated by all players in the server",
            "%r_x%",     "Limit, number of times limit has been activated by player without Spree value being reset",

            // Limit Replacements for Activations & Spree Counts (Evaluations: Any) ... (All Rounds)
            /* Legend:
             * xa - Total count, for all rounds
             */
            "%p_xa_th%",  "Limit, ordinal number of times limit has been activated by the player",
            "%s_xa_th%",  "Limit, ordinal number of times limit has been activated by the player's squad",
            "%t_xa_th%",  "Limit, ordinal number of times limit has been activated by the player's team",
            "%a_xa_th%",  "Limit, ordinal number of times limit has been activated by all players in the server",
            "%p_xa%",     "Limit, number of times limit has been activated by the player",
            "%s_xa%",     "Limit, number of times limit has been activated by the player's squad",
            "%t_xa%",     "Limit, number of times limit has been activated by the player's team",
            "%a_xa%",     "Limit, number of times limit has been activated by all players in the server",


            "%date%", "Current date, e.g. Sunday December 25, 2011",
            "%time%", "Current time, e.g. 12:00 AM",

            "%server_host%", "Server/Layer host/IP, ",
            "%server_port%", "Server/Layer port number",

            "%l_id%", "Limit numeric id",
            "%l_n%", "Limit name",

        };



        public static String NL = System.Environment.NewLine;

        public bool plugin_enabled = false;
        bool plugin_activated = false;
        string oauth_token = String.Empty;
        string oauth_token_secret = String.Empty;
        bool round_over = false; // overloaded to also mean OnRoundOver limits evaled
        //bool sleeping = false;
        public ServerInfo serverInfo = null;
        List<MaplistEntry> mapList = null;
        public DateTime enabledTime;

        int curMapIndex = 0;
        int nextMapIndex = 0;
        public double ctfRoundTimeModifier = 0;
        public double gameModeCounter = 0;
        private List<String> lockedSquads = new List<String>();
        private Dictionary<String, String> squadLeaders = new Dictionary<string,string>();

        public Dictionary<String, DamageTypes> WeaponsDict = null;

        public BattleLog blog = null;
        public Dictionary<string, float> floatVariables;
        public Dictionary<string, string> stringListVariables;
        public Dictionary<string, string> stringVariables;
        public Dictionary<string, Boolean> booleanVariables;
        public Dictionary<string, int> integerVariables;

        public Dictionary<string, integerVariableValidator> integerVarValidators;
        public Dictionary<string, booleanVariableValidator> booleanVarValidators;
        public Dictionary<string, stringVariableValidator> stringVarValidators;
        public Dictionary<string, floatVariableValidator> floatVarValidators;

        public delegate bool integerVariableValidator(string var, int value);
        public delegate bool booleanVariableValidator(string var, bool value);
        public delegate bool stringVariableValidator(string var, string value);
        public delegate bool floatVariableValidator(string var, float value);
        
        static Dictionary<String, String> json2key = new Dictionary<string, string>();
        static Dictionary<String, String> json2keyBF4 = new Dictionary<string, string>();
        static Dictionary<String, String> gamekeys = new Dictionary<string, string>();
        static Dictionary<String, String> wjson2prop = new Dictionary<string, string>();

        public Dictionary<string, PlayerInfo> players;
        public Dictionary<String, List<String>> settings_group;
        public Dictionary<String, bool> hidden_variables;
        public List<String> exported_variables;
        public Dictionary<String, int> settings_group_order;
        public CodeDomProvider compiler;
        public CompilerParameters compiler_parameters;
        public Dictionary<String, String> ReplacementsDict;
        public Dictionary<String, String> AdvancedReplacementsDict;
        public Dictionary<String, Dictionary<String, String>> CarriersDict;

        public DataDictionary DataDict;
        public DataDictionary RoundDataDict;
        private DateTime last_data_change;
        private MatchCommand match_command_update_plugin_data;


        EventWaitHandle fetch_handle;
        EventWaitHandle enforcer_handle;
        EventWaitHandle settings_handle;
        EventWaitHandle say_handle;
        EventWaitHandle info_handle;
        EventWaitHandle scratch_handle;
        EventWaitHandle list_handle;
        EventWaitHandle indices_handle;
        EventWaitHandle server_name_handle;
        EventWaitHandle server_desc_handle;
        EventWaitHandle activate_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
        EventWaitHandle plist_handle;
        EventWaitHandle move_handle;
        EventWaitHandle pending_handle;
        EventWaitHandle reply_handle;

        Thread fetching_thread;
        Thread enforcer_thread;
        Thread say_thread;
        Thread settings_thread;
        Thread finalizer;
        Thread moving_thread;


        public Object players_mutex = new Object();
        public Object settings_mutex = new Object();
        public Object message_mutex = new Object();
        public Object moves_mutex = new Object();
        public Object evaluation_mutex = new Object();
        public Object lists_mutex = new Object();
        public Object limits_mutex = new Object();
        public Object updates_mutex = new Object();


        public Dictionary<String, Limit> limits;
        public Dictionary<String, CustomList> lists;


        public static String default_PIN_message = "Navigate to Twitter's authorization site to obtain the PIN";
        public static String default_twitter_consumer_key = "USQmPjXO3BFLDfWyLoAx0g";
        public static String default_twitter_consumer_secret = "UBpq7ULrfaXe1xLFL4xnAoBFnQ0GVsP2tdJXIRdLbVA";
        public static String default_twitter_access_token = "475558195-h1DII1daqUjvK1KJ8x5CD9taTIeuDq9JqMgQkGva";
        public static String default_twitter_access_token_secret = "LvHjwGMQTfE0f59kRBkE1tBjsz4KYhyh6pzS4iCdkxA";
        public static String default_twitter_user_id = "475558195";
        public static String default_twitter_screen_name = "InsaneLimits";

        public Dictionary<String,String> rcon2bw;
        public Dictionary<String,String> rcon2bwbf4;
        public List<String> rcon2bw_user_var; // user define mappings
        public Dictionary<String,String> rcon2bw_user;
        public Dictionary<String,String> cacheResponseTable;
        
        private bool isRoundReset = false;
        
        private int expectedPBCount = 0;
        
        public Dictionary<String,String> friendlyMaps = null;
        public Dictionary<String,String> friendlyModes = null;
        
        private bool level_loaded = false;
        
        public List<String> reserved_slots_list;

        public const float MIN_UPDATE_INTERVAL = 60;

        /* updateable var.* values */
        public int varBulletDamage = -1;
        public bool varFriendlyFire = false;
        public int varGunMasterWeaponsPreset = -1;
        public double varIdleTimeout = -1;
        public int varSoldierHealth = -1;
        public bool varVehicleSpawnAllowed = true;
        public int varVehicleSpawnDelay = -1;
        /* BF4 */
        public bool varCommander = false;
        public int varMaxSpectators = -1;
        public String varServerType = String.Empty;

        DateTime timerSquad = DateTime.Now;
        DateTime timerVars = DateTime.Now;

        // BF4
        public String game_version = "BF3";
        
        public InsaneLimits()
        {
            try
            {
                players = new Dictionary<string, PlayerInfo>();
                blog = new BattleLog(this);
                settings_group_order = new Dictionary<string, int>();
                hidden_variables = new Dictionary<string, bool>();
                exported_variables = new List<String>();
                settings_group = new Dictionary<string, List<string>>();

                this.limits = new Dictionary<string, Limit>();
                this.lists = new Dictionary<string, CustomList>();




                /* Integers */

                this.integerVariables = new Dictionary<string, int>();
                this.integerVariables.Add("delete_limit", 0);
                this.integerVariables.Add("delete_list", 0);
                this.integerVariables.Add("auto_load_interval", 120);
                this.integerVariables.Add("debug_level", 3);
                this.integerVariables.Add("smtp_port", 587);
                this.integerVariables.Add("wait_timeout", 30);


                this.integerVarValidators = new Dictionary<string, integerVariableValidator>();
                this.integerVarValidators.Add("delete_limit", integerValidator);
                this.integerVarValidators.Add("delete_list", integerValidator);
                this.integerVarValidators.Add("auto_load_interval", integerValidator);
                this.integerVarValidators.Add("debug_level", integerValidator);
                this.integerVarValidators.Add("smtp_port", integerValidator);
                this.integerVarValidators.Add("wait_timeout", integerValidator);

                /* Booleans */
                this.booleanVariables = new Dictionary<String, Boolean>();

                this.booleanVariables.Add("virtual_mode", true);
                this.booleanVariables.Add("save_limits", false);
                this.booleanVariables.Add("load_limits", false);

                this.booleanVariables.Add("use_white_list", false);
                this.booleanVariables.Add("use_direct_fetch", true);
                this.booleanVariables.Add("use_weapon_stats", false);
                this.booleanVariables.Add("use_battlelog_proxy", false);
                this.booleanVariables.Add("use_slow_weapon_stats", false);
                this.booleanVariables.Add("use_stats_log", false);
                this.booleanVariables.Add("use_custom_lists", false);
                this.booleanVariables.Add("use_custom_smtp", false);
                this.booleanVariables.Add("use_custom_storage", false);
                this.booleanVariables.Add("use_custom_twitter", false);
                this.booleanVariables.Add("twitter_setup_account", false);
                this.booleanVariables.Add("twitter_reset_defaults", false);
                this.booleanVariables.Add("use_custom_privacy_policy", false);
                this.booleanVariables.Add("privacy_policy_agreement", false);
                this.booleanVariables.Add("tweet_my_server_bans", true);
                this.booleanVariables.Add("tweet_my_server_kicks", true);
                this.booleanVariables.Add("tweet_my_plugin_state", true);
                this.booleanVariables.Add("auto_hide_sections", true);
                this.booleanVariables.Add("smtp_ssl", true);
                
                this.hidden_variables.Add("use_weapon_stats", true);

                this.booleanVarValidators = new Dictionary<string, booleanVariableValidator>();

                this.booleanVarValidators.Add("virtual_mode", booleanValidator);
                this.booleanVarValidators.Add("save_limits", booleanValidator);
                this.booleanVarValidators.Add("load_limits", booleanValidator);
                this.booleanVarValidators.Add("use_white_list", booleanValidator);
                this.booleanVarValidators.Add("use_direct_fetch", booleanValidator);
                this.booleanVarValidators.Add("use_weapon_stats", booleanValidator);
                this.booleanVarValidators.Add("use_battlelog_proxy", booleanValidator);
                this.booleanVarValidators.Add("use_slow_weapon_stats", booleanValidator);
                this.booleanVarValidators.Add("use_stats_log", booleanValidator);
                this.booleanVarValidators.Add("use_custom_lists", booleanValidator);
                this.booleanVarValidators.Add("smtp_ssl", booleanValidator);
                this.booleanVarValidators.Add("auto_hide_sections", booleanValidator);
                this.booleanVarValidators.Add("use_custom_twitter", booleanValidator);
                this.booleanVarValidators.Add("twitter_setup_account", booleanValidator);
                this.booleanVarValidators.Add("twitter_reset_defaults", booleanValidator);
                this.booleanVarValidators.Add("use_custom_privacy_policy", booleanValidator);
                this.booleanVarValidators.Add("privacy_policy_agreement", booleanValidator);





                /* Floats */
                this.floatVariables = new Dictionary<string, float>();
                this.floatVariables.Add("say_interval", 0.05f);
                this.floatVariables.Add("update_interval", MIN_UPDATE_INTERVAL);

                this.floatVarValidators = new Dictionary<string, floatVariableValidator>();
                this.floatVarValidators.Add("say_interval", floatValidator);
                this.floatVarValidators.Add("update_interval", floatValidator);

                /* String lists */
                this.stringListVariables = new Dictionary<string, string>();
                this.stringListVariables.Add("clan_white_list", @"clan1, clan2, clan3");

                this.stringListVariables.Add("player_white_list", @"micovery, player2, player3");


                /* Strings */
                this.stringVariables = new Dictionary<string, string>();

                this.stringVariables.Add("new_limit", "...");
                this.stringVariables.Add("new_list", "...");
                this.stringVariables.Add("compile_limit", "...");
                this.stringVariables.Add("limits_file", this.GetType().Name + ".conf");
                this.stringVariables.Add("console", "Type a command here to test");
                this.stringVariables.Add("smtp_host", "smtp.gmail.com");
                this.stringVariables.Add("smtp_account", "procon.insane.limits@gmail.com");
                this.stringVariables.Add("smtp_mail", "procon.insane.limits@gmail.com");
                this.stringVariables.Add("smtp_password", Decode("dG90YWxseWluc2FuZQ=="));

                this.stringVariables.Add("proxy_url", "http://127.0.0.1:3128");


                this.stringVariables.Add("twitter_verifier_pin", default_PIN_message);
                this.stringVariables.Add("twitter_consumer_key", default_twitter_consumer_key);
                this.stringVariables.Add("twitter_consumer_secret", default_twitter_consumer_secret);
                this.stringVariables.Add("twitter_access_token", default_twitter_access_token);
                this.stringVariables.Add("twitter_access_token_secret", default_twitter_access_token_secret);
                this.stringVariables.Add("twitter_user_id", default_twitter_user_id);
                this.stringVariables.Add("twitter_screen_name", default_twitter_screen_name);


                this.hidden_variables.Add("twitter_consumer_key", true);
                this.hidden_variables.Add("twitter_consumer_secret", true);
                this.hidden_variables.Add("twitter_access_token", true);
                this.hidden_variables.Add("twitter_access_token_secret", true);
                this.hidden_variables.Add("twitter_user_id", true);
                this.hidden_variables.Add("twitter_screen_name", true);
                this.hidden_variables.Add("twitter_verifier_pin", true);


                this.stringVarValidators = new Dictionary<string, stringVariableValidator>();
                this.stringVarValidators.Add("console", stringValidator);
                this.stringVarValidators.Add("new_limit", stringValidator);
                this.stringVarValidators.Add("compile_limit", stringValidator);
                this.stringVarValidators.Add("new_list", stringValidator);
                this.stringVarValidators.Add("twitter_verifier_pin", stringValidator);




                /* Grouping settings */
                List<String> limit_manager_group = new List<string>();
                limit_manager_group.Add("delete_limit");
                limit_manager_group.Add("new_limit");
                limit_manager_group.Add("compile_limit");
                settings_group.Add(LimitManagerG, limit_manager_group);


                List<String> lists_manager_group = new List<string>();
                lists_manager_group.Add("new_list");
                lists_manager_group.Add("delete_list");
                settings_group.Add(ListManagerG, lists_manager_group);

                List<String> storage_manager = new List<string>();
                storage_manager.Add("save_limits");
                storage_manager.Add("load_limits");
                storage_manager.Add("limits_file");
                storage_manager.Add("auto_load_interval");
                settings_group.Add(StorageG, storage_manager);

                List<String> white_list_group = new List<string>();
                white_list_group.Add("clan_white_list");
                white_list_group.Add("player_white_list");
                settings_group.Add(WhitelistG, white_list_group);

                List<String> custom_stmp_group = new List<string>();
                custom_stmp_group.Add("smtp_host");
                custom_stmp_group.Add("smtp_port");
                custom_stmp_group.Add("smtp_account");
                custom_stmp_group.Add("smtp_mail");
                custom_stmp_group.Add("smtp_password");
                custom_stmp_group.Add("smtp_ssl");
                settings_group.Add(MailG, custom_stmp_group);

                List<String> custom_twitter_group = new List<string>();
                custom_twitter_group.Add("twitter_setup_account");
                custom_twitter_group.Add("twitter_reset_defaults");
                custom_twitter_group.Add("twitter_verifier_pin");
                custom_twitter_group.Add("twitter_access_token");
                custom_twitter_group.Add("twitter_access_token_secret");
                custom_twitter_group.Add("twitter_consumer_key");
                custom_twitter_group.Add("twitter_consumer_secret");
                settings_group.Add(TwitterG, custom_twitter_group);

                List<String> proxy_group = new List<string>();
                proxy_group.Add("proxy_url");
                settings_group.Add(ProxyG, proxy_group);

                List<String> privacy_policy = new List<string>();
                privacy_policy.Add("tweet_my_server_bans");
                privacy_policy.Add("tweet_my_server_kicks");
                privacy_policy.Add("tweet_my_plugin_state");
                privacy_policy.Add("privacy_policy_agreement");

                settings_group.Add(PrivacyPolicyG, privacy_policy);


                settings_group_order.Add(SettingsG, 1);
                settings_group_order.Add(PrivacyPolicyG, 2);
                settings_group_order.Add(WhitelistG, 3);
                settings_group_order.Add(MailG, 4);
                settings_group_order.Add(TwitterG, 5);
                settings_group_order.Add(StorageG, 6);
                settings_group_order.Add(ProxyG, 7);
                settings_group_order.Add(ListManagerG, 8);
                settings_group_order.Add(LimitManagerG, 9);

                /* Exported Variables are those that should live in the *conf file */
                exported_variables.Add("tweet_my_server_bans");
                exported_variables.Add("tweet_my_server_kicks");
                exported_variables.Add("tweet_my_plugin_state");
                exported_variables.Add("privacy_policy_agreement");


                /* Online keys BF3 */

                json2key.Add("rank", "rank");
                json2key.Add("kdRatio", "kdr");
                json2key.Add("timePlayed", "time");
                json2key.Add("kills", "kills");
                json2key.Add("numWins", "wins");
                json2key.Add("elo", "skill");
                json2key.Add("scorePerMinute", "spm");
                json2key.Add("totalScore", "score");
                json2key.Add("deaths", "deaths");
                json2key.Add("numLosses", "losses");


                json2key.Add("repairs", "repairs");
                json2key.Add("revives", "revives");
                json2key.Add("accuracy", "accuracy");
                json2key.Add("resupplies", "ressuplies");
                json2key.Add("quitPercentage", "quit_p");


                json2key.Add("sc_team", "sc_team");
                json2key.Add("combatScore", "sc_combat");
                json2key.Add("sc_vehicle", "sc_vehicle");
                json2key.Add("sc_objective", "sc_objective");
                json2key.Add("vehiclesDestroyed", "vehicles_killed");
                json2key.Add("killStreakBonus", "killStreakBonus");

                json2key.Add("killAssists", "killAssists");
                json2key.Add("rsDeaths", "rsDeaths");
                json2key.Add("rsKills", "rsKills");
                json2key.Add("rsNumLosses", "rsNumLosses");
                json2key.Add("rsNumWins", "rsNumWins");
                json2key.Add("rsScore", "rsScore");
                json2key.Add("rsShotsFired", "rsShotsFired");
                json2key.Add("rsShotsHit", "rsShotsHit");
                json2key.Add("rsTimePlayed", "rsTimePlayed");

                /* Online keys BF4 */

                json2keyBF4.Add("rank", "rank");
                json2keyBF4.Add("kdRatio", "kdr");
                json2keyBF4.Add("timePlayed", "time");
                json2keyBF4.Add("kills", "kills");
                json2keyBF4.Add("numWins", "wins");
                json2keyBF4.Add("skill", "skill"); // diff from BF3
                json2keyBF4.Add("scorePerMinute", "spm");
                json2keyBF4.Add("totalScore", "score");
                json2keyBF4.Add("deaths", "deaths");
                json2keyBF4.Add("numLosses", "losses");


                json2keyBF4.Add("repairs", "repairs");
                json2keyBF4.Add("revives", "revives");
                json2keyBF4.Add("accuracy", "accuracy");
                json2keyBF4.Add("resupplies", "ressuplies");
                json2keyBF4.Add("quitPercentage", "quit_p");


                json2keyBF4.Add("sc_team", "sc_team");
                json2keyBF4.Add("combatScore", "sc_combat");
                json2keyBF4.Add("sc_vehicle", "sc_vehicle");
                json2keyBF4.Add("sc_objective", "sc_objective");
                json2keyBF4.Add("vehiclesDestroyed", "vehicles_killed");
                json2keyBF4.Add("killStreakBonus", "killStreakBonus");

                json2keyBF4.Add("killAssists", "killAssists");
                json2keyBF4.Add("rsDeaths", "rsDeaths");
                json2keyBF4.Add("rsKills", "rsKills");
                json2keyBF4.Add("rsNumLosses", "rsNumLosses");
                json2keyBF4.Add("rsNumWins", "rsNumWins");
                json2keyBF4.Add("rsScore", "rsScore");
                json2keyBF4.Add("rsShotsFired", "rsShotsFired");
                json2keyBF4.Add("rsShotsHit", "rsShotsHit");
                json2keyBF4.Add("rsTimePlayed", "rsTimePlayed");

                /* Game keys */

                gamekeys.Add("score", "score");
                gamekeys.Add("kills", "kills");
                gamekeys.Add("deaths", "deaths");
                gamekeys.Add("tkills", "tkills");
                gamekeys.Add("tdeaths", "tdeaths");
                gamekeys.Add("headshots", "headshots");
                gamekeys.Add("suicides", "suicides");
                gamekeys.Add("rounds", "rounds");

                /* Weapon Stat Keys */

                wjson2prop.Add("category", "Category");
                wjson2prop.Add("code", "Code");
                wjson2prop.Add("headshots", "Headshots");
                wjson2prop.Add("kills", "Kills");
                wjson2prop.Add("name", "Name");
                wjson2prop.Add("shotsFired", "ShotsFired");
                wjson2prop.Add("shotsHit", "ShotsHit");
                wjson2prop.Add("slug", "Slug");
                wjson2prop.Add("timeEquipped", "TimeEquipped");


                DataDict = new DataDictionary(this);
                RoundDataDict = new DataDictionary(this);
                last_data_change = DateTime.Now;
                match_command_update_plugin_data = new MatchCommand("InsaneLimits", "UpdatePluginData", new List<string>(), "InsaneLimits_UpdatePluginData", new List<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.None), "External plugin support, do not use this command in-game");

                rcon2bw = new Dictionary<String,String>();

                rcon2bw["870MCS"] ="870";
                rcon2bw["AEK-971"] ="AEK971";
                rcon2bw["AKS-74u"] ="AKS74U";
                rcon2bw["AN-94 Abakan"] ="AN94";
                rcon2bw["AS Val"] ="AS-VAL";
                rcon2bw["DamageArea"] = null;
                rcon2bw["DAO-12"] ="DAO";
                rcon2bw["Death"] = null;
                rcon2bw["Defib"] = null;
                rcon2bw["FIM92"] ="fim-92-stinger";
                rcon2bw["Glock18"] ="g18";
                rcon2bw["HK53"] ="g53";
                rcon2bw["jackhammer"] ="mk3a1";
                rcon2bw["JNG90"] ="jng-90";
                rcon2bw["Knife_RazorBlade"] ="Knife";
                rcon2bw["M15 AT Mine"] = null;
                rcon2bw["M26Mass"] ="m26-mass";
                rcon2bw["M27IAR"] ="M27";
                rcon2bw["M67"] = null;
                rcon2bw["M93R"] = "93r";
                rcon2bw["Medkit"] = null;
                rcon2bw["Melee"] = null;
                rcon2bw["Model98B"] ="M98B";
                rcon2bw["PP-2000"] ="PP2000";
                rcon2bw["Repair Tool"] = null;
                rcon2bw["RoadKill"] = null;
                rcon2bw["RPK-74M"] ="RPK";
                rcon2bw["SG 553 LB"] ="SG553";
                rcon2bw["Siaga20k"] ="Saiga";
                rcon2bw["SoldierCollision"] = null;
                rcon2bw["Suicide"] = null;
                rcon2bw["Steyr AUG"] = "aug-a3";
                rcon2bw["Taurus .44"] = "Taurus 44";
                rcon2bw["USAS-12"] = "USAS";
                rcon2bw["G3A3"] = "G3A4";
                rcon2bw["C4"] = null;
                rcon2bw["Claymore"] = null;
                rcon2bw["MagpulPDR"] = "PDR";
                rcon2bw["MP412REX"] = "M412 Rex";
                rcon2bw["MP443"] = "MP 443";
                rcon2bw["MP443_GM"] = "mp443-supp";
                rcon2bw["P90_GM"] = "P90";
                rcon2bw["Sa18IGLA"] = "sa-18-igla";
                rcon2bw["SCAR-H"] = "SCAR";
                rcon2bw["UMP45"] = "UMP";
                rcon2bw["ACR"] = "acw-r";
                rcon2bw["L86"] = "l86a2";
                rcon2bw["MP5K"] = "m5k";
                rcon2bw["MTAR"] = "mtar-21";
                rcon2bw["CrossBow"] = "xbow-scoped";

                rcon2bwbf4 = new Dictionary<String,String>();

                // Strip U_ prefix and _detail suffix and upper case the key from RCON weapon code
                rcon2bwbf4["M320"] = "WARSAW_ID_P_INAME_M32MGL";
                rcon2bwbf4["CBJ-MS"] = "WARSAW_ID_P_WNAME_CBJMS";
                rcon2bwbf4["CS-LR4"] = "WARSAW_ID_P_WNAME_CSLR4";
                rcon2bwbf4["FY-JS"] = "WARSAW_ID_P_WNAME_FYJS";
                rcon2bwbf4["GALILACE"] = "WARSAW_ID_P_WNAME_GALIL21";
                rcon2bwbf4["GALILACE23"] = "WARSAW_ID_P_WNAME_GALIL23";
                rcon2bwbf4["GALILACE52"] = "WARSAW_ID_P_WNAME_GALIL52";
                rcon2bwbf4["GALILACE53"] = "WARSAW_ID_P_WNAME_GALIL53";
                rcon2bwbf4["M26MASS"] = "WARSAW_ID_P_INAME_M26_MASS";
                rcon2bwbf4["M39EBR"] = "WARSAW_ID_P_WNAME_M39";
                rcon2bwbf4["M93R"] = "WARSAW_ID_P_WNAME_93R";
                rcon2bwbf4["REPAIRTOOL"] = "WARSAW_ID_P_INAME_REPAIR";
                rcon2bwbf4["RFB"] = "WARSAW_ID_P_WNAME_RFBTARGET";
                rcon2bwbf4["SA18IGLA"] = "WARSAW_ID_P_INAME_IGLA";
                rcon2bwbf4["SAIGA"] = "WARSAW_ID_P_WNAME_SAIGA12";
                rcon2bwbf4["SCAR-H"] = "WARSAW_ID_P_WNAME_SCARH";
                rcon2bwbf4["SCAR-HSV"] = "WARSAW_ID_P_WNAME_SCARHSV";
                rcon2bwbf4["SCORPION"] = "WARSAW_ID_P_WNAME_SCORP";
                rcon2bwbf4["SCOUT"] = "WARSAW_ID_P_WNAME_SCOUTELIT";
                rcon2bwbf4["SERBUSHORTY"] = "WARSAW_ID_P_WNAME_SHORTY";
                rcon2bwbf4["SG553LB"] = "WARSAW_ID_P_WNAME_SG553";
                rcon2bwbf4["TYPE95B"] = "WARSAW_ID_P_WNAME_TYPE95B1";
                rcon2bwbf4["ULTIMAX"] = "WARSAW_ID_P_WNAME_ULTIM";
                rcon2bwbf4["USAS-12"] = "WARSAW_ID_P_WNAME_USAS12";
                rcon2bwbf4["MAGPULPDR"] = "WARSAW_ID_P_WNAME_PDR";
                rcon2bwbf4["GRENADE"] = "WARSAW_ID_P_INAME_IMPACT";
                rcon2bwbf4["CLAYMORE"] = "WARSAW_ID_P_INAME_CLAYMORE";
                rcon2bwbf4["C4"] = "WARSAW_ID_P_INAME_C4";
                rcon2bwbf4["FGM148"] = "WARSAW_ID_P_INAME_FGM148";
                rcon2bwbf4["FIM92"] = "WARSAW_ID_P_INAME_FIM92";
                rcon2bwbf4["M136"] = "WARSAW_ID_P_INAME_M136";
                rcon2bwbf4["M15"] = "WARSAW_ID_P_INAME_M15";
                rcon2bwbf4["M18"] = "WARSAW_ID_P_INAME_M18";
                rcon2bwbf4["M2"] = "WARSAW_ID_P_INAME_M2";
                rcon2bwbf4["M34"] = "WARSAW_ID_P_INAME_M34";
                rcon2bwbf4["RPG7"] = "WARSAW_ID_P_INAME_RPG7";
                rcon2bwbf4["SMAW"] = "WARSAW_ID_P_INAME_SMAW";
                rcon2bwbf4["SRAW"] = "WARSAW_ID_P_INAME_SRAW";
                rcon2bwbf4["STARSTREAK"] = "WARSAW_ID_P_INAME_STARSTREAK";
                rcon2bwbf4["XM25"] = "WARSAW_ID_P_INAME_XM25";
                rcon2bwbf4["MP412REX"] = "WARSAW_ID_P_WNAME_M412REX";
                rcon2bwbf4["SLAM"] = "WARSAW_ID_P_INAME_M2";
                rcon2bwbf4["TOMAHAWK"] = "WARSAW_ID_P_INAME_MACHETE";
                rcon2bwbf4["NLAW"] = "WARSAW_ID_P_INAME_MBTLAW";


                rcon2bwbf4["DAMAGEAREA"] = null;
                rcon2bwbf4["DEATH"] = null;
                rcon2bwbf4["DEFIB"] = null;
                //?? rcon2bwbf4["M15 AT Mine"] = null;
                rcon2bwbf4["MEDKIT"] = null;
                rcon2bwbf4["PORTABLEMEDICPACK"] = null;
                rcon2bwbf4["MELEE"] = null;
                rcon2bwbf4["ROADKILL"] = null;
                rcon2bwbf4["SOLDIERCOLLISION"] = null;
                rcon2bwbf4["SUICIDE"] = null;

                rcon2bw_user_var = new List<String>();
                rcon2bw_user = new Dictionary<String,String>();

                cacheResponseTable = new Dictionary<String,String>();
                
                friendlyMaps = new Dictionary<String,String>();
                friendlyModes = new Dictionary<String,String>();
                
                reserved_slots_list = new List<String>();
            }
            catch (Exception e)
            {
                DumpException(e);
            }
        }

        public void ResetTwitterDefaults()
        {
            ConsoleWrite("Restoring default Twitter account settings for @^b" + default_twitter_screen_name + "^n");
            setStringVarValue("twitter_verifier_pin", default_PIN_message);
            setStringVarValue("twitter_consumer_key", default_twitter_consumer_key);
            setStringVarValue("twitter_consumer_secret", default_twitter_consumer_secret);
            setStringVarValue("twitter_access_token", default_twitter_access_token);
            setStringVarValue("twitter_access_token_secret", default_twitter_access_token_secret);
            setStringVarValue("twitter_user_id", default_twitter_user_id);
            setStringVarValue("twitter_screen_name", default_twitter_screen_name);
        }

        public class CustomList
        {
            public enum ListState
            {
                Enabled = 0x01,
                Disabled = 0x02
            };

            public enum ListComparison
            {
                CaseSensitive = 0x01,
                CaseInsensitve = 0x02
            };

            public static List<String> valid_fields = new List<string>(new String[]
                {
                "id", "hide", "state", "name", "comparison", "data", "delete"
                });

            public Dictionary<String, String> fields = null;
            public InsaneLimits plugin = null;



            public String Name
            {
                get { return fields["name"]; }
            }

            public String FullDisplayName
            {
                get { return "List #^b" + id + "^n - " + Name; }
            }

            public String FullName
            {
                get { return "List #" + id + " - " + Name; }
            }

            public String ShortName
            {
                get { return "List #" + id; }
            }

            public String ShortDisplayName
            {
                get { return "List #" + id; }
            }

            public String id
            {
                get { return fields["id"]; }
            }


            public ListState State
            {
                get { return (ListState)Enum.Parse(typeof(ListState), fields["state"]); }
            }

            public ListComparison Comparison
            {
                get { return (ListComparison)Enum.Parse(typeof(ListComparison), fields["comparison"]); }
            }

            public ShowHide Hide
            {
                get { return ((ShowHide)Enum.Parse(typeof(ShowHide), fields["hide"])); }
            }

            public String Data
            {
                get { return fields["data"]; }
            }

            public bool Contains(String item)
            {
                List<String> items = new List<String>(Regex.Split(Data, @"\s*,\s*"));
                bool ci = Comparison.Equals(ListComparison.CaseInsensitve);
                String ritem = items.Find(delegate(String citem) { return String.Compare(citem, item, ci) == 0; });

                return ritem != null;
            }


            private void SetupFields()
            {
                fields = new Dictionary<string, string>();
                foreach (String field_key in valid_fields)
                    fields.Add(field_key, "");
            }

            private void InitFields(String id)
            {
                String auto_hide = (plugin.getBooleanVarValue("auto_hide_sections") ? ShowHide.Hide : ShowHide.Show).ToString();

                setFieldValue("id", id);
                setFieldValue("hide", auto_hide);
                setFieldValue("state", ListState.Enabled.ToString());
                setFieldValue("name", "Name" + id);
                setFieldValue("comparison", ListComparison.CaseInsensitve.ToString());
                setFieldValue("data", "value1, value2, value3");
                setFieldValue("delete", false.ToString());

            }

            public static String extractFieldKey(String var)
            {
                Match match = Regex.Match(var, @"list_[^_]+_([^0-9]+)");
                if (match.Success)
                    return match.Groups[1].Value;

                return var;
            }

            public bool isValidFieldKey(String var)
            {

                if (valid_fields.Contains(extractFieldKey(var)))
                    return true;

                return false;
            }

            public static String extractId(String var)
            {
                Match vmatch = Regex.Match(var, @"^list_([^_]+)");
                if (vmatch.Success)
                    return vmatch.Groups[1].Value;

                return "UnknownId";
            }

            public String getField(String var)
            {
                if (!isValidFieldKey(var))
                    return "";

                String field_key = extractFieldKey(var);
                return fields[field_key];
            }

            public bool setFieldValue(String var, String val)
            {
                return setFieldValue(var, val, false);
            }


            public bool setFieldValue(String var, String val, bool ui)
            {
                //plugin.ConsoleWrite("Setting: " +var +" = " + val);
                String field_key = extractFieldKey(var);
                if (!isValidFieldKey(field_key))
                    return false;

                return validateAndSetFieldValue(field_key, val, ui);

            }

            public bool validateAndSetFieldValue(String field, String val, bool ui)
            {   //plugin.ConsoleWrite(field + " = " + val + ", UI: " + ui.ToString());
                if (field.Equals("delete"))
                {
                    /* Parse Boolean Values */
                    bool booleanValue = false;

                    if (Regex.Match(val, @"^\s*(1|true|yes)\s*$", RegexOptions.IgnoreCase).Success)
                        booleanValue = true;
                    else if (Regex.Match(val, @"^\s*(0|false|no)\s*$", RegexOptions.IgnoreCase).Success)
                        booleanValue = false;
                    else
                        return false;

                    fields[field] = booleanValue.ToString();
                }
                else if (field.Equals("state") ||
                         field.Equals("hide") ||
                         field.Equals("comparison")
                    )
                {
                    /* Parse Enum */
                    Type type = null;
                    if (field.Equals("state"))
                        type = typeof(ListState);
                    else if (field.Equals("hide"))
                        type = typeof(ShowHide);
                    else if (field.Equals("comparison"))
                        type = typeof(ListComparison);

                    try
                    {
                        fields[field] = Enum.Format(type, Enum.Parse(type, val, true), "G").ToString();

                        return true;
                    }
                    catch (FormatException)
                    {
                        return false;
                    }
                    catch (ArgumentException)
                    {
                        return false;
                    }

                }
                else if (field.Equals("data"))
                {
                    List<String> items = new List<String>(Regex.Split(val, @"\s*,\s*"));
                    items.RemoveAll(delegate(String item) { return item == null || item.Trim().Length == 0; });
                    fields[field] = String.Join(", ", items.ToArray());
                }
                else
                    fields[field] = val;

                return true;
            }

            public static bool isListVar(String var)
            {

                if (Regex.Match(var, @"^list_[^_]+_(" + String.Join("|", valid_fields.ToArray()) + ")").Success)
                    return true;

                return false;
            }

            public Dictionary<String, String> getSettings(bool display)
            {

                Dictionary<String, String> settings = new Dictionary<string, string>();

                /* optimization */
                if (display && Hide.Equals(ShowHide.Hide))
                {
                    settings.Add("list_" + id + "_hide", Hide.ToString());
                    return settings;
                }

                List<String> keys = new List<string>(fields.Keys);
                for (int i = 0; i < keys.Count; i++)
                {
                    String key = keys[i];
                    if (!fields.ContainsKey(key))
                        continue;

                    String value = fields[key];
                    settings.Add("list_" + id + "_" + key, value);

                }

                return settings;
            }

            public bool shouldSkipFieldKey(String name)
            {
                try
                {
                    if (!plugin.Agreement)
                        return true;

                    if (!isValidFieldKey(name))
                        return false;

                    String field_key = extractFieldKey(name);

                    if (Hide.Equals(ShowHide.Hide) && !field_key.Equals("hide"))
                        return true;

                    if (Regex.Match(field_key, @"(id|delete)$").Success)
                        return true;
                }
                catch (Exception e)
                {
                    plugin.DumpException(e);
                }
                return false;

            }


            public CustomList(InsaneLimits plugin, String id)
            {
                this.plugin = plugin;
                SetupFields();
                InitFields(id);
            }
        }

        public DataDictionaryInterface Data { get { return (DataDictionaryInterface)DataDict; } }
        public DataDictionaryInterface RoundData { get { return (DataDictionaryInterface)RoundDataDict; } }
        public DataDictionaryInterface DataRound { get { return (DataDictionaryInterface)RoundDataDict; } }

        public class LimitEvent
        {
            public readonly DateTime Time;
            public String Target;
            public int TeamId;
            public int SquadId;

            String MapFile;
            String Gamemode;
            Limit.LimitAction Action;
            Limit.EvaluationType Evaluation;

            public LimitEvent(Limit limit, PlayerInfoInterface player, ServerInfoInterface server)
            {
                Time = DateTime.Now;
                Target = player.Name;
                TeamId = player.TeamId;
                SquadId = player.SquadId;

                MapFile = server.MapFileName;
                Gamemode = server.Gamemode;
                Action = limit.Action;
                Evaluation = limit.Evaluation;
            }
        }

        public class Limit : LimitInfoInterface
        {
            public InsaneLimits plugin;

            public enum EvaluationType
            {
                OnInterval = 0x0001,
                OnIntervalPlayers = 0x0001,   // duplicate of OnInterval
                OnKill = 0x0002,
                OnDeath = 0x0004,
                OnTeamKill = 0x0008,
                OnTeamDeath = 0x0010,
                OnSuicide = 0x0020,
                OnSpawn = 0x0040,
                OnJoin = 0x0080,
                OnAnyChat = 0x0100,
                OnRoundOver = 0x0200,
                OnRoundStart = 0x0400,
                OnTeamChange = 0x0800,
                OnIntervalServer = 0x1000,
                OnLeave = 0x2000
                /*
                OnGlobalChat = 0x200,
                OnSquadChat  = 0x400,
                OnTeamChat   = 0x800
                */
            };

            public enum LimitType
            {
                Disabled = 0x01,
                Expression = 0x02,
                Code = 0x04
            };

            public enum LimitAction
            {
                None = Actions.None,
                Kick = Actions.Kick,
                Kill = Actions.Kill,
                PBBan = Actions.PBBan,
                EABan = Actions.EABan,
                Say = Actions.Say,
                SMS = Actions.SMS,
                Mail = Actions.Mail,
                Log = Actions.Log,
                TaskbarNotify = Actions.TaskbarNotify,
                Tweet = Actions.Tweet,
                PBCommand = Actions.PBCommand,
                ServerCommand = Actions.ServerCommand,
                PRoConEvent = Actions.PRoConEvent,
                PRoConChat = Actions.PRoConChat,
                SoundNotify = Actions.SoundNotify,
                Yell = Actions.Yell
            };


            public enum LimitState
            {
                Enabled = 0x01,
                Disabled = 0x02,
                Virtual = 0x04,
            };

            public enum LimitScope
            {
                Players = 0x01,
                Server = 0x02
            };

            public enum LimitLogDestination
            {
                File = 0x01,
                Plugin = 0x02,
                Both = 0x01 | 0x02
            };

            public long IntervalCount = long.MaxValue;
            public DateTime LastInterval = new DateTime(1970, 1, 1);
            public object evaluator = null;
            public Type type = null;

            public Dictionary<String, List<LimitEvent>> activations = null;
            /*
            // Not needed anymore, OnJoin limits are evaluated once only in OnPlayerJoin
            public Dictionary<String, List<LimitEvent>> evaluations = null;
             */
            public Dictionary<String, List<LimitEvent>> activations_total = null;
            public Dictionary<String, double> sprees = null;

            public Dictionary<String, String> fields;

            public Dictionary<String, String> group2title;
            public Dictionary<String, String> group2regex;
            public Dictionary<String, String> title2group;

            public DataDictionary DataDict;
            public DataDictionary RoundDataDict;

            public static String[] valid_groups = new String[]
            {
                "Kick Action", "kick_group", @"^kick_",
                "Kill Action", "kill_group", @"^kill_",
                "Say Action",  "say_group", @"^say_",
                "EABan Action", "ea_ban_group", @"^ea_ban_",
                "PBBan Action", "pb_ban_group", @"^pb_ban_",
                "PBCommand Action", "pb_command_group", @"^pb_command_",
                "PRoConEvent Action", "procon_event_group", @"^procon_event_",
                "PRoConChat Action", "procon_chat_group", @"^procon_chat_",
                "ServerCommand Action", "server_command_group", @"^server_command_",
                "Taskbar Notify Action", "taskbar_notify_group", @"^taskbar_",
                "Log Action", "log_group", @"^log_",
                "SMS Action", "sms_group", @"^sms_",
                "Mail Action", "mail_group", @"^mail_",
                "Tweet Action", "tweet_group", @"^tweet_",
                "Sound Notify Action", "sound_notify_group", @"^sound_",
                "Yell Action",  "yell_group", @"^yell_",
            };


            public static List<String> valid_fields = new List<string>(new String[] {
                "id", "hide", "state", "name",
                "evaluation", "evaluation_interval",
                "first_check", "first_check_expression", "first_check_code",
                "second_check", "second_check_code", "second_check_expression",
                "new_action", "action",
                "kick_group", "kick_message",
                "kill_group", "kill_delay",
                "say_group", "say_message", "say_audience", "say_delay", "say_procon_chat",
                "ea_ban_group", "ea_ban_type", "ea_ban_duration", "ea_ban_minutes", "ea_ban_message",
                "pb_ban_group", "pb_ban_type", "pb_ban_duration", "pb_ban_minutes", "pb_ban_message",
                "pb_command_group", "pb_command_text",
                "procon_chat_group", "procon_chat_text",
                "procon_event_group", "procon_event_type", "procon_event_name", "procon_event_text", "procon_event_player",
                "server_command_group", "server_command_text",
                "taskbar_notify_group", "taskbar_notify_title", "taskbar_notify_message",
                "log_group", "log_destination", "log_file", "log_message",
                "sms_group", "sms_country", "sms_carrier", "sms_number", "sms_message",
                "mail_group", "mail_address", "mail_subject", "mail_body",
                "tweet_group", "tweet_status",
                "sound_notify_group", "sound_notify_file", "sound_notify_repeat",
                "delete",
                "yell_group", "yell_message", "yell_audience", "yell_duration", "yell_procon_chat",
                });


            public DataDictionaryInterface Data { get { return (DataDictionaryInterface)DataDict; } }
            public DataDictionaryInterface RoundData { get { return (DataDictionaryInterface)RoundDataDict; } }
            public DataDictionaryInterface DataRound { get { return (DataDictionaryInterface)RoundDataDict; } }

            public String id
            {
                get { return fields["id"]; }
            }

            public String Name
            {
                get { return fields["name"]; }
            }

            public String FullDisplayName
            {
                get { return "Limit #^b" + id + "^n - " + Name; }
            }

            public String FullName
            {
                get { return "Limit #" + id + " - " + Name; }
            }

            public String FullReplaceName
            {
                get { return "Limit #%l_id% %l_n%"; }
            }

            public String ShortName
            {
                get { return "Limit #" + id; }
            }

            public String ShortDisplayName
            {
                get { return "Limit #^b" + id + "^n"; }
            }


            public ShowHide Hide
            {
                get { return ((ShowHide)Enum.Parse(typeof(ShowHide), fields["hide"])); }
            }



            public bool Enabled
            {
                get { return State.Equals(LimitState.Enabled); }
            }

            public bool Virtual
            {
                get { return State.Equals(LimitState.Virtual); }
            }

            public bool Disabled
            {
                get { return State.Equals(LimitState.Disabled); }
            }

            public EvaluationType Evaluation
            {
                get { return (EvaluationType)Enum.Parse(typeof(EvaluationType), fields["evaluation"]); }
            }

            public LimitState State
            {
                get { return (LimitState)Enum.Parse(typeof(LimitState), fields["state"]); }
            }


            public long Interval
            {
                get { return long.Parse(fields["evaluation_interval"]); }
            }



            public bool FirstCheckEmpty
            {
                get
                {
                    return (FirstCheck.Equals(LimitType.Expression) && FirstCheckExpression.Length == 0) ||
                           (FirstCheck.Equals(LimitType.Code) && FirstCheckCode.Length == 0);
                }
            }

            public bool SecondCheckEmpty
            {
                get
                {
                    return (SecondCheck.Equals(LimitType.Expression) && SecondCheckEpression.Length == 0) ||
                           (SecondCheck.Equals(LimitType.Code) && SecondCheckCode.Length == 0);
                }
            }

            public LimitAction Action
            {
                get
                {
                    return Str2Action(fields["action"]);
                }
            }

            private List<String> CleanupActions(String actions)
            {
                List<String> alist = new List<string>(Regex.Split(actions, @"\s*\|\s*"));
                List<String> clean = new List<string>();
                String clean_action = String.Empty;

                foreach (String action in alist)
                {
                    try
                    {
                        LimitAction caction = Str2Action(action);

                        if (caction.Equals(LimitAction.None))
                            continue;

                        if (clean.Contains(caction.ToString()))
                            continue;

                        clean.Add(caction.ToString());
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                }

                if (clean.Count == 0)
                    clean.Add(LimitAction.None.ToString());

                return clean;
            }

            public List<String> ActionsList
            {
                get
                {
                    return CleanupActions(fields["action"]);
                }
                set
                {
                    fields["action"] = String.Join(" | ", value.ToArray());
                }
            }

            private LimitAction Str2Action(String action)
            {
                return (LimitAction)Enum.Parse(typeof(LimitAction), action.Replace("|", ","), true);
            }

            public MessageAudience SayAudience
            {
                get { return (MessageAudience)Enum.Parse(typeof(MessageAudience), fields["say_audience"]); }
            }

            public bool SayProConChat
            {
                get { return ((TrueFalse)Enum.Parse(typeof(TrueFalse), fields["say_procon_chat"])).Equals(TrueFalse.True); }
            }

            public MessageAudience YellAudience
            {
                get { return (MessageAudience)Enum.Parse(typeof(MessageAudience), fields["yell_audience"]); }
            }

            public bool YellProConChat
            {
                get { return ((TrueFalse)Enum.Parse(typeof(TrueFalse), fields["yell_procon_chat"])).Equals(TrueFalse.True); }
            }

            public LimitType SecondCheck
            {
                get { return (LimitType)Enum.Parse(typeof(LimitType), fields["second_check"]); }
            }

            public LimitType FirstCheck
            {
                get { return (LimitType)Enum.Parse(typeof(LimitType), fields["first_check"]); }
            }

            public EABanDuration EABDuration
            {
                get { return (EABanDuration)Enum.Parse(typeof(EABanDuration), fields["ea_ban_duration"]); }
            }

            public EABanType EABType
            {
                get { return (EABanType)Enum.Parse(typeof(EABanType), fields["ea_ban_type"]); }
            }

            public PBBanDuration PBBDuration
            {
                get { return (PBBanDuration)Enum.Parse(typeof(PBBanDuration), fields["pb_ban_duration"]); }
            }

            public PBBanType PBBType
            {
                get { return (PBBanType)Enum.Parse(typeof(PBBanType), fields["pb_ban_type"]); }
            }

            public String PBCommandText
            {
                get { return fields["pb_command_text"]; }
            }

            public EventType PRoConEventType
            {
                get { return (EventType)Enum.Parse(typeof(EventType), fields["procon_event_type"]); }
            }

            public CapturableEvent PRoConEventName
            {
                get { return (CapturableEvent)Enum.Parse(typeof(CapturableEvent), fields["procon_event_name"]); }
            }

            public String PRoConEventText
            {
                get { return fields["procon_event_text"]; }
            }

            public String PRoConEventPlayer
            {
                get { return fields["procon_event_player"]; }
            }

            public String PRoConChatText
            {
                get { return fields["procon_chat_text"]; }
            }


            public String ServerCommandText
            {
                get { return fields["server_command_text"]; }
            }

            public LimitLogDestination LogDestination
            {
                get { return (LimitLogDestination)Enum.Parse(typeof(LimitLogDestination), fields["log_destination"]); }
            }

            public String LogFile
            {
                get { return fields["log_file"]; }
            }

            public String LogMessage
            {
                get { return fields["log_message"]; }
            }


            public String MailAddress
            {
                get { return fields["mail_address"]; }
            }

            public String MailSubject
            {
                get { return fields["mail_subject"]; }
            }

            public String MailBody
            {
                get { return fields["mail_body"]; }
            }


            public String SMSCountry
            {
                get { return fields["sms_country"]; }
            }

            public String SMSCarrier
            {
                get { return fields["sms_carrier"]; }

                set { fields["sms_carrier"] = value; }
            }

            public String TweetStatus
            {
                get { return fields["tweet_status"]; }
            }

            public String SMSNumber
            {
                get { return fields["sms_number"]; }
            }

            public String SMSMessage
            {
                get { return fields["sms_message"]; }
            }

            public String FirstCheckCode
            {
                get { return fields["first_check_code"].Trim(); }
            }

            public String FirstCheckExpression
            {
                get { return fields["first_check_expression"].Trim(); }
            }

            public String SecondCheckCode
            {
                get { return fields["second_check_code"].Trim(); }
            }

            public String SecondCheckEpression
            {
                get { return fields["second_check_expression"].Trim(); }
            }

            public String SayMessage
            {
                get { return fields["say_message"]; }
            }

            public String YellMessage
            {
                get { return fields["yell_message"]; }
            }

            public String PBBMessage
            {
                get { return fields["pb_ban_message"]; }
            }

            public String EABMessage
            {
                get { return fields["ea_ban_message"]; }
            }

            public String KickMessage
            {
                get { return fields["kick_message"]; }
            }

            public int SayDelay
            {
                get { return int.Parse(fields["say_delay"]); }
            }

            public int YellDuration
            {
                get { return int.Parse(fields["yell_duration"]); }
            }

            public int KillDelay
            {
                get { return int.Parse(fields["kill_delay"]); }
            }

            public int PBBMinutes
            {
                get { return int.Parse(fields["pb_ban_minutes"]); }
            }

            public int EABMinutes
            {
                get { return int.Parse(fields["ea_ban_minutes"]); }
            }



            public bool Valid
            {
                get { return !Invalid; }
            }

            public bool Invalid
            {
                get
                {
                    return Disabled ||
                            evaluator == null ||
                            FirstCheck.Equals(LimitType.Disabled) ||
                            FirstCheckEmpty;
                }
            }

            public String TaskbarNotifyMessage
            {
                get { return fields["taskbar_notify_message"]; }
            }


            public String TaskbarNotifyTitle
            {
                get { return fields["taskbar_notify_title"]; }
            }


            public String SoundNotifyFile
            {
                get { return fields["sound_notify_file"]; }
            }
            public String SoundNotifyRepeat
            {
                get { return fields["sound_notify_repeat"]; }
            }

            public void RecordActivation(String PlayerName)
            {
                if (plugin.serverInfo == null)
                    return;

                ServerInfo server = plugin.serverInfo;

                if (!plugin.players.ContainsKey(PlayerName) || plugin.players[PlayerName] == null)
                    return;

                PlayerInfo player = plugin.players[PlayerName];

                if (!activations.ContainsKey(player.Name))
                    activations.Add(player.Name, new List<LimitEvent>());

                activations[player.Name].Add(new LimitEvent(this, player, server));

            }


            /*
            // Not needed anymore, OnJoin limits are evaluated once only in OnPlayerJoin
            public void RecordEvaluation(PlayerInfoInterface player)
            {
                if (plugin.serverInfo == null)
                    return;

                ServerInfoInterface server = (ServerInfoInterface)plugin.serverInfo;

                if (!evaluations.ContainsKey(player.Name))
                    evaluations.Add(player.Name, new List<LimitEvent>());

                evaluations[player.Name].Add(new LimitEvent(this, player, server));
            }
             */

            public double Activations()
            {
                double total = 0;
                foreach (KeyValuePair<String, List<LimitEvent>> pair in activations)
                    if (pair.Value != null)
                        total += pair.Value.Count;

                return total;
            }

            public double ActivationsTotal()
            {
                double total = Activations();

                foreach (KeyValuePair<String, List<LimitEvent>> pair in activations_total)
                    if (pair.Value != null)
                        total += pair.Value.Count;

                return total;

            }

            public double Activations(String PlayerName)
            {

                if (!activations.ContainsKey(PlayerName))
                    return 0;

                return activations[PlayerName].Count;
            }


            public double ActivationsTotal(String PlayerName)
            {
                double total = Activations(PlayerName);

                if (!activations_total.ContainsKey(PlayerName))
                    return total;

                return total + activations_total[PlayerName].Count;
            }


            public double Activations(int TeamId, int SquadId)
            {
                double total = 0;

                //we have to visit every possible limit activation and count
                foreach (KeyValuePair<String, List<LimitEvent>> pair in activations)
                    if (pair.Value != null)
                        foreach (LimitEvent e in pair.Value)
                            if (e.TeamId == TeamId && e.SquadId == SquadId)
                                total++;

                return total;
            }

            public double ActivationsTotal(int TeamId, int SquadId)
            {
                double total = Activations(TeamId, SquadId);

                //we have to visit every possible limit activation and count
                foreach (KeyValuePair<String, List<LimitEvent>> pair in activations_total)
                    if (pair.Value != null)
                        foreach (LimitEvent e in pair.Value)
                            if (e.TeamId == TeamId && e.SquadId == SquadId)
                                total++;

                return total;
            }


            public double Activations(String PlayerName, TimeSpan time)
            {

                if (!activations.ContainsKey(PlayerName))
                    return 0;

                List<LimitEvent> events = activations[PlayerName];

                double total = 0;
                DateTime back = DateTime.Now.Subtract(time);

                //we have to visit every possible limit activation and count
                foreach (LimitEvent e in events)
                    if (e != null && e.Time.CompareTo(back) >= 0)
                        total++;

                return total;
            }

            public double Activations(int TeamId)
            {
                double total = 0;

                //we have to visit every possible limit activation and count
                foreach (KeyValuePair<String, List<LimitEvent>> pair in activations)
                    if (pair.Value != null)
                        foreach (LimitEvent e in pair.Value)
                            if (e.TeamId == TeamId)
                                total++;

                return total;
            }

            public double ActivationsTotal(int TeamId)
            {
                double total = Activations(TeamId);

                //we have to visit every possible limit activation and count
                foreach (KeyValuePair<String, List<LimitEvent>> pair in activations_total)
                    if (pair.Value != null)
                        foreach (LimitEvent e in pair.Value)
                            if (e.TeamId == TeamId)
                                total++;

                return total;
            }


            public double Spree(String PlayerName)
            {
                if (!sprees.ContainsKey(PlayerName))
                    return 0;

                return sprees[PlayerName];
            }

            public void RecordSpree(String PlayerName)
            {
                if (!sprees.ContainsKey(PlayerName))
                    sprees.Add(PlayerName, 0);

                sprees[PlayerName]++;
            }

            public void ResetSpree(String PlayerName)
            {
                if (!sprees.ContainsKey(PlayerName))
                    return;

                sprees.Remove(PlayerName);
            }

            public void ResetSprees()
            {
                sprees.Clear();
            }

            /*
            // Not needed anymore, OnJoin limits are evaluated once only in OnPlayerJoin
            public double EvaluationsPlayer(PlayerInfoInterface player)
            {
                if (!evaluations.ContainsKey(player.Name))
                    return 0;

                return evaluations[player.Name].Count;
            }
             */



            public String fieldKeyByOffset(String name, int offset)
            {
                if (!valid_fields.Contains(name))
                    return String.Empty;

                int index = valid_fields.IndexOf(name) + offset;
                if (index > 0 && index < valid_fields.Count)
                    return valid_fields[index];

                return String.Empty;
            }


            public bool shouldSkipFieldKey(String name)
            {
                try
                {

                    if (!plugin.Agreement)
                        return true;

                    if (!isValidFieldKey(name))
                        return false;

                    String field_key = extractFieldKey(name);



                    if (Hide.Equals(ShowHide.Hide) && !field_key.Equals("hide"))
                        return true;

                    if (field_key.Equals("procon_event_type"))
                        return true;

                    if (field_key.Equals("procon_event_name"))
                        return true;

                    if (Regex.Match(field_key, @"(id|delete|last)$").Success)
                        return true;

                    if (Regex.Match(field_key, @"^evaluation_interval$").Success &&
                        !(EvaluationType.OnInterval.Equals(Evaluation) ||
                          EvaluationType.OnIntervalPlayers.Equals(Evaluation) ||
                          EvaluationType.OnIntervalServer.Equals(Evaluation)))
                        return true;

                    if (Regex.Match(field_key, @"sms_").Success &&
                        !((Action & LimitAction.SMS) > 0))
                        return true;

                    if (Regex.Match(field_key, @"tweet_").Success &&
                     !((Action & LimitAction.Tweet) > 0))
                        return true;

                    if (Regex.Match(field_key, @"mail_").Success &&
                        !((Action & LimitAction.Mail) > 0))
                        return true;


                    if (Regex.Match(field_key, @"ea_ban_").Success &&
                        !((Action & LimitAction.EABan) > 0))
                        return true;

                    if (Regex.Match(field_key, @"log_").Success &&
                        !((Action & LimitAction.Log) > 0))
                        return true;

                    if ((Regex.Match(field_key, @"log_file").Success &&
                        !((LogDestination & LimitLogDestination.File) > 0)))
                        return true;

                    if (Regex.Match(field_key, @"taskbar_notify_.+").Success &&
                        !((Action & LimitAction.TaskbarNotify) > 0))
                        return true;

                    if (Regex.Match(field_key, @"sound_notify_.+").Success &&
                        !((Action & LimitAction.SoundNotify) > 0))
                        return true;

                    if ((Regex.Match(field_key, @"second_check_.+").Success &&
                         SecondCheck.Equals(LimitType.Disabled)))
                        return true;

                    if ((Regex.Match(field_key, @"first_check_.+").Success &&
                         FirstCheck.Equals(LimitType.Disabled)))
                        return true;

                    if (Regex.Match(field_key, @"pb_ban_").Success &&
                        !((Action & LimitAction.PBBan) > 0))
                        return true;

                    if (Regex.Match(field_key, @"pb_command_").Success &&
                       !((Action & LimitAction.PBCommand) > 0))
                        return true;

                    if (Regex.Match(field_key, @"procon_event_").Success &&
                         !((Action & LimitAction.PRoConEvent) > 0))
                        return true;

                    if (Regex.Match(field_key, @"procon_chat_").Success &&
                        !((Action & LimitAction.PRoConChat) > 0))
                        return true;

                    if (Regex.Match(field_key, @"server_command_").Success &&
                        !((Action & LimitAction.ServerCommand) > 0))
                        return true;




                    if (Regex.Match(field_key, @"say_").Success &&
                        !((Action & LimitAction.Say) > 0))
                        return true;


                    if (Regex.Match(field_key, @"yell_").Success &&
                        !((Action & LimitAction.Yell) > 0))
                        return true;


                    if (Regex.Match(field_key, @"kill_").Success &&
                        !((Action & LimitAction.Kill) > 0))
                        return true;


                    if (Regex.Match(field_key, @"(kick)_").Success &&
                        !((Action & LimitAction.Kick) > 0))
                        return true;

                    if (field_key.Equals("first_check_expression") &&
                        !FirstCheck.Equals(LimitType.Expression))
                        return true;

                    if (field_key.Equals("second_check_expression") &&
                        !SecondCheck.Equals(LimitType.Expression))
                        return true;

                    if (field_key.Equals("first_check_code") &&
                        !FirstCheck.Equals(LimitType.Code))
                        return true;

                    if (field_key.Equals("second_check_code") &&
                        !SecondCheck.Equals(LimitType.Code))
                        return true;

                    if (field_key.Equals("ea_ban_minutes") &&
                        !((Action & LimitAction.EABan) > 0 &&
                          EABDuration.Equals(EABanDuration.Temporary)))
                        return true;

                    if (field_key.Equals("pb_ban_minutes") &&
                       !((Action & LimitAction.PBBan) > 0 &&
                         PBBDuration.Equals(PBBanDuration.Temporary)))
                        return true;


                }
                catch (Exception e)
                {
                    plugin.DumpException(e);
                }
                return false;

            }

            public void SetupGroups()
            {

                if (group2title == null)
                    group2title = new Dictionary<string, string>();

                if (group2regex == null)
                    group2regex = new Dictionary<string, string>();

                if (title2group == null)
                    title2group = new Dictionary<string, string>();


                group2title.Clear();
                group2regex.Clear();
                title2group.Clear();

                if ((valid_groups.Length % 3) > 0)
                {
                    plugin.ConsoleError("sanity check failed for limit field groups");
                    return;
                }


                for (int i = 0; i < valid_groups.Length; i = i + 3)
                {
                    String title = valid_groups[i];
                    String group = valid_groups[i + 1];
                    String regex = valid_groups[i + 2];


                    if (!group2title.ContainsKey(group))
                        group2title.Add(group, title);

                    if (!title2group.ContainsKey(title))
                        title2group.Add(title, group);

                    if (!group2regex.ContainsKey(group))
                        group2regex.Add(group, regex);

                }
            }

            private void SetupFields()
            {
                fields = new Dictionary<string, string>();
                foreach (String field_key in valid_fields)
                    fields.Add(field_key, "");
            }

            private void InitFields(String id)
            {
                String auto_hide = (plugin.getBooleanVarValue("auto_hide_sections") ? ShowHide.Hide : ShowHide.Show).ToString();

                setFieldValue("id", id);
                setFieldValue("hide", auto_hide);
                setFieldValue("name", "Name" + id);
                setFieldValue("state", LimitState.Enabled.ToString());
                setFieldValue("evaluation", EvaluationType.OnJoin.ToString());
                setFieldValue("evaluation_interval", (10).ToString());
                setFieldValue("first_check", LimitType.Disabled.ToString());
                setFieldValue("second_check", LimitType.Disabled.ToString());
                setFieldValue("delete", (false).ToString());
                setFieldValue("new_action", LimitAction.None.ToString());
                setFieldValue("action", LimitAction.None.ToString());

                setFieldValue("kill_group", auto_hide);
                setFieldValue("kill_delay", (0).ToString());

                setFieldValue("say_group", auto_hide);
                setFieldValue("say_message", "activated " + FullReplaceName);
                setFieldValue("say_audience", MessageAudience.All.ToString());
                setFieldValue("say_procon_chat", TrueFalse.False.ToString());
                setFieldValue("say_delay", (0).ToString());

                setFieldValue("ea_ban_group", auto_hide);
                setFieldValue("ea_ban_type", EABanType.EA_GUID.ToString());
                setFieldValue("ea_ban_duration", EABanDuration.Temporary.ToString());
                setFieldValue("ea_ban_minutes", (10).ToString());
                setFieldValue("ea_ban_message", "activated " + FullReplaceName);

                setFieldValue("taskbar_notify_group", auto_hide);
                setFieldValue("taskbar_notify_title", FullReplaceName + " activation");
                setFieldValue("taskbar_notify_message", FullReplaceName + " was activated on %date%, at %time%");

                setFieldValue("sound_notify_group", auto_hide);
                setFieldValue("sound_notify_file", FullReplaceName + " activation");
                setFieldValue("sound_notify_repeat", FullReplaceName + " was activated on %date%, at %time%");

                setFieldValue("pb_ban_group", auto_hide);
                setFieldValue("pb_ban_type", PBBanType.PB_GUID.ToString());
                setFieldValue("pb_ban_duration", PBBanDuration.Temporary.ToString());
                setFieldValue("pb_ban_minutes", (10).ToString());
                setFieldValue("pb_ban_message", "activated " + FullReplaceName);

                setFieldValue("pb_command_group", auto_hide);
                setFieldValue("pb_command_text", "pb_sv_plist");

                setFieldValue("procon_chat_group", auto_hide);
                setFieldValue("procon_chat_text", "activated " + FullReplaceName);

                setFieldValue("procon_event_type", EventType.Plugins.ToString());
                setFieldValue("procon_event_name", CapturableEvent.PluginAction.ToString());
                setFieldValue("procon_event_text", "activated " + FullReplaceName);
                setFieldValue("procon_event_player", "player.Name");

                setFieldValue("server_command_group", auto_hide);
                setFieldValue("server_command_text", "admin.say \"Hello World\" all");


                setFieldValue("kick_group", auto_hide);
                setFieldValue("kick_message", "violated " + FullReplaceName);

                setFieldValue("log_group", auto_hide);
                setFieldValue("log_destination", LimitLogDestination.Plugin.ToString());
                setFieldValue("log_file", InsaneLimits.makeRelativePath("InsaneLimits.log"));
                setFieldValue("log_message", "[%date% %time%] %p_n% activated " + FullReplaceName);

                setFieldValue("sms_group", auto_hide);
                setFieldValue("sms_country", "United_States");
                setFieldValue("sms_carrier", "T-Mobile");
                setFieldValue("sms_number", "5555555555");

                setFieldValue("mail_group", auto_hide);
                setFieldValue("mail_address", "admin1@mail.com, admin2@mail.com, etc");
                setFieldValue("mail_subject", FullReplaceName + " Activation@%server_host%:%server_port% - %p_n%");

                setFieldValue("tweet_group", auto_hide);
                setFieldValue("tweet_status", "%p_n% activated " + FullReplaceName); ;

                String body = FullReplaceName + " Activation Report" + NL +
                              @"Server: %server_host%:%server_port%" + NL +
                              @"Player: %p_n%" + NL +
                              @"EA_GUID: %p_eg%" + NL +
                              @"PB_GUID: %p_pg%" + NL +
                              @"Date: %date% %time%";

                setFieldValue("mail_body", body);
                setFieldValue("sms_message", body);

                setFieldValue("yell_group", auto_hide);
                setFieldValue("yell_message", "activated " + FullReplaceName);
                setFieldValue("yell_audience", MessageAudience.All.ToString());
                setFieldValue("yell_procon_chat", TrueFalse.False.ToString());
                setFieldValue("yell_duration", (5).ToString());
            }

            private void SetupCounts()
            {
                this.activations = new Dictionary<string, List<LimitEvent>>();
                this.activations_total = new Dictionary<string, List<LimitEvent>>();
                /*
                // Not needed anymore, OnJoin limits are evaluated once only in OnPlayerJoin
                this.evaluations = new Dictionary<string, List<LimitEvent>>();
                 */
                this.sprees = new Dictionary<string, double>();
            }

            public Limit(InsaneLimits plugin, String id)
            {
                this.plugin = plugin;

                SetupCounts();
                SetupFields();
                InitFields(id);

                SetupGroups();

                DataDict = new DataDictionary(plugin);
                RoundDataDict = new DataDictionary(plugin);


            }

            public bool isGroupFirstField(String var)
            {
                if (group2regex == null || group2regex.Count == 0)
                    return false;

                return group2regex.ContainsKey(extractFieldKey(var));

            }

            public String getGroupNameByKey(String key)
            {
                if (group2regex == null || group2regex.Count == 0)
                    return String.Empty;

                key = extractFieldKey(key);
                foreach (KeyValuePair<String, String> pair in group2regex)
                    if (Regex.Match(key, pair.Value, RegexOptions.IgnoreCase).Success)
                        return pair.Key;

                return String.Empty;
            }

            public String getGroupBaseTitleByKey(String key)
            {
                String name = getGroupNameByKey(key);

                if (name.Length == 0 || !group2title.ContainsKey(name))
                    return String.Empty;

                return group2title[name];
            }

            public String getGroupFormattedTitleByKey(String key)
            {

                String title = getGroupBaseTitleByKey(key);

                if (title.Length == 0)
                    return String.Empty;

                char pchar = '-';
                //int max = 64;
                int flen = 30;

                if (title.Length > flen)
                    title = title.Substring(0, flen - 2);

                int spaces = flen - title.Length;
                int lspace = spaces / 2;
                int rspace = spaces - lspace;

                title = new String(pchar, lspace) + title + new String(pchar, rspace);

                return "[ " + id + " ]" + (new String(' ', 10)) + "[ " + title + " ]" + (new String(' ', 10));
            }

            public bool isValidGroupTitle(String var)
            {
                if (title2group == null || title2group.Count == 0)
                    return false;

                return title2group.ContainsKey(extractGroupBaseTitle(var));
            }


            public bool isValidFieldKey(String var)
            {

                if (valid_fields.Contains(extractFieldKey(var)))
                    return true;

                return false;
            }

            public static String extractGroupBaseTitle(String var)
            {
                Match match = Regex.Match(var, @"\[\s*-+\s*([^\-]+)\s*-+\s*\]");
                if (match.Success)
                    return match.Groups[1].Value;

                return var;
            }

            public static String extractFieldKey(String var)
            {
                Match match = Regex.Match(var, @"limit_[^_]+_([^0-9]+)");
                if (match.Success)
                    return match.Groups[1].Value;

                return var;
            }

            public void recompile(String field, String val, bool ui)
            {

                if (FirstCheck.Equals(LimitType.Disabled))
                    return;

                if ((ui && (field.Equals("evaluation") ||
                            field.Equals("first_check") ||
                            field.Equals("first_check_expression") ||
                            field.Equals("first_check_code") ||
                            field.Equals("second_check") ||
                            field.Equals("second_check_expression") ||
                            field.Equals("second_check_code"))
                    )
                   )
                {

                    plugin.CompileLimit(this);
                }
            }


            public void AccumulateActivations()
            {
                if (activations == null)
                    return;

                List<String> keys = new List<string>(activations.Keys);
                foreach (String key in keys)
                    AccumulateActivations(key);
            }

            public void AccumulateActivations(String PlayerName)
            {
                if (activations == null || !activations.ContainsKey(PlayerName))
                    return;

                List<LimitEvent> levents = null;
                activations.TryGetValue(PlayerName, out levents);
                if (levents == null || levents.Count == 0)
                    return;

                if (!activations_total.ContainsKey(PlayerName))
                    activations_total.Add(PlayerName, new List<LimitEvent>());

                activations_total[PlayerName].AddRange(levents);
            }

            public void ResetActivations(String PlayerName)
            {
                if (PlayerName == null)
                    return;

                if (!activations.ContainsKey(PlayerName))
                    return;

                activations[PlayerName].Clear();
            }

            public void ResetActivationsTotal(String PlayerName)
            {
                if (PlayerName == null)
                    return;

                if (!activations_total.ContainsKey(PlayerName))
                    return;

                activations_total[PlayerName].Clear();
            }

            public void ResetActivations()
            {
                if (activations != null)
                    activations.Clear();
            }

            public void ResetActivationsTotal()
            {
                if (activations_total != null)
                    activations_total.Clear();
            }

            /*
            // Not needed anymore, OnJoin limits are evaluated once only in OnPlayerJoin
            public void ResetEvaluations()
            {
                if (evaluations != null)
                    evaluations.Clear();
            }
             */

            /*
            // Not needed anymore, OnJoin limits are evaluated once only in OnPlayerJoin
            public void ResetEvaluations(String PlayerName)
            {
                if (evaluations == null)
                    return;

                if (!evaluations.ContainsKey(PlayerName))
                    return;

                evaluations.Remove(PlayerName);
            }
             */

            public void ResetLastInterval(DateTime now)
            {
                LastInterval = now;
            }

            public long RemainingSeconds(DateTime now)
            {

                long elapsed = (long)now.Subtract(LastInterval).TotalSeconds;

                if (elapsed >= Interval)
                {
                    ResetLastInterval(now);
                    return 0;
                }


                long r = Interval - elapsed;
                plugin.DebugWrite(ShortDisplayName + " - " + Evaluation.ToString() + ", " + r + " second" + ((r > 1) ? "s" : "") + " remaining", 7);

                return r;
            }

            public void Reset()
            {
                ResetActivations();
                ResetActivationsTotal();
                ResetSprees();
                ResetLastInterval(DateTime.Now);
                Data.Clear();
            }


            public bool validateAndSetFieldValue(String field, String val, bool ui)
            {   //plugin.ConsoleWrite(field + " = " + val + ", UI: " + ui.ToString());
                if (field.Equals("delete"))
                {
                    /* Parse Boolean Values */
                    bool booleanValue = false;

                    if (Regex.Match(val, @"^\s*(1|true|yes)\s*$", RegexOptions.IgnoreCase).Success)
                        booleanValue = true;
                    else if (Regex.Match(val, @"^\s*(0|false|no)\s*$", RegexOptions.IgnoreCase).Success)
                        booleanValue = false;
                    else
                        return false;

                    fields[field] = booleanValue.ToString();
                }
                else if (field.Equals("state") ||
                         field.Equals("first_check") ||
                         field.Equals("second_check") ||
                         field.Equals("evaluation") ||
                         field.Equals("say_audience") ||
                         field.Equals("say_procon_chat") ||
                         field.Equals("yell_audience") ||
                         field.Equals("yell_procon_chat") ||
                         field.Equals("hide") ||
                         field.Equals("procon_event_type") ||
                         field.Equals("procon_event_name") ||
                         Regex.Match(field, "_group").Success

                    )
                {


                    /* Parse Enum */
                    Type type = null;
                    if (field.Equals("state"))
                        type = typeof(LimitState);
                    else if (Regex.Match(field, @"^(first_check|second_check)$").Success)
                        type = typeof(LimitType);
                    else if (field.Equals("evaluation"))
                        type = typeof(EvaluationType);
                    else if (field.Equals("say_audience"))
                        type = typeof(MessageAudience);
                    else if (field.Equals("say_procon_chat"))
                        type = typeof(TrueFalse);
                    else if (field.Equals("yell_audience"))
                        type = typeof(MessageAudience);
                    else if (field.Equals("yell_procon_chat"))
                        type = typeof(TrueFalse);
                    else if (field.Equals("procon_event_type"))
                        type = typeof(EventType);
                    else if (field.Equals("procon_event_name"))
                        type = typeof(CapturableEvent);
                    else if (Regex.Match(field, "_group").Success || field.Equals("hide"))
                        type = typeof(ShowHide);


                    try
                    {
                        String origValue = fields[field];
                        
                        fields[field] = Enum.Format(type, Enum.Parse(type, val, true), "G").ToString();


                        if (field.Equals("second_check") &&
                            !SecondCheck.Equals(LimitType.Disabled) &&
                            FirstCheck.Equals(LimitType.Disabled))
                        {
                            fields[field] = LimitType.Disabled.ToString();
                            plugin.ConsoleWarn("cannot enable ^bsecond_check^n, without enabling ^bfirst_check^n for " + ShortDisplayName);
                            return false;
                        }

                        if (field.Equals("first_check") &&
                           FirstCheck.Equals(LimitType.Disabled) &&
                           !SecondCheck.Equals(LimitType.Disabled))
                        {
                            setFieldValue("second_check", LimitType.Disabled.ToString());
                            plugin.ConsoleWarn("detected that ^bfirst_check^n was disabled for " + ShortDisplayName + ", will also disable ^bsecond_check^n");
                            return true;
                        }

                        if (field.Equals("yell_audience") && fields[field].Equals("Squad"))
                        {
                            setFieldValue("yell_audience", MessageAudience.All.ToString());
                            plugin.ConsoleWarn("^byell_audience^n cannot be set to Squad, reverting to All");
                        }

                        recompile(field, val, ui);

                        // Warning for BF3 player say
                        // if (field.Equals("say_audience") && fields[field].Equals("Player"))
                        // plugin.ConsoleWarn("Battlefield 3 does not support individual player messages");

                        // Reset the activations when disbaling limits
                        if (field.Equals("state") && !Enabled)
                            Reset();

                        if (origValue != fields[field]) {
                        /*
                        if ((field.Equals("evaluation") && origValue != fields[field])
                        || (fields.Equals("evaluation_interval") && origValue != fields[field])) {
                        */
                            ResetLastInterval(DateTime.Now);
                        }

                        return true;
                    }
                    catch (FormatException)
                    {
                        return false;
                    }
                    catch (ArgumentException)
                    {
                        return false;
                    }

                }
                else if (Regex.Match(field, @"(id|((ea|pb)_ban_minutes)|say_delay|yell_duration|kill_delay|evaluation_interval)").Success)
                {
                    /* Parse Integer Values */
                    int integerValue = 0;
                    if (!int.TryParse(val, out integerValue))
                        return false;


                    if (Regex.Match(field, @"(id|((ea|pb)_ban_minutes))").Success &&
                            !plugin.intAssertGTE(field, integerValue, 1))
                        return false;
                    else if (Regex.Match(field, @"(say_delay|kill_delay|yell_duration)").Success &&
                            !plugin.intAssertGTE(field, integerValue, 0))
                        return false;
                    else if (Regex.Match(field, @"^evaluation_interval$").Success &&
                            !plugin.intAssertGTE(field, integerValue, 10))
                        return false;

                    fields[field] = integerValue.ToString();
                    return true;
                }
                else if ((Regex.Match(field, @"first_check_expression").Success &&
                         FirstCheck.Equals(LimitType.Expression))

                         ||

                        (Regex.Match(field, @"first_check_code").Success &&
                         FirstCheck.Equals(LimitType.Code))

                         ||

                        (Regex.Match(field, @"second_check_code").Success &&
                         SecondCheck.Equals(LimitType.Code))

                         ||

                        (Regex.Match(field, @"second_check_expression").Success &&
                         SecondCheck.Equals(LimitType.Expression))
                       )
                {
                    fields[field] = val;
                    recompile(field, val, ui);
                    return true;
                }
                else if (field.Equals("action") && ui)
                {
                    ActionsList = CleanupActions(val);
                    return false;
                }
                else if (ui && field.Equals("new_action"))
                {
                    try
                    {
                        if (Str2Action(val).Equals(LimitAction.None))
                            ActionsList = new List<string>(new string[] { LimitAction.None.ToString() });
                        else
                            ActionsList = CleanupActions(fields["action"] + "|" + val);

                    }
                    catch (Exception)
                    { }

                    fields[field] = "...";

                    return false;
                }
                else
                    fields[field] = val;

                return true;
            }


            public bool setFieldValue(String var, String val)
            {
                return setFieldValue(var, val, false);
            }

            public bool getGroupStateByTitle(String title)
            {

                title = extractGroupBaseTitle(title);

                if (title2group == null || title2group.Count == 0 || !isValidGroupTitle(title))
                    return false;

                if (!title2group.ContainsKey(title))
                    return false;


                String state = getField(title2group[title]);
                try
                {
                    return Enum.Parse(typeof(ShowHide), state).Equals(ShowHide.Show);
                }
                catch (Exception)
                { }

                return true;
            }

            public bool getGroupStateByKey(String key)
            {


                String title = getGroupBaseTitleByKey(extractFieldKey(key));
                if (title.Length == 0)
                    return true;

                return getGroupStateByTitle(title);
            }


            public bool setGroupStateByTitle(String title, String val, bool ui)
            {

                title = extractGroupBaseTitle(title);

                if (!isValidGroupTitle(title))
                    return false;

                if (!title2group.ContainsKey(title))
                    return false;

                return setFieldValue(title2group[title], val, ui);

            }

            public bool setFieldValue(String var, String val, bool ui)
            {
                //plugin.ConsoleWrite("Setting: " +var +" = " + val);
                String field_key = extractFieldKey(var);
                if (!isValidFieldKey(field_key))
                    return false;

                return validateAndSetFieldValue(field_key, val, ui);

            }

            public String getField(String var)
            {
                if (!isValidFieldKey(var))
                    return "";

                String field_key = extractFieldKey(var);
                return fields[field_key];
            }



            public static bool isLimitVar(String var)
            {

                if (Regex.Match(var, @"^limit_[^_]+_(" + String.Join("|", valid_fields.ToArray()) + ")").Success)
                    return true;

                if (Regex.Match(var, @"^\s*\[\s*[^ \]]+\s*\]", RegexOptions.IgnoreCase).Success)
                    return true;

                return false;
            }

            public static String extractId(String var)
            {
                Match vmatch = Regex.Match(var, @"^limit_([^_]+)");
                if (vmatch.Success)
                    return vmatch.Groups[1].Value;


                Match hmatch = Regex.Match(var, @"^\s*\[\s*([^ \]]+)\s*\]", RegexOptions.IgnoreCase);
                if (hmatch.Success)
                    return hmatch.Groups[1].Value;

                return "UnknownId";
            }



            public Dictionary<String, String> getSettings(bool display)
            {

                Dictionary<String, String> settings = new Dictionary<string, string>();

                /* optimization */
                if (display && Hide.Equals(ShowHide.Hide))
                {
                    settings.Add("limit_" + id + "_hide", Hide.ToString());
                    return settings;
                }

                List<String> keys = new List<string>(fields.Keys);
                for (int i = 0; i < keys.Count; i++)
                {
                    String key = keys[i];
                    if (!fields.ContainsKey(key))
                        continue;

                    String value = fields[key];

                    settings.Add("limit_" + id + "_" + key, value);
                }


                return settings;
            }
        }

        public List<int> getSortedListsIds()
        {
            Dictionary<int, CustomList> lookup_table = new Dictionary<int, CustomList>();
            foreach (String listId in lists.Keys)
                lookup_table.Add(int.Parse(listId), lists[listId]);

            // sort the keys
            List<int> ids = new List<int>(lookup_table.Keys);

            // sort in ascending order
            ids.Sort(delegate(int a, int b) { return a.CompareTo(b); });
            return ids;
        }


        public List<int> getSortedLimitIds()
        {
            Dictionary<int, Limit> lookup_table = new Dictionary<int, Limit>();
            foreach (String limitId in limits.Keys)
                lookup_table.Add(int.Parse(limitId), limits[limitId]);

            // sort the keys
            List<int> ids = new List<int>(lookup_table.Keys);

            // sort in ascending order
            ids.Sort(delegate(int a, int b) { return a.CompareTo(b); });
            return ids;
        }

        public String getMaxListId()
        {
            int max = 1;
            foreach (KeyValuePair<String, CustomList> pair in lists)
                if (int.Parse(pair.Key) > max)
                    max = int.Parse(pair.Key);

            return max.ToString();
        }

        public String getMaxLimitId()
        {
            int max = 1;
            foreach (KeyValuePair<String, Limit> pair in limits)
                if (int.Parse(pair.Key) > max)
                    max = int.Parse(pair.Key);

            return max.ToString();
        }

        public String getNextListId()
        {
            if (lists.Count == 0)
                return (1).ToString();

            List<int> ids = getSortedListsIds();

            // no need to loop, if all slots are filled
            if (ids.Count == ids[ids.Count - 1])
                return (ids.Count + 1).ToString();

            // find the first free slot in the list
            int i = 1;
            for (; i <= ids.Count; i++)
            {
                if (ids[i - 1] != i)
                    break;
            }
            return i.ToString();
        }

        public String getNextLimitId()
        {
            if (limits.Count == 0)
                return (1).ToString();

            List<int> ids = getSortedLimitIds();

            // no need to loop, if all slots are filled
            if (ids.Count == ids[ids.Count - 1])
                return (ids.Count + 1).ToString();

            // find the first free slot in the list
            int i = 1;
            for (; i <= ids.Count; i++)
            {
                if (ids[i - 1] != i)
                    break;
            }
            return i.ToString();
        }


        public void createNewLimit()
        {

            String id = getNextLimitId();

            Limit limit = new Limit(this, id);

            lock (limits_mutex)
            {
                limits.Add(limit.id, limit);
            }

            ConsoleWrite("New " + limit.ShortName + " created");
            SaveSettings(true);
        }

        public void createNewList()
        {

            String id = getNextListId();

            CustomList list = new CustomList(this, id);

            lock (lists_mutex)
            {
                lists.Add(list.id, list);
            }

            ConsoleWrite("New " + list.ShortName + " created");
            SaveSettings(true);
        }


        private CompilerParameters GenerateCompilerParameters()
        {

            CompilerParameters parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Data.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            //parameters.ReferencedAssemblies.Add("System.Linq.dll");
            if (game_version == "BF3")
                parameters.ReferencedAssemblies.Add("Plugins/BF3/InsaneLimits.dll");
            else if (game_version == "BFHL")
                parameters.ReferencedAssemblies.Add("Plugins/BFHL/InsaneLimits.dll");
            else
                parameters.ReferencedAssemblies.Add("Plugins/BF4/InsaneLimits.dll");

            parameters.GenerateInMemory = true;
            parameters.IncludeDebugInformation = false;

            String procon_path = Directory.GetParent(Application.ExecutablePath).FullName;
            String plugins_path = Path.Combine(procon_path, Path.Combine("Plugins", "BF3"));

            parameters.TempFiles = new TempFileCollection(plugins_path);
            //parameters.TempFiles.KeepFiles = false;


            return parameters;
        }

        public string GetPluginName()
        {
            return "Insane Limits";
        }

        public string GetPluginVersion()
        {
            return "0.9.18.2";
        }

        public string GetPluginAuthor()
        {
            return "micovery";
        }

        public string GetPluginWebsite()
        {
            return "www.insanegamersasylum.com";
        }


        public string GetPluginDescription()
        {
            return @"... and other contributors.
        <h2>Description</h2>
        This plugin is a customizable limits/rules enforcer. It allows you to setup and enforce limits based on player statistics, and server state. <br />
        <br />
        It tracks extensive Battlelog stats and round stats. Several limits are available in the <a href='http://www.phogue.net/forumvb/forumdisplay.php?36-Plugin-Enhancements'>Procon Plugin Enhancements forum</a>.<br />
        <br />
        Version 0.9.4.0 and later is optionally integrated with a MySQL server (using the <b>Battlelog Cache</b> plugin). This enables caching of Battlelog stats fetching, which
        over time should reduce the delay caused by restarting Procon/enabling Insane Limits when your server is full. This should also reduce load on Battlelog, which in turn will reduce the number of Web errors and exceptions. This version is compatible with TrueBalancer and other plugins that use stats fetching.<br />
        <br />
        In addition to keeping track of player statistics, the plugin also keeps tracks of the number of times a player has activated a certain limit/rule.
        I got this idea from the ProCon Rulz plugin. With this meta-information about limits, you are able to create much more powerful rules such as Spree messages.
        If it's not clear now, it's ok, look at end of the documentation for examples that make use if this information.<br />
        <br />
        By default, the plugin ships with <b>virtual_mode</b> set to <i>True</i>. This allows you to test your limits/rules without any risk of accidentally kicking or banning anyone. Once you feel your limits/rules are ready, you can disable <b>virtual_mode</b>.<br />
        <h2> Minimum Requirements</h2>
        <br />
        This plugin requires you to have sufficient privileges for running the following commands:<br />
        <br />
        <blockquote>
          serverInfo<br />
          mapList.list<br />
          mapList.getMapIndices<br />
          admin.listPlayers all<br />
          punkBuster.pb_sv_command pb_sv_plist<br />
          punkBuster.pb_sv_command pb_sv_ban<br />
          punkBuster.pb_sv_command pb_sv_kick<br />
        </blockquote>
        <br />
        Additionaly, you need to have Read+Write file system permission in the following directories: <br />
        <br />
        <blockquote>
          &lt;ProCon&gt;/<br />
          &lt;ProCon&gt;/Plugins/BF3<br />
          &lt;ProCon&gt;/Plugins/BF4<br />
        </blockquote>
        <br />
        <h2>Supported Limit Evaluations</h2>
        <ul>
            <li><b>OnJoin</b> - Limit evaluated when player joins </li>
            <li><b>OnLeave</b> - Limit evaluated when player leaves </li>
            <li><b>OnSpawn</b> - Limit evaluated when player spawns</li>
            <li><b>OnKill</b> - Limit evaluated when makes a kill (team-kills not counted)</li>
            <li><b>OnTeamKill</b> - Limit evaluated when player makes a team-kill</li>
            <li><b>OnDeath</b> - Limit evaluated when player dies (suices not counted)</li>
            <li><b>OnTeamDeath</b> - Limit evaluated when player is team-killed</li>
            <li><b>OnSuicide</b> - Limit evaluated when player commits suicide</li>
            <li><b>OnAnyChat</b> - Limit evaluated when players sends a chat message</li>
            <li><b>OnInterval</b> - (deprecated) Same behavior as <b>OnIntervalPlayers</b></li>
            <li><b>OnIntervalPlayers</b> - Limit evaluated (for all players) every <b>evaluation_interval</b> number of seconds (minimum 10) </li>
            <li><b>OnIntervalServer</b> - Limit evaluated once every <b>evaluation_interval</b> number of seconds (minimum 10)</li>
            <li><b>OnRoundOver</b> - Limit evaluated when round over event is sent</li>
            <li><b>OnRoundStart</b> - Limit evaluated after round over event, when first player spawns</li>
            <li><b>OnTeamChange</b> - Limit evaluated after after player switches teams</li>
        </ul>

        Note that limit evaluation is only performed after the plugin has fetched the player stats from Battlelog.
        If a player joins the server, and starts team-killing, there will be a couple of seconds before the plugin catches on. Having said that, this is rare behavior.
        Most of the time, by the time the player spawns for the first time, the plugin would have already fetched the stats.<br />
        <br />
        When you enable the plugin for the first time in a full server, it will take a couple of minutes to fetch all player stats<br />
        <br />
        <br />
        <h2>Architecture</h2>
        When the plugin is enabled, it starts two threads:

            <ol>
              <li>
                The <b>fetch</b> thread is in charge of monitoring the players that join the server. It fetches player statistics from battlelog.battlefield.com<br />
              </li>
              <br />
              <li>
               The <b>enforcer</b> thread is in charge of evaluating Interval limits. When the <b>enforcer</b> thread finds that a player violates a limit, it performs an action (Kick, Ban, etc) against that player.<br />
              </li>
            </ol>
            <br />
            The two threads have different responsibilities, but they synchronize their work.<br />
        <br />
        <h2>Fetch-thread Flow</h2>
        <blockquote>
            When players join the server, they are added the stats queue. The fetch thread is constantly monitoring this queue. If there is a player in the queue,
            it removes him from the queue, and fetches the battlelog stats for the player.<br />
            <br />
            The stats queue can grow or shrink depending on how fast players join, and how long the web-requests take. If you enable the plugin on a full server, you will
            see that almost immediately all players are queued for stats fetching. Once the stats are fetched for all players in the queue, they are added to the internal player's list.<br />
            <br />
        </blockquote>
       <br />
       <h2>Enforcer-thread Flow</h2>
        <blockquote>
            The enforcer thread runs on a timer (every second). It checks if there are any interval limits ready to be executed. If there are, it will evaluate those limits.
            <br />
            Each time around that the <b>enforcer</b> checks for the available limits is called an <i>iteration</i>.
            If there are no players in the server, or there are no limits available, the <b>enforcer</b> skips the current <i>iteration</i> and sleeps until the next <i>iteration</i>.<br />
            <br />
            The enforcer is only responsible for Limits that evaluate OnIterval, events. Enforcing for other types of events like OnKill, and OnSpawn, is done in the main thread when procon sends the event information. <br />
        </blockquote>
        <br />
        <h2>Limit Management</h2>
        <blockquote>
            <u>Creation</u> - In order to create a new limit, you have to set <b>new_limit</b> variable to <i>True</i>.<br />
            <br />
            This creates a new limit section with default values that you can change.<br />
            <br />
            <br />
            <u>Deletion</u> - In order to delete a limit, you have to set the variable <b>delete_limit</b> to the numerical <i>id</i> of the limit you want to delete.<br />
            <br />
            Each limit has an <i>id</i> number, you can see the <i>id</i> number in the limit name, e.g. Limit #<b>5</b>.<br />
            <br />
        </blockquote>
        <br />
        <h2>Limit Definition</h2>
        At its basic, there are four fields that determine the structure of a limit. These fields are <b>state</b>, <b>action</b>, and <b>first_check</b>, and <b>second_check</b>.<br />
        <br />
        <ol>
          <li><blockquote><b>state</b><br />
                <i>Enabled</i> - the limit will be used, and actions will be performed live<br />
                <i>Virtual</i> - the limit will be used, but actions will be done in <b>virtual_mode</b><br />
                <i>Disabled</i> - the limit will be ignored<br />
                <br />
                This field is useful if you want to temporarily disable a limit from being used, but still want to preserve its definition.
                <br />
              </blockquote>
          </li>
          <li><blockquote><b>action</b><br />
                <i>(string, psv)</i> - list of actions for this limit (Pipe separated ""|"")<br />
                <br />
                e.g.    Say | PBBan | Mail <br />
                <br />
                These are all the allowed actions:<br />
                <ul>
                <li><i>None</i> - no action is performed against the player<br /></li>
                <li><i>Kick</i> - player is kicked, if the limit evaluates to <i>True</i><br /></li>
                <li><i>EABan</i> - player is banned (using the BF3 ban-list), if the limit evaluates <i>True</i><br /></li>
                <li><i>PBBan</i> - player is banned (using PunkBuster ban-list), if the limit evaluates <i>True</i><br /></li>
                <li><i>Kill</i> - kills the player (delay optional), if the limit evaluates <i>True</i><br /></li>
                <li><i>Say</i> - sends a message the server (All, Team, Squad, or Player), if the limit evaluates <i>True</i><br /></li>
                <li><i>Log</i> - logs a message to a File, Plugin log, or both, if the limit evaluates <i>True</i><br /></li>
                <li><i>Mail</i> - sends an e-mail to specified address, if the limit evaluates <i>True</i><br /></li>
                <li><i>SMS</i> - sends an SMS message to the specified phone number, if the limit evaluates <i>True</i><br /></li>
                <li><i>Tweet</i> - posts a Twitter status update (default account is @InsaneLimits), if the limit evaluates <i>True</i><br /></li>
                <li><i>PBCommand</i> - executes the specified PunkBuster command, if the limit evaluates <i>True</i><br /></li>
                <li><i>ServerCommand</i> - executes the specified Server command, if the limit evaluates <i>True</i><br /></li>
                <li><i>PRoConChat</i> - sends the specified text to PRoCon's Chat-Tab, if the limit evaluates <i>True</i><br /></li>
                <li><i>PRoConEvent</i> - adds the specified event to PRoCon's Events-Tab, if the limit evaluates <i>True</i><br /></li>
                <li><i>TaskbarNotify</i> - sends a Windows Taskbar notification, if the limit evaluates <i>True</i><br /></li>
                <li><i>SoundNotify</i> - plays a sound notification with the specified sound file, if the limit evaluates <i>True</i><br /></li>
                <li><i>Yell</i> - yells a message to the server (All, Team, or Player), if the limit evaluates <i>True</i><br /></li>
                </ul>
                <br />
                <br />
                Depending on the selected action, other fields are shown to specify more information about the action.<br />
                <br />
             </blockquote>
             <br />
             Supported PB ban-duration: <i>Permanent</i>, <i>Temporary</i><br />
             Supported PB ban-type: <i>PB_GUID</i> (default)<br />
             <br />
             Supported EA ban-duration: <i>Permanent</i>, <i>Temporary</i>, <i>Round</i><br />
             Supported EA ban-type: <i>EA_GUID</i>, <i>IPAddress</i>, <i>Name</i><br />
             <br />
             <br />
             Also note that each of these actions have a <b>target</b> player. You have to be careful on what <b>target</b> is for each action.<br />
             <br />
             For example, during a Kill event, the target of the action is the Killer.<br />
             But, during a Death event, the target of the action is the player that was killed <br />
             You don't want to accidentaly Kick/Ban the wrong player!
          </li>
          <li><blockquote><b>first_check</b><br />
                <i>Disabled</i> - the limit does not evaluate anything in the first step of evaluation<br />
                <i>Expression</i> - the limit uses a C# conditional expression during the first step of evaluation<br />
                <i>Code</i> - the limit uses a C# code snippet (must return true/false) during the first step of evaluation<br />
                <br />
              </blockquote>
              <blockquote><b>second_check</b><br />
                <i>Disabled</i> - the limit does not evaluate anything in the second step of evaluation<br />
                <i>Expression</i> - the limit uses a C# conditional expression during the second step of evaluation<br />
                <i>Code</i> - the limit uses a C# code snippet (must return true/false) during the second step of evaluation<br />
                <br />
              </blockquote>
              <br />
              Depending on the selected check type, an extra field will be shown for specifying the <i>Expression</i>, or <i>Code</i> text.<br />
              <br />
              Both <i>Expressions</i>, and <i>Code</i> snippets must be syntactically correct in accordance to the C# language.
              The plugin compiles your <i>Expression</i><i>/</i><i>Code</i> in-memory with the Microsoft C# Compiler.
              If there are compilation errors, those are shown in the plugin log.<br />
              <br />
              If you do not know what C# is, or what an expression is, or what a code snippet is ... do not worry.
              Study the examples in the <a href=""http://www.phogue.net/forumvb/showthread.php?3448-Insane-Limits-Examples&highlight=Insane+Limits"">Examples Index</a> forum thread. Then, if you are still unclear, how to write an expression or a code snippet, ask for help in forums at <a href=""http://phogue.net"">phogue.net</a>
              <br />
          </li>
         </ol>
        <br />
        <h2>Limit Evaluation</h2>
        After compilation, limit evaluation is by far the most important of all steps this plugin goes through.<br />
        <br />
        Limit evaluation is comprised of three steps:<br />
        <br />

        <ol>
        <li><b>first_check</b> Evaluation<br />
        <br />
        During this step, the plugin executes the <i>Expression</i><i>/</i></i>Code</i> in <b>first_check</b> to get a  <i>True</i> or <i>False</i> result.<br />
        <br />
        If the result is <i>False</i>, the plugin does not perform any <b>action</b>, and quits. But, if it's <i>True</i>, it keeps going to the next step <br />
        <br />
        </li>
        <li><b>second_check</b> Evaluation (optional)<br />
        <br />
        Next, the plugin runs the <i>Expression</i><i>/</i></i>Code</i> for the <b>second_check</b>, if it's enabled. If it's not enabled, it keeps going to next step.</br >
        <br />
        </li>
        <li><b>action</b> Execution <br />
        <br />
        If the final result of the limit evaluation is <i>True</i>, the plugin then executes the <b>action</b> associated with the limit.<br />
        <br />
        If the final result of the limit evaluation is <i>False</i>, no <b>action</b> is executed.
        <br />
        </li>
        </ol>

        <h2>Objects</h2>
        When writing a limit <i>Expression</i> or <i>Code</i> snippet, there are several globally defined objects that can be used.
        These are <b>server</b>, <b>player</b>, <b>killer</b>, <b>victim</b>, <b>kill</b>, <b> plugin</b>, <b>team1</b>, <b>team2</b>, <b>team3</b>, <b>team3</b>, and <b>limit</b>. These objects contain values, and functions that can be accessed from within the <i>Expressions</i>, or <i>Code</i> snippets.<br />
        <br />

       <h2>Limit Object</h2>
       The <b>limit</b> object represents the state the limit that was just activated. This object is only available during the <b>second_check</b>. The <b>limit</b> object implements the following interface:<br />
       <br />
<pre>
public interface LimitInfoInterface
{

    //Number of times the limit has been activated, (Current round)
    double Activations(String PlayerName);
    double Activations(int TeamId, int SquadId);
    double Activations(int TeamId);

    // Number of times player has activated this limit (Current round) in the given TimeSpan, e.g. last 10 seconds, etc
    double Activations(String PlayerName, TimeSpan time);
    double Activations();


    //Number of times the limit has been activated (All rounds)
    double ActivationsTotal(String PlayerName);
    double ActivationsTotal(int TeamId, int SquadId);
    double ActivationsTotal(int TeamId);
    double ActivationsTotal();

    // Number of times this limit has been activated by player
    /*
     * Kill, TeamKill: Spree value is reset when player dies
     * Death, TeamDeath, and Suicide: Spree value is reset whe player makes a kill
     *
     * Spawn, Join, Interval: Spree value is never reset, you may reset it manually.
     */

    double Spree(String PlayerName);


    // manually resets the Spree value for the player, (only for power-users)
    void ResetSpree(String PlayerName);

    /* Data Repository set/get custom data */

    DataDictionaryInterface Data { get; }        //this dictionary is user-managed
    DataDictionaryInterface RoundData { get; }   //this dictionary is automatically cleared OnRoundStart

    /* Other methods */
    String LogFile { get; }

}
</pre>
       <h2>Team Object (team1, team2, team3, team4)</h2>
       The <b>teamX</b> object represents the state of the team with id X at the moment that the limit is being evaluated. The <b>teamX</b> object implements the following interface:<br />
       <br />
    <pre>
public interface TeamInfoInterface
{
    List&lt;PlayerInfoInterface&gt; players { get; }

    double KillsRound { get; }
    double DeathsRound { get; }
    double SuicidesRound { get; }
    double TeamKillsRound { get; }
    double TeamDeathsRound { get; }
    double HeadshotsRound { get; }
    double ScoreRound { get; }

    int TeamId { get; }
    double Tickets { get; }
    double RemainTickets { get; }
    double RemainTicketsPercent { get; }
    double StartTickets { get; }

    // BF4
    int Faction { get; } // US = 0, RU = 1, CN = 2
}
    </pre>

       <h2>Server Object</h2>
       The <b>server</b> object represents the state of the server at the moment that the limit is being evaluated. The <b>server</b> object implements the following interface:<br />
       <br />
    <pre>
public interface ServerInfoInterface
{
    /* Server State */
    int CurrentRound { get; }
    int TotalRounds { get; }
    int PlayerCount { get; }
    int MaxPlayers { get; }

    /* Current Map Data */
    int MapIndex { get; }
    String MapFileName { get; }
    String Gamemode { get; }
    double GameModeCounter { get; }
    double CTFRoundTimeModifier { get; }

    /* Next Map Data */
    int NextMapIndex { get; }
    String NextMapFileName { get; }
    String NextGamemode { get; }

    /* Map Rotation */
    List&lt;String&gt; MapFileNameRotation { get; }
    List&lt;String&gt; GamemodeRotation { get; }
    List&lt;int&gt; LevelRoundsRotation { get; }

    /* All players, Current Round, Stats */
    double KillsRound { get; }
    double DeathsRound { get; }   // kind of useless, should be same as KillsTotal (suices not counted as death)
    double HeadshotsRound { get; }
    double SuicidesRound { get; }
    double TeamKillsRound { get; }


    /* All players, All rounds, Stats */

    double KillsTotal { get; }
    double DeathsTotal { get; }  // kind of useless, should be same s KillsTotal (suicides not counted as death)
    double HeadshotsTotal { get; }
    double SuicidesTotal { get; }
    double TeamKillsTotal { get; }


    /* Weapon Stats, Current Round, All Rounds (Total)*/
    WeaponStatsInterface this[String WeaponName] { get; }

    /* Other data */
    double TimeRound { get; }                // Time since round started
    double TimeTotal { get; }                // Time since plugin enabled
    double TimeUp { get; }                   // Time since last server restart
    double RoundsTotal { get; }              //Round played since plugin enabled

    /* Meta Data */
    String Port { get; }                     // Layer/Server port number
    String Host { get; }                     // Layer/Server Host
    String Name { get; }
    String Description { get; }
    String GameVersion { get; } // BF3 or BF4

    /* var.* value that is updated every update_interval seconds */
    int BulletDamage { get; }
    bool FriendlyFire { get; }
    int GunMasterWeaponsPreset { get; }
    double IdleTimeout { get; } // seconds
    int SoldierHealth { get; }
    bool VehicleSpawnAllowed { get; }
    int VehicleSpawnDelay { get; }
    // BF4
    bool Commander { get; }
    int MaxSpectators { get; }
    String ServerType { get; }
    int GetFaction(int TeamId); // US = 0, RU = 1, CN = 2

    /* Team data */
    double Tickets(int TeamId);              //tickets for the specified team
    double RemainTickets(int TeamId);        //tickets remaining on specified team
    double RemainTicketsPercent(int TeamId); //tickets remaining on specified team (as percent)

    double StartTickets(int TeamId);         //tickets at the begining of round for specified team
    double TargetTickets { get; }            //tickets needed to win

    int OppositeTeamId(int TeamId);          //id of the opposite team, 1->2, 2->1, 3->4, 4->3, *->0
    int WinTeamId { get; }                   //id of the team that won previous round

    /* Data Repository set/get custom data */

    DataDictionaryInterface Data { get; }        //this dictionary is user-managed
    DataDictionaryInterface RoundData { get; }   //this dictionary is automatically cleared OnRoundStart

}
    </pre>
       <h2>Kill Object</h2>
       The <b>kill</b> object represents information about the kill event. The <b>kill</b> object implements the following interface:<br />
       <br />
<pre>
public interface KillInfoInterface
{
    String Weapon { get; }
    bool Headshot { get; }
    DateTime Time { get; }
    String Category { get; } // BF3.defs or BF4.defs weapon category, such as SniperRifle or VehicleAir
}
</pre>
       <br />
       The <b>KillReasonInterface</b> object represents the friendly weapon name, broken down into separate fields:<br />
       <br />
<pre>
public interface KillReasonInterface
{
    String Name { get; } // weapon name or reason, like Suicide
    String Detail { get; } // BF4: ammo or attachment
    String AttachedTo { get; } // BF4: main weapon when Name is a secondary attachment, like M320
    String VehicleName { get; } // BF4: if Name is Death, this is the vehicle's name
    String VehicleDetail { get; } // BF4: if Name is Death, this is the vehicle's detail (stuff after final slash)
}
</pre>
       <h2>Player, Killer, Victim Objects</h2>
       The <b>player</b> object represents the state of player for which the current limit is being evaluated. The <b>player</b> object implements the following interface:<br />
       <br />
<pre>
public interface PlayerInfoInterface
{
    /* Online statistics (battlelog.battlefield.com) */
    double Rank { get; }
    double Kdr { get; }
    double Time { get; }
    double Kills { get; }
    double Wins { get; }
    double Skill { get; }
    double Spm { get; }
    double Score { get; }
    double Deaths { get; }
    double Losses { get; }
    double Repairs { get; }
    double Revives { get; }
    double Accuracy { get; }
    double Ressuplies { get; }
    double QuitPercent { get; }
    double ScoreTeam { get; }
    double ScoreCombat{ get; }
    double ScoreVehicle{ get; }
    double ScoreObjective { get; }
    double VehiclesKilled { get; }
    double KillStreakBonus { get; }
    double Kpm { get; }

    double KillAssists { get; }
    double ResetDeaths { get; }
    double ResetKills { get; }
    double ResetLosses { get; }
    double ResetWins { get; }
    double ResetScore { get; }
    double ResetShotsFired { get; }
    double ResetShotsHit { get; }
    double ResetTime { get; }

    double ReconTime { get; }
    double EngineerTime { get; }
    double AssaultTime { get; }
    double SupportTime { get; }
    double VehicleTime { get; }
    double ReconPercent { get; }
    double EngineerPercent { get; }
    double AssaultPercent { get; }
    double SupportPercent { get; }
    double VehiclePercent { get; }

    /* Player data */
    String Name { get; }
    String FullName { get; } // name including clan-tag
    String Tag { get; }
    String IPAddress { get; }
    String CountryCode { get ; }
    String CountryName { get; }
    String PBGuid { get; }
    String EAGuid { get; }
    int TeamId { get; }
    int SquadId { get; }
    int Ping { get; }
    int MaxPing { get; }
    int MinPing { get; }
    int MedianPing { get; } // of the last five samples
    int AveragePing { get; } // of two to five samples
    int Role { get; } // BF4: 0 = PLAYER, 1 = SPECTATOR, 2 = COMMANDER, 3 = MOBILE COMMANDER


    /* Current round, Player Stats */
    double KdrRound { get; }
    double KpmRound { get; }
    double SpmRound { get; }
    double ScoreRound { get; }
    double KillsRound { get; }
    double DeathsRound { get; }
    double HeadshotsRound { get; }
    double TeamKillsRound { get; }
    double TeamDeathsRound { get; }
    double SuicidesRound { get; }
    double TimeRound { get; }

    /* All rounds, Player Stats */
    double KdrTotal { get; }
    double KpmTotal { get; }
    double SpmTotal { get; }
    double ScoreTotal { get; }
    double KillsTotal { get; }
    double DeathsTotal { get; }
    double HeadshotsTotal { get; }
    double TeamKillsTotal { get; }
    double TeamDeathsTotal { get; }
    double SuicidesTotal { get; }
    double TimeTotal { get; }
    double RoundsTotal { get; }

    /* Weapon Stats, Current Round, All Rounds (Total) */
    WeaponStatsInterface this[String WeaponName] { get; }

    /* Battlelog Weapon Stats function: use kill.Weapon for WeaponName */
    BattlelogWeaponStatsInterface GetBattlelog(String WeaponName);

    /* Other Data */
    DateTime JoinTime { get; }
    String LastChat { get; }   // text of the last chat sent by player
    bool Battlelog404 { get; } // True - Player has PC Battlelog profile
    bool StatsError { get; }   // True - Error occurred while processing player stats


    /* Whitelist information */
    bool inClanWhitelist { get; }
    bool inPlayerWhitelist { get; }
    bool isInWhitelist { get; }


    /* Data Repository set/get custom data */

    DataDictionaryInterface Data { get; }        //this dictionary is user-managed
    DataDictionaryInterface RoundData { get; }   //this dictionary is automatically cleared OnRoundStart
}
</pre>

       <h2>Plugin Object</h2>
       The <b>plugin</b> represents this plugin itself. It gives you access to important functions for executing server commands, and interacting with ProCon.
       The <b>plugin</b> object implements the following interface:<br />
       <br />
<pre>
public interface PluginInterface
{

    /*
     * Methods for sending messages
     */
    bool SendGlobalMessage(String message);
    bool SendTeamMessage(int teamId, String message);
    bool SendSquadMessage(int teamId, int squadId, String message);
    bool SendPlayerMessage(String name, String message);

    bool SendGlobalMessage(String message, int delay);
    bool SendTeamMessage(int teamId, String message, int delay);
    bool SendSquadMessage(int teamId, int squadId, String message, int delay);
    bool SendPlayerMessage(String name, String message, int delay);

    bool SendGlobalYell(String message, int duration);
    bool SendTeamYell(int teamId, String message, int duration);
    bool SendPlayerYell(String name, String message, int duration);

    bool SendMail(String address, String subject, String body);
    bool SendSMS(String country, String carrier, String number, String message);

    /*
     * Methods used for writing to the Plugin console
     */
    void ConsoleWrite(String text);
    void ConsoleWarn(String text);
    void ConsoleError(String text);
    void ConsoleException(String text);

    /*
     * Methods for getting whitelist information
     *
     */
    bool isInWhitelist(String PlayerName);
    bool isInPlayerWhitelist(String PlayerName);
    bool isInClanWhitelist(String PlayerName);
    bool isInWhiteList(String PlayerName, String list_name);

    /* Method for checking generic lists */
    bool isInList(String item, String list_name);
    
    List&lt;String&gt; GetReservedSlotsList();

    /*
     * Methods getting and setting the Plugin's variables
     */
    bool setPluginVarValue(String variable, String value);
    String getPluginVarValue(String variable);


    /*
     *  Method: R
     *
     *  Replaces tags like %p_n% (Player Name), %k_n% (Killer Name), %v_n% (Victim Name), etc
     */
    String R(String message);


    /*
     * Methods for actions
     */

    bool KickPlayerWithMessage(String name, String message);
    bool KillPlayer(String name);  /* deprecated, do not use */
    bool KillPlayer(String name, int delay);
    bool EABanPlayerWithMessage(EABanType type, EABanDuration duration, String name, int minutes, String message);
    bool PBBanPlayerWithMessage(PBBanDuration duration, String name, int minutes, String message);
    bool PBCommand(String text);
    bool MovePlayer(String name, int TeamId, int SquadId, bool force);
    bool SendTaskbarNotification(String title, String messagge);
    bool Log(String file, String message);
    bool Tweet(String status);
    bool PRoConChat(String text);
    bool PRoConEvent(String text, String player);
    bool SendSoundNotification(String soundfile, String soundfilerepeat);

    void ServerCommand(params String[] arguments);

    /*
     * Examples:
     *
     *           KickPlayerWithMessage(""micovery"" , ""Kicked you for team-killing!"");
     *           EABanPlayerWithMessage(EABanType.EA_GUID, EABanDuration.Temporary, ""micovery"", 10, ""You are banned for 10 minutes!"");
     *           PBBanPlayerWithMessage(PBBanDuration.Permanent, ""micovery"", 0, ""You are banned forever!"");
     *           ServerCommand(""admin.listPlayers"", ""all"");
     */


    /* Other Methods */
    String FriendlySpan(TimeSpan span);         //converts a TimeSpan into a friendly formatted string e.g. ""2 hours, 20 minutes, 15 seconds""
    String BestPlayerMatch(String name);        //looks in the internal player's list, and finds the best match for the given player name

    bool IsCommand(String text);                //checks if the given text start with one of these characters: !/@?
    String ExtractCommand(String text);         //if given text starts with one of these charactets !/@? it removes them
    String ExtractCommandPrefix(String text);   //if given text starts with one of these chracters !/@? it returns the character
    
    /* This method checks if the currently in-game player with matching name has
       a Procon account on the Procon instance controlling this game server. Returns
       False if the name does not match any of the players currently joined to the game server.
       The canBan value is set to true if the player can temporary ban or permanently ban.
    */
    bool CheckAccount(String name, out bool canKill, out bool canKick, out bool canBan, out bool canMove, out bool canChangeLevel);

    double CheckPlayerIdle(String name); // -1 if unknown, otherwise idle time in seconds

    /* Updated every update_interval seconds */
    bool IsSquadLocked(int teamId, int squadId); // False if unknown or open, True if locked
    String GetSquadLeaderName(int teamId, int squadId); // null if unknown, player name otherwise

    /* This method looks in the internal player's list for player with matching name.
     * If fuzzy argument is set to true, it will find the player name that best matches the given name
     *
     /
    PlayerInfoInterface GetPlayer(String name, bool fuzzy);

    /*
     * Creates a file in ProCon's directory  (InsaneLimits.dump)
     * Detailed information about the exception.
     */
    void DumpException(Exception e);

    /* Data Repository set/get custom data */

    DataDictionaryInterface Data { get; }        //this dictionary is user-managed
    DataDictionaryInterface RoundData { get; }   //this dictionary is automatically cleared OnRoundStart

    /* Friendly names */
    String FriendlyMapName(String mapFileName);  //example: ""MP_001"" -> ""Grand Bazaar""
    String FriendlyModeName(String modeName);    //example: ""TeamDeathMatch0"" -> ""TDM""
    KillReasonInterface FriendlyWeaponName(String killWeapon); 
        // BF3 example: ""Weapons/XP2_L86/L86"" => KillReasonInterface(""L86"", null, null)
        // BF4 example: ""U_AK12_M320_HE"" => KillReasonInterface(""M320"", ""HE"", ""AK12"")
        // BF4 vehicle example: ""Gameplay/Vehicles/AH6/AH6_Littlebird"" => KillReasonInterface(""Death"", null, null, ""AH6"", ""AH6_Littlebird"")

    /* External plugin support */
    bool IsOtherPluginEnabled(String className, String methodName);
    void CallOtherPlugin(String className, String methodName, Hashtable parms);
    DateTime GetLastPluginDataUpdate(); // return timestamp for the last time InsaneLimits.UpdatePluginData() was called
}
</pre>

 <h2>Data and Objects</h2>
       The <b>Data</b> object is a nested dictionary of key/value pairs that you can use to store custom data inside the <b>plugin</b>, <b>server</b>, <b>limit</b>, <b>player</b>, <b>killer</b>, and <b>victim</b> objects. The <b>Data</b> object implements the following interface:<br />
       <br />
    <pre>
public interface DataDictionaryInterface
{
    /* String Data */
    String setString(String key, String value);
    String getString(String key);
    String unsetString(String key);
    bool issetString(String key);
    List<String> getStringKeys();

    /* Boolean Data */
    bool setBool(String key, bool value);
    bool getBool(String key);
    bool unsetBool(String key);
    bool issetBool(String key);
    List<String> getBoolKeys();

    /* Double Data */
    double setDouble(String key, double value);
    double getDouble(String key);
    double unsetDouble(String key);
    bool issetDouble(String key);
    List<String> getDoubleKeys();

    /* Int Data */
    int setInt(String key, int value);
    int getInt(String key);
    int unsetInt(String key);
    bool issetInt(String key);
    List<String> getIntKeys();

    /* Object Data */
    object setObject(String key, object value);
    object getObject(String key);
    object unsetObject(String key);
    bool issetObject(String key);
    List<String> getObjectKeys();

    /* Generic set/get methods */
    Object set(Type type, String key, Object value);
    Object get(Type type, String key);
    Object unset(Type type, String key);
    bool isset(Type type, String key);
    List<String> getKeys(Type type);

    /* Other methods */
    void Clear();  /* clear/unset all data from repository */

}
    </pre>

        <h2>Simple (Traditional) Replacements</h2>
        This plugin supports an extensive list of message text replacements. A replacement is a string that starts and ends with the percent character ""%"".
        When you use them in the text of a message, the plugin will try to replace it with the corresponding value. For example: <br />
        <br />
        The message <br />
        <br />
        <pre>
    ""%k_n% killed %v_n% with a %w_n%""
        </pre>
        <br />
        becomes<br />
        <br />
        <pre>
    ""micovery killed NorthEye with a PP-2000""
        </pre>
        <br />
        Below is a list of all the replacements supported. Some replacements are not available for all types of events. For example, Killer-Name replacement is not available for OnSpawn event. <br />
        <br />
        <pre>
    public String[] Replacements = new string[]
    {
        // Killer Replacements (Evaluations:  OnKill, OnDeath, OnTeamKills, and OnTeamDeath)
        /* k   - killer
         * n   - name
         * ct  - Clan-Tag
         * cn  - Country Name
         * cc  - Country Code
         * ip  - IPAddress
         * eg  - EA GUID
         * pg  - Punk Buster GUID
         */
        ""%k_n%"",    ""Killer name"",
        ""%k_ct%"",   ""Killer clan-Tag"",
        ""%k_cn%"",   ""Killer county-name"",
        ""%k_cc%"",   ""Killer county-code"",
        ""%k_ip%"",   ""Killer ip-address"",
        ""%k_eg%"",   ""Killer EA GUID"",
        ""%k_pg%"",   ""Killer Punk-Buster GUID"",
        ""%k_fn%"",   ""Killer full name, includes Clan-Tag (if any)"",

        // Victim Replacements (Evaluations:  OnKill, OnDeath, OnTeamKills, and OnTeamDeath)

        /* Legend:
         * v   - victim
         */
        ""%v_n%"",    ""Victim name"",
        ""%v_ct%"",   ""Victim clan-Tag"",
        ""%v_cn%"",   ""Victim county-name"",
        ""%v_cc%"",   ""Victim county-code"",
        ""%v_ip%"",   ""Victim ip-address"",
        ""%v_eg%"",   ""Victim EA GUID"",
        ""%v_pg%"",   ""Vitim Punk-Buster GUID"",
        ""%v_fn%"",   ""Victim full name, includes Clan-Tag (if any)"",

        // Player Repalcements (Evaluations: OnJoin, OnLeave, OnSpawn, OnTeamChange, OnAnyChat, and OnSuicide)

        /* Legend:
         * p   - player
         * lc  - last chat
         */
        ""%p_n%"",    ""Player name"",
        ""%p_ct%"",   ""Player clan-Tag"",
        ""%p_cn%"",   ""Player county-name"",
        ""%p_cc%"",   ""Player county-code"",
        ""%p_ip%"",   ""Player ip-address"",
        ""%p_eg%"",   ""Player EA GUID"",
        ""%p_pg%"",   ""Player Punk-Buster GUID"",
        ""%p_fn%"",   ""Player full name, includes Clan-Tag (if any)"",
        ""%p_lc%"",   ""Player, Text of last chat"",
        // Weapon Replacements (Evaluations: OnKill, OnDeath, OnTeamKill, OnTeamDeath, OnSuicide)

        /* Legend:
         * w   - weapon
         * n   - name
         * p   - player
         * a   - All (players)
         * x   - count
         */
        ""%w_n%"",    ""Weapon name"",
        ""%w_p_x%"",  ""Weapon, number of times used by player in current round"",
        ""%w_a_x%"",  ""Weapon, number of times used by All players in current round"",

        // Limit Replacements for Activations & Spree Counts (Evaluations: Any)

        /* Legend:
         * th  - ordinal count suffix e.g. 1st, 2nd, 3rd, 4th, etc
         * x   - count, 1, 2, 3, 4, etc
         * p   - player
         * s   - squad
         * t   - team
         * a   - All (players)
         * r   - SpRee
         */
        ""%p_x_th%"",  ""Limit, ordinal number of times limit has been activated by the player"",
        ""%s_x_th%"",  ""Limit, ordinal number of times limit has been activated by the player's squad"",
        ""%t_x_th%"",  ""Limit, ordinal number of times limit has been activated by the player's team"",
        ""%a_x_th%"",  ""Limit, ordinal number of times limit has been activated by all players in the server"",
        ""%r_x_th%"",  ""Limit, ordinal number of times limit has been activated by player without Spree value being reset"",
        ""%p_x%"",     ""Limit, number of times limit has been activated by the player"",
        ""%s_x%"",     ""Limit, number of times limit has been activated by the player's squad"",
        ""%t_x%"",     ""Limit, number of times limit has been activated by the player's team"",
        ""%a_x%"",     ""Limit, number of times limit has been activated by all players in the server"",
        ""%r_x%"",     ""Limit, number of times limit has been activated by player without Spree value being reset"",


        // Limit Replacements for Activations & Spree Counts (Evaluations: Any) ... (All Rounds)
        /* Legend:
         * xa - Total count, for all rounds
         */
        ""%p_xa_th%"",  ""Limit, ordinal number of times limit has been activated by the player"",
        ""%s_xa_th%"",  ""Limit, ordinal number of times limit has been activated by the player's squad"",
        ""%t_xa_th%"",  ""Limit, ordinal number of times limit has been activated by the player's team"",
        ""%a_xa_th%"",  ""Limit, ordinal number of times limit has been activated by all players in the server"",
        ""%p_xa%"",     ""Limit, number of times limit has been activated by the player"",
        ""%s_xa%"",     ""Limit, number of times limit has been activated by the player's squad"",
        ""%t_xa%"",     ""Limit, number of times limit has been activated by the player's team"",
        ""%a_xa%"",     ""Limit, number of times limit has been activated by all players in the server"",


        ""%date%"", ""Current date, e.g. Sunday December 25, 2011"",
        ""%time%"", ""Current time, e.g. 12:00 AM""

        ""%server_host%"", ""Server/Layer host/IP "",
        ""%server_port%"", ""Server/Layer port number""

        ""%l_id%"", ""Limit numeric id"",
        ""%l_n%"",  ""Limit name""

    };
        </pre>
        <h2>Advanced Replacements</h2>
        In addition to the simple %<b>key</b>% replacments, this plugin also allows you to use a more advanced type of replacement. Within strings, you can use replacements that match properties in known objects. For example, if you use <b>player.Name</b> within a string, the plugin will detect it and replace it appropriately.<br />
        <br />
        A common usage for advanced replacements is to list player stats in the Kick/Ban reason. For example:
        <br />
        <br />
        The message <br />
        <br />
        <pre>
    ""player.Name you were banned for suspicious stats: Kpm: player.Kpm, Spm: player.Spm, Kdr: player.Kdr""
        </pre>
        <br />
        becomes<br />
        <br />
        <pre>
    ""micovery you were banned for suspicious stats: Kpm: 0.4, Spm: 120, Kdr: 0.61""
        </pre>
        <br />
        <h2>Settings</h2>
        <ol>
          <li><blockquote><b>use_direct_fetch</b><br />
                <i>True</i> - if the cache is not available, fetch stats directly from Battlelog<br />
                <i>False</i> - disable direct fetches from Battlelog<br />
                <br />
                If the <b>Battlelog Cache</b> plugin is installed, up to date and enabled,
                it will be used for player stats regardless of the setting of this
                option. If the <b>Battlelog Cache</b> plugin
                is not installed, not up to date or disabled, setting
                <b>use_direct_fetch</b> to True will act as a fallback system, fetching
                stats directly from Battlelog. Otherwise, stats fetching will
                fail since the cache is not available and this setting is False.
                </blockquote>
          </li>
          <li><blockquote><b>use_battlelog_proxy</b><br />
                <i>True</i> - Send requests to web services over a proxy.<br />
                <i>False</i> - Do not use a proxy.<br />
                <br />
                </blockquote>
          </li>
          <li><blockquote><b>proxy_url</b><br />
                <i>(string, url)</i> - Format: http://IP:PORT - http://user:password@IP:PORT<br />
                <br />
                The URL of the proxy server.
                </blockquote>
          </li>
          <li><blockquote><b>use_slow_weapon_stats</b><br />
                <i>False</i> - skip fetching weapon stats for new players<br />
                <i>True</i> - fetch weapon stats for new players<br />
                <br />
                Visible only if <b>use_direct_fetch</b> is set to True.
                Fetching weapon stats from Battlelog takes a long time, 15 seconds or more
                per player. By default, this slow fetch is disabled (False), so that
                your Procon restart or initial plugin enable time on a full server
                won't be delayed or bogged down while fetching weapon stats. However,
                if you have limits that use the GetBattlelog() function, you <b>must</b>
                set this value to True, or else stats will not be available.
                Also, see <b>rcon_to_battlelog_codes</b>.
                </blockquote>
          </li>
          <li><blockquote><b>use_stats_log</b><br />
                <i>False</i> - do not log Battlelog stats to the battle.log file<br />
                <i>True</i> - log player stats to the battle.log file<br />
                <br />
                If stats fetching is enabled and stats are fetched successfully, all the stats that were fetched will be logged in a file that follows the standard logging file name pattern: procon/Logs/<server-ip>_<server-port>/YYYYMMDD_battle.log (text file).
                </blockquote>
          </li>
          <li><blockquote><strong>limits_file</strong><br />
                <i>(string, path)</i> - path to the file where limits, and lists are saved
                </blockquote>
           </li>
          <li><blockquote><b>auto_load_interval</b><br />
                <i>(integer >= 60)</i> - interval in seconds, for auto loading settings from the <b>limits_file</b><br />
                <br />
                </blockquote>
          </li>
           <li><blockquote><strong>player_white_list</strong><br />
                <i>(string, csv)</i> - list of players that should never be kicked or banned
                </blockquote>
           </li>
           <li><blockquote><strong>clan_white_list</strong><br />
                <i>(string, csv)</i> - list of clan (tags) for players that should never be kicked or banned
                </blockquote>
           </li>
           <li><blockquote><strong>virtual_mode</strong><br />
                <i>True</i> - limit <b>actions</b> (kick, ban) are simulated, the actual commands are not sent to server <br />
                <i>False</i> - limit <b>actions</b> (kick, ban) are not simulated <br />
            </blockquote>
           </li>
           <li><blockquote><strong>console</strong><br />
                <i>(string)</i> - you can use this field to run plugin commands <br />
                <br />
                For example: ""!stats micovery"" will print the player statistic for the current round in the plugin console. <br />
                <br />
                Note that plugin commands, are currently supported only inside ProCon, and not In-Game.
                </blockquote>
           </li>
          <li><blockquote><b>rcon_to_battlelog_codes</b><br />
                <i>String[] Array</i> - Syntax: RCON=Battlelog, e.g., U_XBOW=WARSAW_ID_P_WNAME_CROSSBOW<br />
                <br />
                Visible only if <b>use_slow_weapon_stats</b> is True.
                Lets you define mappings from RCON weapon codes to Battelog weapon stats codes. Useful when new unlocks or DLC
                are released and in-use before the next update of this plugin is available. You can also override
                incorrect mappings built-in to the plugin, if any.
                </blockquote>
          </li>
          <li><blockquote><b>smtp_port</b><br />
                <i>(string)</i> - Address of the SMTP Mail server used for <i>Mail</i> action<br />
                </blockquote>
          </li>
           <li><blockquote><b>smtp_port</b><br />
                <i>(integer > 0)</i> - port number of the SMTP Mail server used for <i>Mail</i> action<br />
                </blockquote>
          </li>
          <li><blockquote><b>smtp_account</b><br />
                <i>(string)</i> - mail address for authenticating with the SMTP Mail used for <i>Mail</i> action<br />
                </blockquote>
          </li>
          <li><blockquote><b>smtp_mail</b><br />
                <i>(string)</i> - mail address (Sender/From) that is used for sending used for <i>Mail</i> action<br />
                <br />
                This is usually the same as <b>smtp_account</b> ... depends on your SMTP Mail provider.
                </blockquote>
          </li>
          <li><blockquote><b>say_interval</b><br />
                <i>(float)</i> - interval in seconds between say messages. Default value is 0.05, which is 50 milli-seconds<br />
                <br />
                The point of this setting is to avoid spam, but you should not set this value too large. Ideally it should be between 0 and 1 second.
                </blockquote>
          </li>
          <li><blockquote><b>wait_timeout</b><br />
                <i>(int)</i> - interval in seconds to wait for a response from the game server<br />
                <br />
                If you get several <b>Timeout(xx seconds) expired, while waiting for ...</b>
                exceptions in plugin.log, try increasing the wait_timeout value by 10 seconds.
                Repeat until the exceptions stop, but you should not exceed 90 seconds.
                </blockquote>
          </li>
        </ol>
       <br />
       <h2> Plugin Commands</h2>

       These are the commands supported by this plugin. You can run them from within the <b>console</b> field. Replies to the commands are printed in the plugin log.<br />
       <br />
       <ul>
           <li><blockquote>
                <b> !round stats</b><br />
                Aggregate stats for all players, current round<br />
                <br /><br />
                <b> !total stats</b><br />
                Aggregate stats for all players, all rounds<br />
                <br /><br />
                <b> !weapon round stats</b><br />
                Weapon-Level round stats for all players, current round<br />
                <br /><br />
                <b> !weapon total stats</b><br />
                Weapon-Level stats for all players, all rounds<br />
                <br /><br />
                <b> !web stats {player}</b><br />
                Battlelog stats for the current player<br />
                <br /><br />
                <b> !round stats {player}</b><br />
                Aggregate stats for the current player, current round<br />
                <br /><br />
                <b> !total stats {player}</b ><br />
                Aggregate stats for the current player, all rounds<br />
                <br /><br />
                <b> !weapon round stats {player}</b><br />
                Weapon-Level stats for the current player, current round<br />
                <br /><br />
                <b> !weapon total stats {player}</b><br />
                Weapon-Level stats for the current player, all round<br />
               <br />
               <br />
               These are the most awesome of all the commands this plugin provides. Even if you are not using this plugin to enforce any limit, you could have it enabled for just monitoring player stats.<br />
               <br />
               When calling player specific statistic commands, if you misspell, or only type part of the player name, the plugin will try to find the best match for the player name.<br />
               <br />
               </blockquote>
           </li>
           <li><blockquote><b>!dump limit {id}</b><br />
               <br />
               This command creates a file in ProCon's directory containing the source-code for the limit with the specified <i>id</i><br />
               <br />
               For example, the following command <br />
               <br />
               !dump limit <b>5</b><br />
               <br />
                Creates the file ""LimitEvaluator<b>5</b>.cs"" inside ProCon's directory. <br />
                <br />
                This command is very useful for debugging compilation errors, as you can see the code inside the file exactly as the plugin sees it (with the same line and column offsets).
               </blockquote>
           </li>
           <li><blockquote>
                <b> !set {variable} {to|=} {value}</b><br />
                <b> !set {variable} {value}</b><br />
                <b> !set {variable}</b><br />
                <br />
                This command is used for setting the value of this plugin's variables.<br />
                For the last invocation syntax the value is assumed to be ""True"". <br />
               </blockquote>
           </li>
           <li><blockquote>
                <b>!get {variable} </b><br />
                <br />
                This command prints the value of the specified variable.
               </blockquote>
           </li>
       </ul>

      <h2> In-Game Commands</h2>

       These are the In-Game commands supported by this plugin. You can run them only from within the game. Replies to the commands are printed in the game chat.<br />
       <br />
       <ul>
           <li><blockquote>
                <b> !stats</b><br />
                List the available stats, Battlelog<br />
                <br /><br />
                <b> !stats [web|battlelog]</b><br />
                List the available stats, Battlelog<br />
                <br /><br />
                <b> !stats round</b><br />
                List the available stats, current round<br />
                <br /><br />
                <b> !stats total</b><br />
                List the available stats, all rounds<br />
                <br />
                These commands are used as a shortcut for players to view what type of stats they can query. The plugin will try to fit all stat types into a single chat message.<br />
                <br />
               </blockquote>
            </li>
           <li><blockquote>
                <b> !my {type}</b><br />
                Print Battlelog stat of the specified <b>type</b> for the player that executed the command<br />
                <br />
                <b> !my round {type}</b><br />
                Print current round stat of the specified <b>type</b> for the player that executed the command<br />
                <br />
                <b> !my total {type}</b><br />
                Print all rounds stat of the specified <b>type</b> for the player that executed the command<br />
                <br />
                <b> ?{player} {type}</b><br />
                Print Battlelog stat of the specified <b>type</b> for the specified <b>player</b><br />
                <br />
                <b> ?{player} round {type}</b><br />
                Print current round stat of the specified <b>type</b> for the specified <b>player</b><br />
                <br />
                <b> ?{player} total {type}</b><br />
                Print all rounds stat of the specified <b>type</b> for the specified <b>player</b><br />
                <br />
                <br />
                The <b>player</b> name can be a sub-string, or even misspelled. The plugin will find the best match.<br />
                <br />
               </blockquote>
           </li>
       </ul>
       <blockquote>
       Annex 1 - Boolean Operators: <br />
       <br />
       For combining <i>Expressions</i> you use <i>Boolean Logic</i> operators. These are: <br />
       <br />

       <ul>
              <li>AND (Conjunction): <b>&&</b></li>
              <li>OR  (Disjunction): <b>||</b></li>
              <li>NOT (Negation): <b>!</b></li>
       </ul>
       </blockquote>
       <blockquote>
       Annex 2 - Relational Operators: <br />
       <br />
       All the previous examples use the Greater-Than ( <b>&gt;</b> ) operator a lot, but that is not the only relational operator supported. These are the arithmetic relational operators you can use:<br />
       <br />
       <ul>
              <li>Greater-Than: <b>&gt;</b></li>
              <li>Greater-than-or-Equal: <b>&gt;=</b></li>
              <li>Less-than: <b>&lt;</b></li>
              <li>Less-than-or-Equal: <b>&lt;=</b></li>
              <li>Equality: <b>==</b></li>
              <li>Not-Equal: <b>!=</b></li>
       </ul>
       <br />
        ";
        }



        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {

            activate_handle.Reset();

            server_host = strHostName;
            server_port = strPort;

            /* reset limits file, now that we have host and port */
            setStringVarValue("limits_file", getStringVarValue("limits_file"));


            ConsoleWrite("plugin loaded");
            this.RegisterEvents(
                "OnPlayerLeft",
                "OnPlayerJoin",
                "OnListPlayers",
                "OnPunkbusterPlayerInfo",
                "OnServerInfo",
                "OnMaplistList",
                "OnMaplistGetMapIndices",
                "OnRoundOver",
                "OnPlayerKilled",
                "OnPlayerTeamChange",
                "OnPlayerMovedByAdmin",
                "OnPlayerSpawned",
                "OnGlobalChat",
                "OnTeamChat",
                "OnSquadChat",
                "OnRoundOverPlayers",
                "OnRoundOverTeamScores",
                "OnServerName",
                "OnServerDescription",
                "OnMaplistMapAppended",
                "OnMaplistNextLevelIndex",
                "OnMaplistMapRemoved",
                "OnMaplistMapInserted",
                "OnMaplistCleared",
                "OnMaplistLoad",
                "OnMaplistSave",
                "OnEndRound",
                "OnRunNextLevel",
                "OnCurrentLevel",
                "OnLoadingLevel",
                "OnLevelStarted",
                "OnLevelLoaded",
                "OnRestartLevel",
                "OnReservedSlotsList",
                "OnGameModeCounter",
                /* R38/Procon 1.4.0.7 */
                "OnPlayerIdleDuration",
                "OnPlayerPingedByAdmin",
                "OnSquadLeader",
                "OnSquadIsPrivate",
                "OnCtfRoundTimeModifier",
                /* on update_interval get: */
                "OnBulletDamage",
                "OnFriendlyFire",
                "OnGunMasterWeaponsPreset",
                "OnIdleTimeout",
                "OnSoldierHealth",
                "OnVehicleSpawnAllowed",
                "OnVehicleSpawnDelay",
                /* BF4 additions */
                "OnCommander",
                "OnMaxSpectators",
                "OnServerType",
                "OnTeamFactionOverride"
                );

            //initialize the dictionary with countries, carriers, gateways
            initializeCarriers();
        }

        public void OnPluginEnable()
        {
            try
            {
                if (finalizer != null && finalizer.IsAlive)
                {
                    ConsoleError("Cannot enable plugin while it is finalizing");
                    return;
                }


                ConsoleWrite("^b^2Enabled!^0");
                plugin_enabled = true;
                enabledTime = DateTime.Now;

                this.players.Clear();
                
                bool cacheOK = IsCacheEnabled(true); // side-effect of logging messages
                if (!cacheOK && !getBooleanVarValue("use_direct_fetch")) {
                    ConsoleWarn("Player stats fetching is disabled!");
                }
                
                friendlyMaps.Clear();
                friendlyModes.Clear();
                List<CMap> bf3_defs = this.GetMapDefines();
                foreach (CMap m in bf3_defs) {
                    if (!friendlyMaps.ContainsKey(m.FileName)) friendlyMaps[m.FileName] = m.PublicLevelName;
                    if (!friendlyModes.ContainsKey(m.PlayList)) friendlyModes[m.PlayList] = m.GameMode;
                }
                if (getIntegerVarValue("debug_level") >= 8) {
                    foreach (KeyValuePair<String,String> pair in friendlyMaps) {
                        DebugWrite("friendlyMaps[" + pair.Key + "] = " + pair.Value, 8);
                    }
                    foreach (KeyValuePair<String,String> pair in friendlyModes) {
                        DebugWrite("friendlyModes[" + pair.Key + "] = " + pair.Value, 8);
                    }
                }
                DebugWrite("Friendly names loaded", 6);

                // register a command to indicate availibility to other plugins
                this.RegisterCommand(match_command_update_plugin_data);

                //start a thread that waits for the settings to be read from file

                Thread Activator = new Thread(new ThreadStart(delegate()
                {
                    plugin_activated = false;
                    try
                    {
                        //Thread.CurrentThread.Name = "activator";
                        LoadSettings(true, false, true);
                        ConsoleWrite("Waiting for ^bprivacy_policy_agreement^n value");
                        int timeout = 30;
                        while (true)
                        {
                            activate_handle.WaitOne(timeout * 1000);

                            if (!plugin_enabled)
                                break;

                            // if user has not agreed, wait for user to agree
                            if (!Agreement)
                            {
                                ConsoleWarn("You must review and accept the ^bPrivacy Policy^n before plugin can be activated");
                                activate_handle.Reset();
                                continue;
                            }

                            if (!plugin_enabled)
                                break;

                            // if user has agreed, exit now, and activate the plugin
                            if (Agreement)
                            {
                                activate_handle.Set();
                                break;
                            }
                        }

                        if (!plugin_enabled)
                        {
                            ConsoleWrite("detected that plugin was disabled, aborting");
                            return;
                        }

                        ConsoleWrite("Agreement received, activating plugin now!");
                        ActivatePlugin();

                    }
                    catch (Exception e)
                    {
                        DumpException(e);
                    }


                }));

                Activator.IsBackground = true;
                Activator.Name = "activator";
                Activator.Start();
                Thread.Sleep(1000);

            }
            catch (Exception e)
            {
                DumpException(e);
            }
        }

        public void CompileAll()
        {
            CompileAll(false);
        }
        public void CompileAll(bool force)
        {
            List<String> keys = new List<string>(limits.Keys);
            foreach (String key in keys)
                if (limits.ContainsKey(key) && (limits[key].evaluator == null || force) && plugin_enabled)
                    CompileLimit(limits[key]);
        }

        public void InitWeapons()
        {
            // initialize values for all known weapons

            WeaponDictionary dic = GetWeaponDefines();
            WeaponsDict = new Dictionary<string, DamageTypes>();
            foreach (Weapon weapon in dic)
                if (weapon != null && !WeaponsDict.ContainsKey(weapon.Name))
                    WeaponsDict.Add(weapon.Name, weapon.Damage);

            DebugWrite("^b" + WeaponsDict.Count + "^n weapons in dictionary", 5);

        }

        public void initializeCarriers()
        {
            if (Carriers == null)
                return;

            if (CarriersDict == null)
                CarriersDict = new Dictionary<string, Dictionary<string, string>>();

            if ((Carriers.Length % 3) != 0)
            {
                ConsoleError("sanity check failed for the ^bCarriers^n dictionary");
                return;
            }

            for (int i = 0; i < Carriers.Length; i = i + 3)
            {
                String country = Carriers[i].Replace(" ", "_");
                String carrier = Carriers[i + 1].Replace(" ", "_");
                String gateway = Carriers[i + 2];

                if (!CarriersDict.ContainsKey(country))
                    CarriersDict.Add(country, new Dictionary<string, string>());

                if (!CarriersDict[country].ContainsKey(carrier))
                    CarriersDict[country].Add(carrier, String.Empty);

                CarriersDict[country][carrier] = gateway;
            }

        }

        public void InitReplacements()
        {
            if (AdvancedReplacementsDict == null)
                AdvancedReplacementsDict = new Dictionary<string, string>();

            if (ReplacementsDict == null)
                ReplacementsDict = new Dictionary<string, string>();

            if ((Replacements.Length % 2) != 0)
            {
                ConsoleError("sanity check failed for the ^bReplacements^n dictionary");
                return;
            }

            for (int i = 0; i < Replacements.Length; i = i + 2)
                if (!ReplacementsDict.ContainsKey(Replacements[i]))
                    ReplacementsDict.Add(Replacements[i], Replacements[i]);
        }

        public String R(String message)
        {
            if (ReplacementsDict == null)
                return message;

            foreach (KeyValuePair<String, String> pair in ReplacementsDict)
                if (message.Contains(pair.Key))
                    message = message.Replace(pair.Key, pair.Value);

            if (AdvancedReplacementsDict == null)
                return message;

/*
            foreach (KeyValuePair<String, String> pair in AdvancedReplacementsDict)
                if (message.Contains(pair.Key))
                    message = message.Replace(pair.Key, pair.Value);
*/

            // Ensure correct match for aliased substrings
            // Battlelog404 is the only property that ends with a digit
            // Use [^A-Za-z] as the terminator of the prop name
            
            Match m = null;
            String ds = message;

            foreach (KeyValuePair<String, String> pair in AdvancedReplacementsDict) {
                while ((m = Regex.Match(message, pair.Key + @"(?:[^A-Za-z]|$)")).Success) {
                    if (getIntegerVarValue("debug_level") >= 1) ds = message.Insert(m.Index, "^b").Insert(m.Index + pair.Key.Length + 2, "^n");
                    DebugWrite("Replacing " + pair.Key + ": " + ds, 6);
                    message = message.Replace(pair.Key, pair.Value);
                }
            }
            
            DebugWrite("New repl: " + message, 6);

            return message;

        }

        public void SetupReplacements(Limit limit, PlayerInfoInterface player, PlayerInfoInterface killer, KillInfoInterface kill, PlayerInfoInterface victim)
        {


            //Re-Adjust the targets, depending on the event type, so that no variables are NULL
            // Legend
            // e - event
            // k - kill
            // d - death
            // p - player
            // s - suicide
            // g - group
            // How to Read, kd_e, Kill-Death Event

            Limit.EvaluationType kd_e = Limit.EvaluationType.OnKill | Limit.EvaluationType.OnDeath | Limit.EvaluationType.OnTeamKill | Limit.EvaluationType.OnTeamDeath;

            Limit.EvaluationType k_e = Limit.EvaluationType.OnKill | Limit.EvaluationType.OnTeamKill;

            Limit.EvaluationType d_e = Limit.EvaluationType.OnDeath | Limit.EvaluationType.OnTeamDeath;

            Limit.EvaluationType p_e = Limit.EvaluationType.OnJoin | Limit.EvaluationType.OnLeave | Limit.EvaluationType.OnSpawn | Limit.EvaluationType.OnInterval | Limit.EvaluationType.OnIntervalPlayers | Limit.EvaluationType.OnAnyChat | Limit.EvaluationType.OnTeamChange;
            /*Limit.EvaluationType.OnGlobalChat | Limit.EvaluationType.OnSquadChat | Limit.EvaluationType.OnTeamChat*/

            Limit.EvaluationType s_e = Limit.EvaluationType.OnSuicide;
            Limit.EvaluationType g_e = Limit.EvaluationType.OnRoundOver | Limit.EvaluationType.OnRoundStart | Limit.EvaluationType.OnIntervalServer;

            if ((limit.Evaluation & p_e) > 0 && player != null)
            {
                killer = player;
                victim = player;
                CPlayerInfo dummy = new CPlayerInfo(player.Name, player.Tag, player.TeamId, player.SquadId);
                kill = new KillInfo(new Kill(dummy, dummy, "UnkownWeapon", false, new Point3D(), new Point3D()), BaseEvent.Kill, "None");
            }
            else if ((limit.Evaluation & kd_e) > 0 && kill != null)
            {
                if ((limit.Evaluation & k_e) > 0 && killer != null)
                    player = killer;
                else if ((limit.Evaluation & d_e) > 0 && victim != null)
                    player = victim;
            }
            else if ((limit.Evaluation & s_e) > 0 && player != null)
            {
                killer = player;
                victim = player;
            }
            else if ((limit.Evaluation & g_e) > 0)
            {
                //None of the replacements apply (try/catch takes care of it)
                killer = null;
                victim = null;
                player = null;
                kill = null;
            }

            // use a shorted refernece, lazy
            Dictionary<String, String> dict = ReplacementsDict;
            double value = 0;
            List<String> keys = new List<string>(ReplacementsDict.Keys);

            foreach (String key in keys)
            {
                try
                {
                    switch (key)
                    {
                        // Killer Replacements (Evaluations:  OnKill, OnDeath, OnTeamKills, and OnTeamDeath)
                        case "%k_n%":
                            dict[key] = killer.Name;
                            break;
                        case "%k_ct%":
                            dict[key] = killer.Tag;
                            break;
                        case "%k_cn%":
                            dict[key] = killer.CountryName;
                            break;
                        case "%k_cc%":
                            dict[key] = killer.CountryCode;
                            break;
                        case "%k_ip%":
                            dict[key] = killer.IPAddress;
                            break;
                        case "%k_eg%":
                            dict[key] = killer.EAGuid;
                            break;
                        case "%k_pg%":
                            dict[key] = killer.PBGuid;
                            break;
                        case "%k_fn%":
                            dict[key] = killer.FullName;
                            break;

                        // Victim Replacements (Evaluations:  OnKill, OnDeath, OnTeamKills, and OnTeamDeath)
                        case "%v_n%":
                            dict[key] = victim.Name;
                            break;
                        case "%v_ct%":
                            dict[key] = victim.Tag;
                            break;
                        case "%v_cn%":
                            dict[key] = victim.CountryName;
                            break;
                        case "%v_cc%":
                            dict[key] = victim.CountryCode;
                            break;
                        case "%v_ip%":
                            dict[key] = victim.IPAddress;
                            break;
                        case "%v_eg%":
                            dict[key] = victim.EAGuid;
                            break;
                        case "%v_pg%":
                            dict[key] = victim.PBGuid;
                            break;
                        case "%v_fn%":
                            dict[key] = victim.FullName;
                            break;


                        // Player Repalcements (Evaluations: OnJoin, OnLeave, OnSpawn, OnAnyChat, OnTeamChange, and OnSuicide)
                        case "%p_n%":
                            dict[key] = player.Name;
                            break;
                        case "%p_ct%":
                            dict[key] = player.Tag;
                            break;
                        case "%p_cn%":
                            dict[key] = player.CountryName;
                            break;
                        case "%p_cc%":
                            dict[key] = player.CountryCode;
                            break;
                        case "%p_ip%":
                            dict[key] = player.IPAddress;
                            break;
                        case "%p_eg%":
                            dict[key] = player.EAGuid;
                            break;
                        case "%p_pg%":
                            dict[key] = player.PBGuid;
                            break;
                        case "%p_fn%":
                            dict[key] = player.FullName;
                            break;
                        case "%p_lc%":
                            dict[key] = player.LastChat;
                            break;

                        // Weapon Replacements (Evaluations: OnKill, OnDeath, OnTeamKill, OnTeamDeath, and OnSuicide)
                        case "%w_n%":
                            //dict[key] = kill.Weapon;
                            {
                                KillReasonInterface kr = FriendlyWeaponName(kill.Weapon);
                                dict[key] = kr.Name;
                                if (kr.Name == "Death" && !String.IsNullOrEmpty(kr.VehicleName))
                                    dict[key] = kr.VehicleName;
                            }
                            break;
                        case "%w_p_x%":
                            dict[key] = killer[kill.Weapon].KillsRound.ToString();
                            break;
                        case "%w_a_x%":
                            dict[key] = (serverInfo == null) ? key : serverInfo[kill.Weapon].KillsRound.ToString();
                            break;

                        // Limit Specific Replacements (Evaluations: Any) (Current Round)
                        case "%p_x_th%":
                            value = limit.Activations(player.Name);
                            dict[key] = value.ToString() + Ordinal(value);
                            break;
                        case "%p_x%":
                            dict[key] = limit.Activations(player.Name).ToString();
                            break;
                        case "%s_x_th%":
                            value = limit.Activations(player.TeamId, player.SquadId);
                            dict[key] = value.ToString() + Ordinal(value);
                            break;
                        case "%s_x%":
                            dict[key] = limit.Activations(player.TeamId, player.SquadId).ToString();
                            break;
                        case "%t_x_th%":
                            value = limit.Activations(player.TeamId);
                            dict[key] = value.ToString() + Ordinal(value);
                            break;
                        case "%t_x%":
                            dict[key] = limit.Activations(player.TeamId).ToString();
                            break;
                        case "%a_x_th%":
                            value = limit.Activations();
                            dict[key] = value.ToString() + Ordinal(value);
                            break;
                        case "%a_x%":
                            dict[key] = limit.Activations().ToString();
                            break;

                        case "%r_x_th%":
                            value = limit.Spree(player.Name);
                            dict[key] = value.ToString() + Ordinal(value);
                            break;
                        case "%r_x%":
                            dict[key] = limit.Spree(player.Name).ToString();
                            break;


                        // Limit Specific Replacements (Evaluations: Any) (All Rounds)
                        case "%p_xa_th%":
                            value = limit.ActivationsTotal(player.Name);
                            dict[key] = value.ToString() + Ordinal(value);
                            break;
                        case "%p_xa%":
                            dict[key] = limit.ActivationsTotal(player.Name).ToString();
                            break;
                        case "%s_xa_th%":
                            value = limit.ActivationsTotal(player.TeamId, player.SquadId);
                            dict[key] = value.ToString() + Ordinal(value);
                            break;
                        case "%s_xa%":
                            dict[key] = limit.ActivationsTotal(player.TeamId, player.SquadId).ToString();
                            break;
                        case "%t_xa_th%":
                            value = limit.ActivationsTotal(player.TeamId);
                            dict[key] = value.ToString() + Ordinal(value);
                            break;
                        case "%t_xa%":
                            dict[key] = limit.ActivationsTotal(player.TeamId).ToString();
                            break;
                        case "%a_xa_th%":
                            value = limit.ActivationsTotal();
                            dict[key] = value.ToString() + Ordinal(value);
                            break;
                        case "%a_xa%":
                            dict[key] = limit.ActivationsTotal().ToString();
                            break;


                        // Other Replacements
                        case "%date%":
                            dict[key] = DateTime.Now.ToString("D");
                            break;
                        case "%time%":
                            dict[key] = DateTime.Now.ToString("t");
                            break;

                        case "%server_host%":
                            dict[key] = server_host;
                            break;
                        case "%server_port%":
                            dict[key] = server_port.ToString();
                            break;

                        case "%l_id%":
                            dict[key] = limit.id;
                            break;
                        case "%l_n%":
                            dict[key] = limit.Name;
                            break;

                        default:
                            dict[key] = key;
                            break;
                    }
                }
                catch (NullReferenceException)
                {
                    // this is expected for group events (g_e), so don't spam errors in the console
                    if (!((limit.Evaluation & g_e) > 0))
                        ConsoleWarn("could not determine replacement for %^b" + key.Replace("%", "") + "^n%");
                    dict[key] = key;
                }
            }


            //setup the advanced replacements


            Dictionary<String, Object> map = new Dictionary<string, object>();
            map.Add("limit", (object)limit);
            map.Add("player", (object)player);
            map.Add("killer", (object)killer);
            map.Add("kill", (object)kill);
            map.Add("victim", (object)victim);
            map.Add("plugin", (object)this);
            map.Add("server", (object)serverInfo);


            foreach (KeyValuePair<String, object> pair in map)
            {
                String name = pair.Key;
                Object data = pair.Value;

                if (data == null)
                    continue;

                Type type = data.GetType();
                PropertyInfo[] props = type.GetProperties();

                foreach (PropertyInfo prop in props)
                {
                    String key = name + "." + prop.Name;

                    if (prop.Name.Equals("Item"))
                        continue;

                    //if (prop.PropertyType.Equals(typeof(bool)))
                    //    continue;
                    
                    object result = (object)("?");

                    try
                    {
                        if (!AdvancedReplacementsDict.ContainsKey(key))
                            AdvancedReplacementsDict.Add(key, String.Empty);



                        result = prop.GetValue(data, null);

                        if (result == null)
                            continue;

                        if (result.GetType().Equals(typeof(double)))
                            result = (object)Math.Round((double)result, 2);

                        AdvancedReplacementsDict[key] = result.ToString();
                    }
                    catch (Exception e)
                    {
                        DebugWrite("Advanced replacement failed for " + key + " with result " + result + " and error: " + e, 6);
                        ConsoleWarn("could not determine value for ^b" + key + "^n in replacement");
                        continue;
                    }
                }

            }







        }

        public String Ordinal(double value)
        {

            long last_XX = ((long)Math.Abs(value)) % 100;

            if ((last_XX > 10 && last_XX < 14) || last_XX == 0)
                return "th";

            long last_X = last_XX % 10;

            switch (last_X)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                default:
                    return "th";
            }
        }

        public void InitWaitHandles()
        {
            DebugWrite("Initializing wait handles", 6);
            fetch_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            enforcer_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            settings_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            say_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            info_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            scratch_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            list_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            indices_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            server_name_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            server_desc_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            plist_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            move_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            pending_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            reply_handle = new EventWaitHandle(false, EventResetMode.ManualReset);
        }

        public void DestroyWaitHandles()
        {

            if (fetch_handle != null)
                fetch_handle.Set();
            if (enforcer_handle != null)
                enforcer_handle.Set();
            if (settings_handle != null)
                settings_handle.Set();
            if (say_handle != null)
                say_handle.Set();
            if (info_handle != null)
                info_handle.Set();
            if (scratch_handle != null)
                scratch_handle.Set();

            if (list_handle != null)
                list_handle.Set();
            if (indices_handle != null)
                indices_handle.Set();
            if (server_name_handle != null)
                server_name_handle.Set();
            if (server_desc_handle != null)
                server_desc_handle.Set();
            if (activate_handle != null)
                activate_handle.Set();

            if (plist_handle != null)
                plist_handle.Set();
            if (move_handle != null)
                move_handle.Set();
            if (pending_handle != null)
                pending_handle.Set();
            if (reply_handle != null)
                reply_handle.Set();

            plugin_activated = false;

        }

        public void InitThreads()
        {
            DebugWrite("Initializing threads", 6);
            this.fetching_thread = new Thread(new ThreadStart(fetch_thread_loop));
            this.enforcer_thread = new Thread(new ThreadStart(enforcer_thread_loop));
            this.say_thread = new Thread(new ThreadStart(say_thread_loop));
            this.settings_thread = new Thread(new ThreadStart(settings_thread_loop));
            this.moving_thread = new Thread(new ThreadStart(move_thread_loop));

            this.fetching_thread.IsBackground = true;
            this.enforcer_thread.IsBackground = true;
            this.say_thread.IsBackground = true;
            this.settings_thread.IsBackground = true;
            this.moving_thread.IsBackground = true;
        }

        public void StartThreads()
        {
            DebugWrite("Starting threads", 6);
            settings_thread.Start();
            say_thread.Start();
            enforcer_thread.Start();
            fetching_thread.Start();
            moving_thread.Start();
        }

        public void ActivatePlugin()
        {

            plugin_activated = true;
            activate_handle.Set();

            InitWeapons();
            InitReplacements();

            InitWaitHandles();
            InitThreads();

            ClearData();

            // Initial commands
            getMapInfoSync();
            getServerNameSync();
            getServerDescriptionSync();

            StartThreads();

            getPlayersList();
            getPBPlayersList();
            getReservedSlotsList();

            DelayedCompile(30);

            int lc = limits.Count;
            String lc_msg = "limit" + ((lc > 1 || lc == 0) ? "s" : "");

            if (getBooleanVarValue("tweet_my_plugin_state"))
                DefaultTweet("#InsaneLimits #plugin enabled  @\"" + server_name + "\", using " + lc + " " + lc_msg);
        }

        public void DelayedCompile(int sleep_time)
        {
            //delayed limit compilation
            Thread delayed_compilation = new Thread(new ThreadStart(delegate()
            {
                DebugWrite("sleeping for " + sleep_time + " seconds, before compiling limits", 4);
                Thread.Sleep(sleep_time * 1000);
                CompileAll();

            }));

            delayed_compilation.IsBackground = true;
            delayed_compilation.Name = "delayed_comp";
            delayed_compilation.Start();
        }

        public void ClearData()
        {
            if (serverInfo != null)
                serverInfo.Data.Clear();

            List<String> keys = new List<string>();
            foreach (String key in keys)
                if (limits.ContainsKey(key))
                    limits[key].Data.Clear();

            Data.Clear();
        }


        Dictionary<String, PlayerInfo> new_players_batch = new Dictionary<string, PlayerInfo>();

        private int GetQCount() {
            int npqc = 0;
            lock (players_mutex)
            {
                npqc = new_player_queue.Count;
            }
            return npqc;
        }

        private int GetBCount() {
            int npbc = 0;
            lock (players_mutex)
            {
                npbc = new_players_batch.Count;
            }
            return npbc;
        }
        
        public void fetch_thread_loop()
        {

            try
            {

                Thread.CurrentThread.Name = "fetch";
                DebugWrite(" starting", 4);

                InsaneLimits plugin = this;
                
                Dictionary<String,int> retryCount = new Dictionary<String,int>();
                Dictionary<String, CPunkbusterInfo> retryInfo = new Dictionary<string, CPunkbusterInfo>();
                bool gaveEnforcerTime = false;

                /*
                In order to reduce the rate of fetches to avoid "Too Many Requests"
                errors, apply a lower bound on the amount of time used to do one
                fetch and one insert. Any remaining time is spent sleeping.
                The value for minSecs is adaptive. The more errors there are,
                the longer it gets. Each success reduces it back.
                
                According to
                
                http://www.phogue.net/forumvb/showthread.php?5313-Battlelog-stats-make-plugins-amp-procon-lag-Solution-Global-stats-fetching&p=62259&viewfull=1#post62259
                
                the upper bound for request rate is 15 requests every 20 seconds.
                To allow head-room for other plugins running simultaneously,
                we cap our rate at 5 requests every 20 seconds. For a full
                server of 64 players, that means a minimum time to empty the
                initial fetch queue is about 4.5 minutes.
                */
                DateTime since = DateTime.Now; // lower bound
                double minSecs = 4.0; // min between fetches
                double maxSecs = 10.0; // max between fetches
                double lowerBound = minSecs;

                while (true)
                {
                    gaveEnforcerTime = false;
                    
                    while (GetQCount() == 0)
                    {
                        if (retryCount.Count > 0) {
                            foreach (String k in retryCount.Keys) {
                                lock (players_mutex) {
                                    if (!new_player_queue.ContainsKey(k)) {
                                        new_player_queue.Add(k, retryInfo[k]);
                                    }
                                }
                            }
                            DebugWrite("Retrying fetch for ^b" + retryCount.Count + "^n players in the retry queue", 4);
                            continue;
                        }
                        // if there are no more players, put yourself to sleep
                        DebugWrite("no new players, will wait, signalling ^benforcer^n thread", 7);
                        fetch_handle.Reset();
                        gaveEnforcerTime = true;
                        enforcer_handle.Set();
                        WaitOn("fetch_handle", fetch_handle);
                        fetch_handle.Reset();
                        if (!plugin_enabled) break;
                        DebugWrite("awake! checking queue ...", 7);
                        if (GetQCount() == 0) DebugWrite("Nothing to do, ^bfetch^n going back to sleep...", 7);
                    }
                    
                    DateTime fetchSince = DateTime.Now;

                    if (!gaveEnforcerTime && plugin_enabled) {
                        // Give some time to enforcer thread
                        DebugWrite("players in fetch queue, giving time to ^benforcer^n thread", 7);
                        fetch_handle.Reset();
                        gaveEnforcerTime = true;
                        enforcer_handle.Set();
                        WaitOn("fetch_handle", fetch_handle);
                        fetch_handle.Reset();
                        DebugWrite("awake!, block ^benforcer^n thread", 7);
                    }

                    while (GetQCount() > 0)
                    {
                        if (!plugin_enabled)
                            break;
                        
                        List<String> keys = null;

                        lock (players_mutex)
                        {
                            keys = new List<string>(new_player_queue.Keys);
                        }

                        String name = keys[0];

                        CPunkbusterInfo info = null;

                        lock (players_mutex)
                        {
                            new_player_queue.TryGetValue(name, out info);
                        }

                        if (info == null)
                        {
                            lock (players_mutex)
                            {
                                if (new_player_queue.ContainsKey(name)) new_player_queue.Remove(name);
                            }
                            continue;
                        }

                        // make sure I am the only one modifying these dictionaries at this time
                        lock (players_mutex)
                        {
                            if (new_player_queue.ContainsKey(name))
                                new_player_queue.Remove(name);

                            if (!new_players_batch.ContainsKey(name))
                                new_players_batch.Add(name, null);
                        }

                        int nq = GetQCount();
                        String msg = nq + " more player" + ((nq > 1) ? "s" : "") + " in queue";
                        if (nq == 0) msg = "no more players in queue";

                        bool ck = false;
                        lock (players_mutex)
                        {
                            ck = new_players_batch.ContainsKey(info.SoldierName);
                        }
                        if (ck)
                        {

                            if (lowerBound > minSecs && DateTime.Now.Subtract(since).TotalSeconds < lowerBound) {
                                 // Add some delay between consecutive fetches
                                 DebugWrite("adding delay before next fetch, lower bound is " + lowerBound + " secs", 5);
                                 double upperBound = maxSecs * 2;
                                 while (DateTime.Now.Subtract(since).TotalSeconds < lowerBound && upperBound > 0.0) {
                                    if (!plugin_enabled) break;
                                    // Give some time to enforcer thread
                                    fetch_handle.Reset();
                                    DebugWrite("throttling fetch, giving time to ^benforcer^n thread", 7);
                                    gaveEnforcerTime = true;
                                    enforcer_handle.Set();
                                    WaitOn("fetch_handle", fetch_handle);
                                    fetch_handle.Reset();
                                    DebugWrite("awake, check throttling delay", 7);
                                    upperBound = upperBound - 1.0;
                                 }
                                 DebugWrite("awake, proceeding with next fetch", 5);
                            }

                            DebugWrite("^4getting battlelog stats for ^b" + name + "^n, " + msg + "^0", 5);
                            since = DateTime.Now; // reset timer
                            PlayerInfo ptmp = plugin.blog.fetchStats(new PlayerInfo(plugin, info));

                            /* If there was a fetch error, remember for retry */
                            if (ptmp._web_exception != null) {
                                // Adaptively increment
                                lowerBound = Math.Min(lowerBound + 1.0, maxSecs);
                                if (lowerBound != maxSecs) DebugWrite("increase lower bound to " + lowerBound.ToString("F0") + " secs", 6);
                                
                                lock (players_mutex)
                                {
                                    if (new_players_batch.ContainsKey(name)) new_players_batch.Remove(name);
                                }

                                // Check if player still present
                                bool sheLeft = false;
                                lock (players_mutex)
                                {
                                    sheLeft = (!scratch_list.Contains(name));
                                }
                                if (sheLeft) {
                                    DebugWrite("aborting fetch, looks like player " + name + " left the game!", 6);
                                    continue;
                                }
                            
                                if (!retryCount.ContainsKey(name)) {
                                    retryCount[name] = 0;
                                    retryInfo[name] = info;
                                    ptmp = null; // release failed fetch info
                                    DebugWrite("^b" + name + "^n is one of ^b" + retryCount.Count + "^n players in the retry queue", 5);
                                    continue;
                                }
                                retryCount[name] = retryCount[name] + 1;
                                DebugWrite("Retry " + retryCount[name] + " for " + name, 4);
                                if (retryCount[name] >= 3) {
                                    // give up
                                    retryCount.Remove(name);
                                    retryInfo.Remove(name);
                                    if (ptmp._web_exception == null) ptmp._web_exception = new System.Net.WebException("fetch retry failed");
                                    DebugWrite("Fetching stats for ^b" + name + "^n: " + ptmp._web_exception.Message, 4);
                                } else {
                                    continue;
                                }
                            } else {
                                // Adaptively decrement
                                lowerBound = Math.Max(lowerBound - 1.0, minSecs);
                                if (lowerBound != minSecs) DebugWrite("decrease lower bound to " + lowerBound.ToString("F0") + " secs", 5);

                                if (retryCount.ContainsKey(name)) {
                                    retryCount.Remove(name);
                                    retryInfo.Remove(name);
                                }
                            }

                            lock (players_mutex)
                            {
                                new_players_batch[name] = ptmp;
                            }
                            
                            if (ptmp.StatsError) {
                                DebugWrite("Unable to fetch stats for ^b" + name + "^n", 4);
                            }
                        }

                        if (!plugin_enabled) break;

                        if (GetBCount() > 0) {
                            break;
                        }
                    }

                    DebugWrite("^4^bTIME^n took " + DateTime.Now.Subtract(fetchSince).TotalSeconds.ToString("F2") + " secs, fetching players from queue", 5);

                    // abort the thread if the plugin was disabled
                    if (!plugin_enabled)
                    {
                        DebugWrite("detected that plugin was disabled, aborting", 4);
                        lock (players_mutex)
                        {
                            new_player_queue.Clear();
                            new_players_batch.Clear();
                        }
                        return;
                    }

                    int bb = GetBCount();
                    
                    DateTime batchSince = DateTime.Now;

                    if (bb > 0) {
                        DebugWrite("done fetching stats, " + bb + " player" + ((bb > 1) ? "s" : "") + " in new batch, updating player's list", 5);

                        // Async request for updates
                        scratch_handle.Reset();
                        plist_handle.Reset();
                        DebugWrite("waiting for player list updates", 5);
                        getPlayersList();
                        WaitOn("scratch_handle", scratch_handle);
                        scratch_handle.Reset();
                        WaitOn("plist_handle", plist_handle);
                        plist_handle.Reset();
                        DebugWrite("awake! got player list updates", 5);
                    }

                    List<PlayerInfo> inserted = new List<PlayerInfo>();
                    // first insert the entire player's batch

                    lock (players_mutex)
                    {
                        // remove the nulls, and the ones that left
                        List<String> players_to_remove = new List<string>();
                        foreach (KeyValuePair<String, PlayerInfo> pair in new_players_batch)
                            if (pair.Value == null || !scratch_list.Contains(pair.Key))
                                if (!players_to_remove.Contains(pair.Key))
                                {
                                    plugin.DebugWrite("looks like ^b" + pair.Key + "^n left, removing him from new batch", 5);
                                    players_to_remove.Add(pair.Key);
                                }


                        // now remove them
                        foreach (String pname in players_to_remove)
                            if (new_players_batch.ContainsKey(pname))
                                new_players_batch.Remove(pname);

                        if (new_players_batch.Count > 0) {
                            bb = new_players_batch.Count;
                            DebugWrite("Will insert a batch of " + bb + " player" + ((bb > 1) ? "s" : ""), 5);
                        } else bb = 0;


                        foreach (KeyValuePair<String, PlayerInfo> pair in new_players_batch)
                            if (pair.Value != null && scratch_list.Contains(pair.Key))
                            {
                                //players.Add(pair.Key, pair.Value);
                                if (players.ContainsKey(pair.Key)) {
                                    DebugWrite("--------->>> Why does players dict already have " + pair.Key + " in it????", 5);
                                }
                                players[pair.Key] = pair.Value;
                                inserted.Add(pair.Value);
                            }

                        new_players_batch.Clear();
                    }
                    
                    // abort the thread if the plugin was disabled
                    if (!plugin_enabled)
                    {
                        DebugWrite("detected that plugin was disabled, aborting", 4);
                        lock (players_mutex)
                        {
                            new_player_queue.Clear();
                            new_players_batch.Clear();
                        }
                        return;
                    }

                    // then for each of the players just inserted, evaluate OnJoin
                    if (bb > 0) {
                        DebugWrite("For " + bb + " new players, evaluate OnJoin limits", 5);
                        foreach (PlayerInfo pp in inserted)
                        {
                            OnPlayerJoin(pp); // each call syncs map info
                            // quit early if plugin was disabled
                            if (!plugin_enabled)
                                break;
                        }
                    } else {
                        DebugWrite("No players left in batch, skipping OnJoin limits, synching map info", 5);
                        getMapInfoSync();
                    }
                    
                    DebugWrite("^4^bTIME^n took " + DateTime.Now.Subtract(batchSince).TotalSeconds.ToString("F2") + " secs, process player batch", 5);


                    // abort the thread if the plugin was disabled
                    if (!plugin_enabled)
                    {
                        DebugWrite("detected that plugin was disabled, aborting", 4);
                        lock (players_mutex)
                        {
                            new_player_queue.Clear();
                            new_players_batch.Clear();
                        }
                        return;
                    }
                    
                    DebugWrite("Request PB player's list for new players to queue ...", 8);
                    getPBPlayersList();

                    DebugWrite("^4^bDONE^n inserting " + bb + " new players, " + GetQCount() + " still in queue, took a total of " + DateTime.Now.Subtract(since).TotalSeconds.ToString("F0") + " secs^0", 3);
                }
            }
            catch (Exception e)
            {
                if (typeof(ThreadAbortException).Equals(e.GetType()))
                {
                    Thread.ResetAbort();
                    return;
                }

                DumpException(e);
            }
            finally
            {
                enforcer_handle.Set();
            }

        }
        public int getLineOffset(String haystack, String needle)
        {
            int start_line = 0;
            string[] lines = haystack.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (String line in lines)
                if (++start_line > 0 && Regex.Match(line, needle).Success)
                    break;

            return start_line;
        }


        public string getClassName(Limit limit)
        {
            return "LimitEvaluator" + limit.id;
        }

        public string buildLimitSource(Limit limit)
        {
            string class_name = getClassName(limit);


            string class_source =
@"namespace PRoConEvents
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;
    using System.Collections;
    using System.Net;
    using System.Net.Mail;
    using System.Web;
    using System.Data;
    using System.Threading;
    // .net 3.5 additions: procon 1.4.1.1 and later
    //using System.Linq;
    using System.Xml;



    class %class_name%
    {
        public bool FirstCheck(%first_check_arguments%)
        {
            try
            {
#pragma warning disable
            %FirstCheck%
#pragma warning restore
            }
            catch(Exception e)
            {
                plugin.DumpException(e, this.GetType().Name);
            }
            return false;
        }

        public bool SecondCheck(%second_check_arguments%)
        {
            try
            {
#pragma warning disable
            %SecondCheck%
#pragma warning restore
            }
            catch(Exception e)
            {
               plugin.DumpException(e, this.GetType().Name);
            }
            return true;
        }
    }
}";
            class_source = Regex.Replace(class_source, "%class_name%", class_name);

            // function arguments depend on event
            class_source = buildFunctionArguments(limit, "first_check_arguments", class_source);
            class_source = buildFunctionArguments(limit, "second_check_arguments", class_source);


            class_source = buildClassFunctionBody(limit, "FirstCheck", limit.FirstCheck, limit.FirstCheckCode, limit.FirstCheckExpression, class_source);
            class_source = buildClassFunctionBody(limit, "SecondCheck", limit.SecondCheck, limit.SecondCheckCode, limit.SecondCheckEpression, class_source);

            return class_source;
        }

        public String FormatFunctionCode(Limit limit, String method, String code)
        {
            if (method.Equals("FirstCheck"))
                code += "\nreturn false;";
            else if (method.Equals("SecondCheck"))
                code += "\nreturn true;";
            else
                throw new CompileException(FormatMessage("unknown method ^b" + method + "^n for " + limit.ShortDisplayName, MessageType.Error));


            List<String> lines = new List<String>(Regex.Split(code, "\n"));

            code = lines[0];
            String prefix = "            ";
            for (int i = 1; i < lines.Count; i++)
                code += "\n" + prefix + lines[i];

            return code;
        }

        public String buildClassFunctionBody(Limit limit, String method, Limit.LimitType type, String code, String expression, String class_source)
        {
            String auto_return = String.Empty;


            // if disabled, or empty string make give it an auto-return value
            if (type.Equals(Limit.LimitType.Disabled) ||
                (type.Equals(Limit.LimitType.Code) && code.Length == 0) ||
                (type.Equals(Limit.LimitType.Expression) && expression.Length == 0))
                return Regex.Replace(class_source, "%" + method + "%", FormatFunctionCode(limit, method, ""));


            if (type.Equals(Limit.LimitType.Code))
                return Regex.Replace(class_source, "%" + method + "%", FormatFunctionCode(limit, method, code));

            else if (type.Equals(Limit.LimitType.Expression))
                return Regex.Replace(class_source, "%" + method + "%", "return ( (" + expression + ") == true);");
            else
                throw new CompileException(FormatMessage("unknown type for " + limit.ShortDisplayName, MessageType.Error));
        }


        // used for kill and death events
        public bool evaluateLimit(Limit limit, PlayerInfo killer, KillInfo kill, PlayerInfo victim)
        {
            return executeLimitAction(
                                           limit,
                                           null,
                                           killer,
                                           victim,
                                           kill
                                      );
        }

        // used for suicide events
        public bool evaluateLimit(Limit limit, PlayerInfo player, KillInfo kill)
        {
            return executeLimitAction(
                                            limit,
                                            player,
                                            null,
                                            null,
                                            kill
                                       );
        }

        //used for interval, join, team change, and spawn events
        public bool evaluateLimit(Limit limit, PlayerInfo player)
        {
            return executeLimitAction(
                                            limit,
                                            player,
                                            null,
                                            null,
                                            null
                                       );
        }

        //used for OnIntervalServer, RoundOver, and RoundStart events
        public bool evaluateLimit(Limit limit)
        {
            return executeLimitAction(
                                            limit,
                                            null,
                                            null,
                                            null,
                                            null
                                       );
        }


        public PlayerInfoInterface determineActionTarget(Limit limit, PlayerInfoInterface player, PlayerInfoInterface killer, PlayerInfoInterface victim)
        {
            switch (limit.Evaluation)
            {
                case Limit.EvaluationType.OnKill:
                case Limit.EvaluationType.OnTeamKill:
                    return killer;
                case Limit.EvaluationType.OnDeath:
                case Limit.EvaluationType.OnTeamDeath:
                    return victim;
                case Limit.EvaluationType.OnSuicide:
                    // for suicide, player, killer, and victim are the same
                    return player;
                case Limit.EvaluationType.OnSpawn:
                case Limit.EvaluationType.OnJoin:
                case Limit.EvaluationType.OnLeave:
                case Limit.EvaluationType.OnIntervalPlayers:
                case Limit.EvaluationType.OnAnyChat:
                case Limit.EvaluationType.OnTeamChange:
                    /*
                    case Limit.EvaluationType.OnGlobalChat:
                    case Limit.EvaluationType.OnTeamChat:
                    case Limit.EvaluationType.OnSquadChat:
                     */
                    return player;
                case Limit.EvaluationType.OnRoundOver:
                case Limit.EvaluationType.OnRoundStart:
                case Limit.EvaluationType.OnIntervalServer:
                    return DummyPlayer();
                default:
                    return null;
            }
        }

        PlayerInfo dummy = null;
        public PlayerInfoInterface DummyPlayer()
        {
            if (dummy != null)
                return dummy;

            CPunkbusterInfo pinfo = new CPunkbusterInfo("", "Unknown", "", "", "", "");
            CPlayerInfo info = new CPlayerInfo(pinfo.SoldierName, "", 0, 0);
            dummy = new PlayerInfo(this, pinfo);
            dummy.updateInfo(info);

            return dummy;

        }

        public object[] buildLimitArguments(MethodInfo method, Limit limit, PlayerInfoInterface player, PlayerInfoInterface killer, PlayerInfoInterface victim, KillInfoInterface kill,
                                            TeamInfoInterface team1, TeamInfoInterface team2, TeamInfoInterface team3, TeamInfoInterface team4)
        {
            PluginInterface plugin = (PluginInterface)this;
            ServerInfoInterface server = (ServerInfoInterface)serverInfo;

            List<object> arguments = null;
            switch (limit.Evaluation)
            {
                case Limit.EvaluationType.OnKill:
                case Limit.EvaluationType.OnDeath:
                case Limit.EvaluationType.OnTeamKill:
                case Limit.EvaluationType.OnTeamDeath:
                    arguments = new List<object>(new object[] { player, killer, kill, victim, server, plugin, team1, team2, team3, team4 });
                    break;
                case Limit.EvaluationType.OnSuicide:
                    // special case for suicide all three player, kill, and victim same
                    arguments = new List<object>(new object[] { player, player, kill, player, server, plugin, team1, team2, team3, team4 });
                    break;
                case Limit.EvaluationType.OnSpawn:
                case Limit.EvaluationType.OnJoin:
                case Limit.EvaluationType.OnLeave:
                case Limit.EvaluationType.OnIntervalPlayers:
                case Limit.EvaluationType.OnAnyChat:
                case Limit.EvaluationType.OnTeamChange:
                    /*
                    case Limit.EvaluationType.OnGlobalChat:
                    case Limit.EvaluationType.OnTeamChat:
                    case Limit.EvaluationType.OnSquadChat:
                     */
                    arguments = new List<object>(new object[] { player, server, plugin, team1, team2, team3, team4 });
                    break;
                case Limit.EvaluationType.OnRoundOver:
                case Limit.EvaluationType.OnRoundStart:
                case Limit.EvaluationType.OnIntervalServer:
                    arguments = new List<object>(new object[] { server, plugin, team1, team2, team3, team4 });
                    break;
                default:
                    throw new EvaluateException(FormatMessage("cannot determine arguments for " + limit.Evaluation.ToString() + " event in " + limit.ShortDisplayName, MessageType.Error));
            }

            if (method.Name.Equals("SecondCheck"))
                arguments.Add((LimitInfoInterface)limit);

            return arguments.ToArray();
        }

        public String buildFunctionArguments(Limit limit, String search, String class_source)
        {


            String extra = String.Empty;

            if (search.StartsWith("second"))
                extra += ", LimitInfoInterface limit";


            switch (limit.Evaluation)
            {
                case Limit.EvaluationType.OnKill:
                case Limit.EvaluationType.OnDeath:
                case Limit.EvaluationType.OnTeamKill:
                case Limit.EvaluationType.OnTeamDeath:
                    // for kill-death events, player is always the target of the Event action, e.g. Kill/killer, Death/dead,
                    return Regex.Replace(class_source, "%" + search + "%", "PlayerInfoInterface player, PlayerInfoInterface killer, KillInfoInterface kill, PlayerInfoInterface victim, ServerInfoInterface server, PluginInterface plugin, TeamInfoInterface team1, TeamInfoInterface team2, TeamInfoInterface team3, TeamInfoInterface team4" + extra);
                case Limit.EvaluationType.OnSuicide:
                    // special case for suicide all three player, kill, and victim same
                    return Regex.Replace(class_source, "%" + search + "%", "PlayerInfoInterface player, PlayerInfoInterface killer, KillInfoInterface kill, PlayerInfoInterface victim, ServerInfoInterface server, PluginInterface plugin, TeamInfoInterface team1, TeamInfoInterface team2, TeamInfoInterface team3, TeamInfoInterface team4" + extra);
                case Limit.EvaluationType.OnSpawn:
                case Limit.EvaluationType.OnJoin:
                case Limit.EvaluationType.OnLeave:
                case Limit.EvaluationType.OnIntervalPlayers:
                case Limit.EvaluationType.OnAnyChat:
                case Limit.EvaluationType.OnTeamChange:
                    /*
                      case Limit.EvaluationType.OnGlobalChat:
                      case Limit.EvaluationType.OnTeamChat:
                      case Limit.EvaluationType.OnSquadChat:
                     */
                    return Regex.Replace(class_source, "%" + search + "%", "PlayerInfoInterface player, ServerInfoInterface server, PluginInterface plugin, TeamInfoInterface team1, TeamInfoInterface team2, TeamInfoInterface team3, TeamInfoInterface team4" + extra);
                case Limit.EvaluationType.OnRoundOver:
                case Limit.EvaluationType.OnRoundStart:
                case Limit.EvaluationType.OnIntervalServer:
                    return Regex.Replace(class_source, "%" + search + "%", "ServerInfoInterface server, PluginInterface plugin, TeamInfoInterface team1, TeamInfoInterface team2, TeamInfoInterface team3, TeamInfoInterface team4" + extra);
                default:
                    throw new CompileException(FormatMessage("cannot determine arguments for ^b" + limit.Evaluation.ToString() + "^n event in " + limit.ShortDisplayName, MessageType.Error));
            }

        }



        /*
        // Not needed anymore, OnJoin limits are evaluated once only in OnPlayerJoin
        public bool shouldSkipEvaluation(Limit limit, PlayerInfoInterface player)
        {
            if (limit.Evaluation.Equals(Limit.EvaluationType.OnJoin))
                return limit.EvaluationsPlayer(player) > 0;

            return false;
        }*/


        public bool executeLimitCheck(Limit limit, String method, PlayerInfoInterface player, PlayerInfoInterface killer, PlayerInfoInterface victim, KillInfoInterface kill)
        {

            ServerInfoInterface server = this.serverInfo;

            Type class_type = limit.type;
            object class_object = limit.evaluator;

            if (class_type == null || class_object == null)
                return false;

            MethodInfo class_method = null;
            // find the method through reflection
            if ((class_method = class_type.GetMethod(method)) == null)
                throw new EvaluateException(FormatMessage("could not find method ^b" + method + "^n, in " + limit.ShortDisplayName, MessageType.Error));


            Dictionary<int, TeamInfoInterface> teams = new Dictionary<int, TeamInfoInterface>();
            for (int i = 1; i <= 4; i++)
            {
                if (!teams.ContainsKey(i))
                    teams.Add(i, (TeamInfoInterface)new TeamInfo(this, i, players, (ServerInfo)server));
            }

            // build the arguments
            object[] arguments = buildLimitArguments(class_method, limit, player, killer, victim, kill, teams[1], teams[2], teams[3], teams[4]);

            // invoke the method
            object result = class_method.Invoke(class_object, arguments);

            if (result == null)
                return false;

            return (bool)result;

        }






        //wrapper, to synchronize limit evaluation
        public bool executeLimitAction(
                                        Limit limit,
                                        PlayerInfoInterface player,
                                        PlayerInfoInterface killer,
                                        PlayerInfoInterface victim,
                                        KillInfoInterface kill
                                       )
        {

            lock (evaluation_mutex)
            {
                if (VModeSlot == null)
                    VModeSlot = Thread.AllocateDataSlot();

                Thread.SetData(VModeSlot, (bool)limit.Virtual);
                bool result = evaluateLimitChecks(limit, player, killer, victim, kill);
                Thread.SetData(VModeSlot, (bool)false);
                return result;
            }

        }

        static LocalDataStoreSlot VModeSlot = null;

        public bool evaluateLimitChecks(
                                        Limit limit,
                                        PlayerInfoInterface player,
                                        PlayerInfoInterface killer,
                                        PlayerInfoInterface victim,
                                        KillInfoInterface kill
                                       )
        {


            try
            {

                PlayerInfoInterface target = null;
                if ((target = determineActionTarget(limit, player, killer, victim)) == null)
                    throw new EvaluateException(FormatMessage("could not determine the ^itarget^n for ^baction^n with ^b" + limit.Evaluation.ToString() + "^n event in " + limit.ShortDisplayName, MessageType.Error));

                /*
                // Not needed anymore, OnJoin limits are evaluated once only in OnPlayerJoin
                // check wether we should evaluate this limit or not
                if (shouldSkipEvaluation(limit, target))
                    return false;
                */

                // quit now if first check is not enabled
                if (limit.FirstCheck.Equals(Limit.LimitType.Disabled))
                    return false;


                // call setup replacements early, in case user has a Code type of limit
                SetupReplacements(limit, target, killer, kill, victim);


                bool result = executeLimitCheck(limit, "FirstCheck", target, killer, victim, kill);



                /*
                // Not needed anymore, OnJoin limits are evaluated once only in OnPlayerJoin
                // do some record keeping
                if (limit.Evaluation.Equals(Limit.EvaluationType.OnJoin))
                    limit.RecordEvaluation(target);
                */

                if (!result)
                    return false;

                // more book-keeping
                limit.RecordActivation(target.Name);
                limit.RecordSpree(target.Name);

                //this is the actual call for setup up replacements that matter
                SetupReplacements(limit, target, killer, kill, victim);

                // run the second phase if available
                if (!limit.SecondCheck.Equals(Limit.LimitType.Disabled) && !limit.SecondCheckEmpty)
                    result = executeLimitCheck(limit, "SecondCheck", target, killer, victim, kill);


                if (!result)
                    return false;



                Limit.LimitAction action = limit.Action;

                if (action.Equals(Limit.LimitAction.None))
                {
                    DebugWrite("^b" + target.Name + "^n activated " + limit.ShortDisplayName, 3);
                    return true;
                }



                if ((action & Limit.LimitAction.Say) > 0)
                {
                    action = action & ~Limit.LimitAction.Say;

                    int delay = limit.SayDelay;

                    String message = R(limit.SayMessage);
                    DebugWrite("say(" + limit.SayAudience.ToString() + "), ^b" + target.Name + "^n, activated " + limit.ShortDisplayName + ": " + message, 3);

                    bool chat = limit.SayProConChat;
                    MessageAudience audience = limit.SayAudience;

                    switch (audience)
                    {
                        case MessageAudience.All:
                            SendGlobalMessage(message, delay);
                            if (chat)
                                PRoConChat("Admin > All: " + message);
                            break;
                        case MessageAudience.Team:
                            SendTeamMessage(target.TeamId, message, delay);
                            if (chat)
                                PRoConChat("Admin > Team(" + target.TeamId + "): " + message);
                            break;
                        case MessageAudience.Squad:
                            SendSquadMessage(target.TeamId, target.SquadId, message, delay);
                            if (chat)
                                PRoConChat("Admin > Team(" + target.TeamId + ").Squad(" + target.SquadId + "): " + message);
                            break;
                        case MessageAudience.Player:
                            SendPlayerMessage(target.Name, message, delay);
                            if (chat)
                                PRoConChat("Admin > " + player.Name + ": " + message);
                            break;
                        default:
                            ConsoleError("Unknown " + typeof(MessageAudience).Name + " for " + limit.ShortDisplayName);
                            break;
                    }



                    // exit early if action is only say
                    if (action.Equals(Limit.LimitAction.Say))
                        return !VMode;
                }


                if ((action & Limit.LimitAction.Yell) > 0)
                {
                    action = action & ~Limit.LimitAction.Yell;

                    int duration = limit.YellDuration;

                    String message = R(limit.YellMessage);
                    DebugWrite("yell(" + limit.YellAudience.ToString() + "), ^b" + target.Name + "^n, activated " + limit.ShortDisplayName + ": " + message, 3);

                    bool chat = limit.YellProConChat;
                    MessageAudience audience = limit.YellAudience;

                    switch (audience)
                    {
                        case MessageAudience.All:
                            SendGlobalYell(message, duration);
                            if (chat)
                                PRoConChat("Admin > Yell All: " + message);
                            break;
                        case MessageAudience.Team:
                            SendTeamYell(target.TeamId, message, duration);
                            if (chat)
                                PRoConChat("Admin > Yell Team(" + target.TeamId + "): " + message);
                            break;
                        case MessageAudience.Player:
                            SendPlayerYell(target.Name, message, duration);
                            if (chat)
                                PRoConChat("Admin > Yell " + player.Name + ": " + message);
                            break;
                        case MessageAudience.Squad:
                            /*
                            SendSquadYell(target.TeamId, target.SquadId, message, duration);
                            if (chat)
                                PRoConChat("Admin > Team(" + target.TeamId + ").Squad(" + target.SquadId + "): " + message);
                            */
                        default:
                            ConsoleError("Unknown " + typeof(MessageAudience).Name + " for " + limit.ShortDisplayName);
                            break;
                    }



                    // exit early if action is only yell
                    if (action.Equals(Limit.LimitAction.Yell))
                        return !VMode;
                }

                if ((action & Limit.LimitAction.Log) > 0)
                {
                    action = action & ~Limit.LimitAction.Log;

                    String lmessage = R(limit.LogMessage);

                    Limit.LimitLogDestination destination = limit.LogDestination;

                    if ((destination & Limit.LimitLogDestination.Plugin) > 0)
                        ConsoleWrite(lmessage);

                    if ((destination & Limit.LimitLogDestination.File) > 0)
                        Log(limit.LogFile, lmessage);

                    // exit early if action is only log
                    if (action.Equals(Limit.LimitAction.Log))
                        return !VMode;
                }

                if ((action & Limit.LimitAction.Mail) > 0)
                {
                    action = action & ~Limit.LimitAction.Mail;

                    String message = R(limit.MailBody);
                    String subject = R(limit.MailSubject);
                    String address = limit.MailAddress;


                    DebugWrite("sending mail(" + address + ") player ^b" + target.Name + "^n, (activated " + limit.ShortDisplayName + "), with subject: \"" + subject + "\"", 3);
                    SendMail(address, subject, message);

                    // exit early if action is only log
                    if (action.Equals(Limit.LimitAction.Mail))
                        return !VMode;
                }

                if ((action & Limit.LimitAction.SMS) > 0)
                {
                    action = action & ~Limit.LimitAction.SMS;

                    String number = limit.SMSNumber;
                    String message = R(limit.SMSMessage);
                    String country = limit.SMSCountry;
                    String carrier = limit.SMSCarrier;


                    DebugWrite("sending SMS(" + number + ") player ^b" + target.Name + "^n, (activated " + limit.ShortDisplayName + ")", 3);
                    SendSMS(country, carrier, number, message);

                    // exit early if action is only SMS
                    if (action.Equals(Limit.LimitAction.SMS))
                        return !VMode;
                }

                if ((action & Limit.LimitAction.Tweet) > 0)
                {
                    action = action & ~Limit.LimitAction.Tweet;

                    String status = R(limit.TweetStatus);
                    String account = getStringVarValue("twitter_screen_name");


                    DebugWrite("sending Tweet (@" + account + "): \"" + status + "\"", 3);
                    Tweet(status);

                    // exit early if action is only Tweet
                    if (action.Equals(Limit.LimitAction.Tweet))
                        return !VMode;
                }

                if ((action & Limit.LimitAction.PRoConChat) > 0)
                {
                    action = action & ~Limit.LimitAction.PRoConChat;

                    String text = R(limit.PRoConChatText);


                    DebugWrite("sending procon-chat \"" + text + "\"", 3);
                    PRoConChat(text);

                    // exit early if action is only PRoConChat
                    if (action.Equals(Limit.LimitAction.PRoConChat))
                        return !VMode;
                }


                if ((action & Limit.LimitAction.PRoConEvent) > 0)
                {
                    action = action & ~Limit.LimitAction.PRoConEvent;


                    EventType type = limit.PRoConEventType;
                    CapturableEvent name = limit.PRoConEventName;
                    String text = R(limit.PRoConEventText);
                    String pname = R(limit.PRoConEventPlayer);


                    DebugWrite("sending procon event(type:^b" + type.ToString() + "^n, name: ^b" + name.ToString() + "^n, player:^b" + pname + "^n) \"" + text + "\"", 3);

                    PRoConEvent(type, name, text, pname);

                    // exit early if action is only PRoConEvent
                    if (action.Equals(Limit.LimitAction.PRoConEvent))
                        return !VMode;
                }


                if ((action & Limit.LimitAction.TaskbarNotify) > 0)
                {
                    action = action & ~Limit.LimitAction.TaskbarNotify;

                    DebugWrite("sending taskbar notification,  player ^b" + target.Name + "^n, (activated " + limit.ShortDisplayName + ")", 3);
                    SendTaskbarNotification(R(limit.TaskbarNotifyTitle), R(limit.TaskbarNotifyMessage));

                    // exit early if action is only TaskbarNotify
                    if (action.Equals(Limit.LimitAction.TaskbarNotify))
                        return !VMode;
                }

                if ((action & Limit.LimitAction.SoundNotify) > 0)
                {
                    action = action & ~Limit.LimitAction.SoundNotify;

                    DebugWrite("playing soundnotification,  player ^b" + target.Name + "^n, (activated " + limit.ShortDisplayName + ")", 3);
                    SendSoundNotification(R(limit.SoundNotifyFile), R(limit.SoundNotifyRepeat));

                    // exit early if action is only TaskbarNotify
                    if (action.Equals(Limit.LimitAction.SoundNotify))
                        return !VMode;
                }

                /* Actions that possibly affect server state */

                result = false;

                if ((action & Limit.LimitAction.EABan) > 0)
                {
                    action = action & ~Limit.LimitAction.EABan;

                    EABanType btype = limit.EABType;
                    EABanDuration bduration = limit.EABDuration;

                    String bmessage = R(limit.EABMessage);
                    int bminutes = limit.EABMinutes;

                    DebugWrite("ea-banning(" + btype.ToString() + ":" + bduration.ToString() + ") player ^b" + target.Name + "^n, (activated " + limit.ShortDisplayName + "), with message: \"" + bmessage + "\"", 1);
                    if (EABanPlayerWithMessage(btype, bduration, target.Name, bminutes, bmessage))
                        result = true;
                }




                if ((action & Limit.LimitAction.PBBan) > 0)
                {
                    action = action & ~Limit.LimitAction.PBBan;

                    PBBanDuration bduration = limit.PBBDuration;

                    String bmessage = R(limit.PBBMessage);
                    int bminutes = limit.PBBMinutes;

                    DebugWrite("pb-banning(" + bduration.ToString() + ") player ^b" + target.Name + "^n, (activated " + limit.ShortDisplayName + "), with message: \"" + bmessage + "\"", 1);

                    if (PBBanPlayerWithMessage(bduration, target.Name, bminutes, bmessage))
                        result = true;
                }

                if ((action & Limit.LimitAction.PBCommand) > 0)
                {
                    action = action & ~Limit.LimitAction.PBCommand;

                    String command_text = R(limit.PBCommandText);

                    DebugWrite("sending pb-command (^b" + target.Name + "^n, activated " + limit.ShortDisplayName + "): " + command_text, 3);

                    if (PBCommand(command_text))
                        result = true;
                }

                if ((action & Limit.LimitAction.ServerCommand) > 0)
                {
                    action = action & ~Limit.LimitAction.ServerCommand;

                    String command_text = limit.ServerCommandText;

                    DebugWrite("sending server-command (^b" + target.Name + "^n, activated " + limit.ShortDisplayName + "): " + command_text, 3);

                    if (SCommand(command_text))
                        result = true;
                }

                if ((action & Limit.LimitAction.Kick) > 0)
                {
                    action = action & ~Limit.LimitAction.Kick;

                    String kmessage = R(limit.KickMessage);
                    DebugWrite("kicking player ^b" + target.Name + "^n, (activated " + limit.ShortDisplayName + "), with message: \"" + kmessage + "\"", 1);

                    if (KickPlayerWithMessage(target.Name, kmessage))
                        result = true;
                }

                if ((action & Limit.LimitAction.Kill) > 0)
                {
                    action = action & ~Limit.LimitAction.Kill;

                    int delay = limit.KillDelay;

                    String delay_text = "";
                    if (delay > 0)
                        delay_text = "(delay: ^b" + delay + "^n)";

                    DebugWrite("killing" + delay_text + " player ^b" + target.Name + "^n, (activated " + limit.ShortDisplayName + ")", 2);

                    if (KillPlayer(target.Name, delay))
                        result = true;
                }



                if ((action & (Limit.LimitAction)0xFF) > 0)
                    throw new EvaluateException(FormatMessage("unknown limit action " + action.ToString() + " for " + limit.ShortDisplayName, MessageType.Error));

                return result;

            }
            catch (EvaluateException e)
            {
                LogWrite(e.Message);
                return false;
            }
            catch (Exception e)
            {
                if (limit == null)
                    DumpException(e);
                else
                    DumpException(e, limit.ShortName);
            }

            return true;
        }


        public void SendCompilingMessage(Limit limit)
        {


            ConsoleWrite("Compiling " + limit.FullDisplayName + " - " + limit.Evaluation.ToString());

            String first_check = "^bfirst_check^n = ^i" + limit.FirstCheck.ToString() + "^n";

            if (limit.FirstCheckEmpty)
                ConsoleWarn("^bfirst_check_" + limit.FirstCheck.ToString().ToLower() + "^n is empty for " + limit.ShortDisplayName);


            if (limit.SecondCheckEmpty)
                ConsoleWarn("^bsecond_check_" + limit.SecondCheck.ToString().ToLower() + "^n is empty for " + limit.ShortDisplayName);

        }


        public void CompileLimit(Limit limit)
        {
            try
            {
                limit.Reset();

                if (compiler == null)
                    compiler = CodeDomProvider.CreateProvider("CSharp");

                limit.evaluator = null;
                limit.type = null;

                SendCompilingMessage(limit);

                if (limit.FirstCheckEmpty)
                    return;


                String class_source = buildLimitSource(limit);

                int start_line = 0;

                CompilerParameters cparams = GenerateCompilerParameters();
                CompilerResults cr = compiler.CompileAssemblyFromSource(cparams, class_source);
                cr.TempFiles.Delete();

                if (cr.Errors.Count > 0)
                {
                    // Display compilation errors.
                    ConsoleError("" + cr.Errors.Count + " error" + ((cr.Errors.Count > 1) ? "s" : "") + " compiling " + limit.FirstCheck.ToString());
                    foreach (CompilerError ce in cr.Errors)
                        ConsoleError("(" + ce.ErrorNumber + ", line: " + (ce.Line - start_line) + ", column: " + ce.Column + "):  " + ce.ErrorText);

                    return;
                }
                else
                {
                    String class_name = getClassName(limit);
                    Type class_type = cr.CompiledAssembly.GetType("PRoConEvents." + class_name);

                    ConstructorInfo class_ctor = class_type.GetConstructor(new Type[] { });
                    if (class_ctor == null)
                        throw new CompileException(FormatMessage("could not find constructor for ^b" + class_name + "^n", MessageType.Error));


                    object class_object = class_ctor.Invoke(new object[] { });


                    limit.evaluator = class_object;
                    limit.type = class_type;
                    return;
                }
            }
            catch (CompileException e)
            {
                LogWrite(e.Message);
            }
            catch (Exception e)
            {
                DumpException(e);
            }

            return;
        }


        public void JoinWith(Thread thread, int secs)
        {
            if (thread == null || !thread.IsAlive)
                return;

            DebugWrite("Waiting for ^b" + thread.Name + "^n to finish", 3);
            thread.Join(secs*1000);
        }

        public void JoinWith(Thread thread)
        {
            if (thread == null || !thread.IsAlive)
                return;

            JoinWith(thread, 3);
        }




        public void OnPluginDisable()
        {
            if (finalizer != null && finalizer.IsAlive)
                return;

            try
            {

                plugin_enabled = false;
                round_over = false;
                isRoundReset = false;
                level_loaded = false;


                finalizer = new Thread(new ThreadStart(delegate()
                    {
                        try
                        {
                            DestroyWaitHandles();

                            JoinWith(say_thread);
                            JoinWith(settings_thread);
                            JoinWith(enforcer_thread);
                            JoinWith(moving_thread);
                            JoinWith(fetching_thread, 45);

                            this.blog.CleanUp();
                            this.players.Clear();
                            lock (this.cacheResponseTable) {
                                this.cacheResponseTable.Clear();
                            }

                            CleanupLimits();

                            // unregister the command again to remove availibility-indicator
                            this.UnregisterCommand(match_command_update_plugin_data);

                            ConsoleWrite("^1^bDisabled =(^0");
                        }
                        catch (Exception e)
                        {
                            DumpException(e);
                        }
                    }));

                finalizer.IsBackground = true;
                finalizer.Name = "finalizer";
                finalizer.Start();
                Thread.Sleep(1);
            }
            catch (Exception e)
            {
                DumpException(e);
            }
        }

        public void CleanupLimits()
        {
            List<String> keys = new List<String>(limits.Keys);
            foreach (String key in keys)
            {
                Limit limit = null;
                limits.TryGetValue(key, out limit);
                if (limit == null)
                    continue;

                limit.Reset();
            }

        }


        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();
            try
            {

                List<string> vars = getPluginVars(true, true, true);

                foreach (string name in vars)
                {
                    String var_name = name;
                    String group_name = getPluginVariableGroup(var_name);
                    String var_type = "multiline";
                    String var_value = getPluginVarValue(var_name);
                    String group_order = getGroupOrder(group_name) + ". ";

                    if (shouldSkipGroup(group_name))
                        continue;

                    if (shouldSkipVariable(var_name, group_name))
                        continue;

                    if (var_name.Contains("password"))
                        var_value = Regex.Replace(var_value, ".", "*");


                    String limit_group_title = String.Empty;
                    bool limit_group_visible = true;

                    if (CustomList.isListVar(var_name))
                    {
                        String field = CustomList.extractFieldKey(var_name);
                        String id = CustomList.extractId(var_name);

                        if (!lists.ContainsKey(id))
                            continue;

                        CustomList list = lists[id];

                        if (field.Equals("state"))
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(CustomList.ListState))) + ")";
                        else if (field.Equals("comparison"))
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(CustomList.ListComparison))) + ")";
                        else if (field.Equals("hide"))
                        {
                            var_type = "enum." + var_name + "(...|" + String.Join("|", Enum.GetNames(typeof(ShowHide))) + ")";
                            var_value = "...";
                        }

                        group_order = "";

                    }
                    else if (Limit.isLimitVar(var_name))
                    {
                        String field = Limit.extractFieldKey(var_name);
                        String id = Limit.extractId(var_name);

                        if (!limits.ContainsKey(id))
                            continue;


                        Limit limit = limits[id];

                        if (limit.isGroupFirstField(field))
                            limit_group_title = limit.getGroupFormattedTitleByKey(field);

                        limit_group_visible = limit.getGroupStateByKey(field);


                        if (field.Equals("ea_ban_duration"))
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(EABanDuration))) + ")";
                        else if (field.Equals("pb_ban_duration"))
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(PBBanDuration))) + ")";
                        else if (field.Equals("ea_ban_type"))
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(EABanType))) + ")";
                        else if (field.Equals("pb_ban_type"))
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(PBBanType))) + ")";
                        else if (field.Equals("first_check") || field.Equals("second_check"))
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(Limit.LimitType))) + ")";
                        else if (field.Equals("new_action"))
                        {
                            var_type = "enum." + var_name + "(...|" + String.Join("|", EnumValues(typeof(Limit.LimitAction), true)) + ")";
                            var_value = "...";
                        }
                        else if (field.Equals("evaluation"))
                        {
                            List<String> rawNames = new List<String>(Enum.GetNames(typeof(Limit.EvaluationType)));
                            if (rawNames.Contains("OnIntervalPlayers")) {
                                // move it to the end
                                rawNames.Remove("OnIntervalPlayers");
                                rawNames.Add("OnIntervalPlayers");
                            }
                            if (rawNames.Contains("OnInterval")) {
                                // move it to the end
                                rawNames.Remove("OnInterval");
                                rawNames.Add("OnInterval");
                            }
                            if (rawNames.Contains("OnAnyChat")) {
                                // move to item #3 (will end up being #4)
                                rawNames.Remove("OnAnyChat");
                                rawNames.Insert(2, "OnAnyChat");
                            }
                            if (rawNames.Contains("OnIntervalServer")) {
                                // move to item #3
                                rawNames.Remove("OnIntervalServer");
                                rawNames.Insert(2, "OnIntervalServer");
                            }
                            var_type = "enum." + var_name + "(" + String.Join("|", rawNames.ToArray()) + ")";
                        }
                        else if (field.Equals("say_audience"))
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(MessageAudience))) + ")";
                        else if (field.Equals("say_procon_chat"))
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(TrueFalse))) + ")";
                        else if (field.Equals("state"))
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(Limit.LimitState))) + ")";
                        else if (field.Equals("procon_event_type"))
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(EventType))) + ")";
                        else if (field.Equals("procon_event_name"))
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(CapturableEvent))) + ")";

                        else if (field.Equals("hide"))
                        {
                            var_type = "enum." + var_name + "(...|" + String.Join("|", Enum.GetNames(typeof(ShowHide))) + ")";
                            var_value = "...";
                        }
                        else if (field.Equals("log_destination"))
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(Limit.LimitLogDestination))) + ")";
                        else if (field.Equals("sms_country"))
                            var_type = "enum." + var_name + "(" + String.Join("|", EnumValues(new List<String>(CarriersDict.Keys))) + ")";
                        else if (field.Equals("sms_carrier"))
                        {
                            String country = limit.SMSCountry;

                            if (!CarriersDict.ContainsKey(country))
                                continue;

                            List<String> keys = new List<String>(CarriersDict[country].Keys);

                            // if the carrier does not exist in the country, set the first as default
                            if (!keys.Contains(var_value))
                                var_value = keys[0];

                            limit.SMSCarrier = var_value;

                            var_type = "enum." + var_name + limit.SMSCountry + "(" + String.Join("|", EnumValues(keys)) + ")";
                        } else if (field.Equals("yell_audience")) {
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(MessageAudience))) + ")";
                        } else if (field.Equals("yell_procon_chat")) {
                            var_type = "enum." + var_name + "(" + String.Join("|", Enum.GetNames(typeof(TrueFalse))) + ")";
                        }
                        group_order = "";
                    }
                    else if (var_name.Equals("new_limit") || var_name.Equals("new_list"))
                        var_type = "enum." + var_name + "(...|" + String.Join("|", Enum.GetNames(typeof(TrueFalse))) + ")";
                    else if (var_name.Equals("compile_limit"))
                        var_type = "enum." + var_name + "(...|" + String.Join("|", Enum.GetNames(typeof(LimitChoice))) + ")";
                    else if (var_name.Equals("privacy_policy_agreement"))
                    {
                        var_value = "...";
                        var_type = "enum." + var_name + "(...|" + String.Join("|", Enum.GetNames(typeof(AcceptDeny))) + ")";
                    }

                    if (var_name == "rcon_to_battlelog_codes")
                    {
                        if (!getBooleanVarValue("use_slow_weapon_stats")) continue; // hide if use_slow_weapon_stats is False
                        rcon2bw_user_var.Clear();
                        foreach (String k in rcon2bw_user.Keys)
                        {
                            rcon2bw_user_var.Add(k + "=" + rcon2bw_user[k]);
                        }
                        lstReturn.Add(new CPluginVariable(group_order + group_name + "|" + var_name, typeof(string[]), rcon2bw_user_var.ToArray()));

                    }
                    else if (limit_group_title.Length > 0)
                        lstReturn.Add(new CPluginVariable(group_order + group_name + "|" + limit_group_title, "enum.SH(...|" + String.Join("|", Enum.GetNames(typeof(ShowHide))) + ")", "..."));
                    else if (limit_group_visible)
                        lstReturn.Add(new CPluginVariable(group_order + group_name + "|" + var_name, var_type, Uri.EscapeDataString(var_value)));

                }



            }
            catch (Exception e)
            {
                DumpException(e);
            }

            return lstReturn;

        }


        public string[] EnumValues(List<String> names)
        {
            names.Sort(delegate(String left, String right)
            {


                if (left.Equals("None"))
                    return -1;
                else if (right.Equals("None"))
                    return 1;

                return left.CompareTo(right);

            });

            return names.ToArray();
        }

        public string[] EnumValues(Type enum_type, bool length_check)
        {
            List<String> names = new List<string>(Enum.GetNames(enum_type));

            names.Sort(delegate(String left, String right)
            {
                if (!length_check)
                    return left.CompareTo(right);

                if (left.Length == right.Length)
                    return left.CompareTo(right);

                else if (left.Equals("None"))
                    return -1;
                else if (right.Equals("None"))
                    return 1;
                else
                    return left.Length.CompareTo(right.Length);
            });

            return names.ToArray();
        }


        public String getPluginVariableGroup(String name)
        {
            foreach (KeyValuePair<String, List<String>> group_pair in settings_group)
                if (group_pair.Value.Contains(name))
                    return group_pair.Key;

            if (CustomList.isListVar(name))
            {
                String listId = CustomList.extractId(name);

                if (!lists.ContainsKey(listId))
                    return "List # {Unknown}";

                CustomList list = lists[listId];


                String max = getMaxListId();
                String format = "List #{0," + max.Length + "} - " + list.Name + " (" + list.State.ToString() + ")";
                return String.Format(format, listId);
            }
            else if (Limit.isLimitVar(name))
            {
                String limitId = Limit.extractId(name);

                if (!limits.ContainsKey(limitId))
                    return "Limit # {Unknown}";

                Limit limit = limits[limitId];

                String cstate = "Compiled";

                if (limit.evaluator == null)
                    cstate = "Not" + cstate;

                String max = getMaxLimitId();
                String format = "Limit #{0," + max.Length + "} - " + limit.Name + " (" + limit.State.ToString() + ", " + cstate + ")";
                return String.Format(format, limitId);
            }
            return SettingsG;
        }

        public bool Agreement
        {
            get { return getBooleanVarValue("privacy_policy_agreement"); }
        }


        public const String PrivacyPolicyG = "Custom Privacy Policy";
        public const String WhitelistG = "Whitelist";
        public const String MailG = "Custom SMTP";
        public const String LimitManagerG = "Limit Manager";
        public const String ListManagerG = "Lists Manager";
        public const String StorageG = "Custom Storage";
        public const String TwitterG = "Custom Twitter";
        public const String SettingsG = "Settings";
        public const String ProxyG = "Proxy for HTTP Requests";

        public bool shouldSkipGroup(String name)
        {

            if (name.StartsWith(PrivacyPolicyG) && !Agreement)
                return false;

            if (!Agreement)
                return true;

            if (name.StartsWith(WhitelistG) && !getBooleanVarValue("use_white_list"))
                return true;

            if (name.StartsWith(MailG) && !getBooleanVarValue("use_custom_smtp"))
                return true;

            if (name.StartsWith(ListManagerG) && !getBooleanVarValue("use_custom_lists"))
                return true;

            if (name.StartsWith(StorageG) && !getBooleanVarValue("use_custom_storage"))
                return true;

            if (name.StartsWith(TwitterG) && !getBooleanVarValue("use_custom_twitter"))
                return true;
            
            if (name.StartsWith(ProxyG) && !getBooleanVarValue("use_battlelog_proxy"))
                return true;


            if (name.StartsWith(PrivacyPolicyG) && !getBooleanVarValue("use_custom_privacy_policy"))
                return true;



            return false;
        }

        public bool shouldSkipVariable(String name, String group)
        {

            if (name.Equals("privacy_policy_agreement") && Agreement)
                return true;

            if (!Agreement && !group.Equals(PrivacyPolicyG))
                return true;

            if (CustomList.isListVar(name))
            {
                if (!getBooleanVarValue("use_custom_lists"))
                    return true;

                String listId = CustomList.extractId(name);

                if (!lists.ContainsKey(listId))
                    return false;

                return lists[listId].shouldSkipFieldKey(name);
            }
            else if (Limit.isLimitVar(name))
            {
                String limitId = Limit.extractId(name);

                if (!limits.ContainsKey(limitId))
                    return false;

                return limits[limitId].shouldSkipFieldKey(name);
            }


            if (hidden_variables.ContainsKey(name) && hidden_variables[name])
                return hidden_variables[name];

            if (name.Equals("use_slow_weapon_stats") && !getBooleanVarValue("use_direct_fetch")) return true;

            return false;
        }

        public String getGroupOrder(String name)
        {
            Dictionary<int, String> reverse = new Dictionary<int, string>();
            foreach (KeyValuePair<String, int> pair in settings_group_order)
                reverse.Add(pair.Value, pair.Key);

            int offset = 0;
            for (int i = 0; i <= reverse.Count; i++)
                if (!reverse.ContainsKey(i))
                    continue;
                else
                {
                    if (shouldSkipGroup(reverse[i]))
                        continue;
                    offset++;
                    if (name.Equals(reverse[i]))
                        return String.Format("{0,3}", offset.ToString());
                }

            return String.Format("{0,3}", offset.ToString());
        }

        public List<CPluginVariable> GetPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            List<string> vars = getPluginVars(false, false, false, false);
            foreach (string var in vars)
            {
                if (var == "rcon_to_battlelog_codes")
                {
                    rcon2bw_user_var.Clear();
                    foreach (String k in rcon2bw_user.Keys)
                    {
                        rcon2bw_user_var.Add(k + "=" + rcon2bw_user[k]);
                    }
                    lstReturn.Add(new CPluginVariable(var, typeof(string[]), this.rcon2bw_user_var.ToArray()));
                }
                else
                {
                    lstReturn.Add(new CPluginVariable(var, typeof(string), "BASE64:" + Encode(getPluginVarValue(var))));
                }
            }

            return lstReturn;
        }

        public void SetPluginVariable(string var, string val)
        {
            try
            {
                if (var == "rcon_to_battlelog_codes")
                {
                    rcon2bw_user_var = new List<string>(CPluginVariable.DecodeStringArray(val));
                    rcon2bw_user.Clear();
                    foreach (String item in rcon2bw_user_var)
                    {
                        if (String.IsNullOrEmpty(item)) continue;
                        String[] entry = item.Split(new Char[]{'='});
                        if (entry.Length != 2)
                        {
                            DebugWrite("^1^bWARNING:^n^0 rcon_to_battlelog_codes item '" + item + "' is malformed, ignoring", 3);
                            continue;
                        }
                        String key = entry[0].Trim();
                        String code = entry[1].Trim();
                        DebugWrite("Added weapon code[^b" + key + "^n] = ^b" + code + "^n", 3);
                        rcon2bw_user[key] = code;
                    }
                    return;
                }
                String decoded = val;
                bool ui = true;
                if (decoded.StartsWith("BASE64:"))
                {
                    decoded = decoded.Replace("BASE64:", "");
                    decoded = Decode(decoded);
                    ui = false;
                }

                setPluginVarValue(var, decoded, ui);
            }
            catch (Exception e)
            {
                DumpException(e);
            }
        }

        public override void OnPunkbusterPlayerInfo(CPunkbusterInfo cpbiPlayer)
        {
            DebugWrite("Got ^bOnPunkbusterPlayerInfo^n!", 10); // FIXME 8
            if (!plugin_activated)
                return;

            try
            {
                if (cpbiPlayer == null)
                    return;

                processNewPlayer(cpbiPlayer);
            }
            catch (Exception e)
            {
                DumpException(e);
            }

        }

        Dictionary<String, CPunkbusterInfo> new_player_queue = new Dictionary<string, CPunkbusterInfo>();
        public void processNewPlayer(CPunkbusterInfo cpbiPlayer)
        {
            bool notifyFetch = false;
            
            int dblevel = 10;
            
            /* 
            For debugging, only do verbose logging fetch sent the pb player list command
            */
            if (expectedPBCount > 0) {
                dblevel = 8;
                --expectedPBCount;
            }

            DebugWrite("OnPunkbusterPlayerInfo::processNewPlayer locking " + cpbiPlayer.SoldierName, dblevel);
            
            lock (players_mutex)
            {
                if (this.players.ContainsKey(cpbiPlayer.SoldierName))
                    this.players[cpbiPlayer.SoldierName].pbInfo = cpbiPlayer;
                else
                {

                    // add new player to the queue, and wake the stats fetching loop
                    int other = 10;
                    if (!new_player_queue.ContainsKey(cpbiPlayer.SoldierName)) {
                        other = 5;
                    }
                    DebugWrite("OnPunkbusterPlayerInfo::processNewPlayer player ^b" + cpbiPlayer.SoldierName + "^n", other);
                    if (!(new_player_queue.ContainsKey(cpbiPlayer.SoldierName) ||
                          players.ContainsKey(cpbiPlayer.SoldierName) ||
                          new_players_batch.ContainsKey(cpbiPlayer.SoldierName)))
                    {
                        DebugWrite("Queueing ^b" + cpbiPlayer.SoldierName + "^n for stats fetching", 5);
                        new_player_queue.Add(cpbiPlayer.SoldierName, cpbiPlayer);
                        notifyFetch = true;
                    }

                }
            }
            DebugWrite("OnPunkbusterPlayerInfo::processNewPlayer UNLOCKING " + cpbiPlayer.SoldierName, dblevel);
            if (notifyFetch) {
                //DebugWrite("signalling ^bfetch^n thread", 7); // FIXME
                //fetch_handle.Set(); // FIXME: let enforcer wake up fetch
            }
        }


        public void ResetPlayerSprees(BaseEvent type, PlayerInfo player, PlayerInfo killer, PlayerInfo victim, Kill info)
        {

            List<Limit> all = new List<Limit>();

            all.AddRange(getLimitsForEvaluation((Limit.EvaluationType)0xFFF));
            foreach (Limit limit in all)
                switch (limit.Evaluation)
                {
                    case Limit.EvaluationType.OnDeath:
                    case Limit.EvaluationType.OnTeamDeath:
                        if ((type & (BaseEvent.Kill | BaseEvent.TeamKill)) > 0)
                            limit.ResetSpree(killer.Name);
                        break;
                    case Limit.EvaluationType.OnKill:
                    case Limit.EvaluationType.OnTeamKill:
                        if ((type & (BaseEvent.Kill | BaseEvent.TeamKill)) > 0)
                            limit.ResetSpree(victim.Name);
                        break;
                    case Limit.EvaluationType.OnSuicide:
                        if ((type & (BaseEvent.Kill | BaseEvent.TeamKill)) > 0)
                            limit.ResetSpree(killer.Name);
                        break;
                    case Limit.EvaluationType.OnSpawn:
                    case Limit.EvaluationType.OnIntervalPlayers:
                    case Limit.EvaluationType.OnIntervalServer:
                    case Limit.EvaluationType.OnJoin:
                    case Limit.EvaluationType.OnLeave:
                    case Limit.EvaluationType.OnAnyChat:
                    case Limit.EvaluationType.OnRoundOver:
                    case Limit.EvaluationType.OnRoundStart:
                    case Limit.EvaluationType.OnTeamChange:
                        /*
                         case Limit.EvaluationType.OnGlobalChat:
                         case Limit.EvaluationType.OnTeamChat:
                         case Limit.EvaluationType.OnSquadChat:
                         */
                        break;
                    default:
                        ConsoleError("unknown event evaluation ^b" + limit.Evaluation.ToString() + "^n, for " + limit.ShortDisplayName);
                        break;
                }
        }


        public void evaluateLimitsForEvent(BaseEvent type, PlayerInfo player, PlayerInfo killer, PlayerInfo victim, Kill info)
        {
            DebugWrite("+++ Evaluating all ^b" + type + "^n limits ...", 6);

            KillInfo kill = new KillInfo(info, type, GetCategory(info));

            // first reset the sprees if needed
            ResetPlayerSprees(type, player, killer, victim, info);

            List<Limit> all = new List<Limit>();
            switch (type)
            {
                case BaseEvent.Kill:
                    all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnKill));
                    all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnDeath));
                    all.ForEach(delegate(Limit limit) { if (evaluateLimit(limit, killer, kill, victim)) getServerInfo(); });
                    break;
                case BaseEvent.TeamKill:
                    all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnTeamKill));
                    all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnTeamDeath));
                    all.ForEach(delegate(Limit limit) { if (evaluateLimit(limit, killer, kill, victim)) getServerInfo(); });
                    break;
                case BaseEvent.Suicide:
                    all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnSuicide));
                    all.ForEach(delegate(Limit limit) { if (evaluateLimit(limit, victim, kill)) getServerInfo(); });
                    break;
                case BaseEvent.Spawn:
                    all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnSpawn));
                    all.ForEach(delegate(Limit limit) { if (evaluateLimit(limit, player)) getServerInfo(); });
                    break;
                case BaseEvent.GlobalChat:
                    all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnAnyChat));
                    /* all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnGlobalChat)); */
                    all.ForEach(delegate(Limit limit) { if (evaluateLimit(limit, player)) getServerInfo(); });
                    break;
                case BaseEvent.TeamChat:
                    all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnAnyChat));
                    /* all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnTeamChat)); */
                    all.ForEach(delegate(Limit limit) { if (evaluateLimit(limit, player)) getServerInfo(); });
                    break;
                case BaseEvent.SquadChat:
                    all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnAnyChat));
                    /* all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnSquadChat)); */
                    all.ForEach(delegate(Limit limit) { if (evaluateLimit(limit, player)) getServerInfo(); });
                    break;

                case BaseEvent.TeamChange:
                    all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnTeamChange));
                    all.ForEach(delegate(Limit limit) { if (evaluateLimit(limit, player)) { getServerInfo(); getPlayersList(); } });
                    break;

                case BaseEvent.RoundOver:
                    all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnRoundOver));
                    all.ForEach(delegate(Limit limit) { if (evaluateLimit(limit)) getServerInfo(); });
                    break;
                case BaseEvent.RoundStart:
                    all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnRoundStart));
                    all.ForEach(delegate(Limit limit) { if (evaluateLimit(limit)) getServerInfo(); });
                    break;

                case BaseEvent.Leave:
                    all.AddRange(getLimitsForEvaluation(Limit.EvaluationType.OnLeave));
                    all.ForEach(delegate(Limit limit) { evaluateLimit(limit, player); getServerInfo(); });
                    break;
                default:

                    ConsoleError("unknown event " + type.GetType().Name + " ^b" + type.ToString());
                    return;
            }
            
            DebugWrite("+++ Evaluated ^b" + all.Count + "^n limits", 6);
        }


        public void UpdateStats(PlayerInfo killer, PlayerInfo victim, BaseEvent type, Kill info, String weapon)
        {
          try {
            if (serverInfo == null || killer == null || victim == null || info == null)
                return;

            if (info.Headshot)
            {
                // update the player's Headshots
                killer.W[weapon].HeadshotsRound++;

                // update the server's Headshots
                serverInfo.W[weapon].HeadshotsRound++;
            }

            if (type.Equals(BaseEvent.TeamKill))
            {
                // update the player's TeamKills/TeamDeaths
                killer.W[weapon].TeamKillsRound++;
                victim.W[weapon].TeamDeathsRound++;


                //update the server's TeamKills/TeamDeaths
                serverInfo.W[weapon].TeamKillsRound++;
                serverInfo.W[weapon].TeamDeathsRound++;
            }
            else if (type.Equals(BaseEvent.Suicide))
            {
                // update the player's Suicides
                victim.W[weapon].SuicidesRound++;

                //update the server's Suicides
                serverInfo.W[weapon].SuicidesRound++;
            }
            else if (type.Equals(BaseEvent.Kill))
            {
                // update player's Kills/Deaths
                killer.W[weapon].KillsRound++;
                victim.W[weapon].DeathsRound++;

                // update the server's Kills/Deaths
                serverInfo.W[weapon].KillsRound++;
                serverInfo.W[weapon].DeathsRound++;
            }
          } catch (Exception) {}
        }


        public BaseEvent DetermineBaseEvent(Kill info)
        {

            // determine the event type

            if (info.IsSuicide ||
                /* Stupid Conditions For Suicides */
                info.Killer == null ||
                info.Killer.SoldierName == null ||
                info.Killer.SoldierName.Trim().Length == 0 ||
                info.Killer.GUID == null ||
                info.Killer.GUID.Trim().Length == 0
                )
                return BaseEvent.Suicide;
            else if (info.Victim.TeamID == info.Killer.TeamID)
                return BaseEvent.TeamKill;
            else
                return BaseEvent.Kill;
        }


        public static String InGameCommand_Pattern = @"^\s*([@/!\?])\s*";

        public bool IsCommand(String text)
        {
            return Regex.Match(text, InGameCommand_Pattern).Success;
        }

        public bool IsInGameCommand(String text)
        {
            return IsCommand(text);
        }

        public String ExtractCommand(String text)
        {
            return Regex.Replace(text, InGameCommand_Pattern, "").Trim();
        }

        public String ExtractInGameCommand(String text)
        {
            return ExtractCommand(text);
        }

        public String ExtractCommandPrefix(String text)
        {
            Match match = Regex.Match(text, InGameCommand_Pattern, RegexOptions.IgnoreCase);

            if (match.Success)
                return match.Groups[1].Value;

            return String.Empty;
        }

        public bool CheckAccount(String name, out bool canKill, out bool canKick, out bool canBan, out bool canMove, out bool canChangeLevel)
        {
            bool ret = false;
            canKill = false;
            canKick = false;
            canBan = false;
            canMove = false;
            canChangeLevel = false;
            try {
                if (!players.ContainsKey(name)) {
                    DebugWrite("^1WARNING: Unable to CheckAccount for " + name + ": unrecognized name", 4);
                    return false;
                }
                CPrivileges p = this.GetAccountPrivileges(name);
                if (p == null) return false;
                ret = true;
                
                canKill = p.CanKillPlayers;
                canKick = p.CanKickPlayers;
                canBan = (p.CanTemporaryBanPlayers || p.CanPermanentlyBanPlayers);
                canMove = p.CanMovePlayers;
                canChangeLevel = p.CanUseMapFunctions;
            } catch (Exception e) {
                DebugWrite("EXCEPTION: CheckAccount(" + name + "): " + e.Message, 4);
                ret = false;
            }
            return ret;
        }

        public double CheckPlayerIdle(String name) // -1 if unknown, otherwise idle time in seconds
        {
            double ret = -1;
            try
            {
                if (String.IsNullOrEmpty(name)) return -1;

                PlayerInfo pinfo = null;

                lock (players_mutex) {
                    if (players.ContainsKey(name)) {
                        players.TryGetValue(name, out pinfo);
                    }
                }

                if (pinfo == null) return -1;
                /* old 9.12 version
                if (pinfo._idleTime == 0) {
                    ServerCommand("player.idleDuration", name); // Update it
                }
                ret = pinfo._idleTime;
                pinfo._idleTime = 0; // reset after every check
                */
                ret = pinfo._idleTime;
                pinfo._idleTime = 0; // reset on each check
                ServerCommand("player.idleDuration", name); // Update it
            } 
            catch (Exception e)
            {
                DebugWrite(e.Message, 5);
            }
            return ret;
        }

        public bool IsSquadLocked(int teamId, int squadId)
        {
            String key = teamId.ToString() + "/" + squadId;
            return (lockedSquads.Contains(key));
        }

        public String GetSquadLeaderName(int teamId,  int squadId)
        {
            String key = teamId.ToString() + "/" + squadId;
            if  (squadLeaders.ContainsKey(key)) return squadLeaders[key];
            return null;
        }


        public PlayerInfoInterface GetPlayer(String name)
        {
            return GetPlayer(name, true);
        }

        public PlayerInfoInterface GetPlayer(String name, bool fuzzy)
        {
            if (name == null || name.Trim().Length == 0)
                return null;

            if (fuzzy)
            {
                name = BestPlayerMatch(name);

                if (name == null || name.Trim().Length == 0)
                    return null;
            }

            PlayerInfo pinfo = null;
            if (players.TryGetValue(name, out pinfo))
                return pinfo;
            else
                return null;

        }

        public void InGameCommand(String sender, String text)
        {
            try
            {

                if (!IsCommand(text))
                    return;

                String prefix = ExtractCommandPrefix(text);
                String command = ExtractCommand(text);


// IGC begin
                DebugWrite(@"^bOriginal command^n: " +text, 6);

                Match bstatMatch = Regex.Match(command, @"^\s*bstat\s+([^\s]+)\s+([^\s]+)", RegexOptions.IgnoreCase);
                Match rstatMatch = Regex.Match(command, @"^\s*rstat\s+([^\s]+)\s+([^\s]+)", RegexOptions.IgnoreCase);
// IGC end
                Match one1StatMatch = Regex.Match(command, @"^\s*(round|total|(?:online|battlelog|web))\s+(.+)", RegexOptions.IgnoreCase);
                Match one2StatMatch = Regex.Match(command, @"^\s*(my|[^ ]+)(?:\s+(round|total|(?:online|battlelog|web)))?\s+(.+)", RegexOptions.IgnoreCase);


                //same command, two alternatives
                Match list1StatMatch = Regex.Match(command, @"^\s*info(?:\s+(round|total|(?:online|battlelog|web)))?", RegexOptions.IgnoreCase);
                Match list2StatMatch = Regex.Match(command, @"^\s*(?:(round|total|(?:online|battlelog|web))\s+)?info", RegexOptions.IgnoreCase);

                if (list1StatMatch.Success)
                    ListStatCmd(sender, list1StatMatch.Groups[1].Value);
                else if (list2StatMatch.Success)
                    ListStatCmd(sender, list2StatMatch.Groups[1].Value);
// IGC begin
                else if (bstatMatch.Success) {
                    OneStatCmd(sender, "?", bstatMatch.Groups[1].Value, "battlelog", bstatMatch.Groups[2].Value);
                } else if (rstatMatch.Success) {
                    OneStatCmd(sender, "?", rstatMatch.Groups[1].Value, "round", rstatMatch.Groups[2].Value);
                }
// IGC end
                else if (one1StatMatch.Success)
                    OneStatCmd(sender, prefix, String.Empty, one1StatMatch.Groups[1].Value, one1StatMatch.Groups[2].Value);
                else if (one2StatMatch.Success)
                    OneStatCmd(sender, prefix, one2StatMatch.Groups[1].Value, one2StatMatch.Groups[2].Value, one2StatMatch.Groups[3].Value);

            }
            catch (Exception e)
            {
                DumpException(e);
            }

            return;


        }

        public void ListStatCmd(String sender, String scope)
        {
            if (sender == null)
                return;


            if (scope == null || scope.Length == 0 || !(scope.Equals("round") || scope.Equals("total")))
                scope = "web";

            if (!players.ContainsKey(sender))
                return;

            PlayerInfo sinfo = players[sender];

            //use reflection to go through the properties and find all the ones in the current scope
            PropertyInfo[] properties = typeof(PlayerInfo).GetProperties();

            List<String> props = new List<string>();
            foreach (PropertyInfo property in properties)
            {
                object[] attributes = property.GetCustomAttributes(true);

                if (attributes.Length == 0 || attributes[0] == null || !attributes[0].GetType().Equals(typeof(A)))
                    continue;

                A attrs = (A)attributes[0];

                String pscope = attrs.Scope;
                String pname = attrs.Name;

                if (scope.Equals(pscope) /*&& !pname.Contains(" ")*/)
                    props.Add(pname.ToLower());
            }

            String fscope = scope.Replace("web", "battlelog");
            //format the scope name, with first letter as upper-case
            fscope = fscope.Substring(0, 1).ToUpper() + fscope.Substring(1);

            String message = fscope + " Info " + String.Join(", ", props.ToArray());

            List<String> lines = splitMessageText(message, 120);
            //send only one line to not spam the chat
            if (lines.Count > 0)
                SendGlobalMessageV(lines[0]);
        }

        public void OneStatCmd(String sender, String prefix, String player, String scope, String type)
        {
            DebugWrite(@"^bParsed command^n: " +((sender==null)?"(null)":sender)+", "+((player==null)?"(null)":player)+", "+((scope==null)?"(null)":scope)+", "+((type==null)?"(null)":type), 6);

            if (sender == null)
                return;



            if (player == null || player.Length == 0 || player.Trim().Equals("my"))
                player = sender;

            // avoid command collision
            if (Regex.Match(player, @"^\s*(ban|tban|kick|kill|nuke|say|move|fmove|help|rules|grab|maps|setnext|nextlevel|restart)\s*$").Success)
                return;

            if (scope == null || scope.Length == 0 || !(scope.Equals("round") || scope.Equals("total")))
                scope = "web";

            if (!players.ContainsKey(sender))
                return;

            PlayerInfo sinfo = players[sender];

            int edit_distance = 0;



            String new_player = null;
            if ((new_player = bestMatch(player, new List<string>(players.Keys), out edit_distance)) == null)
                return;

            DebugWrite(@"^bFinal command^n: " +sender+", "+new_player+", "+scope+", "+((type==null)?"(null)":type), 6); // IGC

            //Only allow partial matches if the commnand prefix is ?
            if (edit_distance > 0 && !prefix.Equals("?"))
                return;

            if (!players.ContainsKey(new_player))
                return;

            PlayerInfo pinfo = players[new_player];

            //use reflection to go through the properties and find the matching one
            PropertyInfo[] properties = typeof(PlayerInfo).GetProperties();
            List<String> scopes = new List<string>();

            // this is the order in which scopes are scanned, in case the stat is not found in the give scope
            scopes.Add(scope);
            scopes.Add("web");
            scopes.Add("round");
            scopes.Add("total");

            foreach (String cscope in scopes)
            {
                foreach (PropertyInfo property in properties)
                {
                    object[] attributes = property.GetCustomAttributes(true);

                    if (attributes.Length == 0 || attributes[0] == null || !attributes[0].GetType().Equals(typeof(A)))
                        continue;

                    A attrs = (A)attributes[0];

                    String pscope = attrs.Scope;
                    String pname = attrs.Name;
                    Regex pattern = new Regex("^" + attrs.Pattern + "$", RegexOptions.IgnoreCase);

                    if (cscope.Equals(pscope) && pattern.Match(type).Success)
                    {
                        String fscope = pscope.Replace("web", "battlelog");
                        //format the scope name, with first letter as upper-case
                        fscope = fscope.Substring(0, 1).ToUpper() + fscope.Substring(1);
                        double value = 0;


                        if (!Double.TryParse(property.GetValue((object)pinfo, null).ToString(), out value))
                            return;

                        //make the formatted value
                        value = Math.Round(value, 2);

                        String fvalue = String.Empty;
                        if (Regex.Match(pname, "time", RegexOptions.IgnoreCase).Success)
                        {
                            TimeSpan span = TimeSpan.FromSeconds(value);
                            long thours = (long)span.TotalHours;
                            long tmins = (long)span.TotalMinutes;
                            long hours = (long)span.Hours;
                            long mins = (long)span.Minutes;
                            long tsecs = (long)span.TotalSeconds;


                            if (thours > 0)
                            {
                                fvalue = thours + " hr" + ((thours > 1) ? "s" : "");
                                if (mins > 0)
                                    fvalue += ", " + mins + " min" + ((mins > 1) ? "s" : "");
                            }
                            else if (tmins > 0)
                                fvalue = tmins + " min" + ((tmins > 1) ? "s" : "");
                            else
                                fvalue = tsecs + " sec" + ((tsecs > 1) ? "s" : "");
                        }
                        else if (Regex.Match(pname, "percent", RegexOptions.IgnoreCase).Success)
                            fvalue = value + "%";
                        else
                            fvalue = value.ToString();

                        SendGlobalMessageV(pinfo.FullName + "'s " + fscope + " " + pname + " is " + fvalue);
                        return;
                    }
                }
            }
        }

        public override void OnGlobalChat(string sender, string text)
        {
            DebugWrite("Got ^bOnGlobalChat^n!", 8);
            if (!plugin_activated)
                return;

            PlayerInfo player = null;
            if (!players.ContainsKey(sender))
                return;
            player = players[sender];

            player.LastChat = text;

            evaluateLimitsForEvent(BaseEvent.GlobalChat, player, null, null, null);

            InGameCommand(sender, text);
        }


        public override void OnTeamChat(string sender, string text, int TeamID)
        {
            DebugWrite("Got ^bOnTeamChat^n!", 8);
            if (!plugin_activated)
                return;

            PlayerInfo player = null;
            if (!players.ContainsKey(sender))
                return;
            player = players[sender];

            player.LastChat = text;

            evaluateLimitsForEvent(BaseEvent.TeamChat, player, null, null, null);

            InGameCommand(sender, text);

        }


        public override void OnSquadChat(string sender, string text, int TeamID, int SquadID)
        {
            DebugWrite("Got ^bOnSquadChat^n!", 8);
            if (!plugin_activated)
                return;

            PlayerInfo player = null;
            if (!players.ContainsKey(sender))
                return;
            player = players[sender];

            player.LastChat = text;

            evaluateLimitsForEvent(BaseEvent.SquadChat, player, null, null, null);

            InGameCommand(sender, text);
        }


        public override void OnPlayerSpawned(String name, Inventory inventory)
        {
            DebugWrite("Got ^bOnPlayerSpawned^n!", 8);
            if (!plugin_activated)
                return;
            
            /*
            Sometimes players can spawn after the OnRoundOver event,
            so to prevent spurious round start detections, wait
            until level_loaded is true.
            */
            if (round_over && !level_loaded) {
                DebugWrite("Skipping player spawn, level is not loaded yet!", 8);
                return;
            }

            /* 
            To avoid a situation where OnLevelLoaded executed multiple
            times in a row causes multiple OnRoundOver evals, delay
            reset of isRoundReset flag until first spawn or first
            interval limit evals (see enforcer thread).
            */
            if (isRoundReset) {
                DebugWrite("The round NEEDS resetting!", 8);
                isRoundReset = false;
            }

            //first player to spawn after round over, we fetch the map info again
            if (round_over == true)
            {
                DebugWrite("Marking round as in progress", 8);
                round_over = false;
                //round over, fetch map info again
                DebugWrite(":::::::::::: Round start detected ::::::::::::", 4);
                DebugWrite("async map info update", 8);
                getMapInfo();
                DebugWrite("update vars", 8);
                updateVars();
                evaluateLimitsForEvent(BaseEvent.RoundStart, null, null, null, null);
            }



            PlayerInfo player = null;
            if (!players.ContainsKey(name))
                return;
            player = players[name];

            evaluateLimitsForEvent(BaseEvent.Spawn, player, null, null, null);
        }


        public override void OnPlayerJoin(string name)
        {
            DebugWrite("Got ^bOnPlayerJoin^n! " + name, 8);
            if (!plugin_activated)
                return;
        }

        public void OnPlayerJoin(PlayerInfo player)
        {
            if (player == null)
                return;

            String name = player.Name;

            try
            {
                List<Limit> sorted_limits = getLimitsForEvaluation(Limit.EvaluationType.OnJoin);

                if (sorted_limits.Count == 0)
                {
                    DebugWrite("No valid ^bOnJoin^n or limits founds, skipping this iteration", 8);
                    return;
                }

                // refresh the map information once before all limit evals
                getMapInfoSync();

                for (int i = 0; i < sorted_limits.Count; i++)
                {
                    if (!plugin_enabled)
                        break;

                    Limit limit = sorted_limits[i];

                    if (limit == null || !limit.Evaluation.Equals(Limit.EvaluationType.OnJoin))
                        continue;

                    DebugWrite("Evaluating " + limit.ShortDisplayName + " - " + limit.Evaluation.ToString() + ", for " + name, 6); 
                    evaluateLimit(limit, player);
                }
            }
            catch (Exception e)
            {
                DumpException(e);
            }
        }

        public override void OnPlayerLeft(CPlayerInfo pinfo)
        {
            DebugWrite("Got ^bOnPlayerLeft^n! " + pinfo.SoldierName, 8);
            if (!plugin_activated)
                return;

            if (pinfo == null)
                return;

            String name = pinfo.SoldierName;

            PlayerInfo player = null;
            if (!players.ContainsKey(name))
                return;
            player = players[name];

            evaluateLimitsForEvent(BaseEvent.Leave, player, null, null, null);
        }

        public List<String> RecentMove = new List<String>();
        private Queue<String> moveQ = new Queue<String>();

        public override void OnPlayerMovedByAdmin(string name, int TeamId, int SquadId, bool force)
        {
            DebugWrite("Got ^bOnPlayerMovedByAdmin^n!", 8);
            if (!plugin_activated) return;

            //if player has been moved by admin, remember
            DebugWrite("ADMIN MOVE! Suppress limit eval of move for ^b" + name, 5);
            lock (moves_mutex)
            {
                if (!RecentMove.Contains(name)) {
                    RecentMove.Add(name);
                }
            }
            pending_handle.Set(); // signal queue
            Thread.Sleep(2); // give thread some time
        }

        public void move_thread_loop()
        {
            Thread.CurrentThread.Name = "move";

            DebugWrite("starting", 4);
            
            DateTime since = DateTime.Now;

            /*
            As of this writing, move by admin events FOLLOW the team change event.
            Since we want to suppress move by admin team changes, we need to delay
            processing of a move event until either the next move event happens,
            until the admin event happens, or after time runs out.

            It would be better to avoid the time-out and instead trigger on whatever
            the next event is, but that means putting a signaller into every
            event handler callback.
            */
            while (plugin_enabled) {
                try {
                    int n = 0;
                    
                    lock (moves_mutex) {
                        n = moveQ.Count;
                    }

                    while (n == 0) {
                        DebugWrite("waiting for move event ...", 8);
                        move_handle.WaitOne(); // wait for something in the queue
                        move_handle.Reset();
                        pending_handle.Reset();
                        if (!plugin_enabled) {
                            DebugWrite("detected that plugin was disabled, aborting", 4);
                            return;
                        }
                        lock (moves_mutex) {
                            n = moveQ.Count;
                        }
                        DebugWrite("awake! " + n + " events in queue", 8);
                    }
                    since = DateTime.Now;
                    
                    String name = null;
 
                    lock (moves_mutex) {
                        name = moveQ.Dequeue();
                    }

                    if (name == null) continue;
                    
                    lock (players_mutex) {
                        if (!players.ContainsKey(name)) {
                            name = null;
                        }
                    }
 
                    if (name == null) {
                        DebugWrite("looks like ^b" + name + "^n, left, skipping.", 8);
                        continue;
                    }
                    
                    DebugWrite("Waiting to see if move for ^b" + name + "^n was an admin move ...", 8);
                    
                    /* Add a delay to see if an admin move event comes to suppress this */

                    while (DateTime.Now.Subtract(since).TotalSeconds < 1.0) {
                        pending_handle.WaitOne(200); // 1/5th second
                        lock (moves_mutex) {
                            if (RecentMove.Contains(name)) {
                                RecentMove.Remove(name);
                                name = null;
                                break;
                            }
                        }
                    }
                    pending_handle.Reset();
                    
                    if (name == null) {
                        DebugWrite("Move was by admin, skipping ...", 8);
                        continue;
                    } else {
                        DebugWrite("Delay expired, assuming move is legit", 8);
                    }

                    /* Redo the test, player might have left */
                    
                    lock (players_mutex) {
                        if (!players.ContainsKey(name)) name = null;
                    }
 
                    if (name == null) {
                        DebugWrite("looks like ^b" + name + "^n, left, skipping.", 8);
                        continue;
                    }

                    PlayerInfo pinfo = null;

                    lock (players_mutex) {
                        players.TryGetValue(name, out pinfo);
                    }

                    if (pinfo == null) {
                        DebugWrite("player ^b" + name + "^n isn't known yet, skipping.", 8);
                        continue;
                    }

                    DebugWrite("evaluating TeamChange limits for ^b" + name, 8);
                    evaluateLimitsForEvent(BaseEvent.TeamChange, pinfo, null, null, null);

                } catch (Exception e) {
                    if (typeof(ThreadAbortException).Equals(e.GetType()))
                    {
                        Thread.ResetAbort();
                        return;
                    }
                    DumpException(e);
                }
            }
            DebugWrite("detected that plugin was disabled, aborting", 4);
        }

        public override void OnPlayerTeamChange(string name, int TeamId, int SquadId)
        {
            DebugWrite("Got ^bOnPlayerTeamChange^n!", 8);
            if (!plugin_activated) return;

            try
            {
                pending_handle.Set(); // Process any previous moves that are pending
                Thread.Sleep(2); // Give thread some time
                
                if (TeamId <= 0)
                    return;

                PlayerInfo pinfo = null;

                lock (players_mutex) {
                    if (players.ContainsKey(name)) {
                        players.TryGetValue(name, out pinfo);
                    }
                }

                if (pinfo == null) return;

                /* nothing to do, usually happens during join */
                if (pinfo.TeamId == 0 || pinfo.TeamId == TeamId) return;
                
                /* if between rounds before first spawn, ignore */
                if (round_over) {
                    DebugWrite("Ignoring move between rounds before round start", 8);
                    return;
                }

                /* Add to queue */
                DebugWrite("Queing move of ^b" + name, 5);
                lock (moves_mutex) {
                    moveQ.Enqueue(name);
                }
                
                move_handle.Set(); // Process this move event
            }
            catch (Exception e)
            {
                DumpException(e);
            }
        }

        public override void OnPlayerKilled(Kill info)
        {
            DebugWrite("Got ^bOnPlayerKilled^n!", 8);
            if (!plugin_activated)
                return;

            try
            {
                //get the killer and victim information

                BaseEvent type = DetermineBaseEvent(info);

                CPlayerInfo killer = info.Killer;
                CPlayerInfo victim = info.Victim;

                PlayerInfo vpinfo = null;
                PlayerInfo kpinfo = null;

                players.TryGetValue(victim.SoldierName, out vpinfo);
                players.TryGetValue(killer.SoldierName, out kpinfo);



                // ignore event, no web stats available
                if ((type.Equals(BaseEvent.Suicide) && vpinfo == null))
                    return;

                else if ((type.Equals(BaseEvent.Kill) || type.Equals(BaseEvent.TeamKill)) &&
                        (kpinfo == null || vpinfo == null))
                    return;

                // ignore event if server info is not yet available
                if (serverInfo == null)
                    return;


                UpdateStats(kpinfo, vpinfo, type, info, ":" + info.DamageType);

                evaluateLimitsForEvent(type, null, kpinfo, vpinfo, info);


            }
            catch (Exception e)
            {
                DumpException(e);
            }
        }

        public String getServerNameSync()
        {

            server_name_handle.Reset();
            getServerName();
            Thread.Sleep(500);
            WaitOn("server_name_handle", server_name_handle);
            server_name_handle.Reset();

            return this.server_name;
        }

        public void WaitOn(String name, EventWaitHandle handle)
        {
            int timeout = getIntegerVarValue("wait_timeout");


            DebugWrite("waiting, timeout after " + timeout + " seconds", 7);
            if (handle.WaitOne(timeout * 1000) == false)
            {
                StackTrace stack = new StackTrace();
                String caller = stack.GetFrame(1).GetMethod().Name;
                DebugWrite("^1^bWARNING^n^0: Timeout(" + timeout + " seconds), waiting for " + name + " in " + caller + ". Your net connection to your game server may be congested or another plugin may be lagging Procon.", 4);
            } else {
                DebugWrite("awake! no timeout", 7);
            }
        }

        public String getServerDescriptionSync()
        {
            server_desc_handle.Reset();
            getServerDescription();
            Thread.Sleep(500);
            WaitOn("server_desc_handle", server_desc_handle);
            server_desc_handle.Reset();

            return this.server_desc;
        }

        public void getServerName()
        {
            this.ServerCommand("vars.serverName");
        }

        public void getServerDescription()
        {
            this.ServerCommand("vars.serverDescription");
        }

        public override void OnServerName(string name)
        {
            DebugWrite("Got ^bOnServerName^n!", 8);
            if (!plugin_activated)
                return;

            this.server_name = name;
            this.server_name_handle.Set();
        }

        public override void OnServerDescription(string desc)
        {
            DebugWrite("Got ^bOnServerDescription^n!", 8);
            if (!plugin_activated)
                return;

            this.server_desc = desc;
            this.server_desc_handle.Set();
        }

        public override void OnServerInfo(CServerInfo data)
        {
            DebugWrite("Got ^bOnServerInfo^n!", 8);
            if (!plugin_activated)
                return;

            if (this.serverInfo == null)
                this.serverInfo = new ServerInfo(this, data, this.mapList, new int[] { this.curMapIndex, this.nextMapIndex });
            else
                this.serverInfo.updateData(data);

            info_handle.Set();
        }

        /* Always request the map information, whenever there is map-related event */
        public override void OnMaplistLoad() { getMapInfo(); }
        public override void OnMaplistSave() { getMapInfo(); }
        public override void OnMaplistCleared() { getMapInfo(); }
        public override void OnMaplistMapAppended(string mapFileName) { getMapInfo(); }
        public override void OnMaplistNextLevelIndex(int mapIndex) { getMapInfo(); }
        public override void OnMaplistMapRemoved(int mapIndex) { getMapInfo(); }
        public override void OnMaplistMapInserted(int mapIndex, string mapFileName) { getMapInfo(); }
        public override void OnRestartLevel()
        {
            DebugWrite("Got ^bOnRestartLevel^n!", 8);
        }
        public override void OnEndRound(int winTeamId)
        {
            DebugWrite("Got ^bOnEndRound^n!", 8);
            getMapInfo();
        }
        public override void OnRunNextLevel()
        {
            DebugWrite("Got ^bOnRunNextLevel^n!", 8);
            getMapInfo();
        }
        public override void OnCurrentLevel(string mapFileName) { getMapInfo(); }
        public override void OnLoadingLevel(string mapFileName, int roundsPlayed, int roundsTotal) { getMapInfo(); }
        public override void OnLevelStarted()
        { 
            DebugWrite("Got ^bOnLevelStarted^n!", 8);
            getMapInfo();
        }
        public override void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal) {
            DebugWrite("Got ^bOnLevelLoaded^n!", 8);
            level_loaded = true;
            getMapInfo();
            
            lock (moves_mutex) {
                RecentMove.Clear();
            }

            if (!round_over) {
                DebugWrite("^bRound was aborted, eval OnRoundOver limits", 4);
                evaluateLimitsForEvent(BaseEvent.RoundOver, null, null, null, null);
                DebugWrite(":::::::::::: Marking round as over (level loaded) :::::::::::: ", 4);
                round_over = true;
                if (!isRoundReset) {
                    // Do all of the essential stuff that would happen at normal round end
                    DebugWrite("^bDo reset OnLevelLoaded also", 4);
                    RoundOverReset(); // sets flag to true
                }
            }
        }

        public override void OnMaplistList(List<MaplistEntry> lstMaplist)
        {
            DebugWrite("Got ^bOnMaplistList^n!", 8);
            if (!plugin_activated)
                return;

            this.mapList = lstMaplist;

            if (this.serverInfo != null)
                this.serverInfo.updateMapList(lstMaplist);

            this.list_handle.Set();

        }

        public override void OnRoundOverPlayers(List<CPlayerInfo> players)
        {
            DebugWrite("Got ^bOnRoundOverPlayers^n!", 8);
            if (!plugin_activated)
                return;

            updateQueues(players);
            SyncPlayersList(players);
        }

        public override void OnRoundOverTeamScores(List<TeamScore> teamScores)
        {
            DebugWrite("Got ^bOnRoundOverTeamScores^n!", 8);
            if (!plugin_activated)
                return;

            if (serverInfo == null)
                return;

            serverInfo.updateTickets(teamScores);
            evaluateLimitsForEvent(BaseEvent.RoundOver, null, null, null, null);
            serverInfo.updateTickets(null);
            DebugWrite("::::::::::::  Marking round as over (round over team scores)! :::::::::::: ", 4);
            round_over = true;

            RoundOverReset();
        }

        public override void OnRoundOver(int winTeamId)
        {
            DebugWrite("Got ^bOnRoundOver^n!", 8);
            if (!plugin_activated)
                return;
            level_loaded = false;

            if (serverInfo != null)
                serverInfo.WinTeamId = winTeamId;


        }

        public void RoundOverReset()
        {
            DebugWrite("RoundOverReset called!", 8);
            // reset the activations, and sprees, and round-data for all limits
            List<String> keys = new List<string>(limits.Keys);
            foreach (String key in keys)
                if (limits.ContainsKey(key))
                {
                    limits[key].AccumulateActivations();
                    limits[key].ResetActivations();
                    limits[key].ResetSprees();
                    limits[key].RoundData.Clear();
                }

            // accumlate the round stats for server
            if (serverInfo != null)
            {
                serverInfo.AccumulateRoundStats();
                serverInfo.ResetRoundStats();
                serverInfo.RoundData.Clear();

                //Reset the total limit activations every 10 rounds, so that memory does not grow infinitely
                foreach (KeyValuePair<String, Limit> pair in limits)
                    if (pair.Value != null && (serverInfo.RoundsTotal % 10) == 0)
                        pair.Value.ResetActivationsTotal();
            }

            // accumulate the round stats for players
            foreach (KeyValuePair<String, PlayerInfo> pair in players)
                if (pair.Value != null)
                {
                    pair.Value.AccumulateRoundStats();
                    pair.Value.ResetRoundStats();
                    pair.Value.RoundData.Clear();
                }

            this.RoundData.Clear();
            lockedSquads.Clear();
            squadLeaders.Clear();

            DebugWrite("Round HAS BEEN reset!", 8);
            isRoundReset = true;
        }

        public void getMapList()
        {
            ServerCommand("mapList.list");
        }

        public List<MaplistEntry> getMapListSync()
        {
            list_handle.Reset();
            getMapList();
            Thread.Sleep(500);
            WaitOn("list_handle", list_handle);
            list_handle.Reset();

            return mapList;
        }

        public override void OnMaplistGetMapIndices(int mapIndex, int nextIndex)
        {
            DebugWrite("Got ^bOnMaplistGetMapIndices^n!", 8);
            if (!plugin_activated)
                return;

            this.curMapIndex = mapIndex;
            this.nextMapIndex = nextIndex;

            if (this.serverInfo != null)
                this.serverInfo.updateIndices(new int[] { this.curMapIndex, this.nextMapIndex });

            this.indices_handle.Set();
        }

        public void getMapIndices()
        {
            ServerCommand("mapList.getMapIndices");
        }

        public void getModeCounters()
        {
            ServerCommand("vars.gameModeCounter");
            if (game_version == "BF3")
                ServerCommand("vars.ctfRoundTimeModifier");
        }

        public int[] getMapIndicesSync()
        {
            indices_handle.Reset();
            getMapIndices();
            Thread.Sleep(500);
            WaitOn("indices_handle", indices_handle);
            indices_handle.Reset();

            return new int[] { curMapIndex, nextMapIndex };
        }

        public void getMapInfoSync()
        {
            getModeCounters();
            DebugWrite("waiting for map-list before proceeding", 8);
            getMapListSync();
            DebugWrite("waiting for map-indices before proceeding", 8);
            getMapIndicesSync();
            DebugWrite("waiting for server-info before proceeding", 8);
            getServerInfoSync();
        }

        public void getMapInfo()
        {
            if (!plugin_activated)
                return;

            getModeCounters();
            getMapList();
            getMapIndices();
            getServerInfo();
            getModeCounters();
        }

        public void updateVars()
        {
            if (!plugin_activated) return;

            if (game_version == "BF3")
            {
                ServerCommand("vars.bulletDamage");
                ServerCommand("vars.friendlyFire");
                ServerCommand("vars.gunMasterWeaponsPreset");
                ServerCommand("vars.idleTimeout");
                ServerCommand("vars.soldierHealth");
                ServerCommand("vars.vehicleSpawnAllowed");
                ServerCommand("vars.vehicleSpawnDelay");
            }
            else
            {
                ServerCommand("vars.bulletDamage");
                ServerCommand("vars.friendlyFire");
                ServerCommand("vars.idleTimeout");
                ServerCommand("vars.soldierHealth");
                ServerCommand("vars.vehicleSpawnAllowed");
                ServerCommand("vars.vehicleSpawnDelay");
                if (game_version == "BF4")
                    ServerCommand("vars.commander");
                else if (game_version == "BFHL")
                    ServerCommand("vars.hacker");
                ServerCommand("vars.serverType");
                ServerCommand("vars.maxSpectators");
                if (game_version == "BF4")
                    ServerCommand("vars.teamFactionOverride");
            }

            resetUpdateTimer(WhichTimer.Vars);
        }

        public override void OnReservedSlotsList(List<String> lstSoldierNames)
        {
            DebugWrite("Got ^bOnReservedSlotsList^n!", 8);
            reserved_slots_list = lstSoldierNames;
        }



        public bool stringValidator(string var, string value)
        {
            try
            {
                if (var.Equals("console"))
                    PluginCommand(null, value);
                else if (var.Equals("new_limit") && value.Equals(TrueFalse.True.ToString()))
                    createNewLimit();
                else if (var.Equals("new_list") && value.Equals(TrueFalse.True.ToString()))
                    createNewList();
                else if (var.Equals("twitter_verifier_pin") && !value.Equals(default_PIN_message))
                    VerifyTwitterPin(value);
                else if (var.Equals("compile_limit") && value.Equals(LimitChoice.NotCompiled.ToString()))
                    CompileAll();
                else if (var.Equals("compile_limit") && value.Equals(LimitChoice.All.ToString()))
                    CompileAll(true);

            }
            catch (Exception e)
            {
                DumpException(e);
            }

            return false;
        }






        private void PluginCommand(string sender, string cmd)
        {

            try
            {
                //operations

                Match dumpLimitMatch = Regex.Match(cmd, @"\s*[!@/]\s*dump\s+limit\s+(\d+)\s*", RegexOptions.IgnoreCase);
                Match playerStatsMatch = Regex.Match(cmd, @"\s*[!@/]\s*(?:(weapon)\s+)?(web|round|total)\s+stats\s+(.+)\s*", RegexOptions.IgnoreCase);

                Match serverStatsMatch = Regex.Match(cmd, @"\s*[!@/]\s*(?:(weapon)\s+)?(round|total|map)\s+stats\s*", RegexOptions.IgnoreCase);


                //Setting/Getting keys
                Match setVarValueMatch = Regex.Match(cmd, @"\s*[!@/]\s*set\s+([^ ]+)\s+(.+)", RegexOptions.IgnoreCase);
                Match setVarValueEqMatch = Regex.Match(cmd, @"\s*[!@/]\s*set\s+([^ ]+)\s*=\s*(.+)", RegexOptions.IgnoreCase);
                Match setVarValueToMatch = Regex.Match(cmd, @"\s*[!@/]\s*set\s+([^ ]+)\s+to\s+(.+)", RegexOptions.IgnoreCase);
                Match setVarTrueMatch = Regex.Match(cmd, @"\s*[!@/]\s*set\s+([^ ]+)", RegexOptions.IgnoreCase);
                Match getVarValueMatch = Regex.Match(cmd, @"\s*[!@/]\s*get\s+([^ ]+)", RegexOptions.IgnoreCase);
                Match enableMatch = Regex.Match(cmd, @"\s*[!@/]\s*enable\s+(.+)", RegexOptions.IgnoreCase);
                Match disableMatch = Regex.Match(cmd, @"\s*[!@/]\s*disable\s+(.+)", RegexOptions.IgnoreCase);

                //Information
                Match pluginSettingsMatch = Regex.Match(cmd, @"\s*[!@/]\s*settings", RegexOptions.IgnoreCase);


                bool senderIsAdmin = true;

                if (playerStatsMatch.Success && senderIsAdmin)
                    playerStatsDumpCmd(sender, playerStatsMatch.Groups[1].Value, playerStatsMatch.Groups[2].Value, playerStatsMatch.Groups[3].Value);
                else if (serverStatsMatch.Success)
                    serverStatsDumpCmd(sender, serverStatsMatch.Groups[1].Value, serverStatsMatch.Groups[2].Value);
                else if (dumpLimitMatch.Success && senderIsAdmin)
                    dumpLimitCmd(sender, dumpLimitMatch.Groups[1].Value);
                else if (setVarValueEqMatch.Success && senderIsAdmin)
                    setVariableCmd(sender, setVarValueEqMatch.Groups[1].Value, setVarValueEqMatch.Groups[2].Value);
                else if (setVarValueToMatch.Success && senderIsAdmin)
                    setVariableCmd(sender, setVarValueToMatch.Groups[1].Value, setVarValueToMatch.Groups[2].Value);
                else if (setVarValueMatch.Success && senderIsAdmin)
                    setVariableCmd(sender, setVarValueMatch.Groups[1].Value, setVarValueMatch.Groups[2].Value);
                else if (setVarTrueMatch.Success && senderIsAdmin)
                    setVariableCmd(sender, setVarTrueMatch.Groups[1].Value, "1");
                else if (getVarValueMatch.Success && senderIsAdmin)
                    getVariableCmd(sender, getVarValueMatch.Groups[1].Value);
                else if (enableMatch.Success && senderIsAdmin)
                    enableVarGroupCmd(sender, enableMatch.Groups[1].Value);
                else if (disableMatch.Success && senderIsAdmin)
                    disableVarGroupCmd(sender, disableMatch.Groups[1].Value);
            }
            catch (Exception e)
            {
                DumpException(e);
            }
        }


        // modified algorithm to ignore insertions, and case
        public static int LevenshteinDistance(string s, string t)
        {
            s = s.ToLower();
            t = t.ToLower();

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
                return m;

            if (m == 0)
                return n;

            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;



            for (int i = 1; i <= n; i++)
                for (int j = 1; j <= m; j++)
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 0), d[i - 1, j - 1] + ((t[j - 1] == s[i - 1]) ? 0 : 1));

            return d[n, m];
        }

        public string bestMatch(String name, List<String> names, out int best_distance)
        {
            return bestMatch(name, names, out best_distance, true);
        }

        //find the best match for name in names
        public string bestMatch(String name, List<String> names, out int best_distance, bool fuzzy)
        {

            best_distance = int.MaxValue;


            try
            {

                //do the obvious check first
                if (names.Contains(name))
                {
                    best_distance = 0;
                    return name;
                }

                //name is not in the list, find the best match
                String best_match = null;

                // first try to see if any of the names contains target name as substring, so we can reduce the search
                Dictionary<String, String> sub_names = new Dictionary<string, string>();

                String name_lower = name.ToLower();

                for (int i = 0; i < names.Count; i++)
                {
                    String cname = names[i].ToLower();
                    if (cname.Equals(name_lower))
                        return names[i];
                    else if (cname.Contains(name_lower) && !sub_names.ContainsKey(cname))
                        sub_names.Add(cname, names[i]);
                }

                if (sub_names.Count > 0)
                    names = new List<string>(sub_names.Keys);

                if (sub_names.Count == 1)
                {
                    // we can optimize, and exit early
                    best_match = sub_names[names[0]];
                    best_distance = Math.Abs(best_match.Length - name.Length);
                    return best_match;
                }

                // if we are not doing a fuzzy search, and we have not found more than one sub-string, we can exit here
                if (!fuzzy && sub_names.Count == 0)
                    return null;

                // find the best/fuzzy match using modified Leveshtein algorithm
                foreach (String cname in names)
                {
                    int distance = LevenshteinDistance(name, cname);
                    if (distance < best_distance)
                    {
                        best_distance = distance;
                        best_match = cname;
                    }
                }

                if (best_match == null)
                    return null;

                best_distance += Math.Abs(name.Length - best_match.Length);

                // if we searched through sub-names, get the actual match
                if (sub_names.Count > 0 && sub_names.ContainsKey(best_match))
                    best_match = sub_names[best_match];

                return best_match;
            }
            catch (Exception e)
            {
                DumpException(e);
            }

            return null;
        }

        public String BestPlayerMatch(String name)
        {
            if (name == null || name.Trim().Length == 0)
                return null;

            return bestPlayerMatch(null, name, true, true);
        }

        public string bestPlayerMatch(String sender, String player, bool fuzzy, bool quiet)
        {

            try
            {
                if (player == null)
                    return null;
                if (players.ContainsKey(player))
                    return player;

                int edit_distance = 0;
                String new_player = null;
                if ((new_player = bestMatch(player, new List<string>(players.Keys), out edit_distance, fuzzy)) == null)
                {
                    if (!quiet)
                        SendConsoleWarning(sender, "could not find ^b" + player + "^n");
                    return null;
                }
                if (!quiet)
                    SendConsoleWarning(sender, "could not find ^b" + player + "^n, but found ^b" + new_player + "^n, with edit distance of ^b" + edit_distance + "^n");
                return new_player;
            }
            catch (Exception e)
            {
                DumpException(e);
            }

            return null;
        }

        private void serverStatsDumpCmd(String sender, String level, String scope)
        {
            if (serverInfo == null)
                return;

            if (level == null)
                level = String.Empty;

            if (scope == null)
                scope = String.Empty;

            if (!(scope.Equals("round") || scope.Equals("total") || scope.Equals("map")))
            {
                SendConsoleError(sender, "unknown stats scope ^b" + scope + "^n");
                return;
            }

            if (level.Equals("weapon"))
                serverInfo.dumpWeaponStats(scope);
            else
                serverInfo.dumpStatProperties(scope);


        }

        private void playerStatsDumpCmd(String sender, String level, String scope, String player)
        {
            if (player == null || player.Trim().Length == 0 || players.Count == 0)
                return;

            player = player.Trim();


            if ((player = bestPlayerMatch(sender, player, true, false)) == null)
                return;

            if (level == null)
                level = String.Empty;

            if (scope == null)
                scope = String.Empty;

            PlayerInfo pinfo = players[player];

            if (!(scope.Equals("round") || scope.Equals("total") || scope.Equals("web")))
            {
                SendConsoleError(sender, "unknown stats scope ^b" + scope + "^n");
                return;
            }

            if (level.Equals("weapon"))
                pinfo.dumpWeaponStats(scope);
            else
                pinfo.dumpStatProperties(scope);

        }

        private void dumpLimitCmd(string sender, string id)
        {
            id = id.Trim();
            if (!limits.ContainsKey(id))
            {
                SendConsoleError(sender, "Limit #^b" + id + "^n  does not exist");
                return;
            }

            Limit limit = limits[id];
            String class_source = buildLimitSource(limit);
            String path = getClassName(limit) + ".cs";
            SendConsoleMessage(sender, "Dumping " + limit.ShortDisplayName + " source to file " + path);
            DumpData(class_source, path);
        }

        private void setVariableCmd(string sender, string var, string val)
        {
            if (setPluginVarValue(sender, var, val))
                SendConsoleMessage(sender, var + " set to \"" + val + "\"");
        }

        private void getVariableCmd(string sender, string var)
        {
            string val = getPluginVarValue(sender, var);

            SendConsoleMessage(sender, var + " = " + val);

        }

        private void enableVarGroupCmd(string sender, string group)
        {
            if (group.CompareTo("plugin") == 0)
            {
                ConsoleWrite("Disabling plugin");
                this.ExecuteCommand("procon.plugin.enable", "InsaneLimits", "false");
                return;
            }
            enablePluginVarGroup(sender, group);
        }

        private void disableVarGroupCmd(string sender, string group)
        {
            if (group.CompareTo("plugin") == 0)
            {
                ConsoleWrite("Enabling plugin");
                this.ExecuteCommand("procon.plugin.enable", "InsaneLimits", "true");
                return;
            }

            disablePluginVarGroup(sender, group);
        }

        private bool enablePluginVarGroup(string sender, string group)
        {
            // search for all variable matching
            List<string> vars = getVariableNames(group);
            if (vars.Count == 0)
            {
                SendConsoleError(sender, "no variables match \"" + group + "\"");
                return false;
            }

            return setPluginVarGroup(sender, String.Join(",", vars.ToArray()), "true");
        }

        private bool disablePluginVarGroup(string sender, string group)
        {
            //search for all variables matching
            List<string> vars = getVariableNames(group);

            if (vars.Count == 0)
            {
                SendConsoleError(sender, "no variables match \"" + group + "\"");
                return false;
            }
            return setPluginVarGroup(sender, String.Join(",", vars.ToArray()), "false");
        }


        private bool setPluginVarGroup(string sender, string group, string val)
        {

            if (group == null)
            {
                SendConsoleError(sender, "no variable to enable");
                return false;
            }

            group = group.Replace(";", ",");
            List<string> vars = new List<string>(Regex.Split(group, @"\s*,\s*", RegexOptions.IgnoreCase));
            foreach (string var in vars)
                if (setPluginVarValue(sender, var, val))
                    SendConsoleMessage(sender, var + " set to \"" + val + "\"");

            return true;
        }

        private List<string> getVariableNames(string group)
        {
            List<string> names = new List<string>();
            List<string> list = new List<string>(Regex.Split(group, @"\s*,\s*"));
            List<string> vars = getPluginVars();
            foreach (string search in list)
            {
                foreach (string var in vars)
                {
                    if (var.Contains(search))
                        if (!names.Contains(var))
                            names.Add(var);
                }
            }

            return names;
        }

        public String StripModifiers(String text)
        {
            return Regex.Replace(text, @"\^[0-9a-zA-Z]", "");
        }

        private void SendConsoleMessage(string name, string msg, MessageType type)
        {

            msg = FormatMessage(msg, type);
            LogWrite(msg);

            // remove font style
            msg = StripModifiers(E(msg));

            if (name != null)
                SendPlayerMessageV(name, msg);
        }

        private void SendConsoleMessage(string name, string msg)
        {
            SendConsoleMessage(name, msg, MessageType.Normal);
        }

        private void SendConsoleError(string name, string msg)
        {
            SendConsoleMessage(name, msg, MessageType.Error);
        }

        private void SendConsoleWarning(string name, string msg)
        {
            SendConsoleMessage(name, msg, MessageType.Warning);
        }

        private void SendConsoleException(string name, string msg)
        {
            SendConsoleMessage(name, msg, MessageType.Exception);
        }

        private void SendPlayerMessageV(string name, string message)
        {
            if (name == null)
                return;

            ServerCommand("admin.say", StripModifiers(E(message)), "player", name);
        }

        private bool SendGlobalMessageV(String message)
        {
            ServerCommand("admin.say", StripModifiers(E(message)), "all");
            return true;
        }

        private bool SendTeamMessageV(int teamId, String message)
        {
            ServerCommand("admin.say", StripModifiers(E(message)), "team", (teamId).ToString());
            return true;
        }

        private bool SendSquadMessageV(int teamId, int squadId, String message)
        {
            ServerCommand("admin.say", StripModifiers(E(message)), "squad", (teamId).ToString(), (squadId).ToString());
            return true;
        }


        private void SendPlayerYellV(string name, string message, int duration)
        {
            if (name == null)
                return;

            ServerCommand("admin.yell", StripModifiers(E(message)), duration.ToString(), "player", name);
        }

        private bool SendGlobalYellV(String message, int duration)
        {
            ServerCommand("admin.yell", StripModifiers(E(message)), duration.ToString(), "all");
            return true;
        }

        private bool SendTeamYellV(int teamId, String message, int duration)
        {
            ServerCommand("admin.yell", StripModifiers(E(message)), duration.ToString(), "team", (teamId).ToString());
            return true;
        }

        //escape replacements
        public String E(String text)
        {
            text = Regex.Replace(text, @"\\n", "\n");
            text = Regex.Replace(text, @"\\t", "\t");
            return text;
        }


        /* Messaging functions (Check for Virtual Mode) */


        public bool SendGlobalMessage(String message)
        {
            return SendGlobalMessage(message, 0);
        }

        public bool SendGlobalMessage(String message, int delay)
        {
            if (VMode)
            {
                ConsoleWarn("not sending global-message, ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            if (delay == 0) {
                SendGlobalMessageV(message);
                return true;
            }

            Thread delayed = new Thread(new ThreadStart(delegate()
            {
                Thread.Sleep(delay * 1000);
                QueueSayMessage(new SayMessage(0, 0, String.Empty, MessageAudience.All, message));
            }));

            delayed.IsBackground = true;
            delayed.Name = "msg_delay";
            delayed.Start();

            return true;
        }

        public bool SendTeamMessage(int teamId, String message)
        {
            return SendTeamMessage(teamId, message, 0);
        }

        public bool SendTeamMessage(int teamId, String message, int delay)
        {
            if (VMode)
            {
                ConsoleWarn("not sending team-message to TeamId(^b" + teamId + "^n,), ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            if (delay == 0) {
                SendTeamMessageV(teamId, message);
                return true;
            }

            Thread delayed = new Thread(new ThreadStart(delegate()
            {
                Thread.Sleep(delay * 1000);
                QueueSayMessage(new SayMessage(teamId, 0, String.Empty, MessageAudience.Team, message));
            }));
            
            delayed.IsBackground = true;
            delayed.Name = "team_msg_delay";
            delayed.Start();

            return true;
        }

        public bool SendSquadMessage(int teamId, int squadId, String message)
        {
            return SendSquadMessage(teamId, squadId, message, 0);
        }

        public bool SendSquadMessage(int teamId, int squadId, String message, int delay)
        {
            if (VMode)
            {
                ConsoleWarn("not sending squad-message to TeamId(^b" + teamId + "^n,).SquadId(^b" + squadId + "^n), ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            if (delay == 0) {
                SendSquadMessageV(teamId, squadId, message);
                return true;
            }

            Thread delayed = new Thread(new ThreadStart(delegate()
            {
                Thread.Sleep(delay * 1000);
                QueueSayMessage(new SayMessage(teamId, squadId, String.Empty, MessageAudience.Squad, message));
            }));
            
            delayed.IsBackground = true;
            delayed.Name = "squad_msg_delay";
            delayed.Start();


            return true;
        }

        public bool SendPlayerMessage(string name, string message)
        {
            return SendPlayerMessage(name, message, 0);
        }

        public bool SendPlayerMessage(string name, string message, int delay)
        {
            if (VMode)
            {
                ConsoleWarn("not sending player-message to ^b" + name + "^n, ^bvirtual_mode^n is ^bon^n");
                return false;
            }
            
            if (delay == 0) {
                SendPlayerMessageV(name, message);
                return true;
            }

            Thread delayed = new Thread(new ThreadStart(delegate()
            {
                Thread.Sleep(delay * 1000);
                QueueSayMessage(new SayMessage(0, 0, name, MessageAudience.Player, message));
            }));
            
            delayed.IsBackground = true;
            delayed.Name = "player_msg_delay";
            delayed.Start();

            return true;
        }

        public bool SendPlayerYell(string name, string message, int duration)
        {
            if (name == null) return false;

            if (VMode)
            {
                ConsoleWarn("not yelling player-message to ^b" + name + "^n, ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            SendPlayerYellV(name, message, duration);
            return true;
        }

        public bool SendGlobalYell(String message, int duration)
        {
            if (VMode)
            {
                ConsoleWarn("not yelling global-message, ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            SendGlobalYellV(message, duration);
            return true;
        }

        public bool SendTeamYell(int teamId, String message, int duration)
        {
            if (VMode)
            {
                ConsoleWarn("not yelling team-message, ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            SendTeamYellV(teamId, message, duration);
            return true;
        }


        public bool VMode
        {
            get
            {
                if (getBooleanVarValue("virtual_mode"))
                    return true;

                if (VModeSlot == null)
                    return false;

                object mode = Thread.GetData(VModeSlot);
                if (mode == null || !mode.GetType().Equals(typeof(bool)))
                    return false;

                return (bool)mode;
            }
        }



        List<String> scratch_list = new List<string>();

        public void updateQueues(List<CPlayerInfo> lstPlayers)
        {
            DebugWrite("OnListPlayers::updateQueues locking, " + GetQCount() + " in queue", 8);
            lock (players_mutex)
            {
                scratch_handle.Reset();
                // update the scratch list
                scratch_list.Clear();
                foreach (CPlayerInfo info in lstPlayers)
                    if (!scratch_list.Contains(info.SoldierName))
                        scratch_list.Add(info.SoldierName);

                // make a list of players to drop from the stats queue
                List<String> players_to_remove = new List<string>();
                foreach (KeyValuePair<String, CPunkbusterInfo> pair in new_player_queue)
                    if (!scratch_list.Contains(pair.Key) && !players_to_remove.Contains(pair.Key))
                        players_to_remove.Add(pair.Key);

                // now actually drop them from the new players queue
                foreach (String name in players_to_remove)
                    if (new_player_queue.ContainsKey(name))
                    {
                        DebugWrite("Looks like ^b" + name + "^n left the server, removing him from stats queue", 5);
                        new_player_queue.Remove(name);
                    }

                // make a list of players to drop from the new players batch
                players_to_remove.Clear();
                foreach (KeyValuePair<String, PlayerInfo> pair in new_players_batch)
                    if (!scratch_list.Contains(pair.Key) && !players_to_remove.Contains(pair.Key))
                        players_to_remove.Add(pair.Key);

                // now actually drop them from the new players batch
                foreach (String name in players_to_remove)
                    if (new_players_batch.ContainsKey(name))
                        new_players_batch.Remove(name);
            }
            DebugWrite("OnListPlayers::updateQueues UNLOCKING, " + GetQCount() + " in queue", 8);
            scratch_handle.Set();
        }

        private int SafePlayerCount()
        {
            int num = 0;
            lock (players_mutex)
            {
                num = players.Keys.Count;
            }
            return num;
        }

        public void SyncPlayersList(List<CPlayerInfo> lstPlayers)
        {
            DebugWrite("OnListPlayers::SyncPlayersList locking, " + SafePlayerCount() + " players", 8);
            plist_handle.Reset();
            lock (players_mutex)
            {
                // first update the information that players that still are in list
                foreach (CPlayerInfo cpiPlayer in lstPlayers)
                    if (this.players.ContainsKey(cpiPlayer.SoldierName))
                        this.players[cpiPlayer.SoldierName].updateInfo(cpiPlayer);

                //build a lookup table
                Dictionary<String, bool> player_lookup = new Dictionary<string, bool>();
                foreach (CPlayerInfo pinfo in lstPlayers)
                    if (!player_lookup.ContainsKey(pinfo.SoldierName))
                        player_lookup.Add(pinfo.SoldierName, true);


                List<String> players_to_remove = new List<string>();

                // now make a list of players that will need to be removed
                foreach (KeyValuePair<String, PlayerInfo> pair in players)
                    if (!player_lookup.ContainsKey(pair.Key) && !players_to_remove.Contains(pair.Key))
                        players_to_remove.Add(pair.Key);


                // now actually remove them
                foreach (String pname in players_to_remove)
                    InnerRemovePlayer(pname);
            }
            DebugWrite("OnListPlayers::SyncPlayersList UNLOCKING, " + SafePlayerCount() + " players", 8);
            plist_handle.Set();
        }

        public void UpdateExtraInfo(List<CPlayerInfo> lstPlayers)
        {
            DateTime ts = DateTime.Now;
            lock (updates_mutex)
            {
                ts = timerSquad;
            }
            bool itIsTime = (DateTime.Now.Subtract(ts).TotalSeconds > getFloatVarValue("update_interval"));
            Dictionary<String,int> squadCounts = new Dictionary<string,int>();
            String key = null;

            DebugWrite("UpdateExtraInfo seconds since last update: " + DateTime.Now.Subtract(ts).TotalSeconds.ToString("F0"), 7);
            if (!itIsTime) return;

            int updatedPing = 0;
            int badPing = 0;

            foreach (CPlayerInfo cpiPlayer in lstPlayers) {
                if ((cpiPlayer.Score == 0 || Double.IsNaN(cpiPlayer.Score)) && cpiPlayer.Deaths == 0) {
                    DebugWrite("Updating idle duration for: " + cpiPlayer.SoldierName, 6);
                    ServerCommand("player.idleDuration", cpiPlayer.SoldierName); // Update it
                }

                if (cpiPlayer.TeamID > 0 && cpiPlayer.SquadID > 0) {
                    key = cpiPlayer.TeamID.ToString() + "/" + cpiPlayer.SquadID;
                    if (!squadCounts.ContainsKey(key)) {
                        squadCounts[key] = 1;
                    } else {
                        squadCounts[key] = squadCounts[key] + 1;
                    }
                }

                if (cpiPlayer.Ping > 0 && cpiPlayer.Ping < 65535) {
                    OnPlayerPingedByAdmin(cpiPlayer.SoldierName, cpiPlayer.Ping);
                    ++updatedPing;
                } else {
                    ++badPing;
                }
            }

            if (updatedPing > 0 || badPing > 0)
                DebugWrite("Updated pings: " + updatedPing + " good, " + badPing + " bad, " + lstPlayers.Count + " total", 5);

            String[] ids = null;
            Char[] div = new Char[] {'/'};
            foreach (String k in squadCounts.Keys) {
                DebugWrite("Updating squad privacy and leader for: " + k, 6);
                ids = k.Split(div);
                ServerCommand("squad.private", ids[0], ids[1]);
                // Request leader only for squads with more than one player
                if (squadCounts[k] > 1) 
                {
                    ServerCommand("squad.leader", ids[0], ids[1]);
                }
            }
            resetUpdateTimer(WhichTimer.Squad);
        }



        public void RemovePlayer(String name)
        {
            DebugWrite("RemovePlayer locking, " + name, 8);
            lock (players_mutex)
            {
                InnerRemovePlayer(name);
            }
            DebugWrite("RemovePlayer UNLOCKING, " + name, 8);
        }


        private void InnerRemovePlayer(String name)
        {
            try {
                if (!players.ContainsKey(name))
                    return;

                List<String> lkeys = new List<string>(limits.Keys);

                // for players removed, reset the activations/evaluations
                foreach (String lkey in lkeys)
                    if (limits.ContainsKey(lkey))
                    {
                        limits[lkey].ResetActivationsTotal(name);
                        limits[lkey].ResetActivations(name);
                    }

                players.Remove(name);
            } catch (Exception e) {
                DumpException(e);
            }
        }

        public override void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset)
        {
            DebugWrite("Got ^bOnListPlayers^n!", 8);
            if (!plugin_activated)
                return;

            if (cpsSubset.Subset != CPlayerSubset.PlayerSubsetType.All)
                return;

            updateQueues(lstPlayers);
            SyncPlayersList(lstPlayers);
            UpdateExtraInfo(lstPlayers);
        }


        public int sort_players_t_desc_cmp(String left_name, String right_name)
        {
            PlayerInfo left = null;
            PlayerInfo right = null;

            if (players.ContainsKey(left_name))
                left = players[left_name];

            if (players.ContainsKey(right_name))
                right = players[right_name];

            int result = 0;
            if (left != null && right != null)
                result = left.JoinTime.CompareTo(right.JoinTime);
            else if (left != null && right == null)
                result = 1;
            else if (left == null && right != null)
                result = -1;
            else
                result = 0;

            return result * (-1);

        }

        public int sort_limits_id_asc_cmp(Limit left_limit, Limit right_limit)
        {
            int left = -1; ;
            int right = -1;

            int.TryParse(left_limit.id, out left);
            int.TryParse(right_limit.id, out right);

            int result = 0;
            if (left != -1 && right != -1)
                result = left.CompareTo(right);
            else if (left != -1 && right == -1)
                result = 1;
            else if (left == -1 && right != -1)
                result = -1;
            else
                result = 0;

            return result;
        }

        public void DumpPlayersList(List<String> sorted_players, int debug_level)
        {
            int i = 0;
            DebugWrite("Sorted player's list: ", debug_level);
            foreach (String name in sorted_players)
            {
                i++;
                PlayerInfo info = null;
                if (players.ContainsKey(name))
                    info = players[name];

                String join_t = (info != null) ? info.JoinTime.ToString() : "Null";
                DebugWrite(i + ". " + name + ", JoinTime: " + join_t, debug_level);
            }
        }

        public List<Limit> getLimitsForEvaluation(Limit.EvaluationType type)
        {
            List<Limit> sorted_limits = new List<Limit>(limits.Values);

            // remove all the invalid limits
            sorted_limits.RemoveAll(delegate(Limit limit)
            {
                return limit.Invalid || (type & limit.Evaluation) != limit.Evaluation;
            });

            // sort the remaining valid limits
            sorted_limits.Sort(sort_limits_id_asc_cmp);

            return sorted_limits;
        }

        public class SayMessage
        {
            public MessageAudience audience = MessageAudience.All;
            public String text = String.Empty;
            public int TeamId = 0;
            public int SquadId = 0;
            public String player = String.Empty;

            public SayMessage(int TeamId, int SquadId, String player, MessageAudience audience, String text)
            {
                this.audience = audience;
                this.text = text;
                this.SquadId = SquadId;
                this.TeamId = TeamId;
                this.player = player;
            }
        }

        public Queue<SayMessage> messageQueue = new Queue<SayMessage>();


        public void QueueSayMessage(SayMessage message)
        {
            lock (message_mutex)
            {
                messageQueue.Enqueue(message);
            }
            say_handle.Set();
        }

        public void SendQueuedMessages(int sleep_time)
        {

            DebugWrite("sending " + messageQueue.Count + " queued message" + ((messageQueue.Count > 1) ? "s" : "") + " ...", 7);

            while (messageQueue.Count > 0)
            {
                Thread.Sleep(sleep_time);
                if (!plugin_enabled)
                    return;

                SayMessage message = messageQueue.Dequeue();
                switch (message.audience)
                {
                    case MessageAudience.All:
                        SendGlobalMessageV(message.text);
                        break;
                    case MessageAudience.Team:
                        SendTeamMessageV(message.TeamId, message.text);
                        break;
                    case MessageAudience.Squad:
                        SendSquadMessageV(message.TeamId, message.SquadId, message.text);
                        break;
                    case MessageAudience.Player:
                        SendPlayerMessageV(message.player, message.text);
                        break;
                    default:
                        ConsoleError(message.audience.ToString() + " is not known for " + message.audience.GetType().Name);
                        break;
                }
            }
        }


        public void say_thread_loop()
        {

            try
            {
                InsaneLimits plugin = this;

                Thread.CurrentThread.Name = "say";

                plugin.DebugWrite("starting", 4);
                while (true)
                {

                    int sleep_time = (int)(getFloatVarValue("say_interval") * 1000f);

                    if (messageQueue.Count == 0)
                    {
                        DebugWrite("waiting for say message ...", 7);
                        say_handle.WaitOne();
                        say_handle.Reset();
                        DebugWrite("awake!", 7);
                    }

                    SendQueuedMessages(sleep_time);

                    if (!plugin_enabled)
                        break;
                }


                // abort the thread if the plugin was disabled
                if (!plugin_enabled)
                {
                    plugin.DebugWrite("detected that plugin was disabled, aborting", 4);
                    return;
                }

            }
            catch (Exception e)
            {
                if (typeof(ThreadAbortException).Equals(e.GetType()))
                {
                    Thread.ResetAbort();
                    return;
                }
                DumpException(e);
            }
        }

        public void settings_thread_loop()
        {
            try
            {
                InsaneLimits plugin = this;

                Thread.CurrentThread.Name = "settings";

                plugin.DebugWrite("starting", 4);
                ConsoleWrite(" Version = " + GetPluginVersion());

                while (true)
                {
                    try
                    {
                        int sleep_t = getIntegerVarValue("auto_load_interval");
                        plugin.DebugWrite("sleeping for ^b" + sleep_t + "^n second" + ((sleep_t > 1) ? "s" : "") + ", before next iteration", 7);

                        settings_handle.Reset();
                        settings_handle.WaitOne(sleep_t * 1000);
                        plugin.DebugWrite("awake! loading settings", 7);


                        if (!plugin_enabled)
                            break;

                        LoadSettings(false, true);
                    }
                    catch (Exception e)
                    {
                        if (typeof(ThreadAbortException).Equals(e.GetType()))
                        {
                            Thread.ResetAbort();
                            return;
                        }
                        DumpException(e);
                    }

                    if (!plugin_enabled)
                        break;
                }

                // abort the thread if the plugin was disabled
                if (!plugin_enabled)
                {
                    plugin.DebugWrite("detected that plugin was disabled, aborting", 4);
                    return;
                }
            }
            catch (Exception e)
            {
                DumpException(e);
            }
        }

        enum WhichTimer { Squad, Vars };

        private void resetUpdateTimer(WhichTimer t)
        {
            switch (t) {
                case WhichTimer.Squad:
                    lock (updates_mutex) {
                        timerSquad = DateTime.Now;
                    }
                    break;
                case WhichTimer.Vars:
                    lock (updates_mutex) {
                        timerVars = DateTime.Now;
                    }
                    break;
            }
        }

        public void enforcer_thread_loop()
        {

            try
            {
                InsaneLimits plugin = this;
                enforcer_handle.Reset();

                Thread.CurrentThread.Name = "enforcer";
                
                plugin.DebugWrite("starting", 4);

                resetUpdateTimer(WhichTimer.Vars);

                while (true)
                {

                    DebugWrite("waiting for signal from ^bfetch^n thread", 7);
                    Thread.Sleep(1000);

                    // Wait for fetch thread to let us go through
                    enforcer_handle.WaitOne();
                    enforcer_handle.Reset();
                    DateTime now = DateTime.Now;

                    DebugWrite("awake! Checking update timers  ...", 7);

                    DateTime tv = DateTime.Now;
                    
                    lock (plugin.updates_mutex) {
                        tv = plugin.timerVars;
                    }

                    if (DateTime.Now.Subtract(tv).TotalSeconds > getFloatVarValue("update_interval"))
                    {
                        plugin.updateVars();
                    }

                    DebugWrite("Processing interval limits ...", 7);

                    if (!plugin_enabled)
                        break;

                    try
                    {

                        List<Limit> sorted_limits = getLimitsForEvaluation(Limit.EvaluationType.OnInterval | Limit.EvaluationType.OnIntervalPlayers | Limit.EvaluationType.OnIntervalServer);

                        if (sorted_limits.Count == 0)
                        {
                            plugin.DebugWrite("No valid ^bOnIntervalPlayers^n or ^bOnIntervalServer^n  limits founds, skipping this iteration", 8);
                            continue;
                        }

                        //Remove all limit for which there is still remaining time
                        sorted_limits.RemoveAll(delegate(Limit limit) { return limit.RemainingSeconds(now) > 0; });
                        
                        /*
                        After OnLevelLoaded, if there is an interval limit to
                        evaluate and we haven't seen the first spawn yet
                        and the round was reset;
                        the first limit to evaluate "dirties" the round,
                        and therefore it is no longer reset.
                        */
                        if (isRoundReset && sorted_limits.Count > 0) {
                            DebugWrite("Round NEEDS resetting!", 8);
                            isRoundReset = false;
                        }
                        
                        
                        if (sorted_limits.Count == 0) {
                            continue;
                        } else {
                            DebugWrite("signal from ^bfetch^n received and " + sorted_limits.Count + " interval limits ready to fire ...", 7);
                        }


                        // make sure we are the only ones scanning the player's list
                        List<String> sorted_players = null;
                        lock (players_mutex)
                        {
                            sorted_players = new List<String>(players.Keys);
                        }

                        // sort the players in by join time in descending order
                        sorted_players.Sort(sort_players_t_desc_cmp);
                        DumpPlayersList(sorted_players, 10);


                        for (int i = 0; i < sorted_limits.Count; i++)
                        {
                            if (!plugin_enabled)
                                break;

                            Limit limit = sorted_limits[i];

                            if (limit == null)
                                continue;

                            Limit.EvaluationType type = limit.Evaluation;

                            // skip limit if there are no players in the server
                            if (type.Equals(Limit.EvaluationType.OnIntervalPlayers) && sorted_players.Count == 0)
                                continue;

                            // refresh the map information before each limit evaluation
                            getMapInfoSync();

                            if (type.Equals(Limit.EvaluationType.OnIntervalPlayers) && sorted_players.Count > 0)
                            {
                                DebugWrite("Evaluating " + limit.ShortDisplayName + " for " + sorted_players.Count + " player" + ((sorted_players.Count > 1) ? "s" : ""), 6);

                                for (int j = 0; j < sorted_players.Count; j++)
                                {
                                    if (!plugin_enabled)
                                        break;

                                    String name = sorted_players[j];
                                    PlayerInfo pinfo = null;
                                    if (players.ContainsKey(name))
                                        pinfo = players[name];

                                    // if there are no stats, skip this player
                                    if (pinfo == null)
                                        continue;


                                    plugin.DebugWrite("Evaluating " + limit.ShortDisplayName + " for ^b" + name + "^n", 6);

                                    if (evaluateLimit(limit, pinfo))
                                    {
                                        // refresh server information if evaluation was successful
                                        plugin.DebugWrite("Waiting for server information before proceeding", 7);
                                        getServerInfoSync();
                                    }
                                }

                            }
                            else if (type.Equals(Limit.EvaluationType.OnIntervalServer))
                            {
                                DebugWrite("Evaluating " + limit.ShortDisplayName + " - " + limit.Evaluation.ToString(), 5);
                                evaluateLimit(limit);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (typeof(ThreadAbortException).Equals(e.GetType()))
                        {
                            Thread.ResetAbort();
                            return;
                        }
                        DumpException(e);

                    }
                    finally
                    {
                        // Notify the fetch thread
                        DebugWrite("done, will signal ^bfetch^n thread", 7);
                        fetch_handle.Set();
                    }

                    if (!plugin_enabled)
                        break;

                }


                // abort the thread if the plugin was disabled
                if (!plugin_enabled)
                {
                    plugin.DebugWrite("detected that plugin was disabled, aborting", 4);
                    return;
                }

            }
            catch (Exception e)
            {
                DumpException(e);
            }
            finally
            {
                fetch_handle.Set(); // let fetch handle abort
            }
        }

        public void getServerInfo()
        {
            ServerCommand("serverInfo");
        }

        public ServerInfo getServerInfoSync()
        {
            info_handle.Reset();
            getServerInfo();
            Thread.Sleep(500);
            WaitOn("info_handle", info_handle);
            info_handle.Reset();

            return this.serverInfo;
        }



        public void getPlayersList()
        {
            ServerCommand("admin.listPlayers", "all");
            getServerInfo();

        }

        public void getPlayersListSync()
        {
            ServerCommand("admin.listPlayers", "all");


        }

        public void getPBPlayersList()
        {
            if (this.players.Keys.Count > 0) {
                expectedPBCount = this.players.Keys.Count + 1; // FIXME, not locked on purpose
            } else {
                expectedPBCount = 64;
            }
            ServerCommand("punkBuster.pb_sv_command", "pb_sv_plist");
            getServerInfo();
        }

        public void getReservedSlotsList()
        {
            ServerCommand("reservedSlotsList.list");
        }

        public static List<String> getExtraFields()
        {
            List<String> fields = new List<string>();
            fields.Add("recon_t");
            fields.Add("engineer_t");
            fields.Add("assault_t");
            fields.Add("support_t");
            fields.Add("vehicle_t");
            fields.Add("recon_p");
            fields.Add("engineer_p");
            fields.Add("assault_p");
            fields.Add("support_p");
            fields.Add("vehicle_p");

            return fields;

        }

        // Battlelog JSON key name, to Plugin field name
        public static String JSON2Key(String key, String game_version)
        {
            Dictionary<String,String> j2k = null;
            switch (game_version) {
                case "BF3":
                    j2k = json2key;
                    break;
                case "BF4":
                    j2k = json2keyBF4;
                    break;
                case "BFHL":
                default:
                    j2k = json2keyBF4; // TBH BFH
                    break;
            }

            if (!j2k.ContainsKey(key))
                throw new StatsException("unknown JSON field ^b" + key + "^b");
            return j2k[key];
        }

        public static String WJSON2Prop(String key)
        {
            if (!wjson2prop.ContainsKey(key))
                throw new StatsException("unknown Weapon JSON field ^b" + key + "^b");
            return wjson2prop[key];
        }

        public static List<String> getBasicJSONFieldKeys(String game_version)
        {
            if (game_version == "BF3")
                return new List<string>(json2key.Keys);
            else
                return new List<String>(json2keyBF4.Keys); // TBD BFH
        }

        public static List<String> getBasicWJSONFieldKeys()
        {
            return new List<string>(wjson2prop.Keys);
        }

        public static List<String> getBasicFieldKeys(String game_version)
        {
            if (game_version == "BF3")
                return new List<string>(json2key.Values); 
            else
                return new List<string>(json2keyBF4.Values); // TBD BFH
        }

        public static List<String> getBasicWeaponFieldProps()
        {
            return new List<string>(wjson2prop.Values);
        }

        public static List<String> getGameFieldKeys()
        {
            return new List<string>(gamekeys.Keys);
        }


        public bool setPluginVarValue(string var, string val)
        {
            return setPluginVarValue(null, var, val, false);
        }

        public bool setPluginVarValue(string var, string val, bool ui)
        {
            return setPluginVarValue(null, var, val, ui);
        }

        public bool setPluginVarValue(string sender, string var, string val)
        {
            return setPluginVarValue(sender, var, val, false);
        }

        public bool setPluginVarValue(string sender, string var, string val, bool ui)
        {
            try
            {
                if (var == null || val == null || var == "rcon_to_battlelog_codes")
                    return false;

                if (!isPluginVar(var))
                {
                    SendConsoleError(sender, "unknown variable \"" + var + "\"");
                    return false;
                }



                /* Parse Integer Values */
                int integerValue = 0;
                bool isIntegerValue = int.TryParse(val, out integerValue);


                /* Parse Boolean Values */
                bool booleanValue = false;
                bool isBooleanValue = true;

                if (Regex.Match(val, @"^\s*(1|true|yes|accept)\s*$", RegexOptions.IgnoreCase).Success)
                    booleanValue = true;
                else if (Regex.Match(val, @"^\s*(0|false|no|deny|\.\.\.)\s*$", RegexOptions.IgnoreCase).Success)
                    booleanValue = false;
                else
                    isBooleanValue = false;


                /* Parse Float Values */
                float floatValue = 0F;
                bool isFloatValue = float.TryParse(val, out floatValue);


                /* Parse String List */
                List<string> stringListValue = new List<string>(Regex.Split(val.Replace(";", ",").Replace("|", ","), @"\s*,\s*"));
                bool isStringList = true;


                /* Parse String var */
                string stringValue = val;
                bool isStringValue = (val != null);

                if (Limit.isLimitVar(var))
                    return setLimitVarValue(var, val, ui);
                else if (CustomList.isListVar(var))
                    return setListVarValue(var, val, ui);

                else if (isBooleanVar(var))
                {
                    if (!isBooleanValue)
                    {
                        SendConsoleError(sender, "\"" + val + "\" is invalid for ^b" + var);
                        return false;
                    }

                    if (val.Equals("..."))
                        return false;

                    return setBooleanVarValue(var, booleanValue);
                }
                else if (isIntegerVar(var))
                {
                    if (!isIntegerValue)
                    {
                        SendConsoleError(sender, "\"" + val + "\" is invalid for " + var);
                        return false;
                    }

                    setIntegerVarValue(var, integerValue);
                    return true;
                }
                else if (isFloatVar(var))
                {
                    if (!isFloatValue)
                    {
                        SendConsoleError(sender, "\"" + val + "\" is invalid for ^b" + var);
                        return false;
                    }

                    return setFloatVarValue(var, floatValue);
                }
                else if (isStringListVar(var))
                {
                    if (!isStringList)
                    {
                        SendConsoleError(sender, "\"" + val + "\"  is invalid for " + var);
                        return false;
                    }

                    setStringListVarValue(var, stringListValue);
                    return true;
                }
                else if (isStringVar(var))
                {
                    if (!isStringValue)
                    {
                        SendConsoleError(sender, "invalid value for " + var);
                        return false;
                    }

                    setStringVarValue(var, stringValue);
                    return true;
                }
                else
                {
                    SendConsoleError(sender, "unknown variable ^b" + var);
                    return false;
                }
            }
            catch (Exception e)
            {
                DumpException(e);
            }
            finally
            {
                if (ui)
                    SaveSettings(true);
            }

            return false;
        }



        public String getLimitVarId(String var)
        {
            if (!Limit.isLimitVar(var))
            {
                ConsoleError("^b" + var + "^n is not a limit variable");
                return "";
            }
            return Limit.extractId(var);
        }

        public String getListVarId(String var)
        {
            if (!CustomList.isListVar(var))
            {
                ConsoleError("^b" + var + "^n is not a list variable");
                return "";
            }
            return CustomList.extractId(var);
        }

        public String getListVarValue(String var)
        {
            String listId = getListVarId(var);
            if (listId.Length == 0)
                return "";

            if (!lists.ContainsKey(listId))
            {
                ConsoleError("there are no lists with ^bid^n(" + listId + ")");
                return "";
            }

            CustomList list = lists[listId];

            return list.getField(var).ToString();
        }


        public String getLimitVarValue(String var)
        {
            String limitId = getLimitVarId(var);
            if (limitId.Length == 0)
                return "";

            if (!limits.ContainsKey(limitId))
            {
                ConsoleError("there are no limits with ^bid^n(" + limitId + ")");
                return "";
            }

            Limit limit = limits[limitId];

            return limit.getField(var).ToString();
        }

        public bool setListVarValue(String var, String val, bool ui)
        {
            try
            {

                String listId = getListVarId(var);
                if (listId.Length == 0)
                    return false;

                CustomList list = null;
                if (lists.ContainsKey(listId))
                    list = lists[listId];
                else
                {
                    list = new CustomList(this, listId);
                    lock (lists_mutex)
                    {
                        lists.Add(list.id, list);
                    }
                }

                if (!list.isValidFieldKey(var))
                {
                    ConsoleError("^b" + var + "^n has no valid list field");
                    return false;
                }


                bool result = list.setFieldValue(var, val, ui);

                // if delete was set, remove the limit from list
                if (Boolean.Parse(list.getField("delete")))
                {
                    ConsoleWrite("Deleting List #^b" + listId + "^n");
                    lock (lists_mutex)
                    {
                        lists.Remove(listId);
                    }
                    return false;
                }

                // check if the limit id changed
                if (result && int.Parse(list.id) != int.Parse(listId))
                {
                    if (lists.ContainsKey(list.id))
                    {
                        ConsoleError("cannot use List #^b" + list.id + "^n, already exists");
                        // set back the old limit id
                        list.setFieldValue(var, listId, ui);
                        return false;
                    }

                    // first delete the old limit entry
                    lock (lists_mutex)
                    {
                        lists.Remove(listId);
                    }
                    ConsoleWrite("Renaming List #^b" + listId + "^n to List #^b" + list.id + "^n");

                    lock (lists_mutex)
                    {
                        lists.Add(list.id, list);
                    }
                }

                return result;
            }
            finally
            {
                if (ui)
                    SaveSettings(true);
            }
        }


        public bool setLimitVarValue(String var, String val, bool ui)
        {
            try
            {


                //ConsoleWrite("Updating limit " + var + " to " + val);

                String limitId = getLimitVarId(var);
                if (limitId.Length == 0)
                    return false;

                Limit limit = null;
                if (limits.ContainsKey(limitId))
                    limit = limits[limitId];
                else
                {
                    limit = new Limit(this, limitId);
                    lock (limits_mutex)
                    {
                        limits.Add(limit.id, limit);
                    }
                }

                if (!(limit.isValidFieldKey(var) || limit.isValidGroupTitle(var)))
                {
                    ConsoleError("^b" + var + "^n has no valid field");
                    return false;
                }

                // headers do not need extra processing
                if (limit.isValidGroupTitle(var))
                    return limit.setGroupStateByTitle(var, val, ui);


                bool result = limit.setFieldValue(var, val, ui);

                // if delete was set, remove the limit from list
                if (Boolean.Parse(limit.getField("delete")))
                {
                    ConsoleWrite("Deleting Limit #^b" + limitId + "^n");
                    lock (limits_mutex)
                    {
                        limits.Remove(limitId);
                    }
                    return false;
                }

                // check if the limit id changed
                if (result && int.Parse(limit.id) != int.Parse(limitId))
                {
                    if (limits.ContainsKey(limit.id))
                    {
                        ConsoleError("cannot use Limit #^b" + limit.id + "^n, already exists");
                        // set back the old limit id
                        limit.setFieldValue(var, limitId, ui);
                        return false;
                    }

                    // first delete the old limit entry
                    lock (limits_mutex)
                    {
                        limits.Remove(limitId);
                    }

                    ConsoleWrite("Renaming Limit #^b" + limitId + "^n to Limit #^b" + limit.id + "^n");

                    lock (limits_mutex)
                    {
                        limits.Add(limit.id, limit);
                    }
                }

                return result;
            }
            finally
            {
                if (ui)
                    SaveSettings(true);
            }
        }


        public bool isIntegerVar(string var)
        {
            return this.integerVariables.ContainsKey(var);
        }

        public int getIntegerVarValue(string var)
        {
            if (!isIntegerVar(var))
            {
                ConsoleError("unknown variable \"" + var + "\"");
                return -1;
            }

            return this.integerVariables[var];
        }

        public bool setIntegerVarValue(string var, int val)
        {
            if (!isIntegerVar(var))
            {
                ConsoleError("unknown variable \"" + var + "\"");
                return false;
            }

            if (hasIntegerValidator(var))
            {
                integerVariableValidator validator = integerVarValidators[var];
                if (validator(var, val) == false)
                    return false;
            }

            this.integerVariables[var] = val;
            return true;
        }

        public bool hasIntegerValidator(string var)
        {
            return integerVarValidators.ContainsKey(var);
        }

        public bool hasBooleanValidator(string var)
        {
            return booleanVarValidators.ContainsKey(var);


        }


        public bool floatValidator(string var, float value)
        {
            if (var.Equals("say_interval"))
            {
                if (!floatAssertGT(var, value, 0))
                    return false;
            }
            else if (var.Equals("update_interval"))
            {
                if (!floatAssertGTE(var, value, MIN_UPDATE_INTERVAL))
                    return false;
            }

            return true;
        }

        public bool integerValidator(string var, int value)
        {
            if (var.Equals("delete_list"))
            {
                if (value == 0)
                    return true;

                if (!intAssertGTE(var, value, 1))
                    return false;

                try
                {
                    if (lists.ContainsKey(value.ToString()))
                    {
                        ConsoleWrite("Deleting List #^b" + value + "^n");
                        lock (lists_mutex)
                        {
                            lists.Remove(value.ToString());
                        }
                    }
                    else
                        ConsoleError("List #^b" + value + "^n does not exist");

                    return false;
                }
                finally
                {
                    SaveSettings(true);
                }
            }

            else if (var.Equals("delete_limit"))
            {
                if (value == 0)
                    return true;

                if (!intAssertGTE(var, value, 1))
                    return false;

                try
                {

                    if (limits.ContainsKey(value.ToString()))
                    {
                        ConsoleWrite("Deleting Limit #^b" + value + "^n");
                        lock (limits_mutex)
                        {
                            limits.Remove(value.ToString());
                        }
                    }
                    else
                        ConsoleError("Limit #^b" + value + "^n does not exist");

                    return false;
                }
                finally
                {
                    SaveSettings(true);
                }
            }
            else if (var.Equals("debug_level") || var.Equals("smtp_port"))
            {
                if (!intAssertGTE(var, value, 0))
                    return false;
            }
            else if (var.Equals("auto_load_interval"))
            {
                if (!intAssertGTE(var, value, 30))
                    return false;
            }
            else if (var.Equals("wait_timeout"))
            {
                if (!intAssertGTE(var, value, 30))
                    return false;
            }

            return true;
        }


        public bool booleanValidator(string var, bool value)
        {

            if (var.Equals("save_limits") && value)
            {
                int count = limits.Count;
                SaveSettings(false);


                return false;
            }

            if (var.Equals("load_limits") && value)
            {
                LoadSettings(false, false);
                return false;
            }

            if (var.Equals("twitter_setup_account") && value)
            {
                SetupTwitter();
                return false;
            }

            if (var.Equals("twitter_reset_defaults") && value)
            {
                ResetTwitterDefaults();
                return false;
            }

            if (var.Equals("privacy_policy_agreement"))
            {
                if (value)
                    activate_handle.Set();
                else
                {
                    ConsoleWarn("You have not agreed to the ^bPrivacy Policy^n, disabling plugin");
                    ExecuteCommand("procon.protected.plugins.enable", this.GetType().Name, "false");
                }
                return true;
            }



            return true;
        }


        public bool DefaultTweet(String status)
        {
            return Tweet
                (
                status,
                default_twitter_access_token,
                default_twitter_access_token_secret,
                default_twitter_consumer_key,
                default_twitter_consumer_secret,
                true
                );
        }

        public bool Tweet(String status)
        {
            /* Verify that we have all the required fields */
            String access_token = getStringVarValue("twitter_access_token");
            String access_token_seceret = getStringVarValue("twitter_access_token_secret");
            String consumer_key = getStringVarValue("twitter_consumer_key");
            String consumer_secret = getStringVarValue("twitter_consumer_secret");

            return Tweet(status, access_token, access_token_seceret, consumer_key, consumer_secret, false);
        }


        public bool Tweet
            (
            String status,
            String access_token,
            String access_token_secret,
            String consumer_key,
            String consumer_secret,
            bool quiet
            )
        {
            try
            {
                if (VMode)
                {
                    ConsoleWarn("not tweeting, ^bvirtual_mode^n is ^bon^n");
                    return false;
                }

                if (String.IsNullOrEmpty(status))
                    throw new TwitterException("Cannot update Twitter status, invalid ^bstatus^n value");


                if (String.IsNullOrEmpty(access_token) || String.IsNullOrEmpty(access_token_secret) ||
                    String.IsNullOrEmpty(consumer_key) || String.IsNullOrEmpty(consumer_secret))
                    throw new TwitterException("Cannot update Twitter status, looks like you have not run Twitter setup");

                /* Create the Status Update Request */
                OAuthRequest orequest = TwitterStatusUpdateRequest(status, access_token, access_token_secret, consumer_key, consumer_secret);

                HttpWebResponse oresponse = (HttpWebResponse)orequest.request.GetResponse();

                String protcol = "HTTP/" + oresponse.ProtocolVersion + " " + (int)oresponse.StatusCode;

                if (!oresponse.StatusCode.Equals(HttpStatusCode.OK))
                    throw new TwitterException("Twitter UpdateStatus Request failed, " + protcol);

                if (oresponse.ContentLength == 0)
                    throw new TwitterException("Twitter UpdateStatus Request failed, ContentLength=0");

                StreamReader sin = new StreamReader(oresponse.GetResponseStream());
                String response = sin.ReadToEnd();
                sin.Close();

                Hashtable data = (Hashtable)JSON.JsonDecode(response);

                if (data == null || !data.ContainsKey("id_str"))
                    throw new TwitterException("Twitter UpdateStatus Request failed, response missing ^bid^n field");

                String id = (String)(data["id_str"].ToString());

                DebugWrite("Tweet Successful, id=^b" + id + "^n, Status: " + status, 4);

                return true;
            }
            catch (TwitterException e)
            {
                if (!quiet)
                    ConsoleException(e.Message);
            }
            catch (WebException e)
            {
                if (!quiet)
                    HandleTwitterWebException(e, "UpdateStatus");
            }
            catch (Exception e)
            {
                DumpException(e);
            }

            return false;

        }

        public void VerifyTwitterPin(String PIN)
        {
            try
            {
                if (String.IsNullOrEmpty(PIN))
                {
                    ConsoleError("Cannot verify Twitter PIN, value(^b" + PIN + "^n) is invalid");
                    return;
                }

                DebugWrite("VERIFIER_PIN: " + PIN, 5);

                hidden_variables["twitter_verifier_pin"] = true;

                if (String.IsNullOrEmpty(oauth_token) || String.IsNullOrEmpty(oauth_token_secret))
                    throw new TwitterException("Cannot verify Twitter PIN, There is no ^boauth_token^n or ^boauth_token_secret^n in memory");



                OAuthRequest orequest = TwitterAccessTokenRequest(PIN, oauth_token, oauth_token_secret);

                HttpWebResponse oresponse = (HttpWebResponse)orequest.request.GetResponse();

                String protcol = "HTTP/" + oresponse.ProtocolVersion + " " + (int)oresponse.StatusCode;

                if (!oresponse.StatusCode.Equals(HttpStatusCode.OK))
                    throw new TwitterException("Twitter AccessToken Request failed, " + protcol);

                if (oresponse.ContentLength == 0)
                    throw new TwitterException("Twitter AccessToken Request failed, ContentLength=0");

                StreamReader sin = new StreamReader(oresponse.GetResponseStream());
                String response = sin.ReadToEnd();

                DebugWrite("ACCESS_TOKEN_RESPONSE: " + response, 5);


                Dictionary<String, String> pairs = ParseQueryString(response);


                /* Sanity check the results */
                if (pairs.Count == 0)
                    throw new TwitterException("Twitter AccessToken Request failed, missing fields");

                /* Get the ReuestToken */
                if (!pairs.ContainsKey("oauth_token"))
                    throw new TwitterException("Twitter AccessToken Request failed, missing ^boauth_token^n field");
                oauth_token = pairs["oauth_token"];

                /* Get the RequestTokenSecret */
                if (!pairs.ContainsKey("oauth_token_secret"))
                    throw new TwitterException("Twitter AccessToken Request failed, missing ^boauth_token_secret^n field");
                oauth_token_secret = pairs["oauth_token_secret"];

                /* Get the User-Id  (Optional) */
                String user_id = String.Empty;
                if (pairs.ContainsKey("user_id"))
                    user_id = pairs["user_id"];

                /* Get the Screen-Name (Optional) */
                String screen_name = String.Empty;
                if (pairs.ContainsKey("screen_name"))
                    screen_name = pairs["screen_name"];


                ConsoleWrite("Access token, and secret obtained. Twitter setup is now complete.");
                if (!String.IsNullOrEmpty(user_id))
                    ConsoleWrite("Twitter User-Id: ^b" + user_id + "^n");
                if (!String.IsNullOrEmpty(screen_name))
                    ConsoleWrite("Twitter Screen-Name: ^b" + screen_name + "^n");

                DebugWrite("access_token=" + oauth_token, 4);
                DebugWrite("access_token_secret=" + oauth_token_secret, 4);


                setStringVarValue("twitter_access_token", oauth_token);
                setStringVarValue("twitter_access_token_secret", oauth_token_secret);
                setStringVarValue("twitter_user_id", user_id);
                setStringVarValue("twitter_screen_name", screen_name);

            }
            catch (TwitterException e)
            {
                ConsoleException(e.Message);
                ConsoleWarn("Set the field ^btwitter_setup_account^n to ^bTrue^n to re-initiate the Twitter configuration");
                return;
            }
            catch (WebException e)
            {
                HandleTwitterWebException(e, "AccessToken");
            }
            catch (Exception e)
            {
                DumpException(e);
            }


        }


        public void SetupTwitter()
        {
            try
            {
                //Display the Twitter Pin Field
                hidden_variables["twitter_verifier_pin"] = false;
                oauth_token = String.Empty;
                oauth_token_secret = String.Empty;


                OAuthRequest orequest = TwitterRequestTokenRequest();

                HttpWebResponse oresponse = (HttpWebResponse)orequest.request.GetResponse();
                String protcol = "HTTP/" + oresponse.ProtocolVersion + " " + (int)oresponse.StatusCode;

                if (!oresponse.StatusCode.Equals(HttpStatusCode.OK))
                    throw new TwitterException("Twitter RequestToken Request failed, " + protcol);

                if (oresponse.ContentLength == 0)
                    throw new TwitterException("Twitter RequestToken Request failed, ContentLength=0");

                StreamReader sin = new StreamReader(oresponse.GetResponseStream());
                String response = sin.ReadToEnd();

                Dictionary<String, String> pairs = ParseQueryString(response);

                if (pairs.Count == 0 || !pairs.ContainsKey("oauth_callback_confirmed"))
                    throw new TwitterException("Twitter RequestToken Request failed, missing ^boauth_callback_confirmed^n field");

                String oauth_callback_confirmed = pairs["oauth_callback_confirmed"];

                if (!oauth_callback_confirmed.ToLower().Equals("true"))
                    throw new TwitterException("Twitter RequestToken Request failed, ^boauth_callback_confirmed^n=^b" + oauth_callback_confirmed + "^n");

                /* Get the ReuestToken */
                if (!pairs.ContainsKey("oauth_token"))
                    throw new TwitterException("Twitter RequestToken Request failed, missing ^boauth_token^n field");
                oauth_token = pairs["oauth_token"];

                /* Get the RequestTokenSecret */
                if (!pairs.ContainsKey("oauth_token_secret"))
                    throw new TwitterException("Twitter RequestToken Request failed, missing ^boauth_token_secret^n field");
                oauth_token_secret = pairs["oauth_token_secret"];



                DebugWrite("REQUEST_TOKEN_RESPONSE: " + response, 5);
                DebugWrite("oauth_callback_confirmed=" + oauth_callback_confirmed, 4);
                DebugWrite("oauth_token=" + oauth_token, 4);
                DebugWrite("oauth_token_secret=" + oauth_token_secret, 4);

                ConsoleWrite("Please visit the following site to obtain the ^btwitter_verifier_pin^n");
                ConsoleWrite("http://api.twitter.com/oauth/authorize?oauth_token=" + oauth_token);


            }
            catch (TwitterException e)
            {
                ConsoleException(e.Message);
                return;
            }
            catch (WebException e)
            {
                HandleTwitterWebException(e, "RequestToken");
            }
            catch (Exception e)
            {
                DumpException(e);
            }

        }

        public void HandleTwitterWebException(WebException e, String prefix)
        {
            HttpWebResponse response = (HttpWebResponse)e.Response;
            String protcol = (response == null) ? "" : "HTTP/" + response.ProtocolVersion;

            String error = String.Empty;
            //try reading JSON response
            if (response != null && response.ContentType != null && response.ContentType.ToLower().Contains("json"))
            {
                try
                {
                    StreamReader sin = new StreamReader(response.GetResponseStream());
                    String data = sin.ReadToEnd();
                    sin.Close();

                    Hashtable jdata = (Hashtable)JSON.JsonDecode(data);
                    if (jdata == null || !jdata.ContainsKey("error") ||
                        jdata["error"] == null || !jdata["error"].GetType().Equals(typeof(String)))
                        throw new Exception();

                    error = "Twitter Error: " + (String)jdata["error"] + ", ";
                }
                catch (Exception)
                {
                }
            }

            /* Handle Time-Out Gracefully */
            if (e.Status.Equals(WebExceptionStatus.Timeout))
            {
                ConsoleException("Twitter " + prefix + " Request(" + protcol + ") timed-out");
                return;
            }
            else if (e.Status.Equals(WebExceptionStatus.ProtocolError))
            {
                ConsoleException("Twitter " + prefix + " Request(" + protcol + ") failed, " + error + " " + e.GetType() + ": " + e.Message);
                return;
            }
            else
                throw e;
        }

        public Dictionary<String, String> ParseQueryString(String text)
        {

            MatchCollection matches = Regex.Matches(text, @"([^=]+)=([^&]+)&?", RegexOptions.IgnoreCase);

            Dictionary<String, String> pairs = new Dictionary<string, string>();

            foreach (Match match in matches)
                if (match.Success && !pairs.ContainsKey(match.Groups[1].Value))
                    pairs.Add(match.Groups[1].Value, match.Groups[2].Value);

            return pairs;
        }


        public static int MAX_STATUS_LENGTH = 140;
        public OAuthRequest TwitterStatusUpdateRequest(
            String status,
            String access_token,
            String access_token_secret,
            String consumer_key,
            String consumer_secret)
        {
            System.Net.ServicePointManager.Expect100Continue = false;

            if (String.IsNullOrEmpty(status))
                return null;


            String suffix = "...";
            if (status.Length > MAX_STATUS_LENGTH)
                status = status.Substring(0, MAX_STATUS_LENGTH - suffix.Length) + suffix;


            OAuthRequest orequest = new OAuthRequest(this, "https://api.twitter.com/1.1/statuses/update.json"); // Fix #48
            orequest.Method = HTTPMethod.POST;
            orequest.request.ContentType = "application/x-www-form-urlencoded";

            /* Set the Post Data */

            byte[] data = Encoding.UTF8.GetBytes("status=" + OAuthRequest.UrlEncode(Encoding.UTF8.GetBytes(status)));

            // Parameters required by the Twitter OAuth Protocol
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_consumer_key", consumer_key));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_nonce", Guid.NewGuid().ToString("N")));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_signature_method", "HMAC-SHA1"));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_token", access_token));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_timestamp", ((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString()));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_version", "1.0"));
            orequest.parameters.Add(new KeyValuePair<string, string>("status", OAuthRequest.UrlEncode(Encoding.UTF8.GetBytes(status))));

            // Compute and add the signature
            String signature = orequest.Signature(consumer_secret, access_token_secret);
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_signature", OAuthRequest.UrlEncode(signature)));

            // Add the OAuth authentication header
            String OAuthHeader = orequest.Header();
            orequest.request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequired;
            orequest.request.Headers["Authorization"] = OAuthHeader;

            // Add the POST body
            orequest.request.ContentLength = data.Length;
            Stream sout = orequest.request.GetRequestStream();
            sout.Write(data, 0, data.Length);
            sout.Close();

            return orequest;
        }


        public OAuthRequest TwitterAccessTokenRequest(String verifier, String token, String secret)
        {
            OAuthRequest orequest = new OAuthRequest(this, "https://api.twitter.com/oauth/access_token"); // Fix #48
            orequest.Method = HTTPMethod.POST;
            orequest.request.ContentLength = 0;

            // Parameters required by the Twitter OAuth Protocol
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_consumer_key", getStringVarValue("twitter_consumer_key")));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_nonce", Guid.NewGuid().ToString("N")));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_timestamp", ((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString()));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_signature_method", "HMAC-SHA1"));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_version", "1.0"));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_token", token));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_verifier", verifier));

            // Compute and add the signature
            String signature = orequest.Signature(getStringVarValue("twitter_consumer_secret"), secret);
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_signature", OAuthRequest.UrlEncode(signature)));

            // Add the OAuth authentication header
            String OAuthHeader = orequest.Header();
            orequest.request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequired;
            orequest.request.Headers["Authorization"] = OAuthHeader;



            return orequest;
        }

        public OAuthRequest TwitterRequestTokenRequest()
        {
            OAuthRequest orequest = new OAuthRequest(this, "https://api.twitter.com/oauth/request_token"); // Fix #48
            orequest.Method = HTTPMethod.POST;
            orequest.request.ContentLength = 0;

            // Parameters required by the Twitter OAuth Protocol
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_callback", OAuthRequest.UrlEncode("oob")));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_consumer_key", getStringVarValue("twitter_consumer_key")));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_nonce", Guid.NewGuid().ToString("N")));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_timestamp", ((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString()));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_signature_method", "HMAC-SHA1"));
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_version", "1.0"));

            // Compute and add the signature
            String signature = orequest.Signature(getStringVarValue("twitter_consumer_secret"), null);
            orequest.parameters.Add(new KeyValuePair<string, string>("oauth_signature", OAuthRequest.UrlEncode(signature)));

            // Add the OAuth authentication header
            String OAuthHeader = orequest.Header();
            orequest.request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequired;
            orequest.request.Headers["Authorization"] = OAuthHeader;

            return orequest;
        }


        public bool intAssertGTE(string var, int value, int min_value)
        {
            if (!(value >= min_value))
            {
                ConsoleError("^b" + var + "(^n" + value + "^b)^n must be greater than or equal to ^b" + min_value + "^n^0");
                return false;
            }

            return true;
        }

        public bool floatAssertGTE(string var, float value, float min_value)
        {
            if (!(value >= min_value))
            {
                ConsoleError("^b" + var + "(^n" + value + "^b)^n must be greater than or equal to ^b" + min_value + "^n^0");
                return false;
            }

            return true;
        }

        private bool floatAssertGT(string var, float value, float min_value)
        {
            if (!(value > min_value))
            {
                ConsoleError("^b" + var + "(^n" + value + "^b)^n must be greater than  ^b" + min_value + "^n^0");
                return false;
            }

            return true;
        }


        private bool hasStringValidator(string var)
        {
            return stringVarValidators.ContainsKey(var);
        }

        private bool isStringVar(string var)
        {
            return this.stringVariables.ContainsKey(var);
        }

        public string getStringVarValue(string var)
        {
            if (!isStringVar(var))
            {
                ConsoleError("unknown variable \"" + var + "\"");
                return "";
            }

            return this.stringVariables[var];
        }

        public bool SaveSettings(bool quiet)
        {

            String file = getStringVarValue("limits_file");

            try
            {
                lock (settings_mutex)
                {
                    String settings = "";
                    String cname = this.GetType().Name;

                    //save the exported variables first
                    foreach (String ckey in exported_variables)
                    {
                        String cvalue = getBooleanVarValue(ckey).ToString();
                        settings += "procon.protected.plugins.setVariable \"" + cname + "\" \"" + ckey + "\" \"BASE64:" + Encode(cvalue) + "\"" + NL;
                    }

                    List<String> keys = new List<string>(limits.Keys);
                    foreach (String key in keys)
                    {
                        if (!limits.ContainsKey(key))
                            continue;

                        Limit limit = limits[key];

                        if (limit.Enabled)
                        {
                            DebugWrite("^8^b============= In SaveSettings " + limit.id, 9);
                            limit.ResetLastInterval(DateTime.Now);
                        }

                        Dictionary<String, String> lsettings = limit.getSettings(false);
                        foreach (KeyValuePair<String, String> pair in lsettings)
                            settings += "procon.protected.plugins.setVariable \"" + cname + "\" \"" + pair.Key + "\" \"BASE64:" + Encode(pair.Value) + "\"" + NL;
                    }


                    keys = new List<string>(lists.Keys);
                    foreach (String key in keys)
                    {
                        if (!lists.ContainsKey(key))
                            continue;

                        CustomList list = lists[key];
                        Dictionary<String, String> lsettings = list.getSettings(false);
                        foreach (KeyValuePair<String, String> pair in lsettings)
                            settings += "procon.protected.plugins.setVariable \"" + cname + "\" \"" + pair.Key + "\" \"BASE64:" + Encode(pair.Value) + "\"" + NL;
                    }


                    if (!File.Exists(file) && !quiet)
                        ConsoleWarn("file ^b" + file + "^n does not exist, will create new one");


                    DumpData(settings, file);

                    int lmcount = limits.Count;
                    int lscount = lists.Count;

                    if (!quiet || getIntegerVarValue("debug_level") >= 5)
                        ConsoleWrite(lmcount + " limit" + ((lmcount > 1 || lmcount == 0) ? "s" : "") + " and " + lscount + " list" + ((lscount > 1 || lscount == 0) ? "s" : "") + " saved to ^b" + file + "^n");
                }

            }
            catch (Exception e)
            {
                DumpException(e);
                return false;
            }

            return true;
        }




        public bool LoadSettings(bool force, bool quiet)
        {
            return LoadSettings(force, true, quiet);
        }

        public bool LoadSettings(bool force, bool compile, bool quiet)
        {
            try
            {
                lock (settings_mutex)
                {
                    String file = getStringVarValue("limits_file");

                    bool exist = File.Exists(file);

                    if (!exist && force)
                        File.Create(file, 1024, FileOptions.None).Close();
                    else if (!exist)
                    {
                        ConsoleError("file ^b" + file + "^n does not exist");
                        return false;
                    }

                    String[] lines = File.ReadAllLines(file);
                    int lscount = 0;
                    int lmcount = 0;
                    foreach (String line in lines)
                    {

                        String ln = line.Trim();
                        if (ln.Length == 0 || ln.StartsWith("#") || ln.StartsWith(";"))
                            continue;

                        MatchCollection collection = Regex.Matches(ln, "\"([^\"]+)\"", RegexOptions.IgnoreCase);

                        if (collection.Count != 3)
                            continue;


                        String var = collection[1].Groups[1].Value;
                        String value = collection[2].Groups[1].Value;

                        if (Regex.Match(var, @"limit_\d+_id", RegexOptions.IgnoreCase).Success)
                            lmcount++;

                        if (Regex.Match(var, @"list_\d+_id", RegexOptions.IgnoreCase).Success)
                            lscount++;
 
                        SetPluginVariable(var, value);

                    }

                    if (!quiet || getIntegerVarValue("debug_level") >= 5)
                        ConsoleWrite(lmcount + " limit" + ((lmcount > 1 || lmcount == 0) ? "s" : "") + " and " + lscount + " list" + ((lscount > 1 || lscount == 0) ? "s" : "") + " loaded from ^b" + file + "^n");

                    CompileAll();

                    activate_handle.Set();

                }

            }
            catch (Exception e)
            {
                DumpException(e);
                return false;
            }

            return true;

        }

        public String FixLimitsFilePath()
        {
            String file = getStringVarValue("limits_file");
            try
            {
                if (Path.GetFileNameWithoutExtension(file).Equals(this.GetType().Name))
                    file = makeRelativePath(this.GetType().Name + "_" + server_host + "_" + server_port + ".conf");

            }
            catch (Exception e)
            {
                DumpException(e);
            }

            return file.Trim();

        }

        private bool setStringVarValue(string var, string val)
        {
            if (!isStringVar(var))
            {
                ConsoleError("unknown variable \"" + var + "\"");
                return false;
            }


            if (hasStringValidator(var))
            {
                stringVariableValidator validator = stringVarValidators[var];
                if (validator(var, val) == false)
                    return false;
            }



            this.stringVariables[var] = val;


            if (var.Equals("limits_file"))
                this.stringVariables[var] = FixLimitsFilePath();



            return true;
        }



        private bool isStringListVar(string var)
        {
            return this.stringListVariables.ContainsKey(var);
        }

        private List<string> getStringListVarValue(string var)
        {
            if (!isStringListVar(var))
            {
                ConsoleError("variable \"" + var + "\"");
                return new List<string>();
            }

            string[] out_list = Regex.Split(this.stringListVariables[var].Replace(";", ",").Replace("|", ","), @"\s*,\s*");
            return new List<string>(out_list);
        }

        private bool setStringListVarValue(string var, List<string> val)
        {
            if (!isStringListVar(var))
            {
                ConsoleError("^1^bERROR^0^n: unknown variable \"" + var + "\"");
                return false;
            }

            List<string> cleanList = new List<string>();
            foreach (string item in val)
                if (Regex.Match(item, @"^\s*$").Success)
                    continue;
                else
                    cleanList.Add(item);

            this.stringListVariables[var] = String.Join("|", cleanList.ToArray());
            return true;
        }

        public bool isFloatVar(string var)
        {
            return this.floatVariables.ContainsKey(var);
        }

        public float getFloatVarValue(string var)
        {
            if (!isFloatVar(var))
            {
                ConsoleError("unknown variable 3 ^b" + var);
                return -1F;
            }

            return this.floatVariables[var];
        }

        public bool setFloatVarValue(string var, float val)
        {
            if (!isFloatVar(var))
            {
                ConsoleError("unknown variable 4 ^b" + var);
                return false;
            }

            if (hasFloatValidator(var))
            {
                floatVariableValidator validator = floatVarValidators[var];
                if (validator(var, val) == false)
                    return false;
            }

            this.floatVariables[var] = val;
            return true;
        }

        public bool hasFloatValidator(string var)
        {
            return floatVarValidators.ContainsKey(var);
        }


        public bool isBooleanVar(string var)
        {
            return this.booleanVariables.ContainsKey(var);
        }

        public bool getBooleanVarValue(string var)
        {
            if (!isBooleanVar(var))
            {
                ConsoleError("unknown variable 5 ^b" + var);
                return false;
            }

            return this.booleanVariables[var];
        }

        public bool setBooleanVarValue(string var, bool val)
        {
            if (!isBooleanVar(var))
            {
                ConsoleError("unknown variable 6 ^b" + var);
                return false;
            }


            if (hasBooleanValidator(var))
            {
                booleanVariableValidator validator = booleanVarValidators[var];
                if (validator(var, val) == false)
                    return false;
            }

            this.booleanVariables[var] = val;

            return true;
        }

        public bool isPluginVar(String var)
        {
            return getPluginVars().Contains(var) || Limit.isLimitVar(var) || CustomList.isListVar(var);
        }


        public string getPluginVarValue(string var)
        {
            return getPluginVarValue(null, var);
        }

        public string getPluginVarValue(string sender, string var)
        {

            if (var == "rcon_to_battlelog_codes")
            {
                String condensed = String.Empty;
                foreach (String k in rcon2bw_user.Keys)
                {
                    if (String.IsNullOrEmpty(condensed))
                    {
                        condensed = k + "=" + rcon2bw_user[k];
                    }
                    else
                    {
                        condensed = condensed + ", " + k + "=" + rcon2bw_user[k];
                    }
                }
                return condensed;
            }

            if (!isPluginVar(var))
            {
                SendConsoleError(sender, "unknown variable ^b" + var);
                return "";
            }

            if (isFloatVar(var))
                return getFloatVarValue(var).ToString();
            else if (isBooleanVar(var))
                return getBooleanVarValue(var).ToString();
            else if (isIntegerVar(var))
                return getIntegerVarValue(var).ToString();
            else if (Limit.isLimitVar(var))
                return getLimitVarValue(var).ToString();
            else if (CustomList.isListVar(var))
                return getListVarValue(var).ToString();
            else if (isStringListVar(var))
                return String.Join(", ", getStringListVarValue(var).ToArray());
            else if (isStringVar(var))
                return getStringVarValue(var);
            else
            {
                SendConsoleError(sender, "unknown variable ^b" + var);
                return "";
            }
        }

        public List<string> getPluginVars()
        {
            return getPluginVars(true, true, true, false);
        }

        public List<string> getPluginVars(bool include_limits, bool include_lists, bool display)
        {
            return getPluginVars(include_limits, include_lists, true, display);
        }



        public List<string> getPluginVars(bool include_limits, bool include_lists, bool include_vars, bool display)
        {
            List<string> vars = new List<string>();

            vars.AddRange(getBooleanPluginVars());
            vars.AddRange(getFloatPluginVars());
            vars.AddRange(getIntegerPluginVars());
            vars.AddRange(getStringListPluginVars());
            vars.AddRange(getStringPluginVars());
            vars.Add("rcon_to_battlelog_codes");

            if (include_lists)
            {
                List<String> keys = new List<string>();

                lock (lists_mutex) { keys.AddRange(lists.Keys); }

                for (int i = 0; i < keys.Count; i++)
                {
                    String key = keys[i];

                    CustomList list = null;

                    if (!lists.TryGetValue(key, out list))
                        continue;

                    vars.AddRange(list.getSettings(display).Keys);
                }
            }

            if (include_limits)
            {
                List<String> keys = new List<String>();

                lock (limits_mutex) { keys.AddRange(limits.Keys); }

                for (int i = 0; i < keys.Count; i++)
                {
                    String key = keys[i];

                    Limit limit = null;
                    if (!limits.TryGetValue(key, out limit))
                        continue;

                    vars.AddRange(limit.getSettings(display).Keys);
                }
            }

            if (!include_vars)
            {
                foreach (String var in exported_variables)
                {
                    if (vars.Contains(var))
                        vars.Remove(var);
                }
            }



            return vars;
        }

        public List<string> getFloatPluginVars()
        {
            return new List<string>(this.floatVariables.Keys);
        }

        public List<String> getBooleanPluginVars()
        {
            return new List<string>(this.booleanVariables.Keys);
        }

        public List<string> getIntegerPluginVars()
        {
            return new List<string>(this.integerVariables.Keys);
        }

        private List<string> getStringListPluginVars()
        {
            return new List<string>(this.stringListVariables.Keys);
        }

        private List<string> getStringPluginVars()
        {
            return new List<string>(this.stringVariables.Keys);
        }

        public void DumpData(string s)
        {
            // Create a temporary file
            string path = Path.GetRandomFileName() + ".dump";
            ConsoleWrite("^1Dumping information in file " + path);
            DumpData(s, path);
        }

        public void DumpData(string s, string path)
        {
            try
            {
                FileMode mode = FileMode.CreateNew;
                if (File.Exists(path))
                    mode = FileMode.Truncate;
                    //File.Delete(path);

                using (FileStream fs = File.Open(path, mode, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(s);
                    fs.Write(info, 0, info.Length);
                }
            }
            catch (Exception ex)
            {
                ConsoleError("unable to dump information to file");
                ConsoleException("" + ex.GetType() + ": " + ex.Message);
            }
        }

        public void AppendData(string s, string path)
        {
            try
            {
                if (!Path.IsPathRooted(path))
                    path = Path.Combine(Directory.GetParent(Application.ExecutablePath).FullName, path);


                using (FileStream fs = File.Open(path, FileMode.Append))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(s);
                    fs.Write(info, 0, info.Length);
                }
            }
            catch (Exception ex)
            {
                ConsoleError("unable to append data to file " + path + "");
                ConsoleException("" + ex.GetType() + ": " + ex.Message);
            }
        }

        public void DumpException(Exception e)
        {
            DumpException(e, String.Empty);
        }

        public void DumpException(Exception e, String prefix)
        {
            int debug_level = getIntegerVarValue("debug_level");

            try
            {
                string class_name = this.GetType().Name;
                string path = class_name + ".dump";


                if (prefix == null)
                    prefix = String.Empty;
                else
                    prefix += ": ";


                if (e.GetType().Equals(typeof(ThreadAbortException)))
                {
                    Thread.ResetAbort();
                    return;
                }
                else if (e.GetType().Equals(typeof(TargetInvocationException)) && e.InnerException != null)
                {
                    if (debug_level >= 4) ConsoleException(prefix + e.InnerException.GetType() + ": " + e.InnerException.Message);
                    DebugWrite("^1Extra information dumped in file " + path, 4);
                    DumpExceptionFile(e, path);
                    DumpExceptionFile(e.InnerException, path);
                }
                else
                {
                    if (debug_level >= 4) ConsoleException(prefix + e.GetType() + ": " + e.Message);

                    foreach (DictionaryEntry de in e.Data)
                    {
                        DebugWrite("    " + de.Key.ToString() + ": " + de.Value.ToString(), 4);
                    }

                    DebugWrite("^1Extra information dumped in file " + path, 4);
                    DumpExceptionFile(e, path);
                }
            }
            catch (Exception ex)
            {
                if (debug_level >= 4) ConsoleWarn("unable to dump extra exception information.");
                if (debug_level >= 4) ConsoleException(ex.GetType() + ": " + ex.Message);
            }
        }

        public void DumpExceptionFile(Exception e, string path)
        {
            DumpExceptionFile(e, path, String.Empty);
        }

        public void DumpExceptionFile(Exception e, string path, String extra)
        {
            string class_name = this.GetType().Name;

            using (FileStream fs = File.Open(path, FileMode.Append))
            {
                String version = GetPluginVersion();
                String trace_str = "\n-----------------------------------------------\n";
                trace_str += "Version: " + class_name + " " + version + "\n";
                trace_str += "Date: " + DateTime.Now.ToString() + "\n";

                if (!(extra == null && extra.Length == 0))
                    trace_str += "Data: " + extra + "\n";

                trace_str += e.GetType() + ": " + e.Message + "\n\n";
                trace_str += "Stack Trace: \n" + e.StackTrace + "\n\n";
                trace_str += "MSIL Stack Trace:\n";


                StackTrace trace = new StackTrace(e);
                StackFrame[] frames = trace.GetFrames();
                foreach (StackFrame frame in frames)
                    trace_str += "    " + frame.GetMethod() + ", IL: " + String.Format("0x{0:X}", frame.GetILOffset()) + "\n";


                Byte[] info = new UTF8Encoding(true).GetBytes(trace_str);
                fs.Write(info, 0, info.Length);
            }

        }

        public enum MessageType { Warning, Error, Exception, Normal };


        public String FormatMessage(String msg, MessageType type)
        {
            String prefix = "[^b" + GetPluginName() + "^n] ";

            if (Thread.CurrentThread.Name != null)
                prefix += "Thread(^b" + Thread.CurrentThread.Name + "^n): ";

            if (type.Equals(MessageType.Warning))
                prefix += "^1^bWARNING^0^n: ";
            else if (type.Equals(MessageType.Error))
                prefix += "^1^bERROR^0^n: ";
            else if (type.Equals(MessageType.Exception))
                prefix += "^1^bEXCEPTION^0^n: ";

            return prefix + msg.Replace('{','(').Replace('}',')');
        }


        public void LogWrite(String msg)
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", msg);
        }

        public void ConsoleWrite(string msg, MessageType type)
        {
            LogWrite(FormatMessage(msg, type));
        }

        public void ConsoleWrite(string msg)
        {
            ConsoleWrite(msg, MessageType.Normal);
        }

        public void ConsoleWarn(String msg)
        {
            ConsoleWrite(msg, MessageType.Warning);
        }

        public void ConsoleError(String msg)
        {
            ConsoleWrite(msg, MessageType.Error);
        }

        public void ConsoleException(String msg)
        {
            ConsoleWrite(msg, MessageType.Exception);
        }

        public void DebugWrite(string msg, int level)
        {
            if (getIntegerVarValue("debug_level") >= level)
                ConsoleWrite(msg, MessageType.Normal);
        }



        public void ServerCommand(params string[] args)
        {
            List<string> list = new List<string>();
            list.Add("procon.protected.send");
            list.AddRange(args);
            this.ExecuteCommand(list.ToArray());
        }

        public void PunkBusterCommand(String text)
        {
            ServerCommand("punkBuster.pb_sv_command", text);
        }

        public bool PRoConChat(String text)
        {
            if (VMode)
            {
                ConsoleWarn("not sending procon chat \"" + text + "\", ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            this.ExecuteCommand("procon.protected.chat.write", E(text));

            return true;
        }

        public bool PRoConEvent(String text, String player)
        {
            return PRoConEvent(EventType.Plugins, CapturableEvent.PluginAction, text, player);
        }

        public bool PRoConEvent(EventType type, CapturableEvent name, String text, String player)
        {
            if (VMode)
            {
                ConsoleWarn("not sending procon event(^b" + type.ToString() + "^n:^b" + name.ToString() + "^n:^b" + player + "^n) \"" + text + "\", ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            this.ExecuteCommand("procon.protected.events.write", type.ToString(), name.ToString(), text, player);

            return true;
        }

        public bool SendMail(String address, String subject, String body)
        {
            if (VMode)
            {
                ConsoleWarn("not sending email, ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            //send mail in a separate thread to avoid halting ProCon if SMTP request times out
            Thread mail_thread = new Thread(new ThreadStart(delegate()
            {
                try
                {
                    String smtp_host = getStringVarValue("smtp_host");
                    String smtp_account = getStringVarValue("smtp_account");
                    String smtp_mail = getStringVarValue("smtp_mail");
                    String smtp_password = getStringVarValue("smtp_password");
                    int smtp_port = getIntegerVarValue("smtp_port");
                    Boolean smtp_ssl = getBooleanVarValue("smtp_ssl");

                    MailMessage message = new MailMessage();

                    //split at the commas to allow multiple addresses
                    List<String> address_list = new List<string>(address.Split(','));
                    address_list.RemoveAll(delegate(String i) { return i == null || i.Trim().Length == 0; });

                    foreach (String addrs in address_list)
                        message.To.Add(addrs.Trim());

                    message.Subject = subject;
                    message.From = new MailAddress(smtp_mail);
                    message.Body = body;
                    SmtpClient smtp = new SmtpClient(smtp_host, smtp_port);
                    smtp.EnableSsl = smtp_ssl;
                    smtp.Credentials = new NetworkCredential(smtp_account, smtp_password);
                    smtp.Send(message);
                }
                catch (Exception e)
                {
                    DumpException(e);
                }
            }));

            mail_thread.IsBackground = true;
            mail_thread.Name = "mailer";
            mail_thread.Start();

            return true;
        }

        public bool SendSMS(String country, String carrier, String number, String message)
        {
            if (VMode)
            {
                ConsoleWarn("not sending SMS, ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            if (country == null || carrier == null || message == null || number == null)
                return false;

            country = country.Trim();
            carrier = carrier.Trim();
            number = number.Trim();

            if (country.Length == 0)
                throw new EvaluateException(FormatMessage("SMS country is empty", MessageType.Error));

            if (carrier.Length == 0)
                throw new EvaluateException(FormatMessage("SMS carrier is empty", MessageType.Error));

            if (number.Length == 0)
                throw new EvaluateException(FormatMessage("SMS number is empty", MessageType.Error));

            if (!CarriersDict.ContainsKey(country) || CarriersDict[country] == null)
                throw new EvaluateException(FormatMessage("uknown SMS country ^b" + country + "^n", MessageType.Error));

            number = Regex.Replace(number, @"[^+0-9]", "");

            if (number.Length == 0)
                throw new EvaluateException(FormatMessage("SMS number is empty after removing non-numeric characters", MessageType.Error));

            Dictionary<String, String> gateways = CarriersDict[country];

            if (!gateways.ContainsKey(carrier) || gateways[carrier] == null)
                throw new EvaluateException(FormatMessage("uknown SMS Gateway for carrier ^b" + carrier + "^n", MessageType.Error));

            String gateway = gateways[carrier];

            gateway = Regex.Replace(gateway, "number", number, RegexOptions.IgnoreCase);

            return SendMail(gateway, "Limit Activation", message);

        }

        public bool SendTaskbarNotification(String title, String message)
        {
            if (VMode)
            {
                ConsoleWarn("not sending taskbar notification, ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            //ExecuteCommand("procon.protected.playsound", title, message);
            ExecuteCommand("procon.protected.notification.write", title, message);
            return true;
        }

        public bool SendSoundNotification(String soundfile, String soundfilerepeat)
        {
            if (VMode)
            {
                ConsoleWarn("not sending sound notification, ^bvirtual_mode^n is ^bon^n");
                return false;
            }
            ExecuteCommand("procon.protected.playsound", soundfile, soundfilerepeat);
            //ExecuteCommand("procon.protected.notification.write", title, message);
            return true;
        }

        public String FriendlySpan(TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Days > 0 ? string.Format("{0:0} days, ", span.Days) : string.Empty,
                span.Hours > 0 ? string.Format("{0:0} hours, ", span.Hours) : string.Empty,
                span.Minutes > 0 ? string.Format("{0:0} minutes, ", span.Minutes) : string.Empty,
                span.Seconds > 0 ? string.Format("{0:0} seconds", span.Seconds) : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            return formatted;
        }


        public String FriendlyMapName(String mapFileName)
        {
            if (String.IsNullOrEmpty(mapFileName)) return String.Empty;
            String ret = mapFileName;
            if (friendlyMaps.ContainsKey(mapFileName)) ret = friendlyMaps[mapFileName];
            return ret;
        }

        public String FriendlyModeName(String modeName)
        {
            if (String.IsNullOrEmpty(modeName)) return String.Empty;
            String ret = modeName;
            if (friendlyModes.ContainsKey(modeName)) ret = friendlyModes[modeName];
            return ret;
        }

        private String GetCategory(Kill info)
        {
            DamageTypes category = DamageTypes.None;

            if (info == null || String.IsNullOrEmpty(info.DamageType))
                return "None";

            if (!WeaponsDict.TryGetValue(info.DamageType, out category)) 
            {
                category = DamageTypes.None;
            }

            return category.ToString();
        }

        public KillReasonInterface FriendlyWeaponName(String killWeapon)
        {
            KillReason r = new KillReason();
            r._name = killWeapon;
            DamageTypes category = DamageTypes.None;
            bool hasCategory = false;

            if (WeaponsDict.TryGetValue(killWeapon, out category)) {
                hasCategory = true;
            }

            if (game_version == "BF3")
            {
                Match m = Regex.Match(killWeapon, @"/([^/]+)$");
                r._name = killWeapon;
                if (m.Success) r._name = m.Groups[1].Value;
            }
            else if (killWeapon.StartsWith("U_")) // BF4 weapons // TBD BFH
            {
                String[] tParts = killWeapon.Split(new[]{'_'});

                if (tParts.Length == 2) { // U_Name
                    r._name = tParts[1];
                }  else if (tParts.Length == 3) { // U_Name_Detail
                    r._name = tParts[1];
                    r._detail = tParts[2];
                }  else if (tParts.Length >= 4) { // U_AttachedTo_Name_Detail
                    r._name = tParts[2];
                    r._detail = tParts[3];
                    r._attachedTo = tParts[1];
                } else {
                    DebugWrite("Warning: unrecognized weapon code: " + killWeapon, 5);
                }
            }
            else if (killWeapon != "Death" && hasCategory) // BF4 vehicles?
            {
                if (category == DamageTypes.VehicleAir 
                || category == DamageTypes.VehicleHeavy
                || category == DamageTypes.VehicleLight 
                || category == DamageTypes.VehiclePersonal
                || category == DamageTypes.VehicleStationary
                || category == DamageTypes.VehicleTransport
                || category == DamageTypes.VehicleWater)
                {
                    r._name = "Death";
                    r._vName = killWeapon;
                    Match m = Regex.Match(killWeapon, @"/([^/]+)/([^/]+)$");
                    if (m.Success) {
                        r._vName = m.Groups[1].Value;
                        r._vDetail = m.Groups[2].Value;
                    }

                    // Clean-up heuristics
                    String vn = r._vName;
                    if (vn.StartsWith("CH_"))
                        vn = vn.Replace("CH_", String.Empty);
                    else if (vn.StartsWith("Ch_"))
                        vn = vn.Replace("Ch_", String.Empty);
                    else if (vn.StartsWith("RU_"))
                        vn = vn.Replace("RU_", String.Empty);
                    else if (vn.StartsWith("US_"))
                        vn = vn.Replace("US_", String.Empty);

                    if (vn == "spec" && r._vDetail != null) {
                        if (r._vDetail.Contains("Z-11w"))
                            vn = "Z-11w";
                        else if (r._vDetail.Contains("DV15"))
                            vn = "DV15";
                        else vn = r._vDetail;
                    }
                    
                    if (vn.StartsWith("FAC_"))
                        vn = vn.Replace("FAC_", "Boat ");
                    else if (vn.StartsWith("FAC-"))
                        vn = vn.Replace("FAC-", "Boat ");
                    else if (vn.StartsWith("JET_"))
                        vn = vn.Replace("JET_", "Jet ");
                    else if (vn.StartsWith("FJET_"))
                        vn = vn.Replace("FJET_", "Jet ");

                    if (vn == "LAV25" && r._vDetail != null) {
                        if (r._vDetail == "LAV_AD") {
                            vn = "AA LAV_AD";
                        } else {
                            vn = "IFV LAV25";
                        }
                    }
                    
                    switch (vn) {
                        case "9K22_Tunguska_M": vn = "AA Tunguska"; break;
                        case "AC130": vn = "AC130 Gunship"; break;
                        case "AH1Z": vn = "Chopper AH1Z Viper"; break;
                        case "AH6": vn = "Chopper AH6 Littlebird"; break;
                        case "BTR-90": vn = "IFV BTR-90"; break;
                        case "F35": vn = "Jet F35"; break;
                        case "HIMARS": vn = "Artillery Truck M142 HIMARS"; break;
                        case "M1A2": vn = "MBT M1A2"; break;
                        case "Mi28": vn = "Chopper Mi28 Havoc"; break;
                        case "SU-25TM": vn = "Jet SU-25TM"; break;
                        case "Venom": vn = "Chopper Venom"; break;
                        case "Z-11w": vn = "Chopper Z-11w"; break;
                        case "KLR650": vn = "Bike KLR650"; break;
                        case "DPV": vn = "Jeep DPV"; break;
                        case "LTHE_Z-9": vn = "Chopper Z-9"; break;
                        case "FAV_LYT2021": vn = "Jeep LYT2021"; break;
                        case "GrowlerITV": vn = "Jeep Growler ITV"; break;
                        case "Ka-60": vn = "Chopper Ka-60"; break;
                        case "VDV Buggy": vn = "Jeep VDV Buggy"; break;
                        case "T90": vn = "MBT T90"; break;
                        case "A-10_THUNDERBOLT": vn = "Jet A-10 Thunderbolt"; break;
                        case "B1Lancer": vn = "Jet B1 Lancer"; break;
                        case "H6K": vn = "Jet H6K"; break;
                        case "Z-10w": vn = "Chopper Z-10w"; break;
                        case "RHIB": vn = "Boat RHIB"; break;
                        default: break;
                    }

                    r._vName = vn.Replace('_', ' ');
                }
            }
            return r;
        }

        public bool Log(String file, String message)
        {
            AppendData(StripModifiers(E(message) + NL), file);

            return true;
        }

        public bool KillPlayer(String name, int delay)
        {
            bool cVmode = (bool)Thread.GetData(VModeSlot);

            Thread delayed_kill = new Thread(new ThreadStart(delegate()
            {
                if (VModeSlot == null)
                    VModeSlot = Thread.AllocateDataSlot();

                // propagate the per-limit virtual mode to child thread
                Thread.SetData(VModeSlot, (object)cVmode);
                Thread.Sleep(delay * 1000);
                KillPlayer(name);
            }));
            
            delayed_kill.IsBackground = true;
            delayed_kill.Name = "delayed_kill";
            delayed_kill.Start();

            return !cVmode;
        }

        public bool KillPlayer(String name)
        {
            if (VMode)
            {
                ConsoleWarn("not killing ^b" + name + "^n, ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            if (!players.ContainsKey(name))
                return false;


            this.ServerCommand("admin.killPlayer", name);
            return true;
        }

        public bool KickPlayerWithMessage(String name, string message)
        {
            return KickPlayerWithMessage(name, message, true);
        }

        public bool KickPlayerWithMessage(String name, string message, bool tweet)
        {

            if (VMode)
            {
                ConsoleWarn("not kicking ^b" + name + "^n, ^bvirtual_mode^n is ^bon^n");
                return false;
            }


            if (isInWhitelist(name))
            {
                ConsoleWarn("not kicking ^b" + name + "^n, in white-list");
                return false;
            }

            this.ExecuteCommand("procon.protected.send", "admin.kickPlayer", name, message);
            RemovePlayer(name);

            if (getBooleanVarValue("tweet_my_server_kicks") && tweet)
                DefaultTweet("#Kick " + name + ",  @\"" + server_name + "\", for " + message + "");

            return true;
        }



        public bool EABanPlayerWithMessage(EABanType type, EABanDuration duration, String name, int minutes, string message)
        {

            if (VMode)
            {
                ConsoleWarn("not ea-banning ^b" + name + "^n, ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            if (!players.ContainsKey(name))
            {
                ConsoleError("cannot find player ^b" + name + "^n, not ea-banning");
                return false;
            }

            if (isInWhitelist(name))
            {
                ConsoleWarn("not ea-banning ^b" + name + "^n, in white-list");
                return false;
            }

            PlayerInfo player = players[name];

            // get the type field and value
            string typeField = "guid";
            string typeValue = player.EAGuid;

            if (type.Equals(EABanType.EA_GUID))
            {
                typeField = "guid";
                typeValue = player.EAGuid;
            }
            else if (type.Equals(EABanType.IPAddress))
            {

                typeField = "ip";

                typeValue = player.pbInfo.Ip;

                // remove the port number
                typeValue = Regex.Replace(typeValue, ":(.+)$", "");

                typeValue = typeValue.Trim();
            }
            else if (type.Equals(EABanType.Name))
            {
                typeField = "name";
                typeValue = player.Name;
            }

            // get the time out value
            string timeout = "seconds";
            if (duration.Equals(EABanDuration.Permanent))
                timeout = "perm";
            else if (duration.Equals(EABanDuration.Round))
                timeout = "rounds";
            else if (duration.Equals(EABanDuration.Temporary))
                timeout = "seconds";



            String suffix = String.Empty;

            if (duration.Equals(EABanDuration.Temporary))
                suffix = "(" + EABanDuration.Temporary.ToString() + "/" + minutes.ToString() + ")";
            else if (duration.Equals(EABanDuration.Round))
                suffix = "(" + EABanDuration.Round.ToString() + ")";
            else if (duration.Equals(EABanDuration.Permanent))
                suffix = "(" + EABanDuration.Permanent.ToString() + ")";

            String ea_message = message + suffix;

            int max_length = 80;
            if (ea_message.Length > max_length)
                ea_message = ea_message.Substring(0, max_length);

            if (duration.Equals(EABanDuration.Temporary))
                this.ExecuteCommand("procon.protected.send", "banList.add", typeField, typeValue, timeout, (minutes * 60).ToString(), ea_message);
            else if (duration.Equals(EABanDuration.Round))
                this.ExecuteCommand("procon.protected.send", "banList.add", typeField, typeValue, timeout, (1).ToString(), ea_message);
            else
                this.ExecuteCommand("procon.protected.send", "banList.add", typeField, typeValue, timeout, ea_message);

            this.ExecuteCommand("procon.protected.send", "banList.save");

            if (getBooleanVarValue("tweet_my_server_bans"))
                DefaultTweet("#EABan " + suffix + " " + name + " @\"" + server_name + "\", for " + message);

            KickPlayerWithMessage(name, message, false);

            return true;
        }



        public bool MovePlayer(String name, int TeamId, int SquadId, bool force)
        {
            if (VMode)
            {
                ConsoleWarn("not moving ^b" + name + "^n, ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            if (!players.ContainsKey(name))
            {
                ConsoleError("cannot find player ^b" + name + "^n, not moving");
                return false;
            }

            if (isInWhitelist(name))
            {
                ConsoleWarn("^b" + name + "^n is in white-list, not moving");
                return false;
            }

            this.ServerCommand("admin.movePlayer", name, TeamId.ToString(), SquadId.ToString(), force.ToString().ToLower());
            return true;
        }


        public bool PBCommand(String text)
        {

            if (VMode)
            {
                ConsoleWarn("not sending pb-command \"" + text + "\", ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            this.PunkBusterCommand(text);

            return true;
        }

        public bool SCommand(String text)
        {

            if (VMode)
            {
                ConsoleWarn("not sending server-command \"" + text + "\", ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            List<String> words = ParseCommand(new StringReader(text + "\n"));

            if (words == null || words.Count == 0)
                return false;

            this.ServerCommand(words.ToArray());

            return true;
        }

        /* simple command line parser */
        public List<String> ParseCommand(StringReader sin)
        {
            /* simple parser for command line */
            bool inside_string = false;
            bool previous_space = false;
            bool escape_char = false;

            String word = "";
            char c = (char)0;
            int data = -1;

            List<String> words = new List<String>();

            while ((data = sin.Read()) != -1)
            {
                c = (char)data;

                /* escaping quotes inside string */
                if (escape_char == true && c == (char)'"' && inside_string == true)
                {
                    word += char.ToString(c);
                    escape_char = false;
                    continue;
                }
                /* escaping the escape character anywhere */
                else if (escape_char == true && c == (char)'\\')
                {
                    word += char.ToString(c);
                    escape_char = false;
                    continue;
                }
                /* handle line continuation */
                else if (escape_char == true && c == (char)'n')
                {
                    if (inside_string)
                        word += char.ToString('\n');
                    escape_char = false;
                    continue;
                }
                else if (escape_char == true && c == (char)'t')
                {
                    word += char.ToString('\t');
                    escape_char = false;
                    continue;
                }
                else if (escape_char == true)
                {
                    /* finish readling the line */
                    sin.ReadLine();
                    ConsoleError("unknown escape sequence \\" + char.ToString(c));
                    return new List<String>();
                }
                /* detect start of string */
                else if (c == (char)'"' && inside_string == false)
                    inside_string = true;
                /* detect end of string */
                else if (c == (char)'"' && inside_string == true)
                    inside_string = false;
                /* detect escape character */
                else if (c == (char)'\\')
                    escape_char = true;
                /* detect unterminated stirng literal */
                else if (c == (char)'\n' && inside_string == true)
                {
                    ConsoleError("unterminated string literal");
                    return new List<String>();
                }
                /* skip white-space */
                else if (inside_string == false && previous_space == true &&
                        (c == (char)' ' || c == (char)'\t'))
                    continue;
                /* detect end of word */
                else if (inside_string == false &&
                        (c == (char)' ' || c == (char)'\t' ||
                         c == (char)'\n' || c == (char)'\r'))
                {
                    previous_space = true;
                    word = word.Trim();
                    if (word.Length > 0)
                        words.Add(word);
                    word = "";

                    if (c == (char)'\n')
                        return words;
                }
                else
                {
                    word += char.ToString(c);
                    previous_space = false;
                    escape_char = false; /* fail-safe */
                }

            }

            return null;
        }


        public bool PBBanPlayerWithMessage(PBBanDuration duration, String name, int minutes, string message)
        {

            if (VMode)
            {
                ConsoleWarn("not pb-banning ^b" + name + "^n, ^bvirtual_mode^n is ^bon^n");
                return false;
            }

            if (!players.ContainsKey(name))
            {
                ConsoleError("cannot find player ^b" + name + "^n, not pb-banning");
                return false;
            }

            if (isInWhitelist(name))
            {
                ConsoleWarn("^b" + name + "^n is in white-list, not pb-banning");
                return false;
            }

            String suffix = String.Empty;

            if (duration.Equals(PBBanDuration.Temporary))
                suffix = "(" + PBBanDuration.Temporary.ToString() + ":" + minutes + ")";
            else if (duration.Equals(PBBanDuration.Permanent))
                suffix = "(" + PBBanDuration.Permanent.ToString() + ")";

            String pb_message = message + suffix;


            if (duration.Equals(PBBanDuration.Permanent))
                this.ServerCommand("punkBuster.pb_sv_command", String.Join(" ", new string[] { "pb_sv_ban", name, pb_message, "|", "BC2!" }));
            else if (duration.Equals(PBBanDuration.Temporary))
                this.ServerCommand("punkBuster.pb_sv_command", String.Join(" ", new string[] { "pb_sv_kick", name, (minutes).ToString(), pb_message, "|", "BC2!" }));
            else
            {
                ConsoleError("unknown pb-ban duration, not pb-banning ^b" + name + "^n");
                return false;
            }

            this.ServerCommand("punkBuster.pb_sv_command", String.Join(" ", new string[] { "pb_sv_updbanfile" }));

            if (getBooleanVarValue("tweet_my_server_bans"))
                DefaultTweet("#PBBan " + suffix + " " + name + " @\"" + server_name + "\", for " + message);

            KickPlayerWithMessage(name, message, false);

            return true;
        }


        public static string list2string(List<string> list, string glue)
        {

            if (list == null || list.Count == 0)
                return "";
            else if (list.Count == 1)
                return list[0];

            string last = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);

            string str = "";
            foreach (string item in list)
                str += item + ", ";

            return str + glue + last;
        }


        public static string Encode(string str)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }
        public static string Decode(string str)
        {
            byte[] decbuff = Convert.FromBase64String(str.Replace(" ", "+"));
            return System.Text.Encoding.UTF8.GetString(decbuff);
        }

        public bool isInList(String item, String list_name)
        {
            try
            {
                if (item == null || list_name == null)
                    return false;

                if (!getBooleanVarValue("use_custom_lists"))
                    return false;


                foreach (KeyValuePair<String, CustomList> pair in lists)
                    if (pair.Value != null && pair.Value.Name.Equals(list_name))
                        return pair.Value.Contains(item);
            }
            catch (Exception e)
            {
                DumpException(e);
            }

            return false;

        }

        public bool isInWhitelist(String player)
        {
            return isInPlayerWhitelist(player) || isInClanWhitelist(player);
        }

        public bool isInPlayerWhitelist(String player)
        {
            return isInWhiteList(player, "player_white_list");
        }

        public bool isInClanWhitelist(String player)
        {
            return isInWhiteList(player, "clan_white_list");
        }


        public bool isInWhiteList(String name, String list_name)
        {
            if (!getBooleanVarValue("use_white_list"))
                return false;

            if (!getPluginVars().Contains(list_name))
            {
                ConsoleWarn("unknown white list ^b" + list_name + "^n");
                return false;
            }

            List<String> whitelist = getStringListVarValue(list_name);
            if (whitelist.Count == 0)
                return false;


            String field = "";
            if (Regex.Match(list_name, @"clan").Success)
            {
                /* make sure player is in the list */
                if (!players.ContainsKey(name))
                {
                    ConsoleWarn("could not check if ^b" + name + "^n is in clan white list, he is not in interval players list");
                    return false;
                }
                field = players[name].Tag;
            }
            else if (Regex.Match(list_name, @"player").Success)
                field = name;
            else
            {
                ConsoleWarn("white list ^b" + list_name + "^n does not contain 'player' or 'clan' sub-string");
                return false;
            }

            if (Regex.Match(field, @"^\s*$").Success)
                return false;

            return whitelist.Contains(field);
        }

        public List<String> GetReservedSlotsList()
        {
            return reserved_slots_list;
        }

        public static String makeRelativePath(String file)
        {
            String exe_path = Directory.GetParent(Application.ExecutablePath).FullName;
            String dll_path = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

            String rel_path = dll_path.Replace(exe_path, "");
            rel_path = Path.Combine(rel_path.Trim(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }), file);
            return rel_path;
        }

        public void dumpPairs(Dictionary<String, String> pairs, int fields_line)
        {
            int fcount = pairs.Count;
            int fline = fields_line;

            List<String> lines = new List<string>();
            String line = "";
            List<String> keys = new List<String>(pairs.Keys);

            for (int i = 0, j = 1; i < fcount; i++, j++)
            {
                String glue = "";
                if (line.Length == 0)
                    glue = "";

                line += String.Format("{0,30}", glue + keys[i] + "(" + pairs[keys[i]] + ")");
                if (j == fline)
                {
                    lines.Add(line);
                    j = 0;
                    line = "";
                }
            }

            if (line.Length > 0)
                lines.Add(line);

            foreach (String cline in lines)
                ConsoleWrite(cline);
        }
        
        public void dumpPairs(Dictionary<String, String> pairs, int indent, String logName)
        {
            int fcount = pairs.Count;
            List<String> keys = new List<String>(pairs.Keys);
            String line = "";
            bool first = true;
            
            for (int i = 1; i <= indent; ++i) {
                line = line + " ";
            }

            foreach (String k in keys) {
                if (first) {
                    first = false;
                    line = line + k + ":" + pairs[k];
                } else {
                    line = line + ", " + k + ":" + pairs[k];
                }
            }

            if (logName == null) {
                ConsoleWrite(line);
            } else {
                Log(logName, line);
            }
        }

        public List<PropertyInfo> getProperties(Type type, String scope)
        {
            List<PropertyInfo> plist = new List<PropertyInfo>();

            //use refection to get the field names

            PropertyInfo[] props = type.GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                Object[] attrs = props[i].GetCustomAttributes(true);

                if (attrs.Length > 0 && typeof(A).Equals(attrs[0].GetType()))
                {
                    A src = (A)attrs[0];
                    if (src.Scope.ToLower().Equals(scope.ToLower()))
                        plist.Add(props[i]);
                }
            }

            // sort the properties by name length, and if same length do it alphabetically
            plist.Sort(delegate(PropertyInfo left, PropertyInfo right)
            {
                int cmp = left.Name.Length.CompareTo(right.Name.Length);
                if (cmp == 0)
                    return left.Name.CompareTo(right.Name);

                return cmp;
            });

            return plist;
        }

        public Dictionary<String, String> buildPairs(object Object, List<PropertyInfo> plist)
        {
            Dictionary<String, String> pairs = new Dictionary<string, string>();

            for (int i = 0; i < plist.Count; i++)
            {

                PropertyInfo prop = plist[i];
                String name = prop.Name;


                double value = 0;

                if (!Double.TryParse(prop.GetValue(Object, null).ToString(), out value))
                {
                    ConsoleWarn("cannot cast " + Object.GetType().Name + "." + name + ", from " + prop.PropertyType.Name + " to " + value.GetType().Name);
                    value = double.NaN;
                }

                pairs.Add(name, Math.Round(value, 2).ToString());
            }
            return pairs;
        }

        private List<String> splitMessageText(String text, int max_length)
        {
            List<String> lines = new List<string>();
            while (text.Length > max_length)
            {
                String line = text.Substring(0, max_length);
                text = text.Substring(max_length);

                if (Regex.Match(line[max_length - 1].ToString(), @"[a-zA-Z0-9]$").Success)
                {
                    String putback = "";
                    for (int i = max_length - 1; i >= 0 && !Regex.Match(line[i].ToString(), @"\s+").Success; i--)
                        putback = line[i] + putback;

                    if (putback.Length < max_length)
                    {
                        line = line.Substring(0, max_length - putback.Length);
                        text = putback + text;
                    }
                }

                lines.Add(line);
            }

            if (text.Length > 0)
                lines.Add(text);

            return lines;
        }

        public double Aggregate(String property_name, Type type, Dictionary<String, Object> data)
        {
            double total = 0;
            PropertyInfo property = type.GetProperty(property_name);

            if (property == null)
            {
                ConsoleError(type.Name + ".^b" + property_name + "^n does not exist");
                return 0;
            }

            foreach (KeyValuePair<String, Object> pair in data)
            {
                if (pair.Value == null)
                    continue;

                double value = 0;
                // I know this is awefully slow, but better be safe for future changes
                if (!double.TryParse(property.GetValue(pair.Value, null).ToString(), out value))
                {
                    ConsoleError(type.Name + "." + property.Name + ", cannot be cast to ^b" + typeof(double).Name + "^n");
                    return 0;
                }
                total += value;
            }

            return total;
        }


        /* External plugin support */

        public bool IsOtherPluginEnabled(String className, String methodName)
        {
            List<MatchCommand> registered = this.GetRegisteredCommands();
            foreach (MatchCommand command in registered)
            {
                if (command.RegisteredClassname.Equals(className) && command.RegisteredMethodName.Equals(methodName))
                {
                    return true;
                }
            }
            return false;
        }

        public void CallOtherPlugin(String className, String methodName, Hashtable parms)
        {
            try
            {
                String json = JSON.JsonEncode(parms);
                String filtered = json.Replace('{','(').Replace('}',')');
                filtered = filtered.Substring(0, Math.Min(filtered.Length, 512));
                DebugWrite("Calling ^b" + className + "." + methodName + "^n(" + filtered + "...)", 5);
                this.ExecuteCommand("procon.protected.plugins.call", className, methodName, this.GetType().Name, json);
            }
            catch (Exception e)
            {
                DumpException(e);
            }
        }

        public DateTime GetLastPluginDataUpdate() // return timestamp for the last time InsaneLimits.UpdatePluginData() was called
        {
            return last_data_change;
        }

        public void UpdatePluginData(params String[] parms)
        {
            if (parms.Length != 4)
            {
                ConsoleWarn("UpdatePluginData called with incorrect parameter count: " + parms.Length);
                return;
            }
            /*
            parms[0]: Name of caller (plugin class)
            parms[1]: Name of the dictionary type: "bool", "double", "int", "string" (not possible to pass object type)
            parms[2]: Key
            parms[3]: Stringification of value
            */
            if (String.IsNullOrEmpty(parms[0])) {
                ConsoleWarn("UpdatePluginData parms[0]: caller name is invalid!");
                return;
            }
            if (String.IsNullOrEmpty(parms[1])) {
                ConsoleWarn("UpdatePluginData parms[1]: type is invalid!");
                return;
            }
            if (String.IsNullOrEmpty(parms[2])) {
                ConsoleWarn("UpdatePluginData parms[2]: key is invalid!");
                return;
            }
            try
            {
                String calledFrom = parms[0];
                Type type = typeof(String);
                switch (parms[1]) 
                {
                    case "bool": type = typeof(bool); break;
                    case "double": type = typeof(double); break;
                    case "int": type = typeof(int); break;
                    default: break;
                }
                String key = parms[2];
                Object value = parms[3];

                if (type == typeof(bool))
                {
                    bool v = false;
                    Boolean.TryParse(parms[3], out v);
                    value = (Boolean)v;
                }
                else if (type == typeof(double))
                {
                    double v = 0;
                    Double.TryParse(parms[3], out v);
                    value = (Double)v;
                }
                else if (type == typeof(int))
                {
                    int v = 0;
                    Int32.TryParse(parms[3], out v);
                    value = (Int32)v;
                }

                DataDict.set(type, key, value);
                last_data_change = DateTime.Now;
                DebugWrite("Plugin ^b" + calledFrom + "^n, updated (" + parms[1] + ") plugin.Data[" + key + "]", 5);
            }
            catch (Exception) {}
        }
        
        /* Stats fetching support */

        public bool IsCacheEnabled(bool verbose)
        {
            List<MatchCommand> registered = this.GetRegisteredCommands();
            foreach (MatchCommand command in registered)
            {
                if (command.RegisteredClassname.Equals("CBattlelogCache") && command.RegisteredMethodName.Equals("PlayerLookup"))
                {
                    if (verbose) DebugWrite("^bBattlelog Cache^n plugin will be used for stats fetching!", 3);
                    return true;
                }
                else
                {
                    DebugWrite("Registered P: " + command.RegisteredClassname + ", M: " + command.RegisteredMethodName, 7);
                }
            }
            if (verbose) DebugWrite("^1^bBattlelog Cache^n plugin is disabled; installing/updating and enabling the plugin is recommended for Insane Limits!", 3);
            return false;
        }
        
        public String SendCacheRequest(String playerName, String requestType)
        {
            /* 
            Called in the fetch_thread_loop thread, but defined in the
            main class in order to have access to all the wait handles.
            */
            Hashtable request = new Hashtable();
            request["playerName"] = playerName;
            request["pluginName"] = "InsaneLimits";
            request["pluginMethod"] = "CacheResponse";
            request["requestType"] = requestType;
            
            // Set up response entry
            lock (cacheResponseTable) {
                cacheResponseTable[playerName] = null;
            }

            // Send request
            if (!plugin_enabled) return String.Empty;
            DateTime since = DateTime.Now;
            this.ExecuteCommand("procon.protected.plugins.call", "CBattlelogCache", "PlayerLookup", JSON.JsonEncode(request));

            // block for reply
            DebugWrite("^b" + requestType + "(" + playerName + ")^n, waiting for cache to respond", 5);
            double maxWait = Convert.ToDouble(getIntegerVarValue("wait_timeout"));
            while (DateTime.Now.Subtract(since).TotalSeconds < maxWait) {
                if (!plugin_enabled) return String.Empty;
                // Give some time for the cache to respond
                reply_handle.WaitOne(500);
                reply_handle.Reset();
                if (!plugin_enabled) return String.Empty;
                lock (cacheResponseTable) {
                    if (cacheResponseTable.ContainsKey(playerName) && cacheResponseTable[playerName] != null) break;
                }
            }
            
            bool ok = false;
            
            lock (cacheResponseTable) {
                ok = (cacheResponseTable.ContainsKey(playerName) && cacheResponseTable[playerName] != null);
            }
            if (!ok) {
                DebugWrite(requestType + "(" + playerName + ") timed out, request exceeded " + maxWait.ToString("F1") + " seconds! Network congestion or another plugin lagging Procon?", 4);
                lock (cacheResponseTable) {
                    if (cacheResponseTable.ContainsKey(playerName)) {
                        cacheResponseTable.Remove(playerName);
                    }
                }
                return String.Empty;
            }
            
            String r = String.Empty;
            
            lock (cacheResponseTable) {
                if (cacheResponseTable.ContainsKey(playerName)) {
                    r = cacheResponseTable[playerName];
                    cacheResponseTable.Remove(playerName);
                }
            }
            
            Hashtable header = (Hashtable)JSON.JsonDecode(r);
            
            if (header == null) {
                DebugWrite(requestType + "(" + playerName + "), failed, header is null", 4);
                return r;
            }
            
            double fetchTime = -1;
            Double.TryParse((String)header["fetchTime"], out fetchTime);
            double age = -1;
            Double.TryParse((String)header["age"], out age);
            
            if (fetchTime > 0) {
                DebugWrite(requestType + "(" + playerName + "), cache refreshed from Battlelog, took ^2" + fetchTime.ToString("F1") + " seconds", 5);
            } else if (age > 0) {
                TimeSpan a = TimeSpan.FromSeconds(age);
                DebugWrite(requestType + "(" + playerName + "), cached stats used, age is " + a.ToString().Substring(0,8), 5);
            }
            DebugWrite("^2^bTIME^n took " + DateTime.Now.Subtract(since).TotalSeconds.ToString("F2") + " secs, cache lookup for " + playerName, 5);
            
            return r;
        }
        
        public void CacheResponse(params String[] response)
        {
            /*
            Called from the Battlelog Cache plugin Response thread
            */
            String val = null;
            DebugWrite("CacheResponse called with " + response.Length + " parameters", 5);
            if (getIntegerVarValue("debug_level") >= 5) {
                for (int i = 0; i < response.Length; ++i) {
                    DebugWrite("#" + i + ") Length: " + response[i].Length, 5);
                    val = response[i];
                    if (val.Length > 100) val = val.Substring(0, 100) + " ... ";
                    if (val.Contains("{")) val = val.Replace('{','<').Replace('}','>'); // ConsoleWrite doesn't like messages with "{" in it
                    DebugWrite("#" + i + ") Value: " + val, 5);
                }
            }
            
            String key = response[0]; // Player's name
            val = response[1]; // JSON string
            
            bool ok = false;
            lock (cacheResponseTable) {
                ok = cacheResponseTable.ContainsKey(key);
            }
                
            if (!ok) {
                DebugWrite("^1WARNING: Unknown cache response for " + key + " (perhaps request timed out?)", 4);
                return;
            }
            
            lock (cacheResponseTable) {
                ok = (cacheResponseTable[key] == null);
                
            }
            if (!ok) {
                DebugWrite("^1WARNING: Cache response collision for " + key, 4);
                return;
            }
            
            lock (cacheResponseTable) {
                cacheResponseTable[key] = val;
            }
            DebugWrite("CacheResponse reply, signal SendCacheRequest to unblock", 7);
            reply_handle.Set();
        }

        /* R38/Procon 1.4.0.7 */


        public override void OnPlayerIdleDuration(string soldierName, int idleTime) 
        {
            DebugWrite("Got ^bOnPlayerIdleDuration^n: " + soldierName + ", " + idleTime, 8);
            if (!plugin_activated) return;

            try
            {
                if (String.IsNullOrEmpty(soldierName)) return;

                PlayerInfo pinfo = null;

                lock (players_mutex) {
                    if (players.ContainsKey(soldierName)) {
                        players.TryGetValue(soldierName, out pinfo);
                    }
                }

                if (pinfo == null) return;
                
                pinfo._idleTime = Math.Max(0, idleTime);
            } 
            catch (Exception e)
            {
                DebugWrite(e.Message, 5);
            }
        }

        public override void OnPlayerPingedByAdmin(string soldierName, int ping)
        {
            DebugWrite("Got ^bOnPlayerPingedByAdmin^n: " + soldierName + ", " + ping, 8);
            if (!plugin_activated) return;

            try
            {
                if (String.IsNullOrEmpty(soldierName)) return;

                PlayerInfo pinfo = null;

                lock (players_mutex) {
                    if (players.ContainsKey(soldierName)) {
                        players.TryGetValue(soldierName, out pinfo);
                    }
                }

                if (pinfo == null) return;
                
                int lastPing = Math.Max(0, Math.Min(ping, 1000));
                pinfo.Ping = lastPing;
                if (pinfo.MaxPing < lastPing) pinfo.MaxPing = lastPing;
                if (pinfo.MinPing > lastPing) pinfo.MinPing = lastPing;

                // Update median and average
                const int PQLEN = 5;
                const int PQMED = 2; // median index for PQLEN
                bool changeInPing = true;
                if (pinfo._pingQ.Count == PQLEN) {
                    // If last ping duplicates the median, skip it
                    changeInPing = (pinfo.MedianPing != lastPing);
                }

                if (changeInPing) {
                    pinfo._pingQ.Enqueue(lastPing);
                    while (pinfo._pingQ.Count > PQLEN) pinfo._pingQ.Dequeue();
                    List<int> p = new List<int>(pinfo._pingQ);
                    //  Average just needs more than 1, doesn't matter if it is sorted
                    if (p.Count > 1) {
                        int sum = 0;
                        foreach (int i in p) {
                            sum = sum + i;
                        }
                        pinfo.AveragePing = sum / p.Count;
                    } else {
                        pinfo.AveragePing = lastPing;
                    }
                    // Median must be PQLEN exactly
                    if (p.Count == PQLEN) {
                        p.Sort();
                        pinfo.MedianPing = p[PQMED];
                    } else {
                        // Otherwise it is the same as average
                        pinfo.MedianPing = pinfo.AveragePing;
                    }
                }
            } 
            catch (Exception e)
            {
                DebugWrite(e.Message, 5);
            }
        }

        public override void OnSquadLeader(int teamId, int squadId, string soldierName)
        {
            DebugWrite("Got ^bOnSquadLeader^n: " + soldierName + ", " + teamId + ", " + squadId, 8);

            if (teamId == 0 || squadId == 0)
                return;

            String key = teamId.ToString() + "/" + squadId;
            squadLeaders[key] = soldierName;

            resetUpdateTimer(WhichTimer.Squad);
        }

        public override void OnSquadIsPrivate(int teamId, int squadId, bool isPrivate)
        {
            DebugWrite("Got ^bOnSquadIsPrivate^n: " + teamId + ", " + squadId + ", " + isPrivate, 8);

            if (teamId == 0 || squadId == 0) return;

            String key = teamId.ToString() + "/" + squadId;
            if (isPrivate && !lockedSquads.Contains(key)) lockedSquads.Add(key);
            else if (!isPrivate && lockedSquads.Contains(key)) lockedSquads.Remove(key);

            resetUpdateTimer(WhichTimer.Squad);
        }

        public override void OnCtfRoundTimeModifier(int limit)
        {
            DebugWrite("Got ^bOnCtfRoundTimeModifier^n: " + limit, 8);
            if (!plugin_activated) return;

            this.ctfRoundTimeModifier = limit;
        }

        public override void OnGunMasterWeaponsPreset(int preset)
        {
            DebugWrite("Got ^bOnGunMasterWeaponsPreset^n: " + preset, 8);

            this.varGunMasterWeaponsPreset = preset;

            resetUpdateTimer(WhichTimer.Vars);
        }
        
        public override void OnVehicleSpawnAllowed(bool isEnabled)
        {
            DebugWrite("Got ^bOnVehicleSpawnAllowed^n: " + isEnabled, 8);

            this.varVehicleSpawnAllowed = isEnabled;

            resetUpdateTimer(WhichTimer.Vars);
        }

        public override void OnVehicleSpawnDelay(int limit)
        {
            DebugWrite("Got ^bOnVehicleSpawnDelay^n: " + limit, 8);

            this.varVehicleSpawnDelay = limit;

            resetUpdateTimer(WhichTimer.Vars);
        }

        public override void OnBulletDamage(int limit)
        {
            DebugWrite("Got ^bOnBulletDamage^n: " + limit, 8);

            this.varBulletDamage = limit;

            resetUpdateTimer(WhichTimer.Vars);
        }

        public override void OnSoldierHealth(int limit)
        {
            DebugWrite("Got ^bOnSoldierHealth^n: " + limit, 8);

            this.varSoldierHealth = limit;

            resetUpdateTimer(WhichTimer.Vars);
        }

        public override void OnGameModeCounter(int limit)
        {
            DebugWrite("Got ^bOnGameModeCounter^n: " + limit, 8);
            if (!plugin_activated) return;

            this.gameModeCounter = limit;

            resetUpdateTimer(WhichTimer.Vars);
        }

        public override void OnFriendlyFire(bool isEnabled)
        {
            DebugWrite("Got ^bOnFriendlyFire^n: " + isEnabled, 8);

            this.varFriendlyFire = isEnabled;

            resetUpdateTimer(WhichTimer.Vars);
        }

        public override void OnIdleTimeout(int limit)
        {
            DebugWrite("Got ^bOnIdleTimeout^n: " + limit, 8);

            this.varIdleTimeout = limit;

            resetUpdateTimer(WhichTimer.Vars);
        }

        public void OnPluginLoadingEnv(List<string> lstPluginEnv)
        {
            foreach (String env in lstPluginEnv)
            {
                DebugWrite("Got ^bOnPluginLoadingEnv: " + env, 8);
            }
            game_version = lstPluginEnv[1];
            ConsoleWrite("^2Game Version = " + lstPluginEnv[1]);
        }

        // BF4

        public override void OnCommander(bool isEnabled)
        {
            DebugWrite("Got ^bOnCommander^n: " + isEnabled, 5);

            this.varCommander = isEnabled;

            resetUpdateTimer(WhichTimer.Vars);
        }

        public override void OnMaxSpectators(int limit)
        {
            DebugWrite("Got ^bOnMaxSpectators^n: " + limit, 8);

            this.varMaxSpectators = limit;

            resetUpdateTimer(WhichTimer.Vars);
        }

        public override void OnServerType(string value)
        {
            DebugWrite("Got ^bOnServerType^n: " + value, 8);

            this.varServerType = value;

            resetUpdateTimer(WhichTimer.Vars);
        }

        public override void OnTeamFactionOverride(int teamId, int faction)
        {
            DebugWrite("Got ^bOnTeamFactionOverride^n: " + teamId + " " + faction, 8);

            if (this.serverInfo._Faction != null && teamId >= 0 && teamId < this.serverInfo._Faction.Length)
            {
                this.serverInfo._Faction[teamId] = faction;
            }
        }

    }








    public class StatsException : Exception
    {
        public int code = 0;
        public WebException web_exception = null;
        public StatsException(String message) : base(message) { }
        public StatsException(String message, int code) : base(message) { this.code = code; }
        public StatsException(String message, String url) : base(message + " (" + url + ")") { }
    }

    public class TwitterException : Exception
    {
        public int code = 0;
        public TwitterException(String message) : base(message) { }
        public TwitterException(String message, int code) : base(message) { this.code = code; }
    }


    public class CompileException : Exception
    {
        public CompileException(String message) : base(message) { }
    }

    public class EvaluationException : Exception
    {
        public EvaluationException(String message) : base(message) { }
    }


    public class BattleLog
    {
        private InsaneLimits plugin = null;


        //private HttpWebRequest req = null;
        //private CookieContainer cookies = null;

        private GZipWebClient client = null;
        private String curAddress = "";
        
        public class GZipWebClient : WebClient {
            private String ua;
            private bool compress;
            
            public GZipWebClient() {
                this.ua = "Mozilla/5.0 (compatible; PRoCon 1; Insane Limits)";
                base.Headers["User-Agent"] = ua;
                compress = true;
            }

            public GZipWebClient(bool compress) : this() {
                this.compress = compress;
            }

            public string GZipDownloadString(string address) {
                return this.GZipDownloadString(new Uri(address));
            }

            public string GZipDownloadString(Uri address) {
                base.Headers[HttpRequestHeader.UserAgent] = ua;
                
                if (compress == false)
                    return base.DownloadString(address);
                
                base.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                var stream = this.OpenRead(address);
                if (stream == null)
                    return "";
                
                var contentEncoding = ResponseHeaders[HttpResponseHeader.ContentEncoding];
                base.Headers.Remove(HttpRequestHeader.AcceptEncoding);

                Stream decompressedStream = null;
                StreamReader reader = null;
                if (!string.IsNullOrEmpty(contentEncoding) && contentEncoding.ToLower().Contains("gzip")) {
                    decompressedStream = new GZipStream(stream, CompressionMode.Decompress);
                    reader = new StreamReader(decompressedStream);
                }
                else {
                    reader = new StreamReader(stream);
                }
                var data =  reader.ReadToEnd();
                reader.Close();
                decompressedStream?.Close();
                stream.Close();
                return data;
            }
            
            public void SetProxy(String proxyURL)
            {
                if(!String.IsNullOrEmpty(proxyURL))
                {
                    Uri uri = new Uri(proxyURL);
                    this.Proxy = new WebProxy(proxyURL, true); 
                    if (!String.IsNullOrEmpty(uri.UserInfo))
                    {
                        string[] parameters = uri.UserInfo.Split(':');
                        if (parameters.Length < 2) 
                        {
                            return;
                        }
                        this.Proxy.Credentials = new NetworkCredential(parameters[0], parameters[1]);
                    }
                }
            }
        } 

        public BattleLog(InsaneLimits plugin)
        {
            this.plugin = plugin;

        }
        
        public void CleanUp()
        {
            client = null; // Release WebClient to avoid re-use error
            curAddress = "";
        }

        private String fetchWebPage(ref String html_data, String url)
        {
            try
            {
                if (client == null) {
                    curAddress = null;
                    client = new GZipWebClient(); 
                    // XXX String ua = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0; .NET CLR 3.5.30729)";
                }
                
                // proxy support
                if (plugin.getBooleanVarValue("use_battlelog_proxy")) {
                    // set proxy
                    try {
                        var address = plugin.getStringVarValue("proxy_url");
                        if (curAddress == null || client.Proxy == null || !curAddress.Equals(address)) {
                            client.SetProxy(address); 
                            curAddress = address;
                        }
                    }
                    catch (UriFormatException) {
                        plugin.ConsoleError("Invalid Proxy URL set!");
                    }
                }
                
                DateTime since = DateTime.Now;

                html_data = client.GZipDownloadString(url);
                
                /* TESTS
                String testUrl = "http://status.savanttools.com/?code=";
                html_data = client.DownloadString(testUrl + "429%20Too%20Many%20Requests");
                //html_data = client.DownloadString(testUrl + "509%20Bandwidth%20Limit%20Exceeded");
                //html_data = client.DownloadString(testUrl + "408%20Request%20Timeout");
                //html_data = client.DownloadString(testUrl + "404%20Not%20Found");
                */
                
                plugin.DebugWrite("^2^bTIME^n took " + DateTime.Now.Subtract(since).TotalSeconds.ToString("F2") + " secs, fetchWebPage: " + url, 5);

                if (Regex.Match(html_data, @"that\s+page\s+doesn't\s+exist", RegexOptions.IgnoreCase | RegexOptions.Singleline).Success)
                    throw new StatsException("^b" + url + "^n does not exist", 404);

                return html_data;

            }
            catch (WebException e)
            {
                client = null; // release WebClient
                if (e.Status.Equals(WebExceptionStatus.Timeout)) {
                    StatsException se = new StatsException("HTTP request timed-out");
                    se.web_exception = e;
                    throw se;
                } else {
                    throw;
                }
            }
            catch (Exception ae)
            {
                client = null; // release WebClient
                throw ae;
            }
            //return html_data;
        }
        
        private bool CheckSuccess(Hashtable json, out StatsException statsEx)
        {
            String m;
            
            if (json == null) {
                m = "JSON response is null!";
                plugin.DebugWrite(m, 5);
                statsEx =  new StatsException(m);
                return false;
            }
            
            if (!json.ContainsKey("type")) {
                m = "JSON response malformed: does not contain 'type'!";
                plugin.DebugWrite(m, 5);
                statsEx =  new StatsException(m);
                return false;
            }
            
            String type = (String)json["type"];

            if (type == null) {
                m = "JSON response malformed: 'type' is null!";
                plugin.DebugWrite(m, 5);
                statsEx =  new StatsException(m);
                return false;
            }

            if (Regex.Match(type, @"success", RegexOptions.IgnoreCase).Success) {
                statsEx = null;
                return true;
            }
            
            if (!json.ContainsKey("message")) {
                m = "JSON response malformed: does not contain 'message'!";
                plugin.DebugWrite(m, 5);
                statsEx =  new StatsException(m);
                return false;
            }

            String message = (String)json["message"];

            if (message == null) {
                m = "JSON response malformed: 'message' is null!";
                plugin.DebugWrite(m, 5);
                statsEx =  new StatsException(m);
                return false;
            }

            m = "Cache fetch failed (type: " + type + ", message: " + message + ")!";
            plugin.DebugWrite(m, 5);
            statsEx = new StatsException(m); 
            return false;
        }
        
        
        private String fetchJSON(ref String bigText, String url, String playerName, String requestType)
        {
            bool directFetchEnabled = plugin.getBooleanVarValue("use_direct_fetch");
            bool cacheEnabled = plugin.IsCacheEnabled(false);
            bool ok = false;
            
            bigText = String.Empty;
            
            if (cacheEnabled) {
                // block waiting for cache to respond
                bigText = plugin.SendCacheRequest(playerName, requestType);
                ok = !String.IsNullOrEmpty(bigText);
                if (ok) return String.Empty;
                // if !ok, fall back on direct fetch, if enabled
            }
            
            if (!ok && directFetchEnabled && url != null) {
                return fetchWebPage(ref bigText, url);
            }
            
            if (url == null && requestType == "clanTag") {
                return String.Empty; // caller may try direct
            }
            
            // Unable to fetch JSON
            plugin.DebugWrite("Unable to fetch stats for " + playerName + ", caching is disabled and direct fetch is disabled!", 4);
            throw new StatsException("stats fetching is disabled");
            
            //return String.Empty;
        }


        public PlayerInfo fetchStats(PlayerInfo pinfo)
        {
            try
            {
                bool directFetchEnabled = plugin.getBooleanVarValue("use_direct_fetch");
                bool cacheEnabled = plugin.IsCacheEnabled(false);
                
                String player = pinfo.Name;
                String result = String.Empty;
                String personaId = String.Empty;
                Hashtable json = null;
                //String type = null;
                //String message = null;
                StatsException statsEx = null;
                Hashtable data = null;

                if (!cacheEnabled && !directFetchEnabled) {
                    throw new StatsException("Unable to fetch stats for " + player + ", cache is disabled and direct fetching is disabled!");
                }

                /* First fetch the player's main page to get the persona id */
                
                bool okClanTag = false;
                if (cacheEnabled) {
                    /* Get clan tag from cache */
                    fetchJSON(ref result, null, player, "clanTag");
                    
                    json = (Hashtable)JSON.JsonDecode(result);

                    if (!CheckSuccess(json, out statsEx)) throw statsEx;

                    /* verify there is data structure */
                    Hashtable d = null;
                    if (!json.ContainsKey("data") || (d = (Hashtable)json["data"]) == null)
                        throw new StatsException("JSON clanTag response does not contain a ^bdata^n field, for " + player);

                    if (!d.ContainsKey("clanTag"))
                        throw new StatsException("JSON clanTag response does not contain a ^bclanTag^n field, for " + player);

                    String t = (String)d["clanTag"];
                    if (!String.IsNullOrEmpty(t)) pinfo.tag = t;
                    okClanTag = true;
                }

                if (!okClanTag && directFetchEnabled)
                {
                    if (!plugin.plugin_enabled) {
                        throw new StatsException("fetchStats aborted, disabling plugin ...");
                    }

                    String purl = null;
                    if (plugin.game_version == "BFHL") {
                        purl = "http://battlelog.battlefield.com/bfh/user/";
                    } else if (plugin.game_version == "BF4") {
                        purl = "http://battlelog.battlefield.com/bf4/user/";
                    } else {
                        purl = "http://battlelog.battlefield.com/bf3/user/";
                    }

                    fetchWebPage(ref result, purl + player);

                    if (!plugin.plugin_enabled) {
                        throw new StatsException("fetchStats aborted, disabling plugin ...");
                    }

                    /* Extract the persona id */
                    MatchCollection pid = null;
                    Match spid = null;
                    
                    if (plugin.game_version == "BFHL") {
                        spid = Regex.Match(result, @"agent\/" + player + @"\/stats\/(\d+)");
                    } else if (plugin.game_version == "BF4") {
                        pid = Regex.Matches(result, @"bf4/soldier/" + player + @"/stats/(\d+)(['""]|/\s*['""]|/[^/'""]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    } else {
                        pid = Regex.Matches(result, @"bf3/soldier/" + player + @"/stats/(\d+)(['""]|/\s*['""]|/[^/'""]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    }

                    if (spid == null) {
                        foreach (Match match in pid)
                            if (match.Success && !Regex.Match(match.Groups[2].Value.Trim(), @"(ps3|xbox)", RegexOptions.IgnoreCase).Success) {
                                personaId = match.Groups[1].Value.Trim();
                                break;
                            }
                    } else {
                        if (spid.Success)
                        {
                            personaId = spid.Groups[1].Value.Trim();
                        }
                    }


                    if (String.IsNullOrEmpty(personaId))
                        throw new StatsException("could not find persona-id for ^b" + player + "^n");

                    if (plugin.game_version == "BFHL") {
                        // Get the stats page
                        if (!plugin.plugin_enabled) return pinfo;
                        String bfhfurl = "http://battlelog.battlefield.com/bfh/agent/" + player + "/stats/" + personaId + "/pc/" + "?nocacherandom=" + Environment.TickCount;
                        fetchWebPage(ref result, bfhfurl);
                        if (!plugin.plugin_enabled) return pinfo;

                        // Extract the player tag
                        String bfhTag = String.Empty;         
                        Match tag = Regex.Match(result, @"\[\s*([a-zA-Z0-9]+)\s*\]\s*</span>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        if (tag.Success) {
                            bfhTag = tag.Groups[1].Value.Trim();
                        }
                        if (String.IsNullOrEmpty(bfhTag)) {
                            // No tag
                            pinfo.tag = String.Empty;
                            plugin.DebugWrite("^4Battlelog says ^b" + player + "^n has no BFHL tag", 5);
                        } else {
                            pinfo.tag = bfhTag;
                        }
                    } else if (plugin.game_version == "BF4") {

                        if (!plugin.plugin_enabled) {
                            throw new StatsException("fetchStats aborted, disabling plugin ...");
                        }

                        String turl = "http://battlelog.battlefield.com/bf4/warsawoverviewpopulate/" + personaId + "/1/";
                        fetchWebPage(ref result, turl);

                        if (!plugin.plugin_enabled) {
                            throw new StatsException("fetchStats aborted, disabling plugin ...");
                        }

                        json = (Hashtable)JSON.JsonDecode(result);

                        // check we got a valid response

                        /* verify we got a success message */
                        if (!CheckSuccess(json, out statsEx)) throw statsEx;

                        /* verify there is data structure */
                        if (!json.ContainsKey("data") || (data = (Hashtable)json["data"]) == null)
                            throw new StatsException("JSON response does not contain a ^bdata^n field, for " + player, turl);

                        // verify there is viewedPersonaInfo structure, okay if null!
                        Hashtable info = null;
                        if (!data.ContainsKey("viewedPersonaInfo") || (info = (Hashtable)data["viewedPersonaInfo"]) == null) {
                            // No tag
                            pinfo.tag = String.Empty;
                            plugin.DebugWrite("Battlelog says ^b" + player + "^n has no BF4 tag (no viewedPersonaInfo)", 5);
                        } else {
                            // Extract the player tag
                            String bf4Tag = String.Empty;
                            if (!info.ContainsKey("tag") || String.IsNullOrEmpty(bf4Tag = (String)info["tag"])) {
                                // No tag
                                pinfo.tag = String.Empty;
                                plugin.DebugWrite("^4Battlelog says ^b" + player + "^n has no BF4 tag", 5);
                            } else {
                                pinfo.tag = bf4Tag;
                            }
                        }
                    } else {
                        extractClanTag(result, pinfo);
                    }
                } 

                /* Next, get player's overview stats */
                
                if (!plugin.plugin_enabled) {
                    throw new StatsException("fetchStats aborted, disabling plugin ...");
                }

                String furl = null;
                if (plugin.game_version == "BFHL") {
                    furl = "http://battlelog.battlefield.com/bfh/warsawdetailedstatspopulate/" + personaId + "/1/";
                } else if (plugin.game_version == "BF4") {
                    furl = "http://battlelog.battlefield.com/bf4/warsawdetailedstatspopulate/" + personaId + "/1/";
                } else {
                    furl = "http://battlelog.battlefield.com/bf3/overviewPopulateStats/" + personaId + "/bf3-us-engineer/1/";
                }
                fetchJSON(ref result, furl, player, "overview");


                if (!plugin.plugin_enabled) {
                    throw new StatsException("fetchStats aborted, disabling plugin ...");
                }

                json = (Hashtable)JSON.JsonDecode(result);

                // check we got a valid response

                /* verify we got a success message */
                if (!CheckSuccess(json, out statsEx)) throw statsEx;

                /* verify there is data structure */
                if (!json.ContainsKey("data") || (data = (Hashtable)json["data"]) == null)
                    throw new StatsException("JSON response does not contain a ^bdata^n field, for " + player, furl);

                /* verify there is stats structure */
                Hashtable stats = null;
                String jsonOverviewStatsKey = (plugin.game_version == "BF3") ? "overviewStats" : "generalStats";
                if (!data.ContainsKey(jsonOverviewStatsKey) || (stats = (Hashtable)data[jsonOverviewStatsKey]) == null)
                    throw new StatsException("JSON response ^bdata^n does not contain ^b" + jsonOverviewStatsKey + "^n, for " + player, furl);

                /* extract the fields from the stats */
                extractBasicFields(stats, pinfo);

                /* verify there is a kitmap structure */
                Hashtable kitMap = null;
                if (!data.ContainsKey("kitMap") || (kitMap = (Hashtable)data["kitMap"]) == null) {
                    if (plugin.game_version == "BF3")
                        throw new StatsException("JSON response ^bdata^n does not contain ^bkitMap^n, for " + player, furl);
                    else {
                        kitMap = null;
                    }
                }

                /* Build the id->kit and kit->id maps */
                List<Dictionary<String, String>> maps = null;
                //Dictionary<String, String> kit2id = null;
                Dictionary<String, String> id2kit = null;
                if (kitMap != null) {
                    maps = buildKitMaps(kitMap);
                    //kit2id = maps[1];
                    id2kit = maps[1];
                } else {
                    id2kit = new Dictionary<String,String>();
                    id2kit["1"] = "assault";
                    id2kit["2"] = "engineer";
                    id2kit["8"] = "recon";
                    id2kit["16"] = "vehicle";
                    id2kit["32"] = "support";
                    id2kit["64"] = "general";
                    id2kit["2048"] = "commander";
                }


                /* verify there is kit times (seconds) structure */
                Hashtable kitTimes = null;
                if (!stats.ContainsKey("kitTimes") || (kitTimes = (Hashtable)stats["kitTimes"]) == null)
                    throw new StatsException("JSON response ^boverviewStats^n does not contain ^bkitTimes^n, for " + player, furl);

                /*  extract the kit times (seconds) */
                extractKitTimes(kitTimes, id2kit, pinfo, "_t");

                /* verify there is kit time (percent) structure */
                Hashtable kitTimesInPercentage = null;
                if (!stats.ContainsKey("kitTimesInPercentage") || (kitTimesInPercentage = (Hashtable)stats["kitTimesInPercentage"]) == null)
                    throw new StatsException("JSON response ^boverviewStats^n does not contain ^bkitTimesInPercentage^n, for " + player, furl);

                /*  extract the kit times (percentage) */
                extractKitTimes((Hashtable)stats["kitTimesInPercentage"], id2kit, pinfo, "_p");

                if (!plugin.plugin_enabled) {
                    throw new StatsException("fetchStats aborted, disabling plugin ...");
                }
                
                DateTime since = DateTime.Now;

                try {

                    String logName = @"Logs\" + plugin.server_host + "_" + plugin.server_port + @"\" + DateTime.Now.ToString("yyyyMMdd") + "_battle.log";

                    /* print the collected stats to log */
                    if (plugin.getBooleanVarValue("use_stats_log")) {
                        since = DateTime.Now;
                        
                        plugin.Log(logName, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + pinfo.FullName + ((okClanTag && cacheEnabled) ? " Battlelog CACHED stats: " : " Battlelog player stats:"));
                        pinfo.dumpStatProperties("web", logName);
                        
                        if (DateTime.Now.Subtract(since).TotalSeconds > 1) plugin.DebugWrite("^2^bTIME^n took " + DateTime.Now.Subtract(since).TotalSeconds.ToString("F2") + " secs, dumpStatProperties", 5);
                    }

                    
                    /* extract weapon level statistics */
                    List<BattlelogWeaponStats> wstats = null;
                    if (plugin.getBooleanVarValue("use_slow_weapon_stats") && plugin.game_version != "BFHL") {
                        wstats = extractWeaponStats(pinfo, personaId);
                    } else {
                        plugin.DebugWrite("^1^buse_slow_weapon_stats^n is ^bFalse^n or BFHL, skipping fetch of weapon stats", 5);
                    }

                    pinfo.BWS.setWeaponData(wstats);

                    if (!plugin.plugin_enabled) {
                        throw new StatsException("fetchStats aborted, disabling plugin ...");
                    }

                    if (wstats != null && plugin.getBooleanVarValue("use_stats_log")) {
                        since = DateTime.Now;
                        
                        String bwsBlob = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + pinfo.FullName + " Battlelog weapon stats:\n";
                        foreach (BattlelogWeaponStats bws in wstats) {
                            bwsBlob = bwsBlob + "    N:" + bws.Name + ", S:" + bws.Slug + ", C:" + bws.Category +  ", Code:" + bws.Code + ", K:" + bws.Kills.ToString("F0") + ", SF:" + bws.ShotsFired.ToString("F0") + ", SH:" + bws.ShotsHit.ToString("F0") + ", A:" + bws.Accuracy.ToString("F3") + ", HS:" + bws.Headshots.ToString("F0") + ", T:" + TimeSpan.FromSeconds(bws.TimeEquipped).ToString() + "\n";
                        }
                        bwsBlob = bwsBlob + "=====================\n";
                        plugin.AppendData(bwsBlob, logName); // raw version of Log()
                        
                        if (DateTime.Now.Subtract(since).TotalSeconds > 1) plugin.DebugWrite("^2^bTIME^n took " + DateTime.Now.Subtract(since).TotalSeconds.ToString("F2") + " secs, log weapon stats", 5);
                    }
                    plugin.DebugWrite("done logging stats for " + pinfo.Name, 5);
                
                } catch (Exception e) {
                    throw e;
                } finally {
                    pinfo.StatsError = false;
                }                
            }
            catch (StatsException e)
            {
                if (e.web_exception == null) {
                    if (plugin.getIntegerVarValue("debug_level") >= 4) plugin.ConsoleWarn("(StatsException) " + e.Message);
                } else {
                    plugin.DebugWrite("(StatsException) System.Net.WebException: " + e.web_exception.Message, 4);
                    pinfo._web_exception = e.web_exception;
                }

                if (e.code == 404) {
                    pinfo.Battlelog404 = true;
                }

                pinfo.StatsError = true;
            }
            catch (System.Net.WebException e)
            {
                plugin.DebugWrite("System.Net.WebException: " + e.Message, 4);
                pinfo.StatsError = true;
                pinfo._web_exception = e;
            }
            catch (Exception e)
            {
                pinfo.StatsError = true;
                plugin.DumpException(e);
            }
            finally
            {
                // Clean-up the cache response, if any
                bool didit = false;
                if (pinfo != null && pinfo.Name != null) {
                    lock (plugin.cacheResponseTable) {
                        if (plugin.cacheResponseTable.ContainsKey(pinfo.Name)) {
                            plugin.cacheResponseTable.Remove(pinfo.Name);
                            didit = true;
                        }
                    }
                    if (didit) {
                        plugin.DebugWrite("Finally cleaned up cacheResponseTable for " + pinfo.Name, 4);
                    }
                }
            }

            return pinfo;
        }

        public List<BattlelogWeaponStats> extractWeaponStats(PlayerInfo pinfo, String personaId)
        {
            /* extract per-weapon stats */
            String result = String.Empty;
            String player = pinfo.Name;
            Hashtable json = null;
            StatsException statsEx = null;

            if (!plugin.plugin_enabled) {
                throw new StatsException("fetchStats aborted, disabling plugin ...");
            }

            String furl = null;
            
            if (plugin.game_version == "BF4") {
                furl = "http://battlelog.battlefield.com/bf4/warsawWeaponsPopulateStats/" + personaId + "/1/";
            } else {
                furl = "http://battlelog.battlefield.com/bf3/weaponsPopulateStats/" + personaId + "/1";
            }
            fetchJSON(ref result, furl, player, "weapon");

            json = (Hashtable)JSON.JsonDecode(result);

            result = null;

            // check we got a valid response
            
            /* verify we got a success message */
            if (!CheckSuccess(json, out statsEx)) throw statsEx;

            /* verify there is data structure */
            Hashtable data = null;
            if (!json.ContainsKey("data") || (data = (Hashtable)json["data"]) == null)
                throw new StatsException("JSON weapon response was does not contain a ^bdata^n field, for " + player, furl);

            /* verify there is stats structure */
            ArrayList wstats = null;
            if (!data.ContainsKey("mainWeaponStats") || (wstats = (ArrayList)data["mainWeaponStats"]) == null)
                throw new StatsException("JSON weapon response ^bdata^n does not contain ^bmainWeaponStats^n, for " + player, furl);

            int count = 0;
            Type dtype = typeof(BattlelogWeaponStats);
            List<PropertyInfo> props = new List<PropertyInfo>(dtype.GetProperties());

            List<BattlelogWeaponStats> all_weapons = new List<BattlelogWeaponStats>();
            foreach (Object item in wstats)
            {

                if (!plugin.plugin_enabled) {
                    throw new StatsException("fetchStats aborted, disabling plugin ...");
                }

                String itemName = "(item " + all_weapons.Count.ToString() + ")";

                try
                {
                    if (item == null || !item.GetType().Equals(typeof(Hashtable)))
                        throw new Exception("weapon item invalid");

                    Hashtable wstat = null;
                    if ((wstat = (Hashtable)item) == null)
                        throw new Exception("weapon item null");


                    BattlelogWeaponStats bwstats = new BattlelogWeaponStats();

                    String ttmp = null;
                    if (!wstat.ContainsKey("name") || (ttmp = (String)wstat["name"]) != null)
                        itemName = ttmp;

                    List<String> keys = InsaneLimits.getBasicWJSONFieldKeys();
                    bool failed = false;
                    foreach (String key in keys)
                    {
                        if (!wstat.ContainsKey(key) || wstat[key] == null)
                        {
                            // For BF4, knife and similar weapons don't have the "headshots" property, so add a dummy
                            if (plugin.game_version == "BF4" && key == "headshots") {
                                wstat[key] = 0.0;
                            } else {
                                plugin.DebugWrite("JSON structure of weapon stat for ^b" + itemName + "^n does not contain ^b" + key + "^n, for " + player, 5);
                                failed = true;
                                break;
                            }
                        }

                        String pname = InsaneLimits.WJSON2Prop(key);
                        PropertyInfo prop = null;
                        if ((prop = dtype.GetProperty(pname)) == null)
                        {
                            plugin.DebugWrite(dtype.Name + " does not contain ^b" + pname + "^n property, for " + player, 5);
                            failed = true;
                            break;
                        }

                        Type ptype = prop.PropertyType;

                        Object value = wstat[key];

                        MethodInfo castMethod = this.GetType().GetMethod("Cast").MakeGenericMethod(ptype);
                        object castedObject = castMethod.Invoke(null, new object[] { value });

                        prop.SetValue((object)bwstats, castedObject, null);

                    }

                    // skip this weapon
                    if (failed)
                        continue;

                    all_weapons.Add(bwstats);

                }
                catch (Exception)
                {
                    count++;
                    plugin.DebugWrite("Battlelog weapon stats parse of ^b" + itemName + "^n failed, skipping ...", 5);
                    continue;
                }
            }

            if (count > 0)
                plugin.DebugWrite("could not parse " + count + " weapon" + ((count > 1) ? "s" : "") + " for ^b" + pinfo.Name + "^n", 4);

            return all_weapons;
        }

        public static T Cast<T>(object data)
        {
            return (T)data;
        }

        public void extractBasicFields(Hashtable stats, PlayerInfo pinfo)
        {
            try {
                if (stats == null) {
                    plugin.DebugWrite("extractBasicFields, overviewStats Hashtable is null", 5);
                    return;
                }
                List<String> keys = InsaneLimits.getBasicJSONFieldKeys(plugin.game_version);
                foreach (DictionaryEntry entry in stats)
                {
                    String entry_key = (String)(entry.Key.ToString());

                    try {

                        /* skip entries we are not interested in */
                        if (!keys.Contains(entry_key))
                            continue;

                        String entry_value = (String)(entry.Value.ToString());

                        double dValue = double.NaN;
                        if (Double.TryParse(entry_value, out dValue))
                            pinfo.ovalue[InsaneLimits.JSON2Key(entry_key, plugin.game_version)] = dValue;
                    } catch (Exception e) {
                        plugin.DebugWrite("^1^bNOTE^n^0: overviewStats problem with key ^b" + entry_key + "^n: " + e.Message, 5);
                    }
                }
                // After June 2013 Battlelog stats update, need to fix up kdRatio
                double kills = 0;
                double deaths = 0;
                if (Double.IsNaN(pinfo.ovalue[InsaneLimits.JSON2Key("kdRatio", plugin.game_version)])
                && !Double.IsNaN(kills = pinfo.ovalue[InsaneLimits.JSON2Key("kills", plugin.game_version)])
                && !Double.IsNaN(deaths = pinfo.ovalue[InsaneLimits.JSON2Key("deaths", plugin.game_version)])) {
                    deaths = (deaths == 0) ? 1 : deaths;
                    pinfo.ovalue[InsaneLimits.JSON2Key("kdRatio", plugin.game_version)] = (kills / deaths);
                }
            } catch (Exception e) {
                plugin.DebugWrite("^1^bNOTE^n^0: extractBasicFields problem: " + e.Message, 5);
            }
        }

        public void extractClanTag(String result, PlayerInfo pinfo)
        {
            /* Extract the player tag */
            Match tag = Regex.Match(result, @"\[\s*([a-zA-Z0-9]+)\s*\]\s*" + pinfo.Name, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (tag.Success)
                pinfo.tag = tag.Groups[1].Value;
        }

        public List<Dictionary<String, String>> buildKitMaps(Hashtable kitMap)
        {
            List<Dictionary<String, String>> maps = new List<Dictionary<string, string>>();

            Dictionary<String, String> kit2id = new Dictionary<string, string>();
            Dictionary<String, String> id2kit = new Dictionary<string, string>();
            kit2id.Add("vehicle", null);
            kit2id.Add("support", null);
            kit2id.Add("assault", null);
            kit2id.Add("engineer", null);
            kit2id.Add("recon", null);

            foreach (String id in kitMap.Keys)
            {
                Hashtable kit_detail = (Hashtable)kitMap[id];
                if (!kit_detail.ContainsKey("name"))
                    continue;

                /* extract the kit name */
                String kit = (String)(kit_detail["name"]).ToString();

                if (kit2id.ContainsKey(kit))
                {
                    kit2id[kit] = id;

                    if (!id2kit.ContainsKey(id))
                        id2kit.Add(id, kit);
                }
            }

            /* verify we found the ids for all kits */
            foreach (KeyValuePair<String, String> pair in kit2id)
                if (pair.Value == null)
                    throw new StatsException("could not find ^b" + pair.Key + "^n in the ^bkitMap^n");


            maps.Add(kit2id);
            maps.Add(id2kit);

            return maps;
        }

        public void extractKitTimes(Hashtable table, Dictionary<String, String> id2kit, PlayerInfo pinfo, String suffix)
        {
            if (suffix != null) {
                foreach (DictionaryEntry entry in table)
                {
                    String key = (String)(entry.Key).ToString();
                    String value = (String)(table[key]).ToString();


                    /* skip the ones we are not interested in */
                    if (!id2kit.ContainsKey(key))
                        continue;

                    String kit_name = id2kit[key];

                    double dValue = Double.NaN;
                    if (Double.TryParse(value, out dValue))
                        pinfo.ovalue[kit_name + suffix] = dValue;
                }
            }
        }


    }



    public class KillInfo : KillInfoInterface
    {
        public Kill kill;
        public BaseEvent type;
        public String _category;

        public DateTime _time = DateTime.Now;

        public String Weapon { get { return kill.DamageType; } }
        public bool Headshot { get { return kill.Headshot; } }
        public DateTime Time { get { return _time; } }
        public String Category { get { return _category; } }

        public KillInfo(Kill kill, BaseEvent type, String category)
        {
            this.kill = kill;
            this.type = type;
            this._category = category;
        }

    }


    public class KillReason : KillReasonInterface
    {
        public String _name = String.Empty;
        public String _detail = null;
        public String _attachedTo = null;
        public String _vName = null;
        public String _vDetail = null;

        public String Name { get { return _name;} } // weapon name or reason, like "Suicide"
        public String Detail { get { return _detail;} } // BF4: ammo or attachment
        public String AttachedTo { get { return _attachedTo; } } // BF4: main weapon when Name is a secondary attachment, like M320
        public String VehicleName { get { return _vName; } } // BF4: if Name is "Death", this is the vehicle's name
        public String VehicleDetail { get { return _vDetail; } } // BF4: if Name is "Death", this is the vehicle's detail (stuff after final slash)
        
        public KillReason() {}
        /*
        public KillReason(String name, String detail, String attachedTo)
        {
            _name = name;
            _detail = detail;
            _attachedTo = attachedTo;
        }
        public KillReason(String name, String detail, String attachedTo, String vehicleName, String vehicleDetail)
        {
            _name = name;
            _detail = detail;
            _attachedTo = attachedTo;
            _vName = vehicleName;
            _vDetail = vehicleDetail;
        }
        */
    }


    public class ServerInfo : ServerInfoInterface
    {
        InsaneLimits plugin = null;
        public CServerInfo data = null;
        List<MaplistEntry> mlist = null;
        List<TeamScore> _TeamTickets = null;
        Dictionary<int, double> _StartTickets = null;
        public int[] _Faction = null;
        
        List<String> _mapRotation = new List<String>();
        List<String> _modeRotation = new List<String>();
        List<int> _roundRotation = new List<int>();

        int _WinTeamId = 0;
        int[] indices = null;
        public DataDictionary DataDict = null;
        public DataDictionary RoundDataDict = null;

        public WeaponStatsDictionary W;


        public Dictionary<String, double> svalue;
        public Dictionary<String, double> rvalue;

        [A("map")]
        public int CurrentRound { get { return data.CurrentRound; } }
        [A("map")]
        public int TotalRounds { get { return data.TotalRounds; } }
        [A("map")]
        public String MapFileName { get { return (mlist == null) ? data.Map : mlist[MapIndex].MapFileName; } }
        [A("map")]
        public String Gamemode { get { return (mlist == null) ? data.GameMode : mlist[MapIndex].Gamemode; ; } }
        [A("map")]
        public String NextMapFileName { get { return (mlist == null) ? "" : mlist[NextMapIndex].MapFileName; } }
        [A("map")]
        public String NextGamemode { get { return (mlist == null) ? "" : mlist[NextMapIndex].Gamemode; } }
        [A("map")]
        public int MapIndex { get { return indices[0]; } }
        [A("map")]
        public int NextMapIndex { get { return indices[1]; } }
        [A("map")]
        public double GameModeCounter { get { return plugin.gameModeCounter; } }
        [A("map")]
        public double CTFRoundTimeModifier { get { return plugin.ctfRoundTimeModifier; } }

        public List<String> MapFileNameRotation { get { return _mapRotation; } }
        public List<String> GamemodeRotation { get { return _modeRotation; } }
        public List<int> LevelRoundsRotation { get { return _roundRotation; } }

        [A("round")]
        public int PlayerCount { get { return data.PlayerCount; } }

        [A("round")]
        public double KillsRound { get { return W.Aggregate("KillsRound"); } }
        [A("round")]
        public double DeathsRound { get { return W.Aggregate("DeathsRound"); } }
        [A("round")]
        public double HeadshotsRound { get { return W.Aggregate("HeadshotsRound"); } }
        [A("round")]
        public double SuicidesRound { get { return W.Aggregate("SuicidesRound"); } }
        [A("round")]
        public double TeamKillsRound { get { return W.Aggregate("TeamKillsRound"); } }
        [A("round")]
        public double TimeRound { get { return (double)data.RoundTime; } }

        [A("total")]
        public double KillsTotal { get { return W.Aggregate("KillsTotal"); } }
        [A("total")]
        public double DeathsTotal { get { return W.Aggregate("DeathsTotal"); } }
        [A("total")]
        public double HeadshotsTotal { get { return W.Aggregate("HeadshotsTotal"); } }
        [A("total")]
        public double SuicidesTotal { get { return W.Aggregate("SuicidesTotal"); } }
        [A("total")]
        public double TeamKillsTotal { get { return W.Aggregate("TeamKillsTotal"); } }
        [A("total")]
        public double TimeTotal { get { return DateTime.Now.Subtract(plugin.enabledTime).TotalSeconds; } }

        [A("total")]
        public double RoundsTotal { get { return svalue["rounds"]; } internal set { svalue["rounds"] = value; } }

        [A("total")]
        public double TimeUp { get { return data.ServerUptime; } }


        [A("total")]
        public int MaxPlayers { get { return data.MaxPlayerCount; } }


        public WeaponStatsInterface this[String WeaponName] { get { return W[WeaponName]; } }
        public DataDictionaryInterface Data { get { return (DataDictionaryInterface)DataDict; } }
        public DataDictionaryInterface RoundData { get { return (DataDictionaryInterface)RoundDataDict; } }
        public DataDictionaryInterface DataRound { get { return (DataDictionaryInterface)RoundDataDict; } }

        public int WinTeamId { get { return _WinTeamId; } internal set { _WinTeamId = value; } }

        public double RemainTickets(int TeamId)
        {
            return Math.Abs(Tickets(TeamId) - TargetTickets);
        }

        public double RemainTicketsPercent(int TeamId)
        {
            double stickets = Math.Max(StartTickets(TeamId), TargetTickets);
            double rtickets = RemainTickets(TeamId);

            if (stickets > 0)
                return Math.Round((rtickets / stickets) * 100.0, 2);

            return double.NaN;
        }

        public double StartTickets(int TeamId)
        {
            if (_StartTickets.ContainsKey(TeamId))
                return _StartTickets[TeamId];

            return double.NaN;
        }


        public double Tickets(int TeamId)
        {
            double value = double.NaN;
            if (data == null)
                return value;

            List<TeamScore> scores = (_TeamTickets == null) ? data.TeamScores : _TeamTickets;
            if (scores == null)
                return value;

            foreach (TeamScore score in scores)
                if (score != null && score.TeamID == TeamId)
                    return (double)score.Score;

            return value;
        }

        public double TargetTickets
        {
            get
            {
                //we can take a wild guess for the default, most of the time is 0, except in TDM, and SQDM
                double value = 0;
                if (data == null)
                    return value;

                List<TeamScore> scores = (_TeamTickets == null) ? data.TeamScores : _TeamTickets;
                if (scores == null)
                    return value;

                // all teams contain the target score (silly PRoCon), first one we find, we return
                // there is a defect in PRoCon prior to 1.1.3.0, where TargetScore was always incorrect
                foreach (TeamScore score in scores)
                    if (score != null)
                        return (double)score.WinningScore;

                return value;
            }
        }

        public int OppositeTeamId(int TeamId)
        {
            switch (TeamId)
            {
                case 1:
                    return 2;
                case 2:
                    return 1;
                case 3:
                    return 4;
                case 4:
                    return 3;
                default:
                    return 0;
            }
        }


        /* Meta Data */
        public String Port { get { return plugin.server_port; } }
        public String Host { get { return plugin.server_host; } }
        public String Name { get { return plugin.server_name; } }
        public String Description { get { return plugin.server_desc; } }
        public String GameVersion { get { return plugin.game_version; } }

        /* var.* value that is updated every update_interval seconds */
        public int BulletDamage { get { return plugin.varBulletDamage; } }
        public bool FriendlyFire { get { return plugin.varFriendlyFire; } }
        public int GunMasterWeaponsPreset { get { return plugin.varGunMasterWeaponsPreset; } }
        public double IdleTimeout { get { return plugin.varIdleTimeout; } } // seconds
        public int SoldierHealth { get { return plugin.varSoldierHealth; } }
        public bool VehicleSpawnAllowed { get { return plugin.varVehicleSpawnAllowed; } }
        public int VehicleSpawnDelay { get { return plugin.varVehicleSpawnDelay; } }
        // BF4
        public bool Commander { get { return plugin.varCommander; } }
        public int MaxSpectators { get { return plugin.varMaxSpectators; } }
        public String ServerType { get { return plugin.varServerType; } }
        public int GetFaction(int TeamId)
        {
            if (TeamId < 0 || TeamId >= _Faction.Length)
                return -1;
            return _Faction[TeamId];
        }


        public ServerInfo(InsaneLimits plugin, CServerInfo data, List<MaplistEntry> mlist, int[] indices)
        {
            this.plugin = plugin;
            this.data = data;
            this.mlist = mlist;
            this.indices = indices;

            W = new WeaponStatsDictionary(plugin);

            rvalue = new Dictionary<string, double>();
            svalue = new Dictionary<string, double>();
            List<String> fields = InsaneLimits.getGameFieldKeys();
            foreach (String field in fields)
            {
                if (!svalue.ContainsKey(field))
                    svalue.Add(field, 0);

                if (!rvalue.ContainsKey(field))
                    rvalue.Add(field, 0);
            }

            DataDict = new DataDictionary(plugin);
            RoundDataDict = new DataDictionary(plugin);
            ResetTickets();

            _Faction = new int[5]{-1,-1,-1,-1,-1};
        }

        private void ResetTickets()
        {
            /* initialize start tickets */
            _StartTickets = new Dictionary<int, double>();
            for (int i = 0; i < 32; i++)
                if (!_StartTickets.ContainsKey(i))
                    _StartTickets.Add(i, double.NaN);
                else
                    _StartTickets[i] = double.NaN;
        }

        public void updateField(String name, double value)
        {
            if (!rvalue.ContainsKey(name))
            {
                plugin.ConsoleError(this.GetType().Name + ", Round-Stats does not contain ^b" + name + "^n");
                return;
            }
            rvalue[name] += value;
        }

        public void AccumulateRoundStats()
        {
            RoundsTotal++;
            W.AccumulateRoundStats();
        }

        public void ResetRoundStats()
        {
            W.ResetRoundStats();
            ResetTickets();
        }

        public void updateRotation(List<MaplistEntry> mlist)
        {  
            _mapRotation.Clear();
            _modeRotation.Clear();
            _roundRotation.Clear();
            
            if (mlist == null) return;

            foreach (MaplistEntry m in mlist) {
                _mapRotation.Add(m.MapFileName);
                _modeRotation.Add(m.Gamemode);
                _roundRotation.Add(m.Rounds);
            }
        }

        public void updateMapList(List<MaplistEntry> mlist)
        {
            this.mlist = mlist;
            updateRotation(mlist);
        }

        public void updateIndices(int[] indices)
        {
            this.indices = indices;
        }

        public void updateData(CServerInfo data)
        {
            this.data = data;

            /* update the start tickets if needed */
            List<TeamScore> scores = (_TeamTickets == null) ? data.TeamScores : _TeamTickets;
            foreach (TeamScore ts in scores)
                if (ts != null && _StartTickets.ContainsKey(ts.TeamID) && Double.IsNaN(_StartTickets[ts.TeamID]))
                    _StartTickets[ts.TeamID] = ts.Score;
        }

        public void updateTickets(List<TeamScore> tickets)
        {
            this._TeamTickets = tickets;
        }

        public void dumpStatProperties(String scope)
        {
            List<PropertyInfo> plist = plugin.getProperties(this.GetType(), scope);

            Dictionary<String, String> pairs = plugin.buildPairs(this, plist);

            scope = scope.Substring(0, 1).ToUpper() + scope.Substring(1);

            plugin.ConsoleWrite("Server " + scope + "-Stats:");
            plugin.dumpPairs(pairs, 4);
        }

        public void dumpWeaponStats(String scope)
        {

            scope = scope.Substring(0, 1).ToUpper() + scope.Substring(1);

            plugin.ConsoleWrite("Server Weapon " + scope + "-Stats:");
            W.dumpStats(scope, "    ");
        }

    }



    public class A : Attribute
    {
        private String name;
        public String Name { get { return name; } }

        private String scope;
        public String Scope { get { return scope; } }

        private String pattern;
        public String Pattern { get { return pattern; } }


        public A(String scope)
        {
            this.scope = scope;
        }

        public A(String scope, String name, String pattern)
        {
            this.scope = scope;
            this.name = name;
            this.pattern = pattern;
        }
    }


    public class TeamInfo : TeamInfoInterface
    {
        public InsaneLimits plugin = null;
        public Dictionary<String, PlayerInfo> _players = null;
        public ServerInfo server = null;
        public int _TeamId = 0;

        public int TeamId { get { return _TeamId; } }
        public double ScoreRound { get { return Aggregate("ScoreRound"); } }
        public double KillsRound { get { return Aggregate("KillsRound"); } }
        public double DeathsRound { get { return Aggregate("DeathsRound"); } }
        public double TeamKillsRound { get { return Aggregate("TeamKillsRound"); } }
        public double TeamDeathsRound { get { return Aggregate("TeamDeathsRound"); } }
        public double SuicidesRound { get { return Aggregate("SuicidesRound"); } }
        public double HeadshotsRound { get { return Aggregate("HeadshotsRound"); } }

        public double TicketsRound { get { return server.Tickets(TeamId); } }
        public double Tickets { get { return server.Tickets(TeamId); } }
        public double RemainTickets { get { return server.RemainTickets(TeamId); } }
        public double RemainTicketsPercent { get { return server.RemainTicketsPercent(TeamId); } }
        public double StartTickets { get { return server.StartTickets(TeamId); } }
        // BF4
        public int Faction { get { return server.GetFaction(TeamId); } }

        //use a converter to return the list of players as PlayerInfoInterface
        public List<PlayerInfoInterface> players
        {
            get
            {
                return (new List<PlayerInfo>(_players.Values)).ConvertAll<PlayerInfoInterface>
                        (new Converter<PlayerInfo, PlayerInfoInterface>
                            (
                              delegate(PlayerInfo p) { return (PlayerInfoInterface)p; }
                            )
                        );
            }
        }

        public double Aggregate(String property_name)
        {
            Dictionary<String, Object> dict = new Dictionary<string, object>();
            foreach (KeyValuePair<String, PlayerInfo> pair in _players)
                dict.Add(pair.Key, (Object)pair.Value);

            return plugin.Aggregate(property_name, typeof(PlayerInfo), dict);
        }



        public TeamInfo(InsaneLimits plugin, int TeamId, Dictionary<String, PlayerInfo> players, ServerInfo server)
        {
            this.plugin = plugin;
            this._TeamId = TeamId;

            _players = new Dictionary<string, PlayerInfo>();
            List<String> keys = new List<string>(players.Keys);
            foreach (String pname in keys)
                if (players.ContainsKey(pname) || players[pname] != null)
                    if (players[pname].TeamId == TeamId)
                        _players.Add(pname, players[pname]);

            this.server = server;

        }
    }

    public class PlayerInfo : PlayerInfoInterface
    {
        public CPlayerInfo info;
        public Dictionary<String, double> ovalue;
        public Dictionary<String, double> rvalue;
        public Dictionary<String, double> svalue;
        public string tag = "";
        public InsaneLimits plugin;
        public CPunkbusterInfo pbInfo;
        public bool _stats_error = true;
        public bool _battlelog404 = false;
        public DateTime ctime = DateTime.Now;
        public String _last_chat = "";
        public double _score = 0;
        public WebException _web_exception = null;
        public int _ping = 0;
        public int _maxPing = 0;
        public int _minPing = 0;
        public int _medianPing = 0;
        public int _averagePing = 0;
        public Queue<int> _pingQ = new Queue<int>();
        public double _idleTime = 0;

        public WeaponStatsDictionary W = null;
        public BattlelogWeaponStatsDictionary BWS = null;
        public DataDictionary DataDict = null;
        public DataDictionary RoundDataDict = null;

        public Dictionary<String, List<KillInfoInterface>> tkvDict = null;
        public Dictionary<String, List<KillInfoInterface>> tkkDict = null;
        public Dictionary<String, List<KillInfoInterface>> vDict = null;
        public Dictionary<String, List<KillInfoInterface>> kDict = null;
        

        /* Online statistics (basic)*/
        [A("web", "Rank", @"ra.*")]
        public double Rank { get { return ovalue["rank"]; } }
        [A("web", "Kdr", @"kd.*")]
        public double Kdr { get { return ovalue["kdr"]; } }
        [A("web", "Kpm", @"kp.*")]
        public double Kpm { get { return ratio(Kills, Time / 60.0); } }
        [A("web", "Time", @"ti.*")]
        public double Time { get { return ovalue["time"]; } }
        [A("web", "Kills", @"ki.*")]
        public double Kills { get { return ovalue["kills"]; } }
        [A("web", "Wins", @"wi.*")]
        public double Wins { get { return ovalue["wins"]; } }
        [A("web", "Skill", @"sk.*")]
        public double Skill { get { return ovalue["skill"]; } }
        [A("web", "Spm", @"sp.*")]
        public double Spm { get { return ovalue["spm"]; } }
        [A("web", "Score", @"sc.*")]
        public double Score { get { return ovalue["score"]; } }
        [A("web", "Deaths", @"de.*")]
        public double Deaths { get { return ovalue["deaths"]; } }
        [A("web", "Losses", @"lo.*")]
        public double Losses { get { return ovalue["losses"]; } }
        [A("web", "Repairs", @"rep.*")]
        public double Repairs { get { return ovalue["repairs"]; } }
        [A("web", "Revives", @"rev.*")]
        public double Revives { get { return ovalue["revives"]; } }
        [A("web", "Accuracy", @"ac.*")]
        public double Accuracy { get { return ovalue["accuracy"]; } }
        [A("web", "Ressuplies", @"res.*")]
        public double Ressuplies { get { return ovalue["ressuplies"]; } }
        [A("web", "Quit Percent", @"qu[^ ]*\s*p.*")]
        public double QuitPercent { get { return ovalue["quit_p"]; } }
        [A("web", "Team Score", @"te[^ ]*\s*sc.*")]
        public double ScoreTeam { get { return ovalue["sc_team"]; } }
        [A("web", "Combat Score", @"co[^ ]*\s*sc.*")]
        public double ScoreCombat { get { return ovalue["sc_combat"]; } }
        [A("web", "Vehicle Score", @"ve[^ ]*\s*sc.*")]
        public double ScoreVehicle { get { return ovalue["sc_vehicle"]; } }
        [A("web", "Objective Score", @"ob[^ ]*\s*sc.*")]
        public double ScoreObjective { get { return ovalue["sc_objective"]; } }
        [A("web", "Vehicles Killed", @"ve[^ ]*\s*(ki|de).*")]
        public double VehiclesKilled { get { return ovalue["vehicles_killed"]; } }
        [A("web", "KillStreak Bonus", @"ki[^ ]*\s*(st).*")]
        public double KillStreakBonus { get { return ovalue["killStreakBonus"]; } }

        [A("web", "Kill Assists", @"killAssists")]
        public double KillAssists { get { return ovalue["killAssists"]; } }
        [A("web", "rsDeaths", @"rsDeaths")]
        public double ResetDeaths { get { return ovalue["rsDeaths"]; } }
        [A("web", "rsKills", @"rsKills")]
        public double ResetKills { get { return ovalue["rsKills"]; } }
        [A("web", "rsNumLosses", @"rsNumLosses")]
        public double ResetLosses { get { return ovalue["rsNumLosses"]; } }
        [A("web", "rsNumWins", @"rsNumWins")]
        public double ResetWins { get { return ovalue["rsNumWins"]; } }
        [A("web", "rsScore", @"rsScore")]
        public double ResetScore { get { return ovalue["rsScore"]; } }
        [A("web", "rsShotsFired", @"rsShotsFired")]
        public double ResetShotsFired { get { return ovalue["rsShotsFired"]; } }
        [A("web", "rsShotsHit", @"rsShotsHit")]
        public double ResetShotsHit { get { return ovalue["rsShotsHit"]; } }
        [A("web", "rsTimePlayed", @"rsTimePlayed")]
        public double ResetTime { get { return ovalue["rsTimePlayed"]; } }

        /* Online statistics (extra) */
        [A("web", "Recon Time", @"re[^ ]*\s*ti.*")]
        public double ReconTime { get { return ovalue["recon_t"]; } }
        [A("web", "Engineer Time", @"en[^ ]*\s*ti.*")]
        public double EngineerTime { get { return ovalue["engineer_t"]; } }
        [A("web", "Assault Time", @"as[^ ]*\s*ti.*")]
        public double AssaultTime { get { return ovalue["assault_t"]; } }
        [A("web", "Support Time", @"su[^ ]*\s*ti.*")]
        public double SupportTime { get { return ovalue["support_t"]; } }
        [A("web", "Vehicle Time", @"ve[^ ]*\s*ti.*")]
        public double VehicleTime { get { return ovalue["vehicle_t"]; } }
        [A("web", "Recon Percent", @"re[^ ]*\s*(pe|%).*")]
        public double ReconPercent { get { return ovalue["recon_p"]; } }
        [A("web", "Engineer Percent", @"en[^ ]*\s*(pe|%).*")]
        public double EngineerPercent { get { return ovalue["engineer_p"]; } }
        [A("web", "Assault Percent", @"as[^ ]*\s*(pe|%).*")]
        public double AssaultPercent { get { return ovalue["assault_p"]; } }
        [A("web", "Support Percent", @"su[^ ]*\s*(pe|%).*")]
        public double SupportPercent { get { return ovalue["support_p"]; } }
        [A("web", "Vehicle Percent", @"ve[^ ]*\s*(pe|%).*")]
        public double VehiclePercent { get { return ovalue["vehicle_p"]; } }


        /* Player data */

        public String Name { get { return pbInfo.SoldierName; } }
        public String FullName { get { return (Tag.Length > 0) ? "[" + Tag + "]" + Name : Name; } }
        public String FullDisplayName { get { return (Tag.Length > 0) ? "^b[^n" + Tag + "^b]^n^b" + Name + "^n" : "^b" + Name + "^n"; } }
        public String Tag { get { return tag; } }
        public String EAGuid { get { return info.GUID; } }
        public String IPAddress { get { return pbInfo.Ip; } }
        public String CountryCode { get { return pbInfo.PlayerCountryCode; } }
        public String CountryName { get { return pbInfo.PlayerCountry; } }
        public String PBGuid { get { return pbInfo.GUID; } }
        public int TeamId { get { return info.TeamID; } set { info.TeamID = value; } }
        public int SquadId { get { return info.SquadID; } set { info.SquadID = value; } }
        public int Ping { get { return _ping; } set { _ping = value; } }
        public int MaxPing { get { return _maxPing; } set { _maxPing = value; } }
        public int MinPing { get { return _minPing; } set { _minPing = value; } }
        public int MedianPing { get { return _medianPing; } set { _medianPing = value; } }
        public int AveragePing { get { return _averagePing; } set { _averagePing = value; } }
        public int Role { get { return info.Type; } }

        /* Round Statistics */
        [A("round", "Kdr", @"kd.*")]
        public double KdrRound { get { return ratio(KillsRound, DeathsRound); } }
        [A("round", "Kpm", @"kp.*")]
        public double KpmRound { get { return ratio(KillsRound, TimeRound / 60.0); } }
        [A("round", "Spm", @"sp.*")]
        public double SpmRound { get { return ratio(ScoreRound, TimeRound / 60.0); } }
        [A("round", "Score", @"sc.*")]
        public double ScoreRound { get { return _score; } set { _score = value; } }
        [A("round", "Kills", @"ki.*")]
        public double KillsRound { get { return W.Aggregate("KillsRound"); } }
        [A("round", "Deaths", @"de.*")]
        public double DeathsRound { get { return W.Aggregate("DeathsRound"); } }
        [A("round", "Headshots", @"h(e|s).*")]
        public double HeadshotsRound { get { return W.Aggregate("HeadshotsRound"); } }
        [A("round", "Team Kills", @"te[^ ]*\s*ki.*")]
        public double TeamKillsRound { get { return W.Aggregate("TeamKillsRound"); } }
        [A("round", "Team Deaths", @"te[^ ]*\s*de.*")]
        public double TeamDeathsRound { get { return W.Aggregate("TeamDeathsRound"); } }
        [A("round", "Suicides", @"su.*")]
        public double SuicidesRound { get { return W.Aggregate("SuicidesRound"); } }
        [A("round", "Time", @"ti.*")]
        public double TimeRound { get { return Math.Min(TimeTotal, ((plugin.serverInfo == null) ? TimeTotal : plugin.serverInfo.TimeRound)); } }

        /* Total In-Server Stats */
        [A("total", "Kdr", @"kd.*")]
        public double KdrTotal { get { return ratio(KillsTotal, DeathsTotal); } }
        [A("total", "Kpm", @"kp.*")]
        public double KpmTotal { get { return ratio(KillsTotal, TimeTotal / 60.0); } }
        [A("total", "Spm", @"sp.*")]
        public double SpmTotal { get { return ratio(ScoreTotal, TimeTotal / 60.0); } }
        [A("total", "Score", @"sc.*")]
        public double ScoreTotal { get { return svalue["score"] + ScoreRound; } internal set { svalue["score"] = value; } }
        [A("total", "Kills", @"ki.*")]
        public double KillsTotal { get { return W.Aggregate("KillsTotal"); } }
        [A("total", "Deaths", @"de.*")]
        public double DeathsTotal { get { return W.Aggregate("DeathsTotal"); } }
        [A("total", "Headshots", @"h(e|s).*")]
        public double HeadshotsTotal { get { return W.Aggregate("HeadshotsTotal"); } }
        [A("total", "Team Kills", @"te[^ ]*\s*ki.*")]
        public double TeamKillsTotal { get { return W.Aggregate("TeamKillsTotal"); } }
        [A("total", "Team Deaths", @"te[^ ]*\s*de.*")]
        public double TeamDeathsTotal { get { return W.Aggregate("TeamDeathsTotal"); } }
        [A("total", "Suicides", @"su.*")]
        public double SuicidesTotal { get { return W.Aggregate("SuicidesTotal"); } }
        [A("total", "Time", "ti.*")]
        public double TimeTotal { get { return DateTime.Now.Subtract(JoinTime).TotalSeconds; } }
        [A("total", "Rounds", @"ro.*")]
        public double RoundsTotal { get { return svalue["rounds"]; } internal set { svalue["rounds"] = value; } }

        public Dictionary<String, List<KillInfoInterface>> TeamKillVictims { get { return tkvDict; } }
        public Dictionary<String, List<KillInfoInterface>> TeamKillKillers { get { return tkkDict; } }
        public Dictionary<String, List<KillInfoInterface>> Victims { get { return vDict; } }
        public Dictionary<String, List<KillInfoInterface>> Killers { get { return kDict; } }



        /* Other data */

        public DateTime JoinTime { get { return ctime; } }
        public String LastChat { get { return _last_chat; } set { _last_chat = value; } }
        public bool Battlelog404 { get { return _battlelog404; } set { _battlelog404 = value; } }
        public bool StatsError { get { return _stats_error; } set { _stats_error = value; } }


        /* Whitelist info */
        public bool inClanWhitelist { get { return plugin.isInClanWhitelist(Name); } }
        public bool inPlayerWhitelist { get { return plugin.isInPlayerWhitelist(Name); } }
        public bool isInWhitelist { get { return plugin.isInWhitelist(Name); } }


        public WeaponStatsInterface this[String WeaponName] { get { return W[WeaponName]; } }
        public BattlelogWeaponStatsInterface GetBattlelog(String WeaponName) { return BWS[WeaponName]; }
        public DataDictionaryInterface Data { get { return (DataDictionaryInterface)DataDict; } }
        public DataDictionaryInterface RoundData { get { return (DataDictionaryInterface)RoundDataDict; } }
        public DataDictionaryInterface DataRound { get { return (DataDictionaryInterface)RoundDataDict; } }

        private void setField(String name, double value)
        {
            if (!rvalue.ContainsKey(name))
            {
                plugin.ConsoleError(this.GetType().Name + " Round-Stats does not contain ^b" + name + "^n");
                return;
            }

            double diff = value - rvalue[name];
            rvalue[name] = value;

            if (plugin.serverInfo == null || diff <= 0)
                return;

            plugin.serverInfo.updateField(name, diff);

        }

        public void updateInfo(CPlayerInfo info)
        {
            //hack, so that we can count score from 0
            if (Double.IsNaN(ScoreRound))
            {
                this.info.Score = info.Score;
                ScoreRound = 0;
            }

            int new_score = info.Score;
            int old_score = this.info.Score;
            int score_change = (new_score - old_score);

            this.info = info;


            ScoreRound += score_change;
        }

        public void AccumulateRoundStats()
        {
            // I know what you are thinking, WTF ... (take a look at the set/get methods)
            ScoreTotal = ScoreTotal;
            RoundsTotal++;
            W.AccumulateRoundStats();
        }

        public void ResetRoundStats()
        {
            ScoreRound = 0;
            this.info.Score = 0;
            W.ResetRoundStats();
        }

        public PlayerInfo(InsaneLimits plugin, CPunkbusterInfo pbInfo)
        {
            this.pbInfo = pbInfo;
            this.plugin = plugin;
            this.info = new CPlayerInfo(pbInfo.SoldierName, "", 0, 0);

            ovalue = new Dictionary<string, double>();
            svalue = new Dictionary<string, double>();
            rvalue = new Dictionary<string, double>();

            // fields for web stats
            List<String> fields = InsaneLimits.getBasicFieldKeys(plugin.game_version);
            fields.AddRange(InsaneLimits.getExtraFields());
            foreach (String field_name in fields)
                ovalue.Add(field_name, /* Double.NaN */ 0);

            // fields for game stats
            List<String> gfields = InsaneLimits.getGameFieldKeys();
            foreach (String field_name in gfields)
            {
                svalue.Add(field_name, 0);
                rvalue.Add(field_name, 0);
            }

            W = new WeaponStatsDictionary(plugin);
            BWS = new BattlelogWeaponStatsDictionary(plugin);
            ScoreRound = Double.NaN;

            DataDict = new DataDictionary(plugin);
            RoundDataDict = new DataDictionary(plugin);

            tkvDict = new Dictionary<string, List<KillInfoInterface>>();
            tkkDict = new Dictionary<string, List<KillInfoInterface>>();
            vDict = new Dictionary<string, List<KillInfoInterface>>();
            kDict = new Dictionary<string, List<KillInfoInterface>>();
        }

        public void teamKilled(PlayerInfo victim, Kill kinfo)
        {

        }


        public double ratio(double left, double right)
        {
            if (Double.IsNaN(left) || Double.IsNaN(right) || left <= 0)
                return 0;
            if (right <= 0)
                right = 1;

            return left / right;

        }

        public override string ToString()
        {
            List<String> values = new List<string>();
            foreach (String key in ovalue.Keys)
                values.Add(key + "(" + Math.Round(ovalue[key], 2) + ")");


            return "tag(" + tag + ")," + String.Join(", ", values.ToArray());
        }

        public void dumpStatProperties(String scope)
        {
            dumpStatProperties(scope, null);
        }

        public void dumpStatProperties(String scope, String logName)
        {
            List<PropertyInfo> plist = plugin.getProperties(this.GetType(), scope);

            Dictionary<String, String> pairs = plugin.buildPairs(this, plist);

            scope = scope.Substring(0, 1).ToUpper() + scope.Substring(1);

            String log = (logName == null) ? "plugin.log" : logName;
            
            plugin.DebugWrite(scope + "-Stats for " + FullDisplayName + " logged to: " + log, 3);
            plugin.dumpPairs(pairs, 4, logName);
        }



        public void dumpWeaponStats(String scope)
        {
            scope = scope.Substring(0, 1).ToUpper() + scope.Substring(1);
            plugin.ConsoleWrite("Weapon " + scope + "-Stats for " + FullDisplayName + ":");
            W.dumpStats(scope, "    ");
        }
    }

    public class DataDictionary : DataDictionaryInterface
    {
        InsaneLimits plugin = null;

        public Dictionary<Type, Dictionary<String, object>> data = new Dictionary<Type, Dictionary<string, object>>();

        public DataDictionary(InsaneLimits plugin)
        {
            this.plugin = plugin;

            Init();
        }

        public void Init()
        {
            List<Type> types = new List<Type>(new Type[] { typeof(string), typeof(int), typeof(double), typeof(bool), typeof(object) });

            foreach (Type type in types)
                if (!data.ContainsKey(type))
                    data.Add(type, new Dictionary<string, object>());
        }

        /* Generic set/get/unset/isset methods */

        public Object set(Type type, String key, Object value)
        {
            lock (data)
            {
                if (data.ContainsKey(type))
                {
                    if (!data[type].ContainsKey(key))
                        data[type].Add(key, value);
                    else
                        data[type][key] = value;

                    return data[type][key];
                }
            }
            plugin.ConsoleError(this.GetType().Name + " has no data of ^b" + type.Name + "^n type");
            return (Object)Activator.CreateInstance(type);
        }

        public Object get(Type type, String key)
        {
            bool unknownKey = false;
            lock (data)
            {
                if (data.ContainsKey(type))
                {
                    if (!data[type].ContainsKey(key))
                    {
                        unknownKey = true;
                    }
                    else
                    {
                        return data[type][key];
                    }
                }
            }
            if (unknownKey)
            {
                plugin.ConsoleError(this.GetType().Name + " has no ^b" + type.Name + "^n(" + key + ") key");
                return (Object)Activator.CreateInstance(type);
            }
            plugin.ConsoleError(this.GetType().Name + " has no data of ^b" + type.Name + "^n type");
            return (Object)Activator.CreateInstance(type);
        }

        public Object unset(Type type, String key)
        {
            bool unknownKey = false;
            lock (data)
            {
                if (data.ContainsKey(type))
                {
                    if (!data[type].ContainsKey(key))
                    {
                        unknownKey = true;
                    }
                    else
                    {
                        Object value = data[type][key];
                        data[type].Remove(key);

                        return value;
                    }
                }
            }
            if (unknownKey)
            {
                plugin.ConsoleWarn(this.GetType().Name + " has no ^b" + type.Name + "^n(" + key + ") key");
                return (Object)Activator.CreateInstance(type);
            }
            plugin.ConsoleError(this.GetType().Name + " has no data of ^b" + type.Name + "^n type");
            return (Object)Activator.CreateInstance(type);
        }

        public List<string> getKeys(Type type)
        {
            lock (data)
            {
                if (data.ContainsKey(type))
                {
                    return new List<string>(data[type].Keys);
                }
            }
            plugin.ConsoleError(this.GetType().Name + " has no data of ^b" + type.Name + "^n type");
            return new List<string>();
        }

        public void Clear()
        {
            lock (data)
            {
                data.Clear();
                Init();
            }
        }


        public bool isset(Type type, String key)
        {
            lock (data)
            {
                if (data.ContainsKey(type))
                {
                    return data[type].ContainsKey(key);
                }
            }
            plugin.ConsoleError(this.GetType().Name + " has no data of ^b" + type.Name + "^n type");
            return false;
        }

        /* String Data */
        public String setString(String key, String value)
        {
            return (String)set(typeof(string), key, (object)value);
        }

        public String getString(String key)
        {
            return (String)get(typeof(string), key);
        }

        public bool issetString(String key)
        {
            return isset(typeof(string), key);
        }

        public String unsetString(String key)
        {
            return (String)unset(typeof(string), key);
        }

        public List<String> getStringKeys()
        {
            return getKeys(typeof(string));
        }


        /* Int Data */
        public int setInt(String key, int value)
        {
            return (int)set(typeof(int), key, (object)value);
        }

        public int getInt(String key)
        {
            return (int)get(typeof(int), key);
        }

        public bool issetInt(String key)
        {
            return isset(typeof(int), key);
        }

        public int unsetInt(String key)
        {
            return (int)unset(typeof(int), key);
        }

        public List<String> getIntKeys()
        {
            return getKeys(typeof(int));
        }

        /* Double Data */
        public double setDouble(String key, double value)
        {
            return (double)set(typeof(double), key, (object)value);
        }

        public double getDouble(String key)
        {
            return (double)get(typeof(double), key);
        }

        public bool issetDouble(String key)
        {
            return isset(typeof(double), key);
        }

        public double unsetDouble(String key)
        {
            return (double)unset(typeof(double), key);
        }

        public List<String> getDoubleKeys()
        {
            return getKeys(typeof(double));
        }


        /* Bool Data */
        public bool setBool(String key, bool value)
        {
            return (bool)set(typeof(bool), key, (object)value);
        }

        public bool getBool(String key)
        {
            return (bool)get(typeof(bool), key);
        }

        public bool issetBool(String key)
        {
            return isset(typeof(bool), key);
        }

        public bool unsetBool(String key)
        {
            return (bool)unset(typeof(bool), key);
        }

        public List<String> getBoolKeys()
        {
            return getKeys(typeof(bool));
        }


        /* Object Data */
        public object setObject(String key, object value)
        {
            return (object)set(typeof(object), key, (object)value);
        }

        public object getObject(String key)
        {
            return (object)get(typeof(object), key);
        }

        public bool issetObject(String key)
        {
            return isset(typeof(object), key);
        }

        public object unsetObject(String key)
        {
            return (object)unset(typeof(object), key);
        }

        public List<String> getObjectKeys()
        {
            return getKeys(typeof(object));
        }
    }

    public class BattlelogWeaponStats : BattlelogWeaponStatsInterface
    {
        double _kills = 0;
        double _shots_hit = 0;
        double _shots_fired = 0;
        double _time_equipped = 0;
        double _headshots = 0;
        String _category = String.Empty;
        String _name = String.Empty;
        String _slug = String.Empty;
        String _code = String.Empty;

        public String Category { get { return _category; } set { _category = value; } }
        public String Name { get { return _name; } set { _name = value; } }
        public String Slug { get { return _slug; } set { _slug = value; } }
        public String Code { get { return _code; } set { _code = value; } }

        public double Kills { get { return _kills; } set { _kills = value; } }
        public double ShotsFired { get { return _shots_fired; } set { _shots_fired = value; } }
        public double ShotsHit { get { return _shots_hit; } set { _shots_hit = value; } }
        public double Accuracy { get { return (ShotsFired > 0 && ShotsHit > 0) ? ((ShotsHit / ShotsFired) * 100) : 0; } }
        public double Headshots { get { return _headshots; } set { _headshots = value; } }
        public double TimeEquipped { get { return _time_equipped; } set { _time_equipped = value; } }

    }

    public class WeaponStats : WeaponStatsInterface
    {
        double _kills = 0;
        double _kills_total = 0;

        double _deaths = 0;
        double _deaths_total = 0;

        double _suicides = 0;
        double _suicides_total = 0;

        double _teamkills = 0;
        double _teamkills_total = 0;

        double _teamdeaths = 0;
        double _teamdeaths_total = 0;

        double _headshots = 0;
        double _headshots_total = 0;

        [A("round")]
        public double KillsRound { get { return _kills; } internal set { _kills = value; } }
        [A("round")]
        public double DeathsRound { get { return _deaths; } internal set { _deaths = value; } }
        [A("round")]
        public double SuicidesRound { get { return _suicides; } internal set { _suicides = value; } }
        [A("round")]
        public double TeamKillsRound { get { return _teamkills; } internal set { _teamkills = value; } }
        [A("round")]
        public double TeamDeathsRound { get { return _teamdeaths; } internal set { _teamdeaths = value; } }
        [A("round")]
        public double HeadshotsRound { get { return _headshots; } internal set { _headshots = value; } }

        [A("total")]
        public double KillsTotal { get { return _kills_total + KillsRound; } internal set { _kills_total = value; } }
        [A("total")]
        public double DeathsTotal { get { return _deaths_total + DeathsRound; } internal set { _deaths_total = value; } }
        [A("total")]
        public double SuicidesTotal { get { return _suicides_total + SuicidesRound; } internal set { _suicides_total = value; } }
        [A("total")]
        public double TeamKillsTotal { get { return _teamkills_total + TeamKillsRound; } internal set { _teamkills_total = value; } }
        [A("total")]
        public double TeamDeathsTotal { get { return _teamdeaths_total + TeamDeathsRound; } internal set { _teamdeaths_total = value; } }
        [A("total")]
        public double HeadshotsTotal { get { return _headshots_total + HeadshotsRound; } internal set { _headshots_total = value; } }


        public void ResetRoundStats()
        {
            KillsRound = 0;
            DeathsRound = 0;
            SuicidesRound = 0;
            HeadshotsRound = 0;
            TeamKillsRound = 0;
            TeamDeathsRound = 0;
        }

        public void AccumulateRoundStats()
        {
            // I know you are thinking, WTF ... just look at the set/get
            KillsTotal = KillsTotal;
            DeathsTotal = DeathsTotal;
            SuicidesTotal = SuicidesTotal;
            HeadshotsTotal = HeadshotsTotal;
            TeamKillsTotal = TeamKillsTotal;
            TeamDeathsTotal = TeamDeathsTotal;
        }
    }

    public class WeaponStatsDictionary
    {
        InsaneLimits plugin = null;
        public Dictionary<String, WeaponStats> data;
        public WeaponStatsDictionary parent = null;
        WeaponStats NullWeaponStats = new WeaponStats();

        private void init(InsaneLimits plugin)
        {
            this.plugin = plugin;
            data = new Dictionary<string, WeaponStats>();
        }

        public WeaponStatsDictionary(InsaneLimits plugin)
        {
            init(plugin);
        }

        public WeaponStatsDictionary(InsaneLimits plugin, WeaponStatsDictionary parent)
        {
            init(plugin);
            this.parent = parent;
        }

        public WeaponStats this[String WeaponName] { get { return getWeaponData(WeaponName); } }


        private String bestWeaponMatch(String name)
        {
            return bestWeaponMatch(name, true);
        }

        private String bestWeaponMatch(String name, bool verbose)
        {

            bool EventWeapon = false;
            if (name.StartsWith(":") && (name = name.Substring(1)).Length > 0)
                EventWeapon = true;
            else if (name.StartsWith("U_")) // BF4
                EventWeapon = true;


            if (plugin.WeaponsDict.ContainsKey(name))
                return name;
            else if (EventWeapon)
            {
                plugin.DebugWrite("detected that weapon ^b" + name + "^n is not in dictionary, adding it", 4);
                try { plugin.WeaponsDict.Add(name, DamageTypes.None); }
                catch (Exception) { }
                return name;
            }

            int distance = 0;
            List<String> names = new List<string>(plugin.WeaponsDict.Keys);
            String new_name = plugin.bestMatch(name, names, out distance, false);
            if (new_name == null)
            {
                if (verbose)
                    plugin.ConsoleError("could not find weapon ^b" + name + "^n in dictionary");
                return null;
            }

            if (verbose)
                plugin.DebugWrite("could not find weapon ^b" + name + "^n, but found ^b" + new_name + "^n, edit distance of ^b" + distance + "^n", 4);
            return new_name;
        }

        public WeaponStats getWeaponData(String name)
        {

            try
            {
                // special case
                if (name.Equals("UnkownWeapon"))
                    return NullWeaponStats;

                // the easy case first, weapon is in dictionary
                name = bestWeaponMatch(name);

                if (name == null)
                    return NullWeaponStats;

                if (!data.ContainsKey(name))
                    data.Add(name, new WeaponStats());

                return data[name];
            }
            catch (Exception e)
            {
                plugin.DumpException(e);
            }

            return NullWeaponStats;
        }

        public void ResetRoundStats()
        {
            foreach (KeyValuePair<String, WeaponStats> pair in data)
                pair.Value.ResetRoundStats();

        }

        public void AccumulateRoundStats()
        {
            foreach (KeyValuePair<String, WeaponStats> pair in data)
                pair.Value.AccumulateRoundStats();
        }

        public double Aggregate(String property_name)
        {
            Dictionary<String, Object> dict = new Dictionary<string, object>();
            foreach (KeyValuePair<String, WeaponStats> pair in data)
                dict.Add(pair.Key, (Object)pair.Value);

            return plugin.Aggregate(property_name, typeof(WeaponStats), dict);
        }

        /*
        public double Aggregate(String property_name)
        {


            double total = 0;
            Type type = typeof(WeaponStats);
            PropertyInfo property = type.GetProperty(property_name);

            if (property == null)
            {
                plugin.ConsoleError(type.Name + ".^b" + property_name + "^n does not exist");
                return 0;
            }

            foreach (KeyValuePair<String, WeaponStats> pair in data)
            {
                if (pair.Value == null)
                    continue;

                double value = 0;
                // I know this is awefully slow, but better be safe for future changes
                if (!double.TryParse(property.GetValue(pair.Value, null).ToString(), out value))
                {
                    plugin.ConsoleError(type.Name + "." + property.Name + ", cannot be cast to ^b" + typeof(double).Name + "^n");
                    return 0;
                }
                total += value;
            }

            return total;
        }
        */

        public void dumpStats(String source, String prefix)
        {
            Type type = typeof(WeaponStats);
            List<PropertyInfo> properties = new List<PropertyInfo>(type.GetProperties());

            //remove the properties not matching source

            properties.RemoveAll(delegate(PropertyInfo property)
            {
                Object[] attrs = property.GetCustomAttributes(true);
                return (attrs.Length == 0 || !typeof(A).Equals(attrs[0].GetType()) || !((A)attrs[0]).Scope.ToLower().Equals(source.ToLower()));
            });



            foreach (KeyValuePair<String, WeaponStats> pair in data)
            {
                WeaponStats wstats = pair.Value;
                String weapon_name = pair.Key;

                List<String> properties_data = new List<string>();

                for (int i = 0; i < properties.Count; i++)
                {
                    PropertyInfo property = properties[i];
                    double value = 0;
                    if (!Double.TryParse(property.GetValue(wstats, null).ToString(), out value))
                        value = Double.NaN;

                    if (Double.IsNaN(value))
                    {
                        plugin.ConsoleError(type.Name + "." + property.Name + ", is not of " + typeof(double).Name + " type");
                        continue;
                    }
                    value = Math.Round(value, 2);

                    if (value == 0)
                        continue;

                    properties_data.Add(property.Name + "(" + value + ")");
                }

                if (properties_data.Count == 0)
                    continue;

                plugin.ConsoleWrite(prefix + weapon_name + " ^b=^n " + String.Join(", ", properties_data.ToArray()));
            }
        }
    }


    public class BattlelogWeaponStatsDictionary
    {
        InsaneLimits plugin = null;
        public Dictionary<String, BattlelogWeaponStats> data;
        BattlelogWeaponStats NullWeaponStats = new BattlelogWeaponStats();
        BattlelogWeaponStats UnknownWeaponStats = new BattlelogWeaponStats();
        BattlelogWeaponStats SkippedWeaponStats = new BattlelogWeaponStats();

        private void init(InsaneLimits plugin)
        {
            this.plugin = plugin;
            data = new Dictionary<string, BattlelogWeaponStats>();
            UnknownWeaponStats.Name = "UNKNOWN";
            UnknownWeaponStats.Kills = -1;
            UnknownWeaponStats.ShotsFired = -1;
            UnknownWeaponStats.Headshots = -1;
            SkippedWeaponStats.Name = "UNAVAILABLE";
            SkippedWeaponStats.Category = SkippedWeaponStats.Name;
            SkippedWeaponStats.Slug = SkippedWeaponStats.Name;
            SkippedWeaponStats.Code = SkippedWeaponStats.Name;
        }

        public BattlelogWeaponStatsDictionary(InsaneLimits plugin)
        {
            init(plugin);
        }

        public BattlelogWeaponStats this[String WeaponName] { get { return getWeaponData(WeaponName); } }

        private String bestWeaponMatch(String name)
        {
            return bestWeaponMatch(name, true);
        }

        private String bestWeaponMatch(String name, bool verbose)
        {
            /* Example mappings. RCON weapon name in [brackets].
            [Siaga20k]
            Category:Shotguns, Name:Saiga, Slug:saiga-12k, Code:sgSaiga, Kills:4, ShotsFired:93, ShotsHit:44, Accuracy:47.31, Headshots:2, TimeEquipped:00:05:41
            
            [Weapons/MP443/MP443_GM]
            Category:Handheld weapons, Name:MP443 LIT, Slug:mp443-tact, Code:pMP443L, Kills:0, ShotsFired:8, ShotsHit:0, Accuracy:0.00, Headshots:0, TimeEquipped:00:00:25
            
            [?]
            Category:Handheld weapons, Name:MP443 Silenced, Slug:mp443-supp, Code:pMP443S, Kills:0, ShotsFired:0, ShotsHit:0, Accuracy:0.00, Headshots:0, TimeEquipped:00:00:00
            
            [Weapons/MP443/MP443]
            Category:Handheld weapons, Name:MP 443, Slug:mp443, Code:pMP443, Kills:18, ShotsFired:782, ShotsHit:95, Accuracy:12.15, Headshots:4, TimeEquipped:00:47:30
            
            [SCARL]
            Category:Assault rifles, Name:XP2 SCARL, Slug:scar-l, Code:arSCARL, Kills:89, ShotsFired:4431, ShotsHit:567, Accuracy:12.80, Headshots:11, TimeEquipped:01:22:29
            
            [FGM-148]
            Category:Launchers, Name:FGM-148 JAVELIN, Slug:fgm-148-javelin, Code:wLATJAV, Kills:8, ShotsFired:115, ShotsHit:68, Accuracy:59.13, Headshots:0, TimeEquipped:00:36:07
            
            BF4 Example
            [U_FY-JS]
            Category:Sniper Rifles, Name:WARSAW_ID_P_WNAME_FYJS, Slug:fy-js, Code:wSR, Kills:385, ShortsFired:14659, ShotsHit:2192, Accuracy:0.15, Headshots:74, TimeEquipped:04:55:23

            */

            String shortName = name; // RCON name
            Match m = Regex.Match(name, @"/([^/]+)$");
            if (m.Success) shortName = m.Groups[1].Value;
            String bf4Normalized = name;

            // User mapping
            if (plugin.rcon2bw_user.ContainsKey(name)) return plugin.rcon2bw_user[shortName];

            if (plugin.game_version == "BF4" || plugin.game_version == "BFHL") {
                KillReasonInterface kr = plugin.FriendlyWeaponName(bf4Normalized);
                shortName = kr.Name.ToUpper();
                bf4Normalized = "WARSAW_ID_P_WNAME_" + shortName;
                plugin.DebugWrite("^9bestWeaponMatch(" + name + "), BF4 normalized = " + bf4Normalized, 8);
            }
            
            // Exact match?
            if (plugin.game_version == "BF4" || plugin.game_version == "BFHL") {
                if (data.ContainsKey(bf4Normalized)) return bf4Normalized;
                // Try alternative name
                bf4Normalized = "WARSAW_ID_I_NAME_" + shortName;
                if (data.ContainsKey(bf4Normalized)) return bf4Normalized;
            } else {
                if (data.ContainsKey(name)) return name;
                if (data.ContainsKey(shortName)) return shortName;
            }
            
            // Special cases
            if (plugin.game_version == "BF4" || plugin.game_version == "BFHL") {
                if (plugin.rcon2bwbf4.ContainsKey(shortName)) return plugin.rcon2bwbf4[shortName];
            } else {
                if (plugin.rcon2bw.ContainsKey(shortName)) return plugin.rcon2bw[shortName];
            }
            
            if (data.Keys.Count == 0) {
                if (verbose) plugin.DebugWrite("^1^bWARNING^0^n: GetBattlelog: use_slow_weapon_stats was False at time of fetch for this player, so no stats to find for " + shortName, 4);
                return shortName;
            }

            // Narrow down keys to those that contain shortName as a substring
            List<String> keys = new List<string>(data.Keys);
            
            List<String> subKeys = new List<String>();
            foreach (String k in keys) {
                if (Regex.Match(k, shortName, RegexOptions.IgnoreCase).Success) subKeys.Add(k);
            }
            
            // Exactly one key contains shortName as a substring?
            if (subKeys.Count == 1) return subKeys[0];
            
            // If no substrings match exactly, use the entire list of keys
            if (subKeys.Count == 0) subKeys = keys;
            
            // Last resort, do fuzzy match
            int distance = 0;
            String new_name = plugin.bestMatch((plugin.game_version == "BF4" || plugin.game_version == "BFHL") ? bf4Normalized : shortName, subKeys, out distance, true);
            if (new_name == null)
            {
                if (verbose)
                    plugin.DebugWrite("^1^bWARNING^0^n: GetBattlelog: could not find weapon ^b" + shortName + "^n in dictionary", 4);
                return null;
            }

            if (verbose)
                plugin.DebugWrite("^1^bWARNING^0^n: GetBattlelog: could not find weapon ^b" + shortName + "^n, but guessed ^b" + new_name + "^n, with inaccuracy of ^b" + distance.ToString("F3") + "^n (10 or less is a good guess)", 4);
            return new_name;
        }

        public BattlelogWeaponStats getWeaponData(String name)
        {
            if (data.Keys.Count == 0) {
                plugin.DebugWrite("^1^bWARNING^0^n: GetBattlelog: use_slow_weapon_stats was False at time of fetch for this player, so no weapon stats available", 4);
                return SkippedWeaponStats;
            }
            
            try
            {
                // special case
                if (name.Equals("UnknownWeapon")) return UnknownWeaponStats;

                // the easy case first, weapon is in dictionary
                name = bestWeaponMatch(name);

                if (name == null) return UnknownWeaponStats;

                if (!data.ContainsKey(name)) {
                    data.Add(name, new BattlelogWeaponStats());
                    plugin.DebugWrite("^9getWeaponData(" + name + "): unknown, added default BattlelogWeaponStats", 8);
                }

                return data[name];
            }
            catch (Exception e)
            {
                plugin.DumpException(e);
            }

            return NullWeaponStats;
        }
                
        public void setWeaponData(List<BattlelogWeaponStats> bws)
        {
            if (bws == null) return;

            foreach (BattlelogWeaponStats s in bws) {
                String key = s.Name;
                if (plugin.game_version == "BF3") {
                    /*
                    The names used in the stats are different from the RCON weapon names!
                    Usually the Slug name is the closest, but sometimes the
                    Name is a closer match to the RCON weapon name. As a rough
                    heuristic, we select the shortest string between Name and Slug,
                    favoring Name if they are equal in length.
                    */
                    if (s.Slug.Length < s.Name.Length) key = s.Slug;
                }
                
                data[key] = s;
            }
        }
        
        public void dumpMatchedStats()
        {
            List<String> rconNames = new List<string>(plugin.WeaponsDict.Keys);
            
            foreach (String rconName in rconNames) {
                String shortName = rconName;
                Match m = Regex.Match(rconName, @"/([^/]+)$");
                if (m.Success) shortName = m.Groups[1].Value;
                
                BattlelogWeaponStats bws = this[rconName];
                
                plugin.ConsoleWrite("("+shortName+") = Name:" + bws.Name + ", Slug:" + bws.Slug + ", C:" + bws.Code + ", Kills:" + bws.Kills.ToString("F0") + ", Fired:" + bws.ShotsFired.ToString("F0") + ", Hit:" + bws.ShotsHit.ToString("F0") + ", Acc:" + bws.Accuracy.ToString("F2") + ", HS:" + bws.Headshots.ToString("F0") + ", Time:" + TimeSpan.FromSeconds(bws.TimeEquipped).ToString());
            }
        }
    }


    public class OAuthRequest
    {
        public HttpWebRequest request = null;
        InsaneLimits plugin = null;



        HMACSHA1 SHA1 = null;

        public List<KeyValuePair<String, String>> parameters = new List<KeyValuePair<string, string>>();


        public HTTPMethod Method { set { request.Method = value.ToString(); } get { return (HTTPMethod)Enum.Parse(typeof(HTTPMethod), request.Method); } }


        public OAuthRequest(InsaneLimits plugin, String URL)
        {
            this.plugin = plugin;
            this.request = (HttpWebRequest)HttpWebRequest.Create(URL);
            this.request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.1.3) Gecko/20090824 Firefox/3.5.3 (.NET CLR 4.0.20506)";
        }


        public void Sort()
        {
            // sort the query parameters
            parameters.Sort(delegate(KeyValuePair<String, String> left, KeyValuePair<String, String> right)
            {
                if (left.Key.Equals(right.Key))
                    return left.Value.CompareTo(right.Value);
                else
                    return left.Key.CompareTo(right.Key);
            });
        }

        public String Header()
        {
            String header = "OAuth ";
            List<String> pairs = new List<string>();

            Sort();

            for (int i = 0; i < parameters.Count; i++)
            {

                KeyValuePair<String, String> pair = parameters[i];
                if (pair.Key.Equals("status"))
                    continue;

                pairs.Add(pair.Key + "=\"" + pair.Value + "\"");
            }

            header += String.Join(", ", pairs.ToArray());

            plugin.DebugWrite("OAUTH_HEADER: " + header, 7);

            return header;
        }

        public String Signature(String ConsumerSecret, String AccessTokenSecret)
        {
            String base_url = request.Address.Scheme + "://" + request.Address.Host + request.Address.AbsolutePath;
            String encoded_base_url = UrlEncode(base_url);

            String http_method = request.Method;


            Sort();

            List<String> encoded_parameters = new List<string>();
            List<String> raw_parameters = new List<string>();

            // encode and concatenate the query parameters
            for (int i = 0; i < parameters.Count; i++)
            {
                KeyValuePair<String, String> pair = parameters[i];

                // ignore signature if present
                if (pair.Key.Equals("oauth_signature"))
                    continue;

                raw_parameters.Add(pair.Key + "=" + pair.Value);
                encoded_parameters.Add(UrlEncode(pair.Key) + "%3D" + UrlEncode(pair.Value));
            }

            String encoded_query = String.Join("%26", encoded_parameters.ToArray());
            String raw_query = String.Join("&", raw_parameters.ToArray());

            plugin.DebugWrite("HTTP_METHOD: " + http_method, 8);
            plugin.DebugWrite("BASE_URI: " + base_url, 8);
            plugin.DebugWrite("ENCODED_BASE_URI: " + encoded_base_url, 8);
            //plugin.DebugWrite("RAW_QUERY: " + raw_query, 8);
            //plugin.DebugWrite("ENCODED_QUERY: " + encoded_query, 8);

            String base_signature = http_method + "&" + encoded_base_url + "&" + encoded_query;

            plugin.DebugWrite("BASE_SIGNATURE: " + base_signature, 7);


            String HMACSHA1_signature = HMACSHA1_HASH(base_signature, ConsumerSecret, AccessTokenSecret);

            plugin.DebugWrite("HMACSHA1_SIGNATURE: " + HMACSHA1_signature, 7);

            return HMACSHA1_signature;

        }

        public String HMACSHA1_HASH(String text, String ConsumerSecret, String AccessTokenSecret)
        {
            if (SHA1 == null)
            {
                /* Initialize the SHA1 */
                String HMACSHA1_KEY = String.IsNullOrEmpty(ConsumerSecret) ? "" : UrlEncode(ConsumerSecret) + "&" + (String.IsNullOrEmpty(AccessTokenSecret) ? "" : UrlEncode(AccessTokenSecret));
                plugin.DebugWrite("HMACSHA1_KEY: " + HMACSHA1_KEY, 7);
                SHA1 = new HMACSHA1(Encoding.ASCII.GetBytes(HMACSHA1_KEY));
            }

            return Convert.ToBase64String(SHA1.ComputeHash(System.Text.Encoding.ASCII.GetBytes(text)));
        }


        public static String UnreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        public static String UrlEncode(string Input)
        {
            StringBuilder Result = new StringBuilder();

            for (int x = 0; x < Input.Length; ++x)
            {
                if (UnreservedChars.IndexOf(Input[x]) != -1)
                    Result.Append(Input[x]);
                else
                    Result.Append("%").Append(String.Format("{0:X2}", (int)Input[x]));
            }

            return Result.ToString();
        }

        public static String UrlEncode(byte[] Input)
        {
            StringBuilder Result = new StringBuilder();

            for (int x = 0; x < Input.Length; ++x)
            {
                if (UnreservedChars.IndexOf((char)Input[x]) != -1)
                    Result.Append((char)Input[x]);
                else
                    Result.Append("%").Append(String.Format("{0:X2}", (int)Input[x]));
            }

            return Result.ToString();
        }
    }
}

