// BF3 specific commands, same as BFBC2

                this.RegisterCommand(
                    new MatchCommand(
                        "CInGameAdmin",
                        "OnCommandYell",
                        scopes,
                        this.m_strYellCommand,
                        this.Listify<MatchArgumentFormat>(
                            new MatchArgumentFormat(
                                "text",
                                emptyList
                            )
                        ),
                        new ExecutionRequirements(
                            ExecutionScope.Account,
                            "You do not have enough privileges to server yell"),
                        "Yells a message via the server to all players"
                    )
                );

                this.RegisterCommand(
                    new MatchCommand(
                        "CInGameAdmin",
                        "OnCommandPlayerYell",
                        scopes,
                        this.m_strPlayerYellCommand,
                        this.Listify<MatchArgumentFormat>(
                            new MatchArgumentFormat(
                                "playername",
                                this.m_dicPlayers.GetSoldierNameKeys()
                            ),
                            new MatchArgumentFormat(
                                "text",
                                emptyList
                            )
                        ),
                        new ExecutionRequirements(
                            ExecutionScope.Account,
                            2,
                            confirmationCommand,
                            "You do not have enough privileges to server yell to a player"),
                        "Yells a message via the server to a single player"
                    )
                );

                    this.RegisterCommand(
                        new MatchCommand(
                            "CInGameAdmin",
                            "OnCommandMove",
                            scopes,
                            this.m_strMoveCommand,
                            this.Listify<MatchArgumentFormat>(
                                new MatchArgumentFormat(
                                    "playername",
                                    this.m_dicPlayers.GetSoldierNameKeys()
                                ),
                                new MatchArgumentFormat(
                                    "squad",
                                    this.m_squadNames
                                )
                            ),
                            new ExecutionRequirements(
                                ExecutionScope.Privileges,
                                Privileges.CanMovePlayers,
                                2,
                                confirmationCommand,
                                "You do not have enough privileges to move players"),
                            "Moves a player to a specific squad on the same team next death"
                        )
                    );

                    this.RegisterCommand(
                        new MatchCommand(
                            "CInGameAdmin",
                            "OnCommandMove",
                            scopes,
                            this.m_strMoveCommand,
                            this.Listify<MatchArgumentFormat>(
                                new MatchArgumentFormat(
                                    "playername",
                                    this.m_dicPlayers.GetSoldierNameKeys()
                                ),
                                new MatchArgumentFormat(
                                    "team",
                                    teamNamesForMap
                                ),
                                new MatchArgumentFormat(
                                    "squad",
                                    this.m_squadNames
                                )
                            ),
                            new ExecutionRequirements(
                                ExecutionScope.Privileges,
                                Privileges.CanMovePlayers,
                                2,
                                confirmationCommand,
                                "You do not have enough privileges to move players"),
                            "Moves a player to a specific team and squad next death"
                        )
                    );

                    this.RegisterCommand(
                        new MatchCommand(
                            "CInGameAdmin",
                            "OnCommandForceMove",
                            scopes,
                            this.m_strForceMoveCommand,
                            this.Listify<MatchArgumentFormat>(
                                new MatchArgumentFormat(
                                    "playername",
                                    this.m_dicPlayers.GetSoldierNameKeys()
                                ),
                                new MatchArgumentFormat(
                                    "squad",
                                    this.m_squadNames
                                )
                            ),
                            new ExecutionRequirements(
                                ExecutionScope.Privileges,
                                Privileges.CanMovePlayers,
                                2,
                                confirmationCommand,
                                "You do not have enough privileges to move players"),
                            "Kills a player and moves them to a specific squad on the same team"
                        )
                    );

                    this.RegisterCommand(
                        new MatchCommand(
                            "CInGameAdmin",
                            "OnCommandForceMove",
                            scopes,
                            this.m_strForceMoveCommand,
                            this.Listify<MatchArgumentFormat>(
                                new MatchArgumentFormat(
                                    "playername",
                                    this.m_dicPlayers.GetSoldierNameKeys()
                                ),
                                new MatchArgumentFormat(
                                    "team",
                                    teamNamesForMap
                                ),
                                new MatchArgumentFormat(
                                    "squad",
                                    this.m_squadNames
                                )
                            ),
                            new ExecutionRequirements(
                                ExecutionScope.Privileges,
                                Privileges.CanMovePlayers,
                                2,
                                confirmationCommand,
                                "You do not have enough privileges to move players"),
                            "Kills a player and moves them to a specific team and squad"
                        )
                    );