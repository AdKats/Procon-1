namespace PRoCon.Core.Accounts
{
    public class AccountPrivilege
    {
        public delegate void AccountPrivilegesChangedHandler(AccountPrivilege item);

        public AccountPrivilege(Account accOwner, CPrivileges cpPrivileges)
        {
            Owner = accOwner;
            Privileges = cpPrivileges;
        }

        public Account Owner { get; private set; }

        public CPrivileges Privileges { get; private set; }
        public event AccountPrivilegesChangedHandler AccountPrivilegesChanged;

        public void SetPrivileges(CPrivileges cpUpdatedPrivileges)
        {
            Privileges = cpUpdatedPrivileges;

            if (AccountPrivilegesChanged != null)
            {
                this.AccountPrivilegesChanged(this);
            }
        }
    }
}