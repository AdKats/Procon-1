using System.ComponentModel;

namespace PRoCon.Core.Variables
{
    public class Variable
    {

        public string Name
        {
            get;
            private set;
        }

        public string Value
        {
            get;
            set;
        }

        public Variable(string strName, string strValue)
        {
            this.Name = strName;
            this.Value = strValue;
        }

        public T ConvertValue<T>(T tDefault)
        {
            T tReturn = tDefault;

            TypeConverter tycPossible = TypeDescriptor.GetConverter(typeof(T));
            if (this.Value.Length > 0 && tycPossible.CanConvertFrom(typeof(string)) == true)
            {
                tReturn = (T)tycPossible.ConvertFrom(this.Value);
            }
            else
            {
                tReturn = tDefault;
            }

            return tReturn;
        }
    }
}
