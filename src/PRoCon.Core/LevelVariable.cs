// Copyright 2010 Geoffrey 'Phogue' Green
// 
// http://www.phogue.net
//  
// This file is part of PRoCon Frostbite.
// 
// PRoCon Frostbite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// PRoCon Frostbite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PRoCon.Core
{

    [Serializable]
    public class LevelVariable
    {

        public LevelVariableContext Context
        {
            get;
            private set;
        }

        public string VariableName
        {
            get;
            private set;
        }

        public string RawValue
        {
            get;
            set;
        }

        public T GetValue<T>(T tDefault)
        {
            T tReturn = tDefault;

            TypeConverter tycPossible = TypeDescriptor.GetConverter(typeof(T));
            if (tycPossible.CanConvertFrom(typeof(string)) == true)
            {
                tReturn = (T)tycPossible.ConvertFrom(this.RawValue);
            }
            else
            {
                tReturn = tDefault;
            }

            return tReturn;
        }

        public LevelVariable(LevelVariableContext lvcContext, string strVariableName, string strRawValue)
        {
            this.Context = lvcContext;
            this.VariableName = strVariableName;
            this.RawValue = strRawValue;
        }

        public static LevelVariable ExtractContextVariable(bool skipAllContext, List<string> contextList)
        {

            string contextType = String.Empty, contextTarget = String.Empty, variableName = String.Empty, variableValue = String.Empty;

            if (contextList.Count >= 1)
            {

                int offset = 0;

                contextType = contextList[offset++];

                if (String.Compare(contextType, "all") != 0 && offset < contextList.Count)
                {
                    contextTarget = contextList[offset++];
                }
                else if (skipAllContext == true)
                {
                    offset++;
                }

                if (offset < contextList.Count)
                {
                    variableName = contextList[offset++];
                }

                if (offset < contextList.Count)
                {
                    variableValue = contextList[offset++];
                }
            }

            return new LevelVariable(new LevelVariableContext(contextType, contextTarget), variableName, variableValue);
        }
    }
}
