using System;

namespace PRoCon.Core.Accounts
{
    public class Account
    {
        public delegate void AccountPasswordChangedHandler(Account item);

        private string _password;

        public Account(string strName, string strPassword)
        {
            Name = strName;
            Password = strPassword;
        }

        /// <summary>
        /// The name of this account
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// The password to login to this account.
        /// </summary>
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;

                if (AccountPasswordChanged != null)
                {
                    this.AccountPasswordChanged(this);
                }
            }
        }

        public event AccountPasswordChangedHandler AccountPasswordChanged;
    }
}