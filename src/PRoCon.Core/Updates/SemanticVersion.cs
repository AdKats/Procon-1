using System;
using System.Globalization;

namespace PRoCon.Core.Updates
{
    public struct SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }
        public string PreReleaseLabel { get; }
        public int? PreReleaseNumber { get; }

        public bool IsPreRelease => !string.IsNullOrEmpty(PreReleaseLabel);

        public SemanticVersion(int major, int minor, int patch, string preReleaseLabel = null, int? preReleaseNumber = null)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            PreReleaseLabel = preReleaseLabel;
            PreReleaseNumber = preReleaseNumber;
        }

        /// <summary>
        /// Parses versions like "2.0.0", "2.0.0-alpha.1", "2.0.0-beta.3".
        /// Also handles "2.0.0+buildmeta" (strips build metadata).
        /// </summary>
        public static bool TryParse(string input, out SemanticVersion result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Strip leading 'v' if present
            string s = input.TrimStart('v');

            // Strip build metadata (+...)
            int plusIdx = s.IndexOf('+');
            if (plusIdx >= 0)
                s = s.Substring(0, plusIdx);

            // Split on first '-' to separate core version from pre-release
            string coreStr;
            string preStr = null;
            int dashIdx = s.IndexOf('-');
            if (dashIdx >= 0)
            {
                coreStr = s.Substring(0, dashIdx);
                preStr = s.Substring(dashIdx + 1);
            }
            else
            {
                coreStr = s;
            }

            // Parse core version (major.minor.patch)
            string[] parts = coreStr.Split('.');
            if (parts.Length < 2 || parts.Length > 4)
                return false;

            if (!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out int major))
                return false;
            if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int minor))
                return false;
            int patch = 0;
            if (parts.Length >= 3 && !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out patch))
                return false;

            // Parse pre-release (e.g. "alpha.1", "beta.3", "rc.1")
            string label = null;
            int? number = null;
            if (!string.IsNullOrEmpty(preStr))
            {
                int dotIdx = preStr.IndexOf('.');
                if (dotIdx >= 0)
                {
                    label = preStr.Substring(0, dotIdx);
                    string numStr = preStr.Substring(dotIdx + 1);
                    if (int.TryParse(numStr, NumberStyles.None, CultureInfo.InvariantCulture, out int n))
                        number = n;
                }
                else
                {
                    label = preStr;
                }
            }

            result = new SemanticVersion(major, minor, patch, label, number);
            return true;
        }

        /// <summary>
        /// Comparison: stable > pre-release for the same version.
        /// Pre-release ordering: alpha &lt; beta &lt; rc, then by numeric suffix.
        /// </summary>
        public int CompareTo(SemanticVersion other)
        {
            int c = Major.CompareTo(other.Major);
            if (c != 0) return c;
            c = Minor.CompareTo(other.Minor);
            if (c != 0) return c;
            c = Patch.CompareTo(other.Patch);
            if (c != 0) return c;

            // No pre-release label means stable release — always newer than pre-release
            if (!IsPreRelease && other.IsPreRelease) return 1;
            if (IsPreRelease && !other.IsPreRelease) return -1;
            if (!IsPreRelease && !other.IsPreRelease) return 0;

            // Both have pre-release labels: compare alphabetically then numerically
            c = string.Compare(PreReleaseLabel, other.PreReleaseLabel, StringComparison.OrdinalIgnoreCase);
            if (c != 0) return c;

            int n1 = PreReleaseNumber ?? 0;
            int n2 = other.PreReleaseNumber ?? 0;
            return n1.CompareTo(n2);
        }

        public bool Equals(SemanticVersion other) => CompareTo(other) == 0;
        public override bool Equals(object obj) => obj is SemanticVersion other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, PreReleaseLabel?.ToLowerInvariant(), PreReleaseNumber);

        public static bool operator >(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) > 0;
        public static bool operator <(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) < 0;
        public static bool operator >=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) >= 0;
        public static bool operator <=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) <= 0;
        public static bool operator ==(SemanticVersion a, SemanticVersion b) => a.Equals(b);
        public static bool operator !=(SemanticVersion a, SemanticVersion b) => !a.Equals(b);

        public override string ToString()
        {
            string s = $"{Major}.{Minor}.{Patch}";
            if (IsPreRelease)
            {
                s += $"-{PreReleaseLabel}";
                if (PreReleaseNumber.HasValue)
                    s += $".{PreReleaseNumber.Value}";
            }
            return s;
        }
    }
}
