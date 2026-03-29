			public string FrostbiteBanMethod {
				get {
					string frostbiteBanMethod = "none";
				
					if (this.BanMethod == BanTypes.FrostbiteName) {
						// obsolete R-8 frostbiteBanMethod = "persona";
						frostbiteBanMethod = "name";
					}
					else if (this.BanMethod == BanTypes.FrostbiteEaGuid) {
						frostbiteBanMethod = "guid";
					}

					return frostbiteBanMethod;
				}
			}