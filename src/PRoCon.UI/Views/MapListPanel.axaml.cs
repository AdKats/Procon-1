using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PRoCon.Core.Maps;
using PRoCon.Core.Remote;

namespace PRoCon.UI.Views
{
    public static class GameData
    {
        private static readonly Dictionary<string, string> MapNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // BF4 Maps
            { "MP_Abandoned", "Zavod 311" },
            { "MP_Damage", "Lancang Dam" },
            { "MP_Flooded", "Flood Zone" },
            { "MP_Journey", "Golmud Railway" },
            { "MP_Naval", "Paracel Storm" },
            { "MP_Prison", "Operation Locker" },
            { "MP_Resort", "Hainan Resort" },
            { "MP_Siege", "Siege of Shanghai" },
            { "MP_TheDish", "Rogue Transmission" },
            { "MP_Tremors", "Dawnbreaker" },
            { "XP1_001", "Silk Road" },
            { "XP1_002", "Altai Range" },
            { "XP1_003", "Guilin Peaks" },
            { "XP1_004", "Dragon Pass" },
            { "XP0_Caspian", "Caspian Border 2014" },
            { "XP0_Firestorm", "Firestorm 2014" },
            { "XP0_Metro", "Operation Metro 2014" },
            { "XP0_Oman", "Gulf of Oman 2014" },
            { "XP2_001", "Lost Islands" },
            { "XP2_002", "Nansha Strike" },
            { "XP2_003", "Wave Breaker" },
            { "XP2_004", "Operation Mortar" },
            { "XP3_MarketPl", "Pearl Market" },
            { "XP3_Prpganda", "Propaganda" },
            { "XP3_UrbanGdn", "Lumpini Garden" },
            { "XP3_WtrFront", "Sunken Dragon" },
            { "XP4_Arctic", "Operation Whiteout" },
            { "XP4_SubBase", "Hammerhead" },
            { "XP4_Titan", "Hangar 21" },
            { "XP4_WalkerFactory", "Giants of Karelia" },
            { "XP5_Night_01", "Zavod: Graveyard Shift" },
            { "XP7_Valley", "Dragon Valley 2015" },
            // BF3 Maps
            { "MP_001", "Grand Bazaar" },
            { "MP_003", "Teheran Highway" },
            { "MP_007", "Caspian Border" },
            { "MP_011", "Seine Crossing" },
            { "MP_012", "Operation Firestorm" },
            { "MP_013", "Damavand Peak" },
            { "MP_017", "Noshahr Canals" },
            { "MP_018", "Kharg Island" },
            { "MP_Subway", "Operation Metro" },
            // BFH Maps
            { "mp_bank", "Bank Job" },
            { "mp_bloodout", "The Block" },
            { "mp_desert05", "Dust Bowl" },
            { "mp_downtown", "Downtown" },
            { "mp_eastside", "Derailed" },
            { "mp_glades", "Everglades" },
            { "mp_growhouse", "Growhouse" },
            { "mp_hills", "Hollywood Heights" },
            { "mp_offshore", "Riptide" },
        };

