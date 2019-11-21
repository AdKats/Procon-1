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

using PRoCon.Core.Battlemap;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Remote;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace PRoCon.Core.Plugin
{
    public class CPRoConMarshalByRefObject : MarshalByRefObject
    {
        public delegate void ExecuteCommandHandler(List<string> command);

        public delegate CPrivileges GetAccountPrivilegesHandler(string accountName);

        public delegate List<string> GetLoggedInAccountUsernamesHandler();

        public delegate List<CMap> GetMapDefinesHandler();

        public delegate List<MatchCommand> GetRegisteredCommandsHandler();

        public delegate SpecializationDictionary GetSpecializationDefinesHandler();

        public delegate string GetVariableHandler(string variable);

        public delegate WeaponDictionary GetWeaponDefinesHandler();

        public delegate void RegisterCommandHandler(MatchCommand command);

        public delegate void RegisterEventsHandler(string className, List<string> command);

        public delegate bool TryGetLocalizedHandler(string languageCode, out string localizedText, string variable, string[] arguements);

        public delegate void UnregisterCommandHandler(MatchCommand command);

        private ExecuteCommandHandler _executeCommandDelegate;
        private GetAccountPrivilegesHandler _getAccountPrivilegesDelegate;
        private GetLoggedInAccountUsernamesHandler _getLoggedInAccountUsernamesDelegate;
        private GetMapDefinesHandler _getMapDefinesDelegate;
        private GetRegisteredCommandsHandler _getRegisteredCommandsDelegate;

        private GetSpecializationDefinesHandler _getSpecializationDefinesDelegate;
        private GetVariableHandler _getSvVariableDelegate;
        private GetVariableHandler _getVariableDelegate;
        private GetWeaponDefinesHandler _getWeaponDefinesDelegate;
        private RegisterCommandHandler _registerCommandDelegate;

        private RegisterEventsHandler _registerEventsDelegate;
        private TryGetLocalizedHandler _tryGetLocalizedDelegate;
        private UnregisterCommandHandler _unregisterCommandDelegate;
        protected Dictionary<string, Weapon> WeaponDictionaryByLocalizedName { get; private set; }

        protected Dictionary<string, Specialization> SpecializationDictionaryByLocalizedName { get; private set; }

        protected List<String> PublicMethodNames;

        public void RegisterCallbacks(ExecuteCommandHandler delExecuteCommand, GetAccountPrivilegesHandler delGetAccountPrivileges, GetVariableHandler delGetVariable, GetVariableHandler delGetSvVariable, GetMapDefinesHandler delGetMapDefines, TryGetLocalizedHandler delTryGetLocalized, RegisterCommandHandler delRegisterCommand, UnregisterCommandHandler delUnregisterCommand, GetRegisteredCommandsHandler delGetRegisteredCommands, GetWeaponDefinesHandler delGetWeaponDefines, GetSpecializationDefinesHandler delGetSpecializationDefines, GetLoggedInAccountUsernamesHandler delGetLoggedInAccountUsernames, RegisterEventsHandler delRegisterEvents)
        {
            _executeCommandDelegate = delExecuteCommand;
            _getAccountPrivilegesDelegate = delGetAccountPrivileges;
            _getVariableDelegate = delGetVariable;
            _getSvVariableDelegate = delGetSvVariable;
            _getMapDefinesDelegate = delGetMapDefines;
            _tryGetLocalizedDelegate = delTryGetLocalized;

            _registerCommandDelegate = delRegisterCommand;
            _unregisterCommandDelegate = delUnregisterCommand;
            _getRegisteredCommandsDelegate = delGetRegisteredCommands;

            _getWeaponDefinesDelegate = delGetWeaponDefines;
            _getSpecializationDefinesDelegate = delGetSpecializationDefines;

            _getLoggedInAccountUsernamesDelegate = delGetLoggedInAccountUsernames;
            _registerEventsDelegate = delRegisterEvents;

            SetupInternalDictionaries();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public object Invoke(string methodName, object[] parameters)
        {
            object returnValue = null;

            if (this.PublicMethodNames == null)
            {
                this.PublicMethodNames = this.GetType().GetMethods().Select(method => method.Name).Distinct().ToList();
            }

            if (this.PublicMethodNames.Contains(methodName) == true)
            {
                try
                {
                    returnValue = this.GetType().InvokeMember(methodName, BindingFlags.InvokeMethod, null, this, parameters);
                }
                catch
                {
                    // Do nothing.
                }
            }

            return returnValue;
        }

        public string GetLocalizedByLanguage(string languageCode, string defaultText, string variable, params string[] arguements)
        {
            string strReturn = defaultText;

            if (_tryGetLocalizedDelegate != null)
            {
                if (_tryGetLocalizedDelegate(languageCode, out strReturn, variable, arguements) == false)
                {
                    strReturn = defaultText;
                }
            }

            return strReturn;
        }

        public string GetLocalized(string defaultText, string variable, params string[] arguements)
        {
            return GetLocalizedByLanguage(null, defaultText, variable, arguements);
        }

        public void ExecuteCommand(params string[] words)
        {
            if (_executeCommandDelegate != null)
            {
                _executeCommandDelegate(new List<string>(words));
            }
        }

        /// <summary>
        ///     There is no need to register the methods found in IPRoConPluginInterface.
        ///     They will always be called no matter if you register them or not.
        ///     You will need to register all methods you need within PRoConPluginAPI though
        ///     if you choose to RegisterEvents.
        ///     By default if you load up a plugin all of the events will be fired, but this is
        ///     the most cpu/time consuming task that procon has is calling empty methods within plugins.
        ///     This optimizes your plugin by telling procon to only call your plugin with the registered events.
        ///     You can see the potential gains by enabling all of your plugins and restarting procon.
        ///     See how slowly the console ticks passed compared to having no plugins enabled and logging in.
        /// </summary>
        /// <param name="className">The name of your plugins class</param>
        /// <param name="events">
        ///     A list of methods to be called within your plugin by procon
        ///     Example: ("OnListPlayers", "OnPlayerLeft") - Only these two methods will be called within
        ///     your plugin.
        /// </param>
        public void RegisterEvents(string className, params string[] events)
        {
            if (_registerEventsDelegate != null)
            {
                _registerEventsDelegate(className, new List<string>(events));
            }
        }

        public void RegisterCommand(MatchCommand mtcCommand)
        {
            if (_registerCommandDelegate != null)
            {
                _registerCommandDelegate(mtcCommand);
            }
        }

        public void UnregisterCommand(MatchCommand mtcCommand)
        {
            if (_unregisterCommandDelegate != null)
            {
                _unregisterCommandDelegate(mtcCommand);
            }
        }

        public List<MatchCommand> GetRegisteredCommands()
        {
            List<MatchCommand> lstReturn = default(List<MatchCommand>);

            if (_getRegisteredCommandsDelegate != null)
            {
                lstReturn = _getRegisteredCommandsDelegate();
            }

            return lstReturn;
        }

        public List<CMap> GetMapDefines()
        {
            List<CMap> lstReturn = default(List<CMap>);

            if (_getAccountPrivilegesDelegate != null)
            {
                lstReturn = _getMapDefinesDelegate();
            }

            return lstReturn;
        }

        public CPrivileges GetAccountPrivileges(string accountName)
        {
            CPrivileges spReturn = default(CPrivileges);

            if (_getAccountPrivilegesDelegate != null)
            {
                spReturn = _getAccountPrivilegesDelegate(accountName);
            }

            return spReturn;
        }

        public T GetVariable<T>(string variable, T tDefault)
        {
            T tReturn = tDefault;

            if (_getVariableDelegate != null)
            {
                string strValue = _getVariableDelegate(variable);

                TypeConverter tycPossible = TypeDescriptor.GetConverter(typeof(T));
                if (strValue.Length > 0 && tycPossible.CanConvertFrom(typeof(string)) == true)
                {
                    tReturn = (T)tycPossible.ConvertFrom(strValue);
                }
                else
                {
                    tReturn = tDefault;
                }
            }

            return tReturn;
        }

        public T GetSvVariable<T>(string variable, T tDefault)
        {
            T tReturn = tDefault;

            if (_getSvVariableDelegate != null)
            {
                string strValue = _getSvVariableDelegate(variable);

                TypeConverter tycPossible = TypeDescriptor.GetConverter(typeof(T));
                if (strValue.Length > 0 && tycPossible.CanConvertFrom(typeof(string)) == true)
                {
                    tReturn = (T)tycPossible.ConvertFrom(strValue);
                }
                else
                {
                    tReturn = tDefault;
                }
            }

            return tReturn;
        }

        public WeaponDictionary GetWeaponDefines()
        {
            WeaponDictionary dicReturn = default(WeaponDictionary);

            if (_getWeaponDefinesDelegate != null)
            {
                dicReturn = _getWeaponDefinesDelegate();
            }

            return dicReturn;
        }

        public SpecializationDictionary GetSpecializationDefines()
        {
            SpecializationDictionary dicReturn = default(SpecializationDictionary);

            if (_getSpecializationDefinesDelegate != null)
            {
                dicReturn = _getSpecializationDefinesDelegate();
            }

            return dicReturn;
        }

        public List<string> GetLoggedInAccountUsernames()
        {
            List<string> loggedInList = null;

            if (_getLoggedInAccountUsernamesDelegate != null)
            {
                loggedInList = _getLoggedInAccountUsernamesDelegate();
            }

            return loggedInList;
        }

        /// <summary>
        ///     Provides a way of creating and populating lists inline with the C# 2.0 compiler.
        ///     The below examples would be identical in a 3.5 C# compiler but the first would throw
        ///     compile errors in 2.0
        ///     List<string /> lstHelloWorld = new List<string />() { "Hello", "World!" };
        ///     List<string /> lstHelloWorld = this.Listify<string />("Hello", "World!");
        /// </summary>
        /// <typeparam name="T">The type of object to create a List of</typeparam>
        /// <param name="newList"></param>
        /// <returns>A list populated with the array of params</returns>
        public List<T> Listify<T>(params T[] newList)
        {
            return new List<T>(newList);
        }

        // Converts "hello world! \"this is a string\" 5 6 7
        // to a list with "hello", "world!", "this is a string", "5", "6", "7"
        //
        // Converts "procon.private.servers.add \"1.2.3.4\" 48889 \"password\""
        // to a list with "procon.private.servers.add", "1.2.3.4", "48889", "password"
        public List<string> Wordify(string strCommand)
        {
            return Packet.Wordify(strCommand);
        }

        // Takes a string and splits it on words based on characters 
        // string testString = "this is a string with some words in it";
        // WordWrap(testString, 10) == List<string>() { "this is a", "string", "with some", "words in", "it" }
        //
        // Useful if you want to output a long string to the game and want all of the data outputed without
        // losing any data.
        // 
        // See the @help function in the basic in game information plugin for an example.
        public List<string> WordWrap(string strText, int iColumn)
        {
            var lstReturn = new List<string>(strText.Split(' '));

            for (int i = 0; i < lstReturn.Count - 1; i++)
            {
                if (lstReturn[i].Length + lstReturn[i + 1].Length + 1 <= iColumn)
                {
                    lstReturn[i] = String.Format("{0} {1}", lstReturn[i], lstReturn[i + 1]);
                    lstReturn.RemoveAt(i + 1);
                    i--;
                }
            }

            return lstReturn;
        }

        public void UnregisterZoneTags(params string[] tags)
        {
            string tagList = GetVariable("ZONE_TAG_LIST", String.Empty);
            var tagsList = new ZoneTagList(tagList);

            foreach (string tag in tags)
            {
                if (tagsList.Contains(tag) == true)
                {
                    tagsList.Remove(tag);
                }
            }

            ExecuteCommand("procon.protected.vars.set", "ZONE_TAG_LIST", tagsList.ToString());
        }

        public void RegisterZoneTags(params string[] tags)
        {
            string tagList = GetVariable("ZONE_TAG_LIST", String.Empty);

            var tagsList = new ZoneTagList(tagList);
            foreach (string tag in tags)
            {
                if (tagsList.Contains(tag) == false)
                {
                    tagsList.Add(tag);
                }
            }

            ExecuteCommand("procon.protected.vars.set", "ZONE_TAG_LIST", tagsList.ToString());
        }

        internal void SetupInternalDictionaries()
        {
            string localizedName = String.Empty;

            WeaponDictionary weapons = GetWeaponDefines();
            WeaponDictionaryByLocalizedName = new Dictionary<string, Weapon>();

            foreach (Weapon weapon in weapons)
            {
                localizedName = GetLocalized(weapon.Name, String.Format("global.Weapons.{0}", weapon.Name.ToLower()));

                if (WeaponDictionaryByLocalizedName.ContainsKey(localizedName) == false)
                {
                    WeaponDictionaryByLocalizedName.Add(localizedName, weapon);
                }
            }

            SpecializationDictionary specializations = GetSpecializationDefines();
            SpecializationDictionaryByLocalizedName = new Dictionary<string, Specialization>();

            foreach (Specialization specialization in specializations)
            {
                localizedName = GetLocalized(specialization.Name, String.Format("global.Specialization.{0}", specialization.Name.ToLower()));

                if (SpecializationDictionaryByLocalizedName.ContainsKey(localizedName) == false)
                {
                    SpecializationDictionaryByLocalizedName.Add(localizedName, specialization);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="damageType">
        ///     DamageTypes.None = Full list of weapons
        ///     DamageTypes.SniperRifle = List of sniper rifles
        /// </param>
        /// <returns></returns>
        protected List<string> GetWeaponList(DamageTypes damageType)
        {
            var returnWeaponList = new List<string>();

            foreach (var weapon in WeaponDictionaryByLocalizedName)
            {
                if (damageType == DamageTypes.None || (damageType & weapon.Value.Damage) == weapon.Value.Damage)
                {
                    if (returnWeaponList.Contains(weapon.Key) == false)
                    {
                        returnWeaponList.Add(weapon.Key);
                    }
                }
            }

            return returnWeaponList;
        }

        /// <summary>
        /// </summary>
        /// <param name="variableName">The name to display in the plugin settings</param>
        /// <param name="assemblyName">A unique name for this enum list, should be unique.  Put your plugin class name in to avoid clashes.</param>
        /// <param name="value">The current value/selected value</param>
        /// <param name="damageType">DamageTypes.None, gets full list otherwise limits the returned list</param>
        /// <returns></returns>
        protected CPluginVariable GetWeaponListPluginVariable(string variableName, string assemblyName, string value, DamageTypes damageType)
        {
            return new CPluginVariable(variableName, String.Format("enum.{0}({1})", assemblyName, String.Join("|", GetWeaponList(damageType).ToArray())), value);
        }

        protected Weapon GetWeaponByLocalizedName(string localizedName)
        {
            Weapon returnWeapon = null;

            if (WeaponDictionaryByLocalizedName.ContainsKey(localizedName) == true)
            {
                returnWeapon = WeaponDictionaryByLocalizedName[localizedName];
            }

            return returnWeapon;
        }

        /// <summary>
        ///     this.GetSpecializationList(SpecializationSlots.None) gets a full list of specs
        ///     this.GetSpecializationList(SpecializationSlots.Kit) will get a list of specs based on Kits
        /// </summary>
        /// <param name="slotType"></param>
        /// <returns></returns>
        protected List<string> GetSpecializationList(SpecializationSlots slotType)
        {
            var returnSpecializationList = new List<string>();

            foreach (var specialization in SpecializationDictionaryByLocalizedName)
            {
                if (slotType == SpecializationSlots.None || slotType == specialization.Value.Slot)
                {
                    if (returnSpecializationList.Contains(specialization.Key) == false)
                    {
                        returnSpecializationList.Add(specialization.Key);
                    }
                }
            }

            return returnSpecializationList;
        }

        /// <summary>
        /// </summary>
        /// <param name="variableName">The name to display in the plugin settings</param>
        /// <param name="assemblyName">A unique name for this enum list, should be unique.  Put your plugin class name in to avoid clashes.</param>
        /// <param name="value">The current value/selected value</param>
        /// <param name="slotType">SpecializationSlots.None, gets full list otherwise limits the returned list</param>
        /// <returns></returns>
        protected CPluginVariable GetSpecializationListPluginVariable(string variableName, string assemblyName, string value, SpecializationSlots slotType)
        {
            return new CPluginVariable(variableName, String.Format("enum.{0}({1})", assemblyName, String.Join("|", GetSpecializationList(slotType).ToArray())), value);
        }

        protected Specialization GetSpecializationByLocalizedName(string localizedName)
        {
            Specialization returnSpecialization = null;

            if (SpecializationDictionaryByLocalizedName.ContainsKey(localizedName) == true)
            {
                returnSpecialization = SpecializationDictionaryByLocalizedName[localizedName];
            }

            return returnSpecialization;
        }

        internal bool IsValidPlaylist(string validatePlayList, string[] requestedPlayLists)
        {
            bool isValid = false;

            if (requestedPlayLists == null)
            {
                isValid = true;
            }
            else
            {
                if (requestedPlayLists.Length == 0)
                {
                    isValid = true;
                }
                else
                {
                    if (requestedPlayLists.Any(playlist => String.CompareOrdinal(validatePlayList, playlist) == 0))
                    {
                        isValid = true;
                    }
                }
            }

            return isValid;
        }

        /// <summary>
        ///     Gets a list of formatted maps from the Map Defines.
        /// </summary>
        /// <param name="format">
        ///     {PublicLevelName}
        ///     {GameMode}
        ///     {FileName}
        ///     {PlayList}
        /// </param>
        /// <param name="playList"></param>
        /// <returns></returns>
        protected List<string> GetMapList(string format, params string[] playList)
        {
            var returnMapList = new List<string>();

            foreach (CMap map in GetMapDefines())
            {
                if (IsValidPlaylist(map.PlayList, playList) == true)
                {
                    string formattedMap = format.Replace("{PublicLevelName}", map.PublicLevelName).Replace("{GameMode}", map.GameMode).Replace("{FileName}", map.FileName).Replace("{PlayList}", map.PlayList);

                    if (returnMapList.Contains(formattedMap) == false)
                    {
                        returnMapList.Add(formattedMap);
                    }
                }
            }

            return returnMapList;
        }

        /// <summary>
        /// </summary>
        /// <param name="variableName">The name to display in the plugin settings</param>
        /// <param name="assemblyName">A unique name for this enum list, should be unique.  Put your plugin class name in to avoid clashes.</param>
        /// <param name="value">The current value/selected value</param>
        /// <param name="format">The format of the team list.  See this.GetMapList for more information</param>
        /// <param name="playList">SQDM, SQRUSH, CONQUEST, RUSH</param>
        /// <returns></returns>
        protected CPluginVariable GetMapListPluginVariable(string variableName, string assemblyName, string value, string format, params string[] playList)
        {
            return new CPluginVariable(variableName, String.Format("enum.{0}({1})", assemblyName, String.Join("|", GetMapList(format, playList).ToArray())), value);
        }

        /// <summary>
        /// </summary>
        /// <param name="format">See this.GetMapList(string, params string[]) for more information about the format variable</param>
        /// <param name="formattedMapName"></param>
        /// <returns></returns>
        protected CMap GetMapByFormattedName(string format, string formattedMapName)
        {
            return (GetMapDefines().Select(map => new
            {
                map,
                formattedMap = format.Replace("{PublicLevelName}", map.PublicLevelName).Replace("{GameMode}", map.GameMode).Replace("{FileName}", map.FileName)
            }).Where(@t => String.CompareOrdinal(@t.formattedMap, formattedMapName) == 0).Select(@t => @t.map)).FirstOrDefault();
        }

        /// <summary>
        /// </summary>
        /// <param name="format">
        ///     {PublicLevelName}
        ///     {GameMode}
        ///     {FileName}
        ///     {TeamName}
        /// </param>
        /// <param name="playList"></param>
        /// <returns></returns>
        protected List<string> GetTeamList(string format, params string[] playList)
        {
            var returnMapList = new List<string>();

            foreach (CMap map in GetMapDefines())
            {
                if (IsValidPlaylist(map.PlayList, playList) == true)
                {
                    foreach (CTeamName teamname in map.TeamNames)
                    {
                        string formattedTeamName = format.Replace("{PublicLevelName}", map.PublicLevelName).Replace("{GameMode}", map.GameMode).Replace("{FileName}", map.FileName).Replace("{TeamName}", GetLocalized(teamname.LocalizationKey, teamname.LocalizationKey));

                        if (returnMapList.Contains(formattedTeamName) == false)
                        {
                            returnMapList.Add(formattedTeamName);
                        }
                    }
                }
            }

            return returnMapList;
        }

        protected List<string> GetTeamListByPlayList(string format, params string[] playList)
        {
            var returnMapList = new List<string>();

            foreach (CMap map in GetMapDefines())
            {
                if (IsValidPlaylist(map.PlayList, playList) == true)
                {
                    foreach (CTeamName teamname in map.TeamNames)
                    {
                        if (String.CompareOrdinal(teamname.Playlist, playList[0]) == 0)
                        {
                            string formattedTeamName = format.Replace("{PublicLevelName}", map.PublicLevelName).Replace("{GameMode}", map.GameMode).Replace("{FileName}", map.FileName).Replace("{TeamName}", GetLocalized(teamname.LocalizationKey, teamname.LocalizationKey));

                            if (returnMapList.Contains(formattedTeamName) == false)
                            {
                                returnMapList.Add(formattedTeamName);
                            }
                        }
                    }
                }
            }

            return returnMapList;
        }

        protected List<string> GetTeamListByPlayListForMap(string format, string FileName, params string[] playList)
        {
            var returnMapList = new List<string>();

            foreach (CMap map in GetMapDefines())
            {
                if (IsValidPlaylist(map.PlayList, playList) == true)
                {
                    if (String.CompareOrdinal(map.FileName, FileName) == 0)
                    {
                        foreach (CTeamName teamname in map.TeamNames)
                        {
                            if (String.CompareOrdinal(teamname.Playlist, playList[0]) == 0)
                            {
                                string formattedTeamName = format.Replace("{PublicLevelName}", map.PublicLevelName).Replace("{GameMode}", map.GameMode).Replace("{FileName}", map.FileName).Replace("{TeamName}", GetLocalized(teamname.LocalizationKey, teamname.LocalizationKey));

                                if (returnMapList.Contains(formattedTeamName) == false)
                                {
                                    returnMapList.Add(formattedTeamName);
                                }
                            }
                        }
                    }
                }
            }

            return returnMapList;
        }

        /// <summary>
        /// </summary>
        /// <param name="variableName">The name to display in the plugin settings</param>
        /// <param name="assemblyName">A unique name for this enum list, should be unique.  Put your plugin class name in to avoid clashes.</param>
        /// <param name="value">The current value/selected value</param>
        /// <param name="format">The format of the team list.  See this.GetTeamList for more information </param>
        /// <param name="playList">SQDM, SQRUSH, CONQUEST, RUSH</param>
        /// <returns></returns>
        protected CPluginVariable GetTeamListPluginVariable(string variableName, string assemblyName, string value, string format, params string[] playList)
        {
            return new CPluginVariable(variableName, String.Format("enum.{0}({1})", assemblyName, String.Join("|", GetTeamList(format, playList).ToArray())), value);
        }

        protected CTeamName GetTeamNameByFormattedTeamName(string format, string formattedTeamName)
        {
            CTeamName returnTeamName = null;

            foreach (CMap map in GetMapDefines())
            {
                foreach (CTeamName teamname in from teamname in map.TeamNames let formattedTeam = format.Replace("{PublicLevelName}", map.PublicLevelName).Replace("{GameMode}", map.GameMode).Replace("{FileName}", map.FileName).Replace("{TeamName}", GetLocalized(teamname.LocalizationKey, teamname.LocalizationKey)) where String.CompareOrdinal(formattedTeam, formattedTeamName) == 0 select teamname)
                {
                    returnTeamName = teamname;
                    break;
                }

                if (returnTeamName != null)
                {
                    break;
                }
            }

            return returnTeamName;
        }

        // Get Gamemode List

        protected CMap GetMapByFilename(string strMapFileName)
        {
            CMap cmReturn = null;

            List<CMap> mapDefines = GetMapDefines();

            if (mapDefines != null)
            {
                foreach (CMap cmMap in mapDefines)
                {
                    if (String.Compare(cmMap.FileName, strMapFileName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        cmReturn = cmMap;
                        break;
                    }
                }
            }

            return cmReturn;
        }

        protected CMap GetMapByFilenamePlayList(string strMapFileName, string strMapPlayList)
        {
            CMap cmReturn = null;

            List<CMap> mapDefines = GetMapDefines();

            if (mapDefines != null)
            {
                foreach (CMap cmMap in mapDefines.Where(cmMap => String.Compare(cmMap.FileName, strMapFileName, StringComparison.OrdinalIgnoreCase) == 0 && String.Compare(cmMap.PlayList, strMapPlayList, System.StringComparison.OrdinalIgnoreCase) == 0))
                {
                    cmReturn = cmMap;
                    break;
                }
            }

            return cmReturn;
        }

        protected string SecondsToText(UInt32 iSeconds, string[] a_strTimeDescriptions, bool blShowSecondsOver60)
        {
            string strReturn = String.Empty;

            double dblSeconds = iSeconds;
            double dblMinutes = (iSeconds / 60);
            double dblHours = (dblMinutes / 60);
            double dblDays = (dblHours / 24);
            double dblWeeks = (dblDays / 7);
            double dblMonths = (dblWeeks / 4);
            double dblYears = (dblMonths / 12);

            if ((Int32)dblYears > 0)
            {
                strReturn += String.Empty + ((Int32)dblYears) + (((Int32)dblYears) == 1 ? a_strTimeDescriptions[0] : a_strTimeDescriptions[1]);
            }
            if ((Int32)dblMonths % 12 > 0)
            {
                strReturn += String.Empty + ((Int32)dblMonths) % 12 + (((Int32)dblMonths % 12) == 1 ? a_strTimeDescriptions[2] : a_strTimeDescriptions[3]);
            }
            if ((Int32)dblWeeks % 4 > 0)
            {
                strReturn += String.Empty + ((Int32)dblWeeks) % 4 + (((Int32)dblWeeks % 4) == 1 ? a_strTimeDescriptions[4] : a_strTimeDescriptions[5]);
            }
            if ((Int32)dblDays % 7 > 0)
            {
                strReturn += String.Empty + ((Int32)dblDays) % 7 + (((Int32)dblDays % 7) == 1 ? a_strTimeDescriptions[6] : a_strTimeDescriptions[7]);
            }
            if ((Int32)dblHours % 24 > 0)
            {
                strReturn += String.Empty + ((Int32)dblHours) % 24 + (((Int32)dblHours % 24) == 1 ? a_strTimeDescriptions[8] : a_strTimeDescriptions[9]);
            }
            if ((Int32)dblMinutes % 60 > 0)
            {
                strReturn += String.Empty + ((Int32)dblMinutes) % 60 + (((Int32)dblMinutes % 60) == 1 ? a_strTimeDescriptions[10] : a_strTimeDescriptions[11]);
            }

            if ((iSeconds > 60 && blShowSecondsOver60 == true) || (iSeconds < 60 && blShowSecondsOver60 == false))
            {
                if ((Int32)dblSeconds % 60 > 0)
                {
                    strReturn += String.Empty + ((Int32)dblSeconds) % 60 + (((Int32)dblSeconds % 60) == 1 ? a_strTimeDescriptions[12] : a_strTimeDescriptions[13]);
                }
            }

            return strReturn;
        }
    }
}