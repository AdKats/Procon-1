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
using System.Collections.Generic;

namespace PRoCon.Core.Plugin
{
    using Core.Players.Items;
    using Core.Plugin.Commands;

    public class CPRoConPluginCallbacks : MarshalByRefObject
    {

        private CPRoConMarshalByRefObject.ExecuteCommandHandler m_delExecuteCommand;
        private CPRoConMarshalByRefObject.GetAccountPrivilegesHandler m_delGetAccountPrivileges;
        private CPRoConMarshalByRefObject.GetVariableHandler m_delGetVariable;
        private CPRoConMarshalByRefObject.GetVariableHandler m_delGetSvVariable;
        private CPRoConMarshalByRefObject.GetMapDefinesHandler m_delGetMapDefines;
        private CPRoConMarshalByRefObject.TryGetLocalizedHandler m_delTryGetLocalized;

        private CPRoConMarshalByRefObject.RegisterCommandHandler m_delRegisterCommand;
        private CPRoConMarshalByRefObject.UnregisterCommandHandler m_delUnregisterCommand;
        private CPRoConMarshalByRefObject.GetRegisteredCommandsHandler m_delGetRegisteredCommands;

        private CPRoConMarshalByRefObject.GetWeaponDefinesHandler m_delGetWeaponDefines;
        private CPRoConMarshalByRefObject.GetSpecializationDefinesHandler m_delGetSpecializationDefines;

        private CPRoConMarshalByRefObject.GetLoggedInAccountUsernamesHandler m_delGetLoggedInAccountUsernames;
        private CPRoConMarshalByRefObject.RegisterEventsHandler m_delRegisterEvents;

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public CPRoConPluginCallbacks(CPRoConMarshalByRefObject.ExecuteCommandHandler delExecuteCommandCallback,
                                      CPRoConMarshalByRefObject.GetAccountPrivilegesHandler delGetAccountPrivileges,
                                      CPRoConMarshalByRefObject.GetVariableHandler delGetVariable,
                                      CPRoConMarshalByRefObject.GetVariableHandler delGetSvVariable,
                                      CPRoConMarshalByRefObject.GetMapDefinesHandler delGetMapDefines,
                                      CPRoConMarshalByRefObject.TryGetLocalizedHandler delTryGetLocalized,
                                      CPRoConMarshalByRefObject.RegisterCommandHandler delRegisterCommand,
                                      CPRoConMarshalByRefObject.UnregisterCommandHandler delUnregisterCommand,
                                      CPRoConMarshalByRefObject.GetRegisteredCommandsHandler delGetRegisteredCommands,
                                      CPRoConMarshalByRefObject.GetWeaponDefinesHandler delGetWeaponDefines,
                                      CPRoConMarshalByRefObject.GetSpecializationDefinesHandler delGetSpecializationDefines,
                                      CPRoConMarshalByRefObject.GetLoggedInAccountUsernamesHandler delGetLoggedInAccountUsernames,
                                      CPRoConMarshalByRefObject.RegisterEventsHandler delRegisterEvents)
        {
            this.m_delExecuteCommand = delExecuteCommandCallback;
            this.m_delGetAccountPrivileges = delGetAccountPrivileges;
            this.m_delGetVariable = delGetVariable;
            this.m_delGetSvVariable = delGetSvVariable;
            this.m_delGetMapDefines = delGetMapDefines;
            this.m_delTryGetLocalized = delTryGetLocalized;

            this.m_delRegisterCommand = delRegisterCommand;
            this.m_delUnregisterCommand = delUnregisterCommand;
            this.m_delGetRegisteredCommands = delGetRegisteredCommands;

            this.m_delGetWeaponDefines = delGetWeaponDefines;
            this.m_delGetSpecializationDefines = delGetSpecializationDefines;

            this.m_delGetLoggedInAccountUsernames = delGetLoggedInAccountUsernames;
            this.m_delRegisterEvents = delRegisterEvents;
        }

        public WeaponDictionary GetWeaponDefines_Callback()
        {
            return this.m_delGetWeaponDefines();
        }

        public SpecializationDictionary GetSpecializationDefines_Callback()
        {
            return this.m_delGetSpecializationDefines();
        }

        public void ExecuteCommand_Callback(List<string> lstCommand)
        {
            this.m_delExecuteCommand(lstCommand);
        }

        public string GetVariable_Callback(string strVariable)
        {
            return this.m_delGetVariable(strVariable);
        }

        public string GetSvVariable_Callback(string strSvVariable)
        {
            return this.m_delGetSvVariable(strSvVariable);
        }

        public CPrivileges GetAccountPrivileges_Callback(string strAccountName)
        {
            return this.m_delGetAccountPrivileges(strAccountName);
        }

        public List<CMap> GetMapDefines_Callback()
        {
            return this.m_delGetMapDefines();
        }

        public bool TryGetLocalized_Callback(string strLanguageCode, out string strLocalizedText, string strVariable, string[] a_strArguements)
        {
            return this.m_delTryGetLocalized(strLanguageCode, out strLocalizedText, strVariable, a_strArguements);
        }

        public void RegisterCommand_Callback(MatchCommand mtcCommand)
        {
            this.m_delRegisterCommand(mtcCommand);
        }

        public void UnregisterCommand_Callback(MatchCommand mtcCommand)
        {
            this.m_delUnregisterCommand(mtcCommand);
        }

        public List<MatchCommand> GetRegisteredCommands_Callback()
        {
            return this.m_delGetRegisteredCommands();
        }

        public List<string> GetLoggedInAccountUsernames_Callback()
        {
            return this.m_delGetLoggedInAccountUsernames();
        }

        public void RegisterEvents_Callback(string className, List<string> events)
        {
            this.m_delRegisterEvents(className, events);
        }
    }
}
