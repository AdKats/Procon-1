using System;

namespace PRoCon.Core.Plugin.Commands
{
    [Serializable]
    public enum ExecutionScope
    {
        None, // It won't be executed by anyone for any reason.. 
        All, // Any player can execute the command
        Account, // Only an account is needed, privileges won't be checked
        Privileges, // The account must exist and have 
    }
}
