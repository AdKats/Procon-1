using System;

namespace PRoCon.Core.Maps
{

    [Serializable]
    public class MaplistEntry
    {

        public int Index
        {
            get;
            private set;
        }

        public string Gamemode
        {
            get;
            private set;
        }
        /*
        public string FileNameOnly {
            get;
            private set;
        }
        */
        public string MapFileName
        {
            get;
            private set;
        }

        public int Rounds
        {
            get;
            private set;
        }

        public MaplistEntry(string strMapFileName)
        {
            this.Index = -1;
            this.MapFileName = strMapFileName;
            this.Rounds = 0;
        }

        public MaplistEntry(string strMapFileName, int iRounds)
        {
            this.Index = -1;
            this.MapFileName = strMapFileName;
            this.Rounds = iRounds;
        }

        public MaplistEntry(int index, string strMapFileName, int iRounds)
        {
            this.Index = index;
            this.MapFileName = strMapFileName;
            this.Rounds = iRounds;
        }

        public MaplistEntry(string gameMode, string strMapFileName, int iRounds)
        {
            this.Gamemode = gameMode;
            this.MapFileName = strMapFileName;
            this.Rounds = iRounds;
        }

        public MaplistEntry(string gameMode, string strMapFileName, int iRounds, int index)
        {
            this.Gamemode = gameMode;
            this.MapFileName = strMapFileName;
            this.Rounds = iRounds;
            this.Index = index;
        }
    }
}