        private static readonly Dictionary<string, string> ModeNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "ConquestLarge0", "Conquest Large" },
            { "ConquestSmall0", "Conquest Small" },
            { "Domination0", "Domination" },
            { "Elimination0", "Defuse" },
            { "Obliteration", "Obliteration" },
            { "RushLarge0", "Rush" },
            { "SquadDeathMatch0", "Squad Deathmatch" },
            { "TeamDeathMatch0", "Team Deathmatch" },
            { "AirSuperiority0", "Air Superiority" },
            { "CaptureTheflag0", "Capture the Flag" },
            { "CarrierAssaultSmall0", "Carrier Assault Small" },
            { "CarrierAssaultLarge0", "Carrier Assault Large" },
            { "SquadObliteration0", "Squad Obliteration" },
            { "GunMaster0", "Gun Master" },
            { "GunMaster1", "Gun Master" },
            { "SquadRush0", "Squad Rush" },
            { "TurfWarLarge0", "Conquest Large" },
            { "TurfWarSmall0", "Conquest Small" },
            { "Heist0", "Heist" },
            { "Hotwire0", "Hotwire" },
            { "Bloodmoney0", "Blood Money" },
            { "Hit0", "Crosshair" },
            { "Hostage0", "Rescue" },
            { "CashGrab0", "Bounty Hunter" },
            { "SquadHeist0", "Squad Heist" },
            { "Chainlink0", "Chain Link" },
        };

        public static string GetMapName(string engineName)
        {
            if (string.IsNullOrEmpty(engineName)) return engineName;
            return MapNames.TryGetValue(engineName, out var name) ? name : engineName;
        }

        public static string GetModeName(string engineName)
        {
            if (string.IsNullOrEmpty(engineName)) return engineName;
            return ModeNames.TryGetValue(engineName, out var name) ? name : engineName;
        }
    }

    /// <summary>
    /// Provides the available map pool organized by expansion pack for each supported game.
    /// Each map entry includes supported game modes.
    /// </summary>
    public static class MapPool
    {
        public class MapInfo
        {
            public string FileName { get; set; }
            public string DisplayName { get; set; }
            public string[] SupportedModes { get; set; }
        }

        public class MapGroup
        {
            public string GroupName { get; set; }
            public List<MapInfo> Maps { get; set; }
        }

        private static readonly string[] BF4StandardModes = {
            "ConquestLarge0", "ConquestSmall0", "Domination0", "Elimination0",
            "Obliteration", "RushLarge0", "SquadDeathMatch0", "TeamDeathMatch0"
        };

        private static readonly string[] BF4DlcModes = {
            "ConquestLarge0", "ConquestSmall0", "Domination0", "Elimination0",
            "Obliteration", "RushLarge0", "SquadDeathMatch0", "TeamDeathMatch0",
            "AirSuperiority0", "CaptureTheflag0", "CarrierAssaultSmall0",
            "CarrierAssaultLarge0", "Chainlink0"
        };

        private static readonly string[] BF3Modes = {
            "ConquestLarge0", "ConquestSmall0", "RushLarge0",
            "SquadDeathMatch0", "TeamDeathMatch0", "SquadRush0"
        };

        private static readonly string[] BFHModes = {
            "TurfWarLarge0", "TurfWarSmall0", "Heist0", "Hotwire0",
            "Bloodmoney0", "Hit0", "Hostage0", "TeamDeathMatch0"
        };

        public static List<MapGroup> GetMapGroups(string gameType)
        {
            if (string.Equals(gameType, "BF4", StringComparison.OrdinalIgnoreCase))
                return GetBF4Maps();
            if (string.Equals(gameType, "BF3", StringComparison.OrdinalIgnoreCase))
                return GetBF3Maps();
            if (string.Equals(gameType, "BFHL", StringComparison.OrdinalIgnoreCase))
                return GetBFHMaps();

            // Default: return BF4 maps as fallback
            return GetBF4Maps();
        }

        private static MapInfo MakeMap(string fileName, string[] modes)
        {
            return new MapInfo
            {
                FileName = fileName,
                DisplayName = GameData.GetMapName(fileName),
                SupportedModes = modes
            };
        }

        private static List<MapGroup> GetBF4Maps()
        {
            return new List<MapGroup>
            {
                new MapGroup
                {
                    GroupName = "Base Maps",
                    Maps = new List<MapInfo>
                    {
                        MakeMap("MP_Abandoned", BF4StandardModes),
                        MakeMap("MP_Damage", BF4StandardModes),
                        MakeMap("MP_Flooded", BF4StandardModes),
                        MakeMap("MP_Journey", BF4StandardModes),
                        MakeMap("MP_Naval", BF4StandardModes),
                        MakeMap("MP_Prison", BF4StandardModes),
                        MakeMap("MP_Resort", BF4StandardModes),
                        MakeMap("MP_Siege", BF4StandardModes),
                        MakeMap("MP_TheDish", BF4StandardModes),
                        MakeMap("MP_Tremors", BF4StandardModes),
                    }
                },
                new MapGroup
                {
                    GroupName = "China Rising (XP1)",
                    Maps = new List<MapInfo>
                    {
                        MakeMap("XP1_001", BF4DlcModes),
                        MakeMap("XP1_002", BF4DlcModes),
                        MakeMap("XP1_003", BF4DlcModes),
                        MakeMap("XP1_004", BF4DlcModes),
                    }
                },
                new MapGroup
                {
                    GroupName = "Second Assault (XP0)",
                    Maps = new List<MapInfo>
                    {
                        MakeMap("XP0_Caspian", BF4DlcModes),
                        MakeMap("XP0_Firestorm", BF4DlcModes),
                        MakeMap("XP0_Metro", BF4DlcModes),
                        MakeMap("XP0_Oman", BF4DlcModes),
                    }
                },
                new MapGroup
                {
                    GroupName = "Naval Strike (XP2)",
                    Maps = new List<MapInfo>
                    {
                        MakeMap("XP2_001", BF4DlcModes),
                        MakeMap("XP2_002", BF4DlcModes),
                        MakeMap("XP2_003", BF4DlcModes),
                        MakeMap("XP2_004", BF4DlcModes),
                    }
                },
                new MapGroup
                {
                    GroupName = "Dragon's Teeth (XP3)",
                    Maps = new List<MapInfo>
                    {
                        MakeMap("XP3_MarketPl", BF4DlcModes),
                        MakeMap("XP3_Prpganda", BF4DlcModes),
                        MakeMap("XP3_UrbanGdn", BF4DlcModes),
                        MakeMap("XP3_WtrFront", BF4DlcModes),
                    }
                },
                new MapGroup
                {
                    GroupName = "Final Stand (XP4)",
                    Maps = new List<MapInfo>
                    {
                        MakeMap("XP4_Arctic", BF4DlcModes),
                        MakeMap("XP4_SubBase", BF4DlcModes),
                        MakeMap("XP4_Titan", BF4DlcModes),
                        MakeMap("XP4_WalkerFactory", BF4DlcModes),
                    }
                },
                new MapGroup
                {
                    GroupName = "Night Ops (XP5)",
                    Maps = new List<MapInfo>
                    {
                        MakeMap("XP5_Night_01", BF4StandardModes),
                    }
                },
                new MapGroup
                {
                    GroupName = "Legacy (XP7)",
                    Maps = new List<MapInfo>
                    {
                        MakeMap("XP7_Valley", BF4DlcModes),
                    }
                },
            };
        }

        private static List<MapGroup> GetBF3Maps()
        {
            return new List<MapGroup>
            {
                new MapGroup
                {
                    GroupName = "Base Maps",
                    Maps = new List<MapInfo>
                    {
                        MakeMap("MP_001", BF3Modes),
                        MakeMap("MP_003", BF3Modes),
                        MakeMap("MP_007", BF3Modes),
                        MakeMap("MP_011", BF3Modes),
                        MakeMap("MP_012", BF3Modes),
                        MakeMap("MP_013", BF3Modes),
                        MakeMap("MP_017", BF3Modes),
                        MakeMap("MP_018", BF3Modes),
                        MakeMap("MP_Subway", BF3Modes),
                    }
                },
            };
        }

        private static List<MapGroup> GetBFHMaps()
        {
            return new List<MapGroup>
            {
                new MapGroup
                {
                    GroupName = "Base Maps",
                    Maps = new List<MapInfo>
                    {
                        MakeMap("mp_bank", BFHModes),
                        MakeMap("mp_bloodout", BFHModes),
                        MakeMap("mp_desert05", BFHModes),
                        MakeMap("mp_downtown", BFHModes),
                        MakeMap("mp_eastside", BFHModes),
                        MakeMap("mp_glades", BFHModes),
                        MakeMap("mp_growhouse", BFHModes),
                        MakeMap("mp_hills", BFHModes),
                        MakeMap("mp_offshore", BFHModes),
                    }
                },
            };
        }
    }

    public partial class MapListPanel : UserControl
    {
        private PRoConClient _client;
        private readonly List<MaplistEntry> _mapList = new List<MaplistEntry>();
        private MapPool.MapInfo _selectedMapInfo;
        private bool _pendingRefresh;

        public MapListPanel()
        {
            InitializeComponent();
        }

        public void SetClient(PRoConClient client)
        {
            // Unwire previous client
            if (_client?.Game != null)
            {
                _client.Game.MapListListed -= OnMapListListed;
                _client.Game.MapListMapAppended -= OnMapListMapAppended;
                _client.Game.MapListMapInserted -= OnMapListMapInserted;
                _client.Game.MapListMapRemoved -= OnMapListMapRemoved;
                _client.Game.MapListCleared -= OnMapListCleared;
                _client.Game.MapListSave -= OnMapListSaved;
            }

            _client = client;

            if (_client?.Game != null)
            {
                _client.Game.MapListListed += OnMapListListed;
                _client.Game.MapListMapAppended += OnMapListMapAppended;
                _client.Game.MapListMapInserted += OnMapListMapInserted;
                _client.Game.MapListMapRemoved += OnMapListMapRemoved;
                _client.Game.MapListCleared += OnMapListCleared;
                _client.Game.MapListSave += OnMapListSaved;

                PopulateAvailableMaps();
                System.Console.WriteLine("[MapListPanel] Client set: " + client.HostNamePort);
            }
        }

        public void LoadData()
        {
            System.Console.WriteLine("[MapListPanel] LoadData");
            if (_client?.Game != null)
            {
                _client.Game.SendMapListListRoundsPacket();
            }
        }

        private void OnMapListSaved(FrostbiteClient sender)
        {
            System.Console.WriteLine("[MapListPanel] MapListSave OK received — refreshing map list");
            // Server confirmed save — now re-list
            if (_pendingRefresh && _client?.Game != null)
            {
                _pendingRefresh = false;
                _client.Game.SendMapListListRoundsPacket();
            }
        }

        private void PopulateAvailableMaps()
        {
            string gameType = _client?.Game?.GameType ?? "BF4";
            var groups = MapPool.GetMapGroups(gameType);

            var treeItems = new List<TreeViewItem>();
            foreach (var group in groups)
            {
                var groupNode = new TreeViewItem
                {
                    Header = group.GroupName,
                    IsExpanded = true
                };

                foreach (var map in group.Maps)
                {
                    var mapNode = new TreeViewItem
                    {
                        Header = $"{map.DisplayName}  ({map.FileName})",
                        Tag = map
                    };
                    groupNode.Items.Add(mapNode);
                }

                treeItems.Add(groupNode);
            }

            AvailableMapsTree.ItemsSource = treeItems;
        }

        private void OnAvailableMapSelected(object sender, SelectionChangedEventArgs e)
        {
            if (AvailableMapsTree.SelectedItem is TreeViewItem tvi && tvi.Tag is MapPool.MapInfo mapInfo)
            {
                _selectedMapInfo = mapInfo;
                var modeItems = mapInfo.SupportedModes
                    .Select(m => new ComboBoxItem
                    {
                        Content = GameData.GetModeName(m),
                        Tag = m
                    })
                    .ToList();

                ModeCombo.ItemsSource = modeItems;
                if (modeItems.Count > 0)
                    ModeCombo.SelectedIndex = 0;
            }
            else
            {
                _selectedMapInfo = null;
                ModeCombo.ItemsSource = null;
            }
        }

        private void OnAddToRotation(object sender, RoutedEventArgs e)
        {
            if (_client?.Game == null || _selectedMapInfo == null) return;

            string gamemode = string.Empty;
            if (ModeCombo.SelectedItem is ComboBoxItem cbi && cbi.Tag is string modeTag)
            {
                gamemode = modeTag;
            }

            int rounds = (int)(RoundsInput.Value ?? 1);
            if (rounds < 1) rounds = 1;

            var entry = new MaplistEntry(gamemode, _selectedMapInfo.FileName, rounds);
            System.Console.WriteLine("[MapListPanel] ADD: " + _selectedMapInfo.FileName + " " + gamemode + " rounds=" + rounds);
            _pendingRefresh = true;
            _client.Game.SendMapListAppendPacket(entry);
            _client.Game.SendMapListSavePacket();
        }

        // --- Event handlers for server responses ---

        private void OnMapListListed(FrostbiteClient sender, List<MaplistEntry> lstMapList)
        {
            System.Console.WriteLine("[MapListPanel] MapListListed: " + lstMapList.Count + " maps");
            Dispatcher.UIThread.Post(() =>
            {
                _mapList.Clear();
                _mapList.AddRange(lstMapList);
                RefreshRotationDisplay();
            });
        }

        private void OnMapListMapAppended(FrostbiteClient sender, MaplistEntry mapEntry)
        {
            System.Console.WriteLine("[MapListPanel] MapAppended: " + mapEntry.MapFileName);
            Dispatcher.UIThread.Post(() =>
            {
                _mapList.Add(mapEntry);
                RefreshRotationDisplay();
            });
        }

        private void OnMapListMapInserted(FrostbiteClient sender, MaplistEntry entry)
        {
            System.Console.WriteLine("[MapListPanel] MapInserted: " + entry.MapFileName);
            Dispatcher.UIThread.Post(() =>
            {
                LoadData();
            });
        }

        private void OnMapListMapRemoved(FrostbiteClient sender, int index)
        {
            System.Console.WriteLine("[MapListPanel] MapRemoved at index: " + index);
            Dispatcher.UIThread.Post(() =>
            {
                if (index >= 0 && index < _mapList.Count)
                {
                    _mapList.RemoveAt(index);
                }
                RefreshRotationDisplay();
            });
        }

        private void OnMapListCleared(FrostbiteClient sender)
        {
            System.Console.WriteLine("[MapListPanel] MapListCleared");
            Dispatcher.UIThread.Post(() =>
            {
                _mapList.Clear();
                RefreshRotationDisplay();
            });
        }

        private void RefreshRotationDisplay()
        {
            var items = new List<string>();
            for (int i = 0; i < _mapList.Count; i++)
            {
                var entry = _mapList[i];
                string mapDisplay = GameData.GetMapName(entry.MapFileName);
                string gamemodeRaw = !string.IsNullOrEmpty(entry.Gamemode) ? entry.Gamemode : "default";
                string gamemodeDisplay = GameData.GetModeName(gamemodeRaw);
                items.Add($"{i + 1}.  {mapDisplay}  |  {gamemodeDisplay}  |  Rounds: {entry.Rounds}");
            }
            MapRotationList.ItemsSource = items;
            System.Console.WriteLine("[MapListPanel] UI refreshed: " + items.Count + " items");
        }

        // --- Right column actions ---

        private void OnRemoveMap(object sender, RoutedEventArgs e)
        {
            if (_client?.Game == null) return;

            int index = MapRotationList.SelectedIndex;
            if (index < 0 || index >= _mapList.Count) return;

            System.Console.WriteLine("[MapListPanel] REMOVE at index: " + index);
            _pendingRefresh = true;
            _client.Game.SendMapListRemovePacket(index);
            _client.Game.SendMapListSavePacket();

            // Immediate local feedback
            _mapList.RemoveAt(index);
            RefreshRotationDisplay();
        }

        private void OnMoveUp(object sender, RoutedEventArgs e)
        {
            if (_client?.Game == null) return;

            int index = MapRotationList.SelectedIndex;
            if (index <= 0 || index >= _mapList.Count) return;

            System.Console.WriteLine("[MapListPanel] MOVE UP index: " + index);
            _pendingRefresh = true;
            var entry = _mapList[index];
            _client.Game.SendMapListRemovePacket(index);
            var insertEntry = new MaplistEntry(
                entry.Gamemode ?? string.Empty,
                entry.MapFileName,
                entry.Rounds,
                index - 1);
            _client.Game.SendMapListInsertPacket(insertEntry);
            _client.Game.SendMapListSavePacket();

            // Immediate local feedback
            _mapList.RemoveAt(index);
            _mapList.Insert(index - 1, entry);
            RefreshRotationDisplay();
            MapRotationList.SelectedIndex = index - 1;
        }

        private void OnMoveDown(object sender, RoutedEventArgs e)
        {
            if (_client?.Game == null) return;

            int index = MapRotationList.SelectedIndex;
            if (index < 0 || index >= _mapList.Count - 1) return;

            System.Console.WriteLine("[MapListPanel] MOVE DOWN index: " + index);
            _pendingRefresh = true;
            var entry = _mapList[index];
            _client.Game.SendMapListRemovePacket(index);
            var insertEntry = new MaplistEntry(
                entry.Gamemode ?? string.Empty,
                entry.MapFileName,
                entry.Rounds,
                index + 1);
            _client.Game.SendMapListInsertPacket(insertEntry);
            _client.Game.SendMapListSavePacket();

            // Immediate local feedback
            _mapList.RemoveAt(index);
            _mapList.Insert(index + 1, entry);
            RefreshRotationDisplay();
            MapRotationList.SelectedIndex = index + 1;
        }

        private void OnSetNextMap(object sender, RoutedEventArgs e)
        {
            if (_client?.Game == null) return;

            int index = MapRotationList.SelectedIndex;
            if (index < 0 || index >= _mapList.Count) return;

            _client.Game.SendMapListNextLevelIndexPacket(index);
        }

        // --- Bottom bar actions ---

        private void OnRestartRound(object sender, RoutedEventArgs e)
        {
            _client?.Game?.SendAdminRestartRoundPacket();
        }

        private void OnRunNextRound(object sender, RoutedEventArgs e)
        {
            _client?.Game?.SendAdminRunNextRoundPacket();
        }

        private void OnEndRound(object sender, RoutedEventArgs e)
        {
            // End round by running next round - same practical effect
            _client?.Game?.SendAdminRunNextRoundPacket();
        }
    }
}
