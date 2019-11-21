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

namespace PRoCon.Core
{
    using System.ComponentModel;

    /// <summary>
    /// Custom property class 
    /// </summary>
    public class CustomProperty
    {
        private string sName = string.Empty;
        private string strCategory = string.Empty;
        private string strClassName = string.Empty;
        private bool bReadOnly = false;
        private bool bVisible = true;
        private object objValue = null;
        private AttributeCollection atbAttributes = null;

        public CustomProperty(string sName, string strCategory, string strClassName, object value, Type type, bool bReadOnly, bool bVisible)
        {
            this.sName = sName;
            this.strCategory = strCategory;
            this.strClassName = strClassName;
            this.objValue = value;
            this.type = type;
            this.bReadOnly = bReadOnly;
            this.bVisible = bVisible;
        }

        private Type type;
        public Type Type
        {
            get { return type; }

        }

        public AttributeCollection Attributes
        {
            get { return atbAttributes; }
            set { atbAttributes = value; }
        }

        public bool ReadOnly
        {
            get
            {
                return bReadOnly;
            }
        }

        public string Name
        {
            get
            {
                return sName;
            }
        }

        public bool Visible
        {
            get
            {
                return bVisible;
            }
        }

        public object Value
        {
            get
            {
                return objValue;
            }
            set
            {
                objValue = value;
            }
        }

        public string Category { get { return this.strCategory; } set { this.strCategory = value; } }
        public string ClassName { get { return this.strClassName; } set { this.strClassName = value; } }
    }
}
