using System.ComponentModel;

namespace PRoCon.UI.Models
{
    public class PlayerDisplayInfo : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string ClanTag { get; set; }
        public int TeamID { get; set; }

        private int _score;
        public int Score
        {
            get => _score;
            set { _score = value; OnPropertyChanged(nameof(Score)); OnPropertyChanged(nameof(ScoreText)); }
        }

        private int _kills;
        public int Kills
        {
            get => _kills;
            set { _kills = value; OnPropertyChanged(nameof(Kills)); OnPropertyChanged(nameof(KillsText)); }
        }

        private int _deaths;
        public int Deaths
        {
            get => _deaths;
            set { _deaths = value; OnPropertyChanged(nameof(Deaths)); OnPropertyChanged(nameof(DeathsText)); }
        }

        public int Ping { get; set; }
        public int Squad { get; set; }
        public string IP { get; set; }

        // Player type from Frostbite: 0=player, 1=spectator, 2=commander
        public int PlayerType { get; set; }
        public bool IsSpectator => PlayerType == 1;
        public bool IsCommander => PlayerType == 2;

        private bool _isAlive = true;
        public bool IsAlive
        {
            get => _isAlive;
            set { _isAlive = value; OnPropertyChanged(nameof(IsAlive)); OnPropertyChanged(nameof(StatusIcon)); }
        }

        public string StatusIcon => IsAlive ? "" : "\u2620";

        private string _country = "";
        public string Country
        {
            get => _country;
            set { _country = value; OnPropertyChanged(nameof(Country)); OnPropertyChanged(nameof(CountryText)); OnPropertyChanged(nameof(FlagText)); }
        }

        private string _countryCode = "";
        public string CountryCode
        {
            get => _countryCode;
            set { _countryCode = value; OnPropertyChanged(nameof(CountryCode)); OnPropertyChanged(nameof(FlagText)); }
        }

        private bool _isVPN;
        public bool IsVPN
        {
            get => _isVPN;
            set { _isVPN = value; OnPropertyChanged(nameof(IsVPN)); OnPropertyChanged(nameof(ThreatText)); }
        }

        private bool _isProxy;
        public bool IsProxy
        {
            get => _isProxy;
            set { _isProxy = value; OnPropertyChanged(nameof(IsProxy)); OnPropertyChanged(nameof(ThreatText)); }
        }

        public string ScoreText => Score.ToString();
        public string KillsText => Kills.ToString();
        public string DeathsText => Deaths.ToString();
        public string PingText => Ping.ToString();
        public string SquadText => Squad > 0 ? Squad.ToString() : "-";
        public string CountryText => !string.IsNullOrEmpty(Country) ? Country : "";
        public string FlagText => CountryCodeToFlag(CountryCode);
        public string ThreatText => IsVPN ? "VPN" : IsProxy ? "PROXY" : "";

        private static string CountryCodeToFlag(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length != 2
                || !char.IsLetter(code[0]) || !char.IsLetter(code[1])) return "";
            // Convert country code to regional indicator emoji
            return string.Concat(
                char.ConvertFromUtf32(0x1F1E6 + (code.ToUpper()[0] - 'A')),
                char.ConvertFromUtf32(0x1F1E6 + (code.ToUpper()[1] - 'A'))
            );
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
