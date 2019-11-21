using System.Collections.ObjectModel;

namespace PRoCon.Core.Accounts
{
    public class AccountPrivilegeDictionary : KeyedCollection<string, AccountPrivilege>
    {
        public delegate void AccountPrivilegeAlteredHandler(AccountPrivilege item);

        public event AccountPrivilegeAlteredHandler AccountPrivilegeAdded;
        public event AccountPrivilegeAlteredHandler AccountPrivilegeChanged;
        public event AccountPrivilegeAlteredHandler AccountPrivilegeRemoved;

        protected override string GetKeyForItem(AccountPrivilege item)
        {
            return item.Owner.Name;
        }

        protected override void InsertItem(int index, AccountPrivilege item)
        {
            base.InsertItem(index, item);

            if (AccountPrivilegeAdded != null)
            {
                this.AccountPrivilegeAdded(item);
            }
        }

        protected override void RemoveItem(int index)
        {
            AccountPrivilege apRemoved = this[index];

            base.RemoveItem(index);

            if (AccountPrivilegeRemoved != null)
            {
                this.AccountPrivilegeRemoved(apRemoved);
            }
        }

        protected override void SetItem(int index, AccountPrivilege item)
        {
            if (AccountPrivilegeChanged != null)
            {
                this.AccountPrivilegeChanged(item);
            }

            base.SetItem(index, item);
        }
    }
}