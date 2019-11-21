using PRoCon.Core;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using System;
using System.Collections.Generic;

namespace PRoCon.Controls.Containers
{
    public class AdditionalPlayerInfo
    {
        public CPunkbusterInfo Punkbuster { get; set; }
        public String ResolvedHostName { get; set; }
        public CPlayerInfo Player { get; set; }
        public Inventory SpawnedInventory { get; set; }

        public Dictionary<Kits, int> KitCounter { get; private set; }

        public AdditionalPlayerInfo()
        {
            this.KitCounter = new Dictionary<Kits, int>();
        }

        public void AddKitCount(Kits kit)
        {

            if (this.KitCounter.ContainsKey(kit) == true)
            {
                this.KitCounter[kit] = this.KitCounter[kit] + 1;
            }
            else
            {
                this.KitCounter.Add(kit, 1);
            }
        }
    }
}
