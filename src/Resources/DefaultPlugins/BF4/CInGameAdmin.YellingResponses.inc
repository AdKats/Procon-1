
        private void m_dicPlayers_SendYellingResponse(string strScope, string strAccountName, string strMessage) {
            if (String.Compare(strScope, this.m_strPrivatePrefix) == 0) {
                this.ExecuteCommand("procon.protected.send", "admin.yell", strMessage, this.m_strShowMessageLength, "player", strAccountName);
            }
            else if (String.Compare(strScope, this.m_strPublicPrefix) == 0) {
                this.ExecuteCommand("procon.protected.send", "admin.yell", strMessage, this.m_strShowMessageLength, "all");
            }
            else if (String.Compare(strScope, this.m_strAdminsPrefix) == 0) {

                CPrivileges cpAccount = null;

                foreach (string soldierNames in this.m_dicPlayers.GetSoldierNameKeys()) {
                    cpAccount = this.GetAccountPrivileges(soldierNames);

                    if (cpAccount != null && cpAccount.PrivilegesFlags > 0) {
                        this.ExecuteCommand("procon.protected.send", "admin.yell", strMessage, this.m_strShowMessageLength, "player", soldierNames);
                    }
                }
            }
        }

        private void m_dicPlayers_QueueYellingResponse(string strScope, string strAccountName, string strMessage, string strTaskName, int iDelay, int iInterval, int iRepeat) {
            if (String.Compare(strScope, this.m_strPrivatePrefix) == 0) {
                this.ExecuteCommand("procon.protected.tasks.add", strTaskName, iDelay.ToString(), iInterval.ToString(), iRepeat.ToString(), "procon.protected.send", "admin.yell", strMessage, this.m_strShowMessageLength, "player", strAccountName);
            }
            else if (String.Compare(strScope, this.m_strPublicPrefix) == 0) {
                this.ExecuteCommand("procon.protected.tasks.add", strTaskName, iDelay.ToString(), iInterval.ToString(), iRepeat.ToString(), "procon.protected.send", "admin.yell", strMessage, this.m_strShowMessageLength, "all");
            }
            else if (String.Compare(strScope, this.m_strAdminsPrefix) == 0) {

                CPrivileges cpAccount = null;

                foreach (string soldierNames in this.m_dicPlayers.GetSoldierNameKeys()) {
                    cpAccount = this.GetAccountPrivileges(soldierNames);

                    if (cpAccount != null && cpAccount.PrivilegesFlags > 0) {
                        this.ExecuteCommand("procon.protected.tasks.add", strTaskName, iDelay.ToString(), iInterval.ToString(), iRepeat.ToString(), "procon.protected.send", "admin.yell", strMessage, this.m_strShowMessageLength, "player", soldierNames);
                    }
                }
            }
        }