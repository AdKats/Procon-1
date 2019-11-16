using System;
using System.Collections.Generic;
using System.Text;

namespace PRoCon.Core {
    [Serializable]
    public class TimeoutSubset {

        public enum TimeoutSubsetType {
            None,
            Permanent,
            Round,
            Seconds,
        }

        public TimeoutSubsetType Subset {
            get;
            private set;
        }

        public int Timeout {
            get;
            set;
        }

		[Obsolete("This property is obsolete. Please use Timeout instead.", false)]
		public int Seconds {
            get { return this.Timeout; }
			set { this.Timeout = value; }
        }

        public TimeoutSubset(List<string> lstTimeoutSubsetWords) {

            this.Subset = TimeoutSubsetType.None;
            int iLength = 0;

            if (String.Compare(lstTimeoutSubsetWords[0], "perm") == 0) {
                this.Subset = TimeoutSubsetType.Permanent;
            }
			else if (String.Compare(lstTimeoutSubsetWords[0], "rounds") == 0 && (lstTimeoutSubsetWords.Count == 3 && int.TryParse(lstTimeoutSubsetWords[2], out iLength) == true) ||
																				(lstTimeoutSubsetWords.Count == 2 && int.TryParse(lstTimeoutSubsetWords[1], out iLength) == true)) {
				this.Subset = TimeoutSubsetType.Round;
				this.Timeout = iLength;
			}
			else if (String.Compare(lstTimeoutSubsetWords[0], "seconds") == 0 && lstTimeoutSubsetWords.Count >= 2 && int.TryParse(lstTimeoutSubsetWords[1], out iLength) == true) {
				this.Subset = TimeoutSubsetType.Seconds;
				this.Timeout = iLength;
			}
		}

        public TimeoutSubset(TimeoutSubsetType enTimeoutType) {
            this.Subset = enTimeoutType;
        }

        // Punkbuster..
        public TimeoutSubset(string strLength, string strServed) {

            int iServed = 0, iLength = 0;

            //if (String.Compare(strServed, "0") == 0 || String.Compare(strServed, "-1") == 0) {
            if (String.Compare(strServed, "0") == 0 && String.Compare(strLength, "-1") == 0) {
                this.Subset = TimeoutSubsetType.Permanent;
            }
            else if (int.TryParse(strServed, out iServed) == true && int.TryParse(strLength, out iLength) == true) {
                this.Subset = TimeoutSubsetType.Seconds;
                this.Timeout = (iLength - iServed) * 60;
            }
        }

        public TimeoutSubset(TimeoutSubsetType enTimeoutType, int iSeconds) {
            this.Subset = enTimeoutType;
            this.Timeout = iSeconds;
        }


        public static int RequiredLength(string strSubsetType) {

            int iRequiredLength = 1;

            if (String.Compare(strSubsetType, "seconds") == 0 || String.Compare(strSubsetType, "rounds") == 0) {
                iRequiredLength = 2;
            }
            // perm only need a List<string> with 1 string in it.

            return iRequiredLength;
        }
    }
}
