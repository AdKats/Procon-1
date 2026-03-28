using System.ComponentModel;

namespace PRoCon.UI.Models
{
    public class PlayerDisplayInfo : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Ping { get; set; }
        public int Squad { get; set; }
        public string IP { get; set; }

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
            if (string.IsNullOrEmpty(code) || code.Length != 2) return "";
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
