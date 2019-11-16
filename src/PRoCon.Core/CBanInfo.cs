namespace PRoCon.Core {
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class CBanInfo {
        public CBanInfo(string strIdType, string strId) {
            this.IdType = strIdType;
            if (String.Compare(strIdType, "name") == 0 || String.Compare(strIdType, "persona") == 0) {
                this.SoldierName = strId;
            }
            else if (String.Compare(strIdType, "ip") == 0) {
                this.IpAddress = strId;
            }
            else if (String.Compare(strIdType, "guid") == 0) {
                this.Guid = strId;
            }

            //this.m_ui32BanLength = 0;
            //this.m_ui32Time = 0;
            this.Reason = String.Empty;
            this.BanLength = new TimeoutSubset(TimeoutSubset.TimeoutSubsetType.None);
        }

        public CBanInfo(List<string> lstBanWords) {
            // Id-type, id, ban-type, time, (rounds) and reason
            // Used to pull data from a banList.list command which is 5 or 6 words long.
            if (lstBanWords.Count == 5 || lstBanWords.Count == 6) {

                this.IdType = lstBanWords[0];
                this.BanLength = new TimeoutSubset(lstBanWords.GetRange(2, lstBanWords.Count - 3));

                if (String.Compare(lstBanWords[0], "name") == 0 || String.Compare(lstBanWords[0], "persona") == 0) {
                    this.SoldierName = lstBanWords[1];
                }
                else if (String.Compare(lstBanWords[0], "ip") == 0) {
                    this.IpAddress = lstBanWords[1];
                }
                else if (String.Compare(lstBanWords[0], "guid") == 0) {
                    this.Guid = lstBanWords[1];
                }

                this.Reason = lstBanWords[lstBanWords.Count - 1];
            }
        }

        // Only used for a pbguid
        public CBanInfo(string strSoldierName, string strGUID, string strIP, TimeoutSubset ctsBanLength,
                       string strReason) {
            this.SoldierName = strSoldierName;
            this.Guid = strGUID;
            this.IpAddress = strIP;
            this.IdType = "pbguid";

            this.BanLength = ctsBanLength;
            this.Reason = strReason;
        }

        // Ban added
        public CBanInfo(string strBanType, string strId, TimeoutSubset ctsBanLength, string strReason) {
            this.IdType = strBanType;
            this.BanLength = ctsBanLength;
            this.Reason = strReason;

            if (String.Compare(this.IdType, "name") == 0 || String.Compare(this.IdType, "persona") == 0) {
                this.SoldierName = strId;
            }
            else if (String.Compare(this.IdType, "ip") == 0) {
                this.IpAddress = strId;
            }
            else if (String.Compare(this.IdType, "guid") == 0) {
                this.Guid = strId;
            }
        }

        /// <summary>
        /// The place in the list offset this item was recorded at. e.g banList.list 100 => [{ 100 ..  }, { 101 ..  }, { 102 ..  }] etc.
        /// </summary>
        public int? Offset { get; set; }

        public string SoldierName { get; private set; }

        public string Guid { get; private set; }

        public string IpAddress { get; private set; }

        public string IdType { get; private set; }

        public string Reason { get; private set; }

        public TimeoutSubset BanLength { get; private set; }

        public static List<CBanInfo> GetVanillaBanlist(List<string> lstWords, int offset) {

            List<CBanInfo> lstBans = new List<CBanInfo>();
            int iBans = 0;

            if (lstWords.Count >= 1 && int.TryParse(lstWords[0], out iBans) == true) {
                lstWords.RemoveAt(0);
                for (int i = 0; i < iBans; i++) {
                    lstBans.Add(new CBanInfo(lstWords.GetRange(i * 5, 5)) {
                        Offset = offset + i
                    });
                }
            }
            else {
                for (int i = 0; i < lstWords.Count / 6; i++) {
                    List<string> words = lstWords.GetRange(i * 6, 6);

                    lstBans.Add(new CBanInfo(words) {
                        Offset = offset + i
                    });
                }
            }

            return lstBans;
        }
    }
}