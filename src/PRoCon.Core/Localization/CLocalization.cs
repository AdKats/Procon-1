using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PRoCon.Core
{
    using Core.Remote;
    public class CLocalization
    {

        // VariableName=LocalizedString
        protected Dictionary<String, String> LocalizedStrings { get; set; }

        public string FilePath { get; private set; }

        public string FileName { get; private set; }

        public CLocalization()
        {
            this.FileName = String.Empty;
            this.FilePath = String.Empty;
            this.LocalizedStrings = new Dictionary<String, String>();
        }

        internal class LocalizationKeyComparer : IEqualityComparer<String[]>
        {
            public bool Equals(string[] x, string[] y)
            {
                return x[0].Equals(y[0]);
            }

            public int GetHashCode(string[] obj)
            {
                return obj[0].GetHashCode();
            }
        }

        // string strLocalizationFilePath,
        public CLocalization(string filePath, string fileName)
        {

            this.FileName = fileName;
            this.FilePath = filePath;
            this.LocalizedStrings = new Dictionary<string, string>();

            try
            {
                this.LocalizedStrings = File.ReadAllText(this.FilePath).Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries))
                    .Where(items => items.Length == 2)
                    .Distinct(new LocalizationKeyComparer())
                    .ToDictionary(items => items[0], items => items[1]);
            }
            catch (Exception e)
            {
                FrostbiteConnection.LogError("CLocalization", String.Empty, e);
                // TO DO: Nice error message for loading localization file error.
            }
        }

        public bool LocalizedExists(string strVariable)
        {
            return this.LocalizedStrings.ContainsKey(strVariable);
        }

        public bool TryGetLocalized(out string strLocalizedText, string strVariable, params object[] a_strArguements)
        {

            bool blFoundLocalized = false;
            strLocalizedText = String.Empty;

            if (this.LocalizedStrings.ContainsKey(strVariable) == true)
            {
                if (a_strArguements == null)
                {
                    strLocalizedText = this.LocalizedStrings[strVariable];
                    blFoundLocalized = true;
                }
                else
                {
                    try
                    {
                        strLocalizedText = String.Format(this.LocalizedStrings[strVariable], a_strArguements);
                        blFoundLocalized = true;
                    }
                    catch (Exception)
                    {
                        blFoundLocalized = false;
                    }
                }
            }

            return blFoundLocalized;
        }

        public string GetDefaultLocalized(string defaultText, string variable, params object[] arguements)
        {
            string returnText = defaultText;

            if (this.TryGetLocalized(out returnText, variable, arguements) == false)
            {
                returnText = defaultText;
            }

            return returnText;
        }

        public string GetLocalized(string strVariable, params string[] a_strArguements)
        {
            string strReturn = String.Empty;

            if (this.LocalizedStrings.ContainsKey(strVariable) == true)
            {

                if (a_strArguements == null)
                {
                    strReturn = this.LocalizedStrings[strVariable];
                }
                else
                {
                    try
                    {
                        strReturn = String.Format(this.LocalizedStrings[strVariable], a_strArguements);
                    }
                    catch (FormatException)
                    {
                        strReturn = "{FE: " + this.LocalizedStrings[strVariable] + "}";
                    }
                    catch (Exception)
                    {
                        // So people can debug their localized file.
                        strReturn = this.LocalizedStrings[strVariable];
                    }
                }
            }
            else
            {
                strReturn = "{MISSING: " + strVariable + "}";
            }

            return strReturn;
        }

        public void SetLocalized(string strVariable, string strValue)
        {

            try
            {

                string strFullFileContents;

                using (StreamReader streamReader = new StreamReader(this.FilePath, Encoding.Unicode))
                {
                    strFullFileContents = streamReader.ReadToEnd();
                }

                strFullFileContents = Regex.Replace(strFullFileContents, String.Format("^{0}=(.*?)[\\r]?$", strVariable), String.Format("{0}={1}", strVariable, strValue), RegexOptions.Multiline);

                using (StreamWriter streamWriter = new StreamWriter(this.FilePath, false, Encoding.Unicode))
                {
                    streamWriter.Write(strFullFileContents);
                }

                if (this.LocalizedStrings.ContainsKey(strVariable) == true)
                {
                    this.LocalizedStrings[strVariable] = strValue;
                }

            }
            catch (Exception)
            {

            }
        }
    }
}
