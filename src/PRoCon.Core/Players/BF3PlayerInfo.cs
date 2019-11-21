using System;
using System.Collections.Generic;

namespace PRoCon.Core.Players
{

    [Serializable, Obsolete]
    public class BF3PlayerInfo : CPlayerInfo
    {
        //  Player list is needed in OnPlayerList, OnPlayerLeave and server.onRoundOverPlayers
        public new static List<CPlayerInfo> GetPlayerList(List<string> words)
        {
            List<CPlayerInfo> lstReturnList = new List<CPlayerInfo>();

            int currentOffset = 0;
            int parameterCount = 0;
            int playerCount = 0;

            if (words.Count > currentOffset && int.TryParse(words[currentOffset++], out playerCount) == true)
            {

                if (words.Count > 0 && int.TryParse(words[currentOffset++], out parameterCount) == true)
                {
                    List<string> lstParameters = words.GetRange(currentOffset, parameterCount);

                    currentOffset += parameterCount;

                    for (int i = 0; i < playerCount; i++)
                    {
                        if (words.Count > currentOffset + (i * parameterCount))
                        {
                            lstReturnList.Add(new CPlayerInfo(lstParameters, words.GetRange(currentOffset + i * parameterCount, parameterCount)));
                        }
                    }

                }

            }

            return lstReturnList;
        }
    }
}
