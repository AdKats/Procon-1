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
using System.Text;

namespace PRoCon.Core
{
    using System;
    using System.Web;

    [Serializable]
    public class CPluginVariable
    {
        private string m_strVariableName;
        private string m_strVariableType;
        private string m_strVariableValue;
        private bool m_blVariableReadonly = false;

        public CPluginVariable(string strVariableName, string strVariable, string strValue)
        {
            this.m_strVariableName = strVariableName;
            this.m_strVariableType = strVariable;
            this.m_strVariableValue = strValue;
        }

        public CPluginVariable(string strVariableName, Type tyVariable, object objValue)
        {
            this.m_strVariableName = strVariableName;

            if (tyVariable == typeof(bool))
            {
                this.m_strVariableType = "bool";
                this.m_strVariableValue = objValue.ToString();
            }
            else if (tyVariable == typeof(enumBoolOnOff))
            {
                this.m_strVariableType = "onoff";
                this.m_strVariableValue = Enum.GetName(tyVariable, (enumBoolOnOff)objValue);
            }
            else if (tyVariable == typeof(enumBoolYesNo))
            {
                this.m_strVariableType = "yesno";
                this.m_strVariableValue = Enum.GetName(tyVariable, (enumBoolYesNo)objValue);
            }
            else if (tyVariable == typeof(int))
            {
                this.m_strVariableType = "int";
                this.m_strVariableValue = objValue.ToString();
            }
            else if (tyVariable == typeof(double))
            {
                this.m_strVariableType = "double";
                this.m_strVariableValue = objValue.ToString();
            }
            else if (tyVariable == typeof(string))
            {
                if (objValue != null)
                {
                    this.m_strVariableType = "multiline";
                    this.m_strVariableValue = objValue.ToString();
                }
                else
                {
                    this.m_strVariableType = "multiline";
                    this.m_strVariableValue = String.Empty;
                }
            }
            else if (tyVariable == typeof(string[]))
            {
                if (objValue != null)
                {
                    this.m_strVariableType = "stringarray";
                    this.m_strVariableValue = EncodeStringArray((string[])objValue);
                }
                else
                {
                    this.m_strVariableType = "stringarray";
                    this.m_strVariableValue = String.Empty;
                }
            }
        }

        public CPluginVariable(string strVariableName, string strVariable, string strValue, bool isReadOnly)
        {
            this.m_strVariableName = strVariableName;
            this.m_strVariableType = strVariable;
            this.m_strVariableValue = strValue;
            this.m_blVariableReadonly = isReadOnly;
        }

        public CPluginVariable(string strVariableName, Type tyVariable, object objValue, bool isReadOnly)
        {
            this.m_strVariableName = strVariableName;
            this.m_blVariableReadonly = isReadOnly;

            if (tyVariable == typeof(bool))
            {
                this.m_strVariableType = "bool";
                this.m_strVariableValue = objValue.ToString();
            }
            else if (tyVariable == typeof(enumBoolOnOff))
            {
                this.m_strVariableType = "onoff";
                this.m_strVariableValue = Enum.GetName(tyVariable, (enumBoolOnOff)objValue);
            }
            else if (tyVariable == typeof(enumBoolYesNo))
            {
                this.m_strVariableType = "yesno";
                this.m_strVariableValue = Enum.GetName(tyVariable, (enumBoolYesNo)objValue);
            }
            else if (tyVariable == typeof(int))
            {
                this.m_strVariableType = "int";
                this.m_strVariableValue = objValue.ToString();
            }
            else if (tyVariable == typeof(double))
            {
                this.m_strVariableType = "double";
                this.m_strVariableValue = objValue.ToString();
            }
            else if (tyVariable == typeof(string))
            {
                if (objValue != null)
                {
                    this.m_strVariableType = "multiline";
                    this.m_strVariableValue = objValue.ToString();
                }
                else
                {
                    this.m_strVariableType = "multiline";
                    this.m_strVariableValue = String.Empty;
                }
            }
            else if (tyVariable == typeof(string[]))
            {
                if (objValue != null)
                {
                    this.m_strVariableType = "stringarray";
                    this.m_strVariableValue = EncodeStringArray((string[])objValue);
                }
                else
                {
                    this.m_strVariableType = "stringarray";
                    this.m_strVariableValue = String.Empty;
                }
            }
        }

        public string Name
        {
            get { return this.m_strVariableName; }
        }

        public string Type
        {
            get { return this.m_strVariableType; }
        }

        public string Value
        {
            get { return this.m_strVariableValue; }
        }

        public bool ReadOnly
        {
            get { return this.m_blVariableReadonly; }
        }

        // Yes I know.  I got lazy..
        public static string Encode(string strData)
        {
            string strReturn = HttpUtility.UrlEncode(strData);
            if (strReturn != null)
            {
                strReturn = strReturn.Replace("(", "%28");
                strReturn = strReturn.Replace(")", "%29");
                strReturn = strReturn.Replace(",", "%2C");
            }
            return strReturn;
        }

        public static string Decode(string strData)
        {
            return HttpUtility.UrlDecode(strData);
        }

        public static string HtmlDecode(string strData)
        {
            return HttpUtility.HtmlDecode(strData);
        }

        public static string[] DecodeStringArray(string strValue)
        {
            var a_strReturn = new string[] { };

            a_strReturn = strValue.Split(new char[] { '|' });

            for (int i = 0; i < a_strReturn.Length; i++)
            {
                a_strReturn[i] = Decode(a_strReturn[i]);
            }

            return a_strReturn;
        }

        public static string EncodeStringArray(string[] a_strValue)
        {

            StringBuilder encodedString = new StringBuilder();

            for (int i = 0; i < a_strValue.Length; i++)
            {
                if (i > 0)
                {
                    encodedString.Append("|");
                    //strReturn += "|";
                }
                encodedString.Append(Encode(a_strValue[i]));
                //strReturn += Encode(a_strValue[i]);
            }

            return encodedString.ToString();
        }
    }
}