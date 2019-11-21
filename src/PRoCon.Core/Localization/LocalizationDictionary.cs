using System.Collections.ObjectModel;


namespace PRoCon.Core.Localization
{
    public class LocalizationDictionary : KeyedCollection<string, CLocalization>
    {

        public delegate void AccountAlteredHandler(CLocalization item);
        public event AccountAlteredHandler LanguageAdded;
        public event AccountAlteredHandler LanguageRemoved;

        protected override string GetKeyForItem(CLocalization item)
        {
            return item.FileName;
        }

        protected override void InsertItem(int index, CLocalization item)
        {
            base.InsertItem(index, item);

            if (this.LanguageAdded != null)
            {
                this.LanguageAdded(item);
            }
        }

        protected override void RemoveItem(int index)
        {
            CLocalization clocRemoved = this[index];

            base.RemoveItem(index);

            if (this.LanguageRemoved != null)
            {
                this.LanguageRemoved(clocRemoved);
            }
        }

        public CLocalization LoadLocalizationFile(string fullFilePath, string localizationFileName)
        {

            CLocalization clocLoadedLanguage = new CLocalization(fullFilePath, localizationFileName);

            if (this.Contains(clocLoadedLanguage.FileName) == false)
            {
                this.Add(clocLoadedLanguage);
            }
            else
            {
                this.SetItem(this.IndexOf(this[clocLoadedLanguage.FileName]), clocLoadedLanguage);
            }

            return clocLoadedLanguage;
        }
    }
}
