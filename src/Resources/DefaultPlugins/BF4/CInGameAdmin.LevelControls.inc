			public void RestartLevel(string responseScope, string accountName, int time) {

				if (this.m_dtCountdownBlocker < DateTime.Now) {
					if (time <= 60 && time >= 0) {
						this.m_dtCountdownBlocker = DateTime.Now.AddSeconds((double)time);

						// I have no doubts over 60 seconds the timer won't be accurate
						// but the end result will roughly be correct.
						for (int i = 0; i < time; i++) {
							if (i == time - 1) {
								this.QueueYellingResponse(responseScope, accountName, "Restarting map..", "CInGameAdminRestart" + accountName, i, 1, 1);
							}
							else {
								this.QueueYellingResponse(responseScope, accountName, String.Format("Restarting map in {0} seconds", time - i), "CInGameAdminRestart" + accountName, i, 1, 1);
							}
						}

						this.ExecuteCommand("procon.protected.tasks.add", "CInGameAdminRestart" + accountName, time.ToString(), "1", "1", "procon.protected.send", "mapList.restartRound");
					}
					else {
						this.ExecuteCommand("procon.protected.send", "admin.say", "Invalid arguments.  [timeout - value between 0 and 60 seconds]", "player", accountName);
					}
				}
				else {
					this.ExecuteCommand("procon.protected.send", "admin.say", "A countdown is already in progress.", "player", accountName);
				}
			}

			public void RestartLevel(string responseScope, string accountName) {
				if (this.m_dtCountdownBlocker < DateTime.Now) {
					this.ExecuteCommand("procon.protected.send", "mapList.restartRound");
				}
				else {
					this.ExecuteCommand("procon.protected.send", "admin.say", "A countdown is already in progress.", "player", accountName);
				}
			}

			public void ForwardLevel(string responseScope, string accountName, int time) {

				if (this.m_dtCountdownBlocker < DateTime.Now) {
					if (time <= 60 && time >= 0) {
						this.m_dtCountdownBlocker = DateTime.Now.AddSeconds((double)time);

						// I have no doubts over 60 seconds the timer won't be accurate
						// but the end result will roughly be correct.
						for (int i = 0; i < time; i++) {
							if (i == time - 1) {
								this.QueueYellingResponse(responseScope, accountName, "Forwarding to next level..", "CInGameAdminForward" + accountName, i, 1, 1);
							}
							else {
								this.QueueYellingResponse(responseScope, accountName, String.Format("Forwarding level in {0} seconds", time - i), "CInGameAdminForward" + accountName, i, 1, 1);
							}
						}

						this.ExecuteCommand("procon.protected.tasks.add", "CInGameAdminForward" + accountName, time.ToString(), "1", "1", "procon.protected.send", "mapList.runNextRound");
					}
					else {
						this.ExecuteCommand("procon.protected.send", "admin.say", "Invalid arguments.  [timeout - value between 0 and 60 seconds]", "player", accountName);
					}
				}
				else {
					this.ExecuteCommand("procon.protected.send", "admin.say", "A countdown is already in progress.", "player", accountName);
				}
			}

			public void ForwardLevel(string responseScope, string accountName) {
				if (this.m_dtCountdownBlocker < DateTime.Now) {
					this.ExecuteCommand("procon.protected.send", "mapList.runNextRound");
				}
				else {
					this.ExecuteCommand("procon.protected.send", "admin.say", "A countdown is already in progress.", "player", accountName);
				}
			}

            public void EndLevel(string responseScope, string accountName, string teamID, int time) {

				if (this.m_dtCountdownBlocker < DateTime.Now) {
					if (time <= 60 && time >= 0) {
						this.m_dtCountdownBlocker = DateTime.Now.AddSeconds((double)time);

						// I have no doubts over 60 seconds the timer won't be accurate
						// but the end result will roughly be correct.
						for (int i = 0; i < time; i++) {
							if (i == time - 1) {
								this.QueueYellingResponse(responseScope, accountName, "Ending round..", "CInGameAdminEndRond" + accountName, i, 1, 1);
							}
							else {
								this.QueueYellingResponse(responseScope, accountName, String.Format("Ending round in {0} seconds", time - i), "CInGameAdminEndRound" + accountName, i, 1, 1);
							}
						}

						this.ExecuteCommand("procon.protected.tasks.add", "CInGameAdminEndRound" + accountName, time.ToString(), "1", "1", "procon.protected.send", "mapList.endRound", teamID);
					}
					else {
						this.ExecuteCommand("procon.protected.send", "admin.say", "Invalid arguments.  [timeout - value between 0 and 60 seconds]", "player", accountName);
					}
				}
				else {
					this.ExecuteCommand("procon.protected.send", "admin.say", "A countdown is already in progress.", "player", accountName);
				}
			}

			public void EndLevel(string responseScope, string accountName, string teamID) {
				if (this.m_dtCountdownBlocker < DateTime.Now) {
					this.ExecuteCommand("procon.protected.send", "mapList.endRound", teamID);
				}
				else {
					this.ExecuteCommand("procon.protected.send", "admin.say", "A countdown is already in progress.", "player", accountName);
				}
			}
