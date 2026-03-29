// This code may be distributed under the terms and conditions of the GNU LGPL v3
// The LGPL can be read here: http://www.gnu.org/licenses/lgpl.html

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using PRoCon.Core;
using PRoCon.Core.Players;
using PRoCon.Core.Plugin;
using Api = PRoCon.Core.Plugin.PRoConPluginAPI;

namespace PRoConEvents
{
	public class LanguageEnforcer : LanguageEnforcerBase, IPRoConPluginInterface
	{
		private readonly Regex _failPrevent = new Regex("(\\W(?<![!%\\|\\+\\*'-\\.]))");

		private string _resLatentKill = "LanguageEnforcer killed for previous language";
		private string _resOnlineAdmins = "Online Admins: ";
		private string _resNoOnlineAdmins = "Sorry. No Admins are online at this time.";
		private string _resCounterReset = "Your Language counter has been reset.";
		private string[] _resLangInfo = { "LanguageEnforcer kills do not affect your stats!", "Lang killed while dead = kill on spawn", "Your counter will be decreased by %cooldown% daily", "Your current counter reads %count%" };
		private string[] _adminHelpText = { "!kill, !kick [reason], !tban [reason], !ban [reason]", "!nuke + !cancel", "!say, !psay [player], !yell, !pyell [player]", "!langreset [player] -- badword akte loeschen", "!langpunish [player] -- manuelles ausloesen bei badword" };
		private string[] _helpText = { "use !admin to list online Admins", "NO RULES on this Server" };


		private readonly char[] _commandStartChars = { '!', '#', '@', '/' };
		private DateTime _startup; //deactivate the regex setting for the first few seconds to prevent messup via procon
		internal readonly Dictionary<string, PlayerInfo> _players = new Dictionary<string, PlayerInfo>(); //contains the list of cursing players. key is playername
		private readonly List<SuccessiveMeasure> _measures = GetDefaultMeasures();
		private readonly Dictionary<string, MeasureOverride> _overrides = new Dictionary<string, MeasureOverride>();

		private readonly Dictionary<string, string> _badwordSection = new Dictionary<string, string>();
		private readonly Dictionary<string, string> _regexBadwordSection = new Dictionary<string, string>();
		private string[] _badwords = new string[0];
		private Regex[] _regexBadwords = new Regex[0];
		private string[] _badwordsCache = new string[0];
		private string[] _regexBadwordsCache = new string[0]; //used for displaying since Regex.ToString may return other things than the user specified
		private float _coolDown = 3; //cooldown steps per day
		private float _adminCoolDown = 3; //cooldown steps per day
		private HashSet<string> _whitelist = new HashSet<string>(); //Hashset Contains is the fastest
		private bool _whitelistAdmins;
		private bool _disallowPlayerSelfReset;
		private bool _warnWhitelisted; //tells if whitelisted players should be warned
		private bool _ignoreSquadChat;
		private bool _enableLatentKills = true;
		private bool _extendedFunctions = true; //!help !admin ...

		private uint _updateCounter = 5;
		private uint _maxUpdateCounter = 6;
		private bool _updateAvailable;
		private bool _saveCountersAsap = true;

		private static List<SuccessiveMeasure> GetDefaultMeasures()
		{
			return new List<SuccessiveMeasure> {
				new SuccessiveMeasure{ Action = BadwordAction.Warn, PublicMessage = new []{"%player% warned for Language violation. {Next Time: Kill}"}, PrivateMessage = new []{"Type \"!langinfo\" for more information"}, YellMessage = new []{"Watch your Language!"}, YellTime = 15},
				new SuccessiveMeasure{ Action = BadwordAction.Kill, PublicMessage = new []{"%player% killed for Language violation. {Next Time: Kick}"}, PrivateMessage = new []{"%player%, you risk being REMOVED if you continue using this type of lanuage!"}, YellMessage = new []{"Watch your Language!"}, YellTime = 30},
				new SuccessiveMeasure{ Action = BadwordAction.Kick, PublicMessage = new []{"%player% kicked for Language violation. {Next Time: TempBan}"}, PrivateMessage = new []{"%player% kicked for Language violation. {Next Time: TempBan}"}},
				new SuccessiveMeasure{ Action = BadwordAction.TBan, PublicMessage = new []{"%player% temp banned %time% minutes for Language violation."}, PrivateMessage= new []{"%player% temp banned %time% minutes for Language violation."}, TBanTime = 30},
				new SuccessiveMeasure{ Action = BadwordAction.TBan, PublicMessage = new []{"%player% temp banned %time% minutes for Language violation."}, PrivateMessage= new []{"%player% temp banned %time% minutes for Language violation."}, TBanTime = 45},
				new SuccessiveMeasure(),
			};
		}

		/// <summary>
		/// Wrapper for the regex Badwords since a ready regex class is faster than making a new one
		/// </summary>
		private string[] RegexBadwords
		{
			get { return _regexBadwordsCache; }
			set
			{
				_regexBadwordsCache = value;
				//cut off anything after '#' to use that as a comment
				_regexBadwords = CreateSections(value, true).Select(r => r.IndexOf('#') >= 0 ? r.Substring(0, r.IndexOf('#')) : r).Where(r => !string.IsNullOrEmpty(r)).Select(r => new Regex(r, RegexOptions.IgnoreCase)).ToArray();
			}
		}

		#region Settings

		public bool LookForUpdates
		{
			get { return _lookForUpdates; }
			set
			{
				_lookForUpdates = value;
				RunUpdateTask = value;
			}
		}

		public List<CPluginVariable> GetDisplayPluginVariables()
		{
			return new List<CPluginVariable>(GetVariables(false));
		}

		public List<CPluginVariable> GetPluginVariables()
		{
			return new List<CPluginVariable>(GetVariables(true));
		}

		public IEnumerable<CPluginVariable> GetVariables(bool getAll)
		{
			//type safety + needs less space
			string badActEnum = ProconUtil.CreateEnumString<BadwordAction>();
			string badActEnumNoEnd = badActEnum.Replace(BadwordAction.ListEnd + "|", "");
			Func<string, float, CPluginVariable> FloatPluginVariable = (name, value) => new CPluginVariable(name, typeof(string), value.ToString("0.00", CultureInfo.InvariantCulture.NumberFormat));
			Func<string, uint, CPluginVariable> UnIntPluginVariable = (name, value) => new CPluginVariable(name, typeof(int), value);
			Func<string, bool, CPluginVariable> YesNoPluginVariable = (name, value) => new CPluginVariable(name, typeof(enumBoolYesNo), value ? enumBoolYesNo.Yes : enumBoolYesNo.No);
			Func<string, string, CPluginVariable> StrngPluginVariable = (name, value) => new CPluginVariable(name, typeof(string), value.SavePrepare(getAll));
			Func<string, IEnumerable<string>, CPluginVariable> SArayPluginVariable = (name, value) => new CPluginVariable(name, typeof(string[]), value.SavePrepare(getAll));
			Func<string, string, CPluginVariable> ActinPluginVariable = (name, value) => new CPluginVariable(name, badActEnum, value);

			Func<string, string, CPluginVariable> OvrrdPluginVariable = (name, value) => new CPluginVariable(name, badActEnumNoEnd, value);
			Func<string, uint?, CPluginVariable> OvIntPluginVariable = (name, value) => new CPluginVariable(name, typeof(string), value == null ? "No override" : value.ToString());


			yield return FloatPluginVariable("2 - General|Cooldown steps per day", _coolDown);
			yield return FloatPluginVariable("2 - General|Admin cooldown per day", _adminCoolDown);
			yield return new CPluginVariable("2 - General|Log to", ProconUtil.CreateEnumString<LoggingTarget>(), LogTarget.ToString());
			yield return YesNoPluginVariable("2 - General|Enable latent kills", _enableLatentKills);
			yield return YesNoPluginVariable("2 - General|Load/Save counters to disk", SaveCounters);
			yield return YesNoPluginVariable("2 - General|Use Punkbuster GUIDs", _usePbGuid);
			yield return UnIntPluginVariable("2 - General|Kill delay (ms)", _killDelay);
			yield return UnIntPluginVariable("2 - General|Kill delay on spawn (ms)", _killOnspawnDelay);
			if (SaveCounters)
				yield return YesNoPluginVariable("2 - General|Save counters on every punish", _saveCountersAsap);

			yield return YesNoPluginVariable("2 - General|Let AdKats determine who is an admin", _useAdKatsAdmins);
			yield return YesNoPluginVariable("2 - General|Use AdKats punishment", _useAdKatsPunish);
			if (!_useAdKatsPunish)
				yield return YesNoPluginVariable("2 - General|Use AdKats to issue kills and bans", _useAdKatsBan);
		    if (!_useAdKatsAdmins)
		        yield return YesNoPluginVariable("2 - General|Get Admins from textfile", GetAdminsFromDisk);

			yield return YesNoPluginVariable("2 - General|Look for Updates", LookForUpdates);
			if (LookForUpdates || getAll)
				yield return UnIntPluginVariable("2 - General|Look for Updates every X hours", _maxUpdateCounter);


			for (int i = 0; i < _measures.Count; i++)
			{
				var measure = _measures[i];
				string meastring = measure.Action.ToString();
				int dispNo = i + 1;
				switch (measure.Action)
				{
					case BadwordAction.Warn:
						yield return ActinPluginVariable(String.Format("3.{0} - Measure {0} - {1} x{2}|Measure #{0} - Measure", dispNo, meastring, measure.Count), meastring);
						yield return UnIntPluginVariable(String.Format("3.{0} - Measure {0} - {1} x{2}|Measure #{0} - Repeat X times", dispNo, meastring, measure.Count), measure.Count);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - {1} x{2}|Measure #{0} - Public chat message", dispNo, meastring, measure.Count), measure.PublicMessage);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - {1} x{2}|Measure #{0} - Private chat message", dispNo, meastring, measure.Count), measure.PrivateMessage);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - {1} x{2}|Measure #{0} - Yell message", dispNo, meastring, measure.Count), measure.YellMessage);
						yield return UnIntPluginVariable(String.Format("3.{0} - Measure {0} - {1} x{2}|Measure #{0} - Yell time (sec.)", dispNo, meastring, measure.Count), measure.YellTime);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - {1} x{2}|Measure #{0} - Command", dispNo, meastring, measure.Count), measure.Command);
						break;
					case BadwordAction.Kill:
						goto case BadwordAction.Warn;
					case BadwordAction.Kick:
						yield return ActinPluginVariable(String.Format("3.{0} - Measure {0} - Kick x{1}|Measure #{0} - Measure", dispNo, measure.Count), meastring);
						yield return UnIntPluginVariable(String.Format("3.{0} - Measure {0} - Kick x{1}|Measure #{0} - Repeat X times", dispNo, measure.Count), measure.Count);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - Kick x{1}|Measure #{0} - Public chat message", dispNo, measure.Count), measure.PublicMessage);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - Kick x{1}|Measure #{0} - Kick reason", dispNo, measure.Count), measure.PrivateMessage);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - Kick x{1}|Measure #{0} - Command", dispNo, measure.Count), measure.Command);
						break;
					case BadwordAction.TBan:
						yield return ActinPluginVariable(String.Format("3.{0} - Measure {0} - TBan{2} x{1}|Measure #{0} - Measure", dispNo, measure.Count, measure.TBanTime), meastring);
						yield return UnIntPluginVariable(String.Format("3.{0} - Measure {0} - TBan{2} x{1}|Measure #{0} - Repeat X times", dispNo, measure.Count, measure.TBanTime), measure.Count);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - TBan{2} x{1}|Measure #{0} - Public chat message", dispNo, measure.Count, measure.TBanTime), measure.PublicMessage);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - TBan{2} x{1}|Measure #{0} - TBan reason", dispNo, measure.Count, measure.TBanTime), measure.PrivateMessage);
						yield return UnIntPluginVariable(String.Format("3.{0} - Measure {0} - TBan{2} x{1}|Measure #{0} - TBan minutes", dispNo, measure.Count, measure.TBanTime), measure.TBanTime);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - TBan{2} x{1}|Measure #{0} - Command", dispNo, measure.Count, measure.TBanTime), measure.Command);
						break;
					case BadwordAction.PermBan:
						yield return ActinPluginVariable(String.Format("3.{0} - Measure {0} - Permanent Ban|Measure #{0} - Measure", dispNo), meastring);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - Permanent Ban|Measure #{0} - Public chat message", dispNo), measure.PublicMessage);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - Permanent Ban|Measure #{0} - Ban reason", dispNo), measure.PrivateMessage);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - Permanent Ban|Measure #{0} - Command", dispNo), measure.Command);
						break;
					case BadwordAction.ShowRules:
						yield return ActinPluginVariable(String.Format("3.{0} - Measure {0} - Show Rules|Measure #{0} - Measure", dispNo), meastring);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - Show Rules|Measure #{0} - Command", dispNo), measure.Command);
						break;
					case BadwordAction.Custom:
						yield return ActinPluginVariable(String.Format("3.{0} - Measure {0} - Custom Command|Measure #{0} - Measure", dispNo), meastring);
						yield return SArayPluginVariable(String.Format("3.{0} - Measure {0} - Custom Command|Measure #{0} - Command", dispNo), measure.Command);
						break;
				}
				if (measure.Action == BadwordAction.ListEnd)
				{
					yield return ActinPluginVariable(String.Format("3.{0} - Measure List End|Measure #{0} - Measure", dispNo), meastring);
					break; //end the for-loop (not possible inside switch)
				}
			}

			yield return SArayPluginVariable("4 - Excluded Players|Whitelist", _whitelist);
			yield return YesNoPluginVariable("4 - Excluded Players|Treat Admins as Whitelisted", _whitelistAdmins);
			yield return YesNoPluginVariable("4 - Excluded Players|Disallow player self reset", _disallowPlayerSelfReset);
			yield return YesNoPluginVariable("4 - Excluded Players|Warn Whitelisted", _warnWhitelisted);
			yield return YesNoPluginVariable("4 - Excluded Players|Ignore squad chat", _ignoreSquadChat);

			yield return SArayPluginVariable("1 - Wordlists|Badwords", _badwordsCache);
			yield return SArayPluginVariable("1 - Wordlists|Regex Badwords", RegexBadwords);

			yield return StrngPluginVariable("5 - Messages|Latent kill message", _resLatentKill);
			yield return StrngPluginVariable("5 - Messages|Counter reset message", _resCounterReset);
			yield return SArayPluginVariable("5 - Messages|!langinfo message", _resLangInfo);
			if (_extendedFunctions || getAll)
			{
				yield return StrngPluginVariable("5 - Messages|Online admins message header", _resOnlineAdmins);
				yield return StrngPluginVariable("5 - Messages|No admins online message", _resNoOnlineAdmins);
			}

			var sections = _badwordSection.Values.Concat(_regexBadwordSection.Values).Distinct().ToArray();
			for (int index = 0; index < sections.Length; index++)
			{
				var dispNo = index + 1;
				var section = sections[index];
				var enabled = _overrides.ContainsKey(section);
				yield return YesNoPluginVariable(String.Format("6.{0} - Measure override section '{1}'|Section '{1}' - Enabled", dispNo, section), enabled);
				if (enabled)
				{
					var mo = _overrides[section];
					if (_useAdKatsPunish)
					{
						yield return YesNoPluginVariable(String.Format("6.{0} - Measure override section '{1}'|Section '{1}' - Use AdKats punish", dispNo, section), !mo.NoAdKats);
					}
					else
					{
						yield return FloatPluginVariable(String.Format("6.{0} - Measure override section '{1}'|Section '{1}' - Severity", dispNo, section), mo.Severity);
						yield return YesNoPluginVariable(String.Format("6.{0} - Measure override section '{1}'|Section '{1}' - Allow higher measures", dispNo, section), !mo.AlwaysUseMinAction);
						yield return FloatPluginVariable(String.Format("6.{0} - Measure override section '{1}'|Section '{1}' - Minimum counter afterwards", dispNo, section), mo.MinimumCounter + 1);
					}

					if (!_useAdKatsPunish || (_useAdKatsPunish && mo.NoAdKats))
					{
						yield return OvrrdPluginVariable(String.Format("6.{0} - Measure override section '{1}'|Section '{1}' - Measure", dispNo, section), mo.MinimumAction.ToString());
						if (mo.MinimumAction != BadwordAction.ShowRules && mo.MinimumAction != BadwordAction.Custom)
						{
							yield return SArayPluginVariable(String.Format("6.{0} - Measure override section '{1}'|Section '{1}' - Public message", dispNo, section), mo.PublicMessage);
							yield return SArayPluginVariable(String.Format("6.{0} - Measure override section '{1}'|Section '{1}' - Private message", dispNo, section), mo.PrivateMessage);
							yield return SArayPluginVariable(String.Format("6.{0} - Measure override section '{1}'|Section '{1}' - Yell message", dispNo, section), mo.YellMessage);
							yield return OvIntPluginVariable(String.Format("6.{0} - Measure override section '{1}'|Section '{1}' - Yell time (sec.)", dispNo, section), mo.YellTime);
							yield return SArayPluginVariable(String.Format("6.{0} - Measure override section '{1}'|Section '{1}' - Command", dispNo, section), mo.Command);
						}
					}
					else
					{
						yield return SArayPluginVariable(String.Format("6.{0} - Measure override section '{1}'|Section '{1}' - Public message", dispNo, section), mo.PublicMessage);
					}

					if (mo.MinimumAction != BadwordAction.ShowRules && mo.MinimumAction != BadwordAction.Custom && (!_useAdKatsPunish || (_useAdKatsPunish && mo.NoAdKats && mo.MinimumAction == BadwordAction.TBan)))
						yield return OvIntPluginVariable(String.Format("6.{0} - Measure override section '{1}'|Section '{1}' - TBan minutes", dispNo, section), mo.TBanTime);

				}
			}

			yield return YesNoPluginVariable("7 - Extended Functions|Enable Extended Functions", _extendedFunctions);
			if (_extendedFunctions || getAll)
			{
				yield return SArayPluginVariable("7 - Extended Functions|Helptext for Admins", _adminHelpText);
				yield return SArayPluginVariable("7 - Extended Functions|Helptext for Players", _helpText);
			}

			if (!getAll) //do not save
			{
				yield return new CPluginVariable("0 - Commands|Manually punish Player (Not a setting)", typeof(string), "");
				yield return new CPluginVariable("0 - Commands|Manually reset Player (Not a setting)", typeof(string), "");
			}
		}

		public void SetPluginVariable(string strVariable, string strValue)
		{
			const string yes = "Yes";
			if (strVariable.Contains('|'))
				strVariable = strVariable.Substring(strVariable.IndexOf('|') + 1);

			switch (strVariable)
			{
				case "Cooldown steps per day":
					_coolDown = Convert.ToSingle(strValue.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
					return;
				case "Admin cooldown per day":
					_adminCoolDown = Convert.ToSingle(strValue.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
					return;
				case "Log to":
					LogTarget = Enum.Parse(typeof(LoggingTarget), CPluginVariable.Decode(strValue)) as LoggingTarget? ?? LoggingTarget.Console;
					return;
				case "Enable latent kills":
					_enableLatentKills = strValue == yes;
					return;
				case "Load/Save counters to disk":
					SaveCounters = strValue == yes;
					return;
				case "Save counters on every punish":
					_saveCountersAsap = strValue == yes;
					return;
				case "Use Punkbuster GUIDs":
					_usePbGuid = strValue == yes;
					return;
				case "Kill delay (ms)":
					_killDelay = uint.Parse(strValue);
					return;
				case "Kill delay on spawn (ms)":
					_killOnspawnDelay = uint.Parse(strValue);
					return;
				case "Let AdKats determine who is an admin":
					_useAdKatsAdmins = strValue == yes;
					return;
				case "Use AdKats punishment":
					_useAdKatsPunish = strValue == yes;
					return;
				case "Use AdKats to issue kills and bans":
					_useAdKatsBan = strValue == yes;
					return;
				case "Get Admins from textfile":
					GetAdminsFromDisk = strValue == yes;
					return;
					
				case "Look for Updates":
					LookForUpdates = strValue == yes;
					return;
				case "Look for Updates every X hours":
					_updateCounter = (_maxUpdateCounter = uint.Parse(strValue)) - 1;
					return;

				case "Whitelist":
					_whitelist = new HashSet<string>(CPluginVariable.DecodeStringArray(strValue.ToLowerFast()));
					return;
				case "Treat Admins as Whitelisted":
					_whitelistAdmins = strValue == yes;
					return;
				case "Disallow player self reset":
					_disallowPlayerSelfReset = strValue == yes;
					return;
				case "Warn Whitelisted":
					_warnWhitelisted = strValue == yes;
					return;
				case "Ignore squad chat":
					_ignoreSquadChat = strValue == yes;
					return;

				case "Badwords":
					WriteBadwords(strValue, false);
					return;
				case "Regex Badwords":
					WriteBadwords(strValue, true);
					return;

				case "Enable Extended Functions":
					_extendedFunctions = strValue == yes;
					return;
				case "Helptext for Admins":
					_adminHelpText = CPluginVariable.DecodeStringArray(strValue);
					return;
				case "Helptext for Players":
					_helpText = CPluginVariable.DecodeStringArray(strValue);
					return;

				case "Latent kill message":
					_resLatentKill = CPluginVariable.Decode(strValue);
					return;
				case "Counter reset message":
					_resCounterReset = CPluginVariable.Decode(strValue);
					return;
				case "!langinfo message":
					_resLangInfo = CPluginVariable.DecodeStringArray(strValue);
					return;

				case "Online admins message header":
					_resOnlineAdmins = CPluginVariable.Decode(strValue);
					return;
				case "No admins online message":
					_resNoOnlineAdmins = CPluginVariable.Decode(strValue);
					return;

				case "Manually punish Player (Not a setting)":
					ManuallyPunishPlayer("Server", CPluginVariable.Decode(strValue).Trim());
					return;
				case "Manually reset Player (Not a setting)":
					ManuallyResetPlayer("Server", CPluginVariable.Decode(strValue).Trim());
					return;
			}

			//Measure list assignment
			if (Regex.IsMatch(strVariable, "^Measure #\\d+ - "))
			{
				int index = int.Parse(strVariable.Substring(9, strVariable.IndexOf(' ', 9) - 9)) - 1;

				if (strVariable.ContainsFast("reason"))
				{
					_measures[index].PrivateMessage = CPluginVariable.DecodeStringArray(strValue);
					return;
				}

				strVariable = Regex.Replace(strVariable, "^Measure #\\d+ - ", "");
				string[] msg = new string[0];
				switch (strVariable)
				{
					case "Repeat X times":
						_measures[index].Count = uint.Parse(strValue);
						return;
					case "Yell time (sec.)":
						_measures[index].YellTime = uint.Parse(strValue);
						return;
					case "TBan minutes":
						_measures[index].TBanTime = uint.Parse(strValue);
						return;
					case "Public chat message":
						msg = CPluginVariable.DecodeStringArray(strValue);
						if (msg.Length == 1 && string.IsNullOrEmpty(msg[0]))
							msg = new string[0];
						_measures[index].PublicMessage = msg;
						return;
					case "Private chat message":
						msg = CPluginVariable.DecodeStringArray(strValue);
						if (msg.Length == 1 && string.IsNullOrEmpty(msg[0]))
							msg = new string[0];
						_measures[index].PrivateMessage = msg;
						return;
					case "Yell message":
						msg = CPluginVariable.DecodeStringArray(strValue);
						if (msg.Length == 1 && string.IsNullOrEmpty(msg[0]))
							msg = new string[0];
						_measures[index].YellMessage = msg;
						return;
					case "Command":
						msg = CPluginVariable.DecodeStringArray(strValue);
						if (msg.Length == 1 && string.IsNullOrEmpty(msg[0]))
							msg = new string[0];
						_measures[index].Command = msg;
						return;
					case "Measure":
						var act = (BadwordAction)Enum.Parse(typeof(BadwordAction), CPluginVariable.Decode(strValue));
						if (act == BadwordAction.ListEnd && index == 0)
							return;
						if (act != BadwordAction.ListEnd && _measures[index].Action == BadwordAction.ListEnd && index + 1 < _measures.Count)
							_measures[index + 1].Action = BadwordAction.ListEnd;
						if (index >= _measures.Count - 1 && act != BadwordAction.ListEnd)
							_measures.Add(new SuccessiveMeasure());

						_measures[index].Action = act;
						return;

				}
			}

			var r = new Regex("^Section '(.+)' - (.+)");
			Match m = r.Match(strVariable);
			if (m.Success)
			{
				var section = m.Groups[1].Value;
				var setting = m.Groups[2].Value;

				if (setting == "Enabled")
				{
					if (strValue == yes)
					{
						if (!_overrides.ContainsKey(section))
							_overrides.Add(section, new MeasureOverride());
					}
					else
					{
						if (_overrides.ContainsKey(section))
							_overrides.Remove(section);
					}
					return;
				}

				if (!_overrides.ContainsKey(section))
				{
					_overrides.Add(section, new MeasureOverride());
				}
				var ovr = _overrides[section];
				var msg = new string[0];
				switch (setting)
				{
					case "Use AdKats punish":
						ovr.NoAdKats = strValue != yes;
						return;
					case "Severity":
						ovr.Severity = Convert.ToSingle(strValue.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
						return;
					case "Measure":
						ovr.MinimumAction = (BadwordAction)Enum.Parse(typeof(BadwordAction), CPluginVariable.Decode(strValue));
						break;
					case "Allow higher measures":
						ovr.AlwaysUseMinAction = strValue != yes;
						return;
					case "Minimum counter afterwards":
						ovr.MinimumCounter = Convert.ToSingle(strValue.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat) - 1;
						return;
					case "Public message":
						msg = CPluginVariable.DecodeStringArray(strValue);
						if (msg.Length == 1 && string.IsNullOrEmpty(msg[0]))
							msg = null;
						ovr.PublicMessage = msg;
						break;
					case "Private message":
						msg = CPluginVariable.DecodeStringArray(strValue);
						if (msg.Length == 1 && string.IsNullOrEmpty(msg[0]))
							msg = null;
						ovr.PrivateMessage = msg;
						break;
					case "Yell message":
						msg = CPluginVariable.DecodeStringArray(strValue);
						if (msg.Length == 1 && string.IsNullOrEmpty(msg[0]))
							msg = null;
						ovr.YellMessage = msg;
						break;
					case "Command":
						msg = CPluginVariable.DecodeStringArray(strValue);
						if (msg.Length == 1 && string.IsNullOrEmpty(msg[0]))
							msg = null;
						ovr.Command = msg;
						break;
					case "Yell time (sec.)":
						uint data1;
						if (uint.TryParse(strValue, out data1))
							ovr.YellTime = data1;
						else
							ovr.YellTime = null;
						return;
					case "TBan minutes":
						uint data2;
						if (uint.TryParse(strValue, out data2))
							ovr.TBanTime = data2;
						else
							ovr.TBanTime = null;
						return;
				}

				if (ovr.MinimumAction == BadwordAction.ListEnd)
					ovr.MinimumAction = BadwordAction.Warn;
			}
		}


		private void WriteBadwords(string setting, bool isRegex)
		{
			string[] words = string.IsNullOrEmpty(setting) ? new string[0] : CPluginVariable.DecodeStringArray(setting);

			if (isRegex)
			{
				if (_failPrevent.IsMatch(setting))
				{
					WriteLog("LanguageEnforcer: prevented regex messup. PLEASE CHECK YOUR SETTINGS!");
					return;
				}
				if ((DateTime.Now - _startup).TotalSeconds < 5)
					return;

				RegexBadwords = words;
			}
			else
			{
				_badwordsCache = words;
				_badwords = CreateSections(words, false).ToArray();
			}
			CheckForWordlistDuplicates();

			try
			{
				File.WriteAllLines(PluginFolder + (isRegex ? "regexbadwords.txt" : "badwords.txt"), words);
			}
			catch
			{
				WriteLog("^bLanguage Enforcer^2: Couldn't save badwords. Please make sure filesystem access is granted");
			}
		}

		private IEnumerable<string> CreateSections(IEnumerable<string> words, bool isRegex)
		{
			if (isRegex)
				_regexBadwordSection.Clear();
			else
				_badwordSection.Clear();

			var r = new Regex("^{\\w+}$");
			string key = null;
			foreach (var word in words.Where(word => !string.IsNullOrEmpty(word)))
			{
				if (r.IsMatch(word))
				{
					key = word.Substring(1, word.Length - 2);
					continue;
				}

				if (key != null)
				{
					try
					{
						if (isRegex)
							_regexBadwordSection.Add(word.IndexOf('#') >= 0 ? word.Substring(0, word.IndexOf('#')) : word, key);
						else
							_badwordSection.Add(word, key);
					}
					catch (ArgumentException exc) { } // duplicate key
				}
				yield return word;
			}
		}

		protected override void WriteCounters()
		{
			try
			{
				File.WriteAllLines(PluginFolder + "LangEnforcerCounters.txt", _players.Select(pair => string.Join(" ", new[] { pair.Key, pair.Value.Heat.ToString("0.0000", CultureInfo.InvariantCulture), pair.Value.LastAction.Ticks.ToString() })).ToArray());
			}
			catch
			{
				WriteLog("^bLanguage Enforcer^2: Couldn't save counters. Please make sure filesystem access is granted");
			}
		}

		protected override void LoadCounters()
		{
			try
			{
				_players.Clear();
				string[] data = File.ReadAllLines(PluginFolder + "LangEnforcerCounters.txt");
				foreach (var tokens in data.Select(line => line.Split(' ')))
				{
					_players.Add(tokens[0], new PlayerInfo { Heat = double.Parse(tokens[1], CultureInfo.InvariantCulture), LastAction = new DateTime(long.Parse(tokens[2])) });
				}
			}
			catch
			{
				WriteLog("^bLanguage Enforcer^2: Couldn't load counters. Please make sure filesystem access is granted");
			}
		}

		private void CheckForWordlistDuplicates()
		{
			bool foundone = false;

			foreach (var word in _badwords)
			{
				var rgx = _regexBadwords.Select((r, i) => new { Regex = r, Index = i }).FirstOrDefault(r => r.Regex.IsMatch(word));
				if (rgx != null)
				{
					var rword = RegexBadwords[rgx.Index];
					WriteLog("^bLanguage Enforcer detected a duplicate wordlist entry: " + word + " =Regex " + rword);
					foundone = true;
				}
			}

			for (int i = 0; i < _badwords.Length; i++)
			{
				for (int j = 0; j < _badwords.Length; j++)
				{
					if (i == j)
						continue;
					if (_badwords[i].ContainsIgnoreCaseFast(_badwords[j]))
					{
						WriteLog("^bLanguage Enforcer detected a duplicate wordlist entry: " + _badwords[i] + " = " + _badwords[j]);
						foundone = true;
					}
				}
			}

			if (foundone)
				WriteLog("^bEnd of List for Language Enforcer duplicates");
		}

		#endregion

		public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
		{
			try
			{
				_badwords = File.ReadAllLines(PluginFolder + "badwords.txt");
				RegexBadwords = File.ReadAllLines(PluginFolder + "regexbadwords.txt");
				_startup = DateTime.Now;
			}
			catch
			{
				WriteLog("^bLanguage Enforcer^2: Couldn't load badwords. Please make sure filesystem access is granted");
			}

			//by default all will be registered! so this should speed up the layer
			RegisterEvents(this.GetType().Name, "OnAccountLogin", "OnListPlayers", "OnPlayerJoin", "OnPlayerKilled", "OnPlayerSpawned", "OnRoundOver", "OnPlayerLeft", "OnGlobalChat", "OnTeamChat", "OnSquadChat", "OnPluginDisable", "OnPluginEnable", "OnPunkbusterPlayerInfo");
		}

		public override void OnAccountLogin(string accountName, string ip, CPrivileges privileges)
		{
			if (!LookForUpdates)
				return;
			_updateCounter = _maxUpdateCounter - 1;
			CheckForUpdates();
		}

		public void CheckForUpdates()
		{
			if (_updateAvailable)
			{
				const string m = "^1Your Version of the LanguageEnforcer is outdated!";
				ConsoleWrite(m);
				ExecuteCommand("procon.protected.chat.write", m);
				return;
			}

			_updateCounter++;
			if (_updateCounter == _maxUpdateCounter)
			{
				_updateCounter = 0;
				try
				{
					string result;
					using (var webClient = new System.Net.WebClient())
					{
						result = webClient.DownloadString("http://le-version.mygigahost.de/version.php");
					}
					if (result != GetPluginVersion())
					{
						_updateAvailable = true;
						CheckForUpdates();
					}
				}
				catch
				{
					WriteLog("^bLanguage Enforcer^2: Update Check failed!");
				}
			}
		}

		public override void OnPlayerKilled(Kill k)
		{
			if (!_usePbGuid)
				try
				{
					CachePlayerInfo(k.Killer);
					CachePlayerInfo(k.Victim);
				}
				catch (Exception exc)
				{
					WriteLog(exc.ToString());
				}

			try
			{
				if (_players.ContainsKey(k.Victim.SoldierName))
				{
					_players[k.Victim.SoldierName].Dead = true;
				}
			}
			catch (Exception exc)
			{
				WriteLog(exc.ToString());
			}
		}

		public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory)
		{
			try
			{
				if (Tbd.Contains(soldierName))
				{
					Tbd.Remove(soldierName);
					KillPlayer(soldierName, _killOnspawnDelay);
					ThreadPool.QueueUserWorkItem(callback =>
					{
						try
						{
							Thread.Sleep(2900);
							AdminSay("LanguageEnforcer: Latent kill of " + soldierName + " done");
							PlayerSay(soldierName, ProconUtil.ProcessMessage(_resLatentKill, soldierName, false, 0, "", false, this));
						}
						catch (Exception exc)
						{
							WriteLog(exc.ToString());
						}
					}, null);
				}
				else if (_players.ContainsKey(soldierName))
				{
					_players[soldierName].Dead = false;
				}
			}
			catch (Exception exc)
			{
				WriteLog(exc.ToString());
			}
		}

		public override void OnRoundOver(int winningTeamId)
		{
			base.OnRoundOver(winningTeamId);

			foreach (var player in _players)
			{
				player.Value.Dead = true;
			}
		}

		/// <summary>
		/// Clears the GUIDs, the online admins, the tbd-list and calculates the a cooled down heat value for each player in the players list.
		/// </summary>
		protected override void Cleanup()
		{
			base.Cleanup();

			var temp = _players.ToArray(); //remove in foreach will otherwise kill the enumerator
			foreach (var player in temp)
			{
				var days = (DateTime.Now - player.Value.LastAction).TotalDays;
				player.Value.Heat = Math.Max(-1D, player.Value.Heat - GetCooldown(player.Key) * days);
				player.Value.LastAction = DateTime.Now;
				if (player.Value.Heat < -0.9D)
					_players.Remove(player.Key);
			}
		}

		public override void OnSquadChat(string speaker, string message, int teamId, int squadId)
		{
			if (_ignoreSquadChat)
			{
				if (message.StartsWithOneOf(_commandStartChars))
					ExecuteInGameCommand(speaker, message);
			}
			else
			{
				OnChat(speaker, message);
			}
		}

		protected override void OnChat(string speaker, string message)
		{
			try
			{
				if (message.StartsWithOneOf(_commandStartChars) && ExecuteInGameCommand(speaker, message))
					return; //no search for badwords necessary

				if (speaker == "Server")
					return;

				var admin = IsAdmin(speaker);
				var whitelisted = (_whitelistAdmins && admin) || _whitelist.Contains(speaker.ToLowerFast());
				if (!whitelisted || _warnWhitelisted)
				{
					string match;
					Regex rmatch;
					var mo = MeasureOverride.NoOverride;

					if ((match = _badwords.FirstOrDefault(message.ContainsIgnoreCaseFast)) != null)
					{
						if (_badwordSection.ContainsKey(match))
						{
							var section = _badwordSection[match];
							if (_overrides.ContainsKey(section))
								mo = _overrides[section];
						}
						TakeMeasure(speaker, message, WhitelistOverride(mo, whitelisted));
						WriteLog(String.Format("LanguageEnforcer: Player {0} triggered the word '{1}'", speaker, match));
					}
					else if ((rmatch = _regexBadwords.FirstOrDefault(r => r.IsMatch(message))) != null)
					{
						var idx = _regexBadwords.Select((r, i) => new { regex = r, index = i }).FirstOrDefault(item => item.regex == rmatch);
						if (idx != null)
						{
							match = RegexBadwords.Select(r => r.IndexOf('#') >= 0 ? r.Substring(0, r.IndexOf('#')) : r).Where(r => !string.IsNullOrEmpty(r) && !Regex.IsMatch(r, "^{\\w+}$")).ElementAt(idx.index);
							if (_regexBadwordSection.ContainsKey(match))
							{
								var section = _regexBadwordSection[match];
								if (_overrides.ContainsKey(section))
									mo = _overrides[section];
							}
							TakeMeasure(speaker, message, WhitelistOverride(mo, whitelisted));
							WriteLog(String.Format("LanguageEnforcer: Player {0} triggered the word {1}", speaker, match));
						}
						else
						{
							WriteLog("LanguageEnforcer: Error while trying to determine match");
							TakeMeasure(speaker, message, WhitelistOverride(mo, whitelisted));
						}

					}

					if (whitelisted)
					{
						_players.Remove(speaker);
					}
				}
			}
			catch (Exception exc)
			{
				WriteLog(exc.ToString());
			}
		}

		private MeasureOverride WhitelistOverride(MeasureOverride mo, bool whitelisted)
		{
			if (!whitelisted)
				return mo;

			return new MeasureOverride
			{
				TBanTime = 0,
				YellTime = mo.YellTime,
				PrivateMessage = mo.PrivateMessage,
				PublicMessage = mo.PublicMessage,
				YellMessage = mo.YellMessage,
				MinimumAction = BadwordAction.Warn,
				AlwaysUseMinAction = true,
				MinimumCounter = -1,
				Severity = 0,
				IsWhitelisted = true,
				NoAdKats = true
			};
		}

		/// <summary>
		/// Check if the player issued a command and execute the according command
		/// </summary>
		/// <returns>true if there is no need to check for badwords anymore</returns>
		private bool ExecuteInGameCommand(string speaker, string message)
		{
			message = message.Substring(1);

			if (IsAdmin(speaker)) //admin commands
			{
				if (message.StartsWith("langreset", StringComparison.OrdinalIgnoreCase) || message.StartsWith("langr ", StringComparison.OrdinalIgnoreCase))
				{
					var idx = message.IndexOf(' ');
					return ManuallyResetPlayer(speaker, message.Substring(idx + 1));
				}

				if (message.StartsWith("langpunish", StringComparison.OrdinalIgnoreCase) || message.StartsWith("langp ", StringComparison.OrdinalIgnoreCase))
				{
					var idx = message.IndexOf(' ');
					return ManuallyPunishPlayer(speaker, message.Substring(idx + 1));
				}
				if (message.StartsWith("langcounter", StringComparison.OrdinalIgnoreCase) || message.StartsWith("langc ", StringComparison.OrdinalIgnoreCase))
				{
					var args = message.Split(' ');
					if (args.Length != 2 && args.Length != 3)
					{
						PlayerSay(speaker, "Wrong command usage");
						return false;
					}
					string player = FindPlayerName(args[1]);
					if (player == null)
					{
						PlayerSay(speaker, "Player not found");
						return false;
					}

					if (_disallowPlayerSelfReset && speaker == player)
						return false;

					PlayerInfo pi;
					if (_players.ContainsKey(speaker))
					{
						pi = _players[speaker];
					}
					else
					{
						pi = new PlayerInfo
						{
							LastAction = DateTime.Now,
							Heat = -1F
						};
						_players.Add(speaker, pi);
					}
					if (args.Length == 2)
					{
						PlayerSay(speaker, "Counter of " + player + " reads " + GetCounter(player));
					}
					else //length=3
					{
						pi.Heat = Convert.ToDouble(args[2].Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat) - 1;
						pi.LastAction = DateTime.Now;
						PlayerSay(speaker, "Counter of " + player + " now reads " + (pi.Heat + 1).ToString("0.00"));
					}
					return true;
				}

				if (_extendedFunctions && (message.StartsWith("help", StringComparison.OrdinalIgnoreCase) || message.StartsWith("rules", StringComparison.OrdinalIgnoreCase)))
				{
					foreach (var line in _adminHelpText)
					{
						PlayerSay(speaker, ProconUtil.ProcessMessage(line, speaker, false, 0, "", false, this));
					}
					return message.Length <= 5;
				}
			}
			else if (_extendedFunctions) //non-admin extended commands
			{
				if (message.StartsWith("help", StringComparison.OrdinalIgnoreCase) || message.StartsWith("rules", StringComparison.OrdinalIgnoreCase))
				{
					foreach (var line in _helpText)
					{
						PlayerSay(speaker, ProconUtil.ProcessMessage(line, speaker, false, 0, "", false, this));
					}
					return message.Length <= 5;
				}
			}

			if (_extendedFunctions) //public extended commands
			{
				if (message.StartsWith("admin", StringComparison.OrdinalIgnoreCase))
				{
					var admns = Admins.ToArray(); //avoid multitasking interferences + multi enumeration
					if (admns.Length > 0)
					{
						var sb = new StringBuilder(ProconUtil.ProcessMessage(_resOnlineAdmins, speaker, false, 0, "", false, this));
						foreach (var admin in admns)
						{
							sb.Append(admin);
							sb.Append(", ");
						}
						sb.Remove(sb.Length - 2, 2);
						var txt = sb.ToString();
						PlayerSay(speaker, txt);
						AdminSay(speaker + " issued the !admin command");
					}
					else
					{
						PlayerSay(speaker, ProconUtil.ProcessMessage(_resNoOnlineAdmins, speaker, false, 0, "", false, this));
					}
					return message.Length == 5;
				}
			}

			//public commands
			if (message.StartsWith("langinfo", StringComparison.OrdinalIgnoreCase))
			{
				foreach (var line in _resLangInfo)
					PlayerSay(speaker, ProconUtil.ProcessMessage(line, speaker, false, 0, "", false, this));

				return message.Length == 8;
			}

			return false;
		}

		internal void ShowRules(string target)
		{
			foreach (var line in _helpText)
			{
				PlayerSay(target, ProconUtil.ProcessMessage(line, target, false, 0, "", false, this));
			}
		}

		private bool ManuallyResetPlayer(string speaker, string player)
		{
			player = FindPlayerName(player);
			if (player == null)
			{
				PlayerSay(speaker, "Player not found!");
				return false;
			}

			if (_disallowPlayerSelfReset && speaker == player)
				return false;

			_players.Remove(player);
			PlayerSay(player, ProconUtil.ProcessMessage(_resCounterReset, player, false, 0, "", false, this));
			AdminSay(string.Format("LanguageEnforcer: Player {0} now has a clean jacket", player));
			return true;
		}

		private bool ManuallyPunishPlayer(string speaker, string player)
		{
			player = FindPlayerName(player);
			if (player == null)
			{
				PlayerSay(speaker, "Player not found!");
				return false;
			}
			TakeMeasure(player, "", MeasureOverride.NoOverride);
			return true;
		}

		/// <summary>
		/// Autocompletion for in-game commands
		/// </summary>
		/// <param name="player">a fragment of a player name</param>
		/// <returns>the actual player name or null if there were 0 or multiple matches</returns>
		private string FindPlayerName(string player)
		{
			var names = _players.Where(p => p.Key.ContainsIgnoreCaseFast(player)).ToArray();
			if (names.Length != 1)
			{
				var names2 = Guids.Where(p => p.Key.ContainsIgnoreCaseFast(player)).ToArray();
				if (names2.Length != 1)
				{
					return null;
				}
				return names2[0].Key;
			}
			return names[0].Key;
		}

		/// <summary>
		/// Punishes a Player for bad language
		/// </summary>
		private void TakeMeasure(string speaker, string quote, MeasureOverride mo)
		{
			PlayerInfo pi;
			if (_players.ContainsKey(speaker))
			{
				pi = _players[speaker];
			}
			else
			{
				pi = new PlayerInfo
				{
					LastAction = DateTime.Now,
					Heat = -1F
				};
				_players.Add(speaker, pi);
			}

			if (pi.Heat > -1)
			{
				//cooldown logic
				var days = (DateTime.Now - pi.LastAction).TotalDays;
				pi.Heat = Math.Max(-1F, pi.Heat - GetCooldown(speaker) * days);
			}

			if (!mo.IsWhitelisted)
				pi.Heat = Math.Max(pi.Heat + mo.Severity, mo.MinimumCounter);
			pi.LastAction = DateTime.Now;
			var mea = (int)Math.Ceiling(pi.Heat);
			TakeMeasure(speaker, mea, quote, mo);
			if (SaveCounters && _saveCountersAsap)
				WriteCounters();
		}

		private void TakeMeasure(string player, int measureIdx, string quote, MeasureOverride mo)
		{
			BadwordAction next;
			SuccessiveMeasure now = GetMeasure(measureIdx, out next);
			if (now.GetAction(mo, _useAdKatsPunish) == BadwordAction.Warn)
			{
				string guid = Guids.ContainsKey(player) ? Guids[player] : "unknown";
				WriteLog(String.Format("LanguageEnforcer: Player {0} warned. GUID = {1}", player, guid));
			}
			now.TakeMeasure(this, player, now.Action != next, quote, mo);
		}

		public void KillOrLatentKillPlayer(string player, string message)
		{
			if (_useAdKatsBan || _useAdKatsPunish)
			{
				var requestHashtable = new Hashtable {
						{"caller_identity", GetType().Name},
						{"response_requested", false},
						{"command_type", "player_kill"},
						{"source_name", GetType().Name},
						{"target_name", player},
						{"record_message", message},
					};
				if (Guids.ContainsKey(player))
					requestHashtable.Add("target_guid", Guids[player]);

				ExecuteCommand("procon.protected.plugins.call", "AdKats", "IssueCommand", GetType().Name, JSON.JsonEncode(requestHashtable));
			}
			else

				if (_enableLatentKills && _players[player].Dead)
				{
					Tbd.Add(player);
				}
				else
				{
					KillPlayer(player);
					_players[player].Dead = true;
				}
		}

		private SuccessiveMeasure GetMeasure(int measureIdx, out BadwordAction nextAction)
		{
			int current = 0;
			int count = _measures.Count;
			SuccessiveMeasure ret = null;
			for (int i = 0; i < count; i++)
			{
				if (_measures[i].Action == BadwordAction.ListEnd)
					break;
				ret = _measures[i];
				var next = (int)(current + ret.Count);
				if (measureIdx < next)
				{
					nextAction = measureIdx + 1 < next ? ret.Action : _measures[Math.Min(i + 1, count - 1)].Action;

					if (nextAction == BadwordAction.ListEnd)
						nextAction = ret.Action;

					return ret;
				}
				current = next;
			}

			nextAction = ret.Action;
			return ret;
		}

		internal int GetCounter(string player)
		{
			if (_players.ContainsKey(player))
			{
				var pi = _players[player];
				var days = (DateTime.Now - pi.LastAction).TotalDays;
				return (int)Math.Ceiling(Math.Max(0F, pi.Heat - GetCooldown(player) * days)) + 1;
			}
			return 0;
		}

		internal float GetCooldown(string player)
		{
			return IsAdmin(player) ? _adminCoolDown : _coolDown;
		}
	}

	public abstract class LanguageEnforcerBase : Api
	{
		protected internal Dictionary<string, string> Guids = new Dictionary<string, string>(); //name to guid dict for banning via guid
		protected internal Dictionary<string, string> Countries = new Dictionary<string, string>(); //name to countrycode dict for country specific messages
		protected readonly HashSet<string> Admins = new HashSet<string>(); //online admins for AdminSay() and !admin / HashSet is faster with Contains
		protected readonly HashSet<string> Tbd = new HashSet<string>(); //list of players who cursed while dead / HashSet doesn't allow duplicates, but we don't want to annoy them too much

		protected int OnlinePlayerCount = 0; //online players to know when to trigger a cleanup
		protected LoggingTarget LogTarget = LoggingTarget.PluginConsole;
		private static string _folder; //folder target cache
		private bool _updateTaskIsRunning;
		protected bool _lookForUpdates = true;
		protected bool SaveCounters = true;
		public bool GetAdminsFromDisk;

		protected internal bool _useAdKatsBan;
		protected internal bool _useAdKatsPunish;
		protected internal bool _useAdKatsAdmins;
		protected bool _usePbGuid;
		protected uint _killDelay = 1000;
		protected uint _killOnspawnDelay = 3000;
		private string[] admins;

		internal static string PluginFolder
		{
			get
			{
				if (_folder != null)
					return _folder;

				try
				{
					string codeBase = Assembly.GetExecutingAssembly().CodeBase;
					var uri = new UriBuilder(codeBase);
					string path = Uri.UnescapeDataString(uri.Path);
					var match = Regex.Match(path, "Plugins/.*/");
					if (match.Success)
					{
						string ret = match.Value;
						return _folder = ret.Replace('/', Path.DirectorySeparatorChar);
					}
				}
				catch
				{
				}
				return _folder = "Plugins" + Path.DirectorySeparatorChar + "BF4" + Path.DirectorySeparatorChar;
			}
		}

		protected bool RunUpdateTask
		{
			get { return _updateTaskIsRunning; }
			set
			{
				if (value != _updateTaskIsRunning)
					if (value)
					{
						if (!Enabled)
							return;
						ExecuteCommand("procon.protected.tasks.add", "UpdateTask", "3", "3600", "-1", "procon.protected.plugins.call", "LanguageEnforcer", "CheckForUpdates");
					}
					else
					{
						ExecuteCommand("procon.protected.tasks.remove", "UpdateTask");
					}
				_updateTaskIsRunning = value;
			}
		}

		public override void OnPunkbusterPlayerInfo(CPunkbusterInfo playerInfo)
		{
			if (_usePbGuid)
				CachePlayerInfo(playerInfo);

			if (Countries.ContainsKey(playerInfo.SoldierName))
				Countries.Remove(playerInfo.SoldierName);

			Countries.Add(playerInfo.SoldierName, playerInfo.PlayerCountryCode);
		}

		/// <summary>
		/// Enlists the GUID from the player for banning with GUIDs instead of names
		/// </summary>
		protected void CachePlayerInfo(CPlayerInfo player)
		{
			CachePlayerInfo(player.SoldierName, player.GUID);
		}

		/// <summary>
		/// Enlists the GUID from the player for banning with GUIDs instead of names
		/// </summary>
		protected void CachePlayerInfo(CPunkbusterInfo player)
		{
			CachePlayerInfo(player.SoldierName, player.GUID);
		}

		/// <summary>
		/// Enlists the GUID from the player for banning with GUIDs instead of names
		/// </summary>
		protected void CachePlayerInfo(string name, string guid)
		{
			if (Guids.ContainsKey(name))
				Guids.Remove(name);
			Guids.Add(name, guid);
		}

		protected virtual void Cleanup()
		{
			Guids.Clear();
			Countries.Clear();
			Admins.Clear();
			Tbd.Clear();
		}

		#region player list change handlers
		protected abstract void WriteCounters();
		protected abstract void LoadCounters();

		public override void OnPlayerJoin(string soldierName)
		{
			try
			{
				if (IsAdmin(soldierName))
					Admins.Add(soldierName); //hashset will ignore duplicates
				OnlinePlayerCount++;
			}
			catch (Exception exc)
			{
				WriteLog(exc.ToString());
			}
		}

		public override void OnPlayerLeft(CPlayerInfo playerInfo)
		{
			try
			{
				//no need to check if contained in HashSet before remove
				Admins.Remove(playerInfo.SoldierName);
				Tbd.Remove(playerInfo.SoldierName);

				OnlinePlayerCount--;
				if (OnlinePlayerCount <= 0)
				{
					Cleanup();
					if (SaveCounters)
						WriteCounters();
				}
			}
			catch (Exception exc)
			{
				WriteLog(exc.ToString());
			}
		}

		public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
		{
			try
			{
				Guids.Clear();
				if (!_usePbGuid)
					players.ForEach(CachePlayerInfo);
				OnlinePlayerCount = players.Count;

				if (_useAdKatsAdmins)
				{
					RefreshAdKatsAdmins();
				}
				else
				{
					Admins.Clear();
					foreach (var admin in players.Where(p => IsAdmin(p.SoldierName)).Select(p => p.SoldierName))
					{
						Admins.Add(admin);
					}
				}

				if (OnlinePlayerCount <= 0)
				{
					Cleanup();
					if (SaveCounters)
						WriteCounters();
				}
				else
					Tbd.RemoveWhere(player => !players.Any(p => p.SoldierName == player));
			}
			catch (Exception exc)
			{
				WriteLog(exc.ToString());
			}
		}

		#endregion
		#region basic functionality

		public bool IsAdmin(string player)
		{
			if (player == "Server")
				return true;
			if (Admins.Contains(player))
				return true;

			if (_useAdKatsAdmins)
			{
				RefreshAdKatsAdmins();
				return false;
			}

			try
			{
				if (GetAdminsFromDisk)
				{
					if (admins == null)
						admins=File.ReadAllLines(PluginFolder + "le_admins.txt");
					return admins.Any(a => a.Equals(player, StringComparison.OrdinalIgnoreCase));
				}

				CPrivileges p = GetAccountPrivileges(player);
				if (p != null && p.CanKillPlayers)
					return true;
			}
			catch
			{
			}
			return false;
		}

		private void RefreshAdKatsAdmins()
		{
			var requestHashtable = new Hashtable {
						{"caller_identity", GetType().Name},
						{"response_class", GetType().Name},
						{"response_method", "HandleAdKatsAdminResponse"},
						{"response_requested", true},
						{"command_type", "player_ban_temp"},
						{"source_name", GetType().Name},
						{"user_subset", "admin"},
					};

			ExecuteCommand("procon.protected.plugins.call", "AdKats", "FetchAuthorizedSoldiers", GetType().Name, JSON.JsonEncode(requestHashtable));
		}

		public void HandleAdKatsAdminResponse(params string[] response)
		{
			if (response.Length != 2)
				return;

			var values = (Hashtable)JSON.JsonDecode(response[1]);

			if (values["response_type"] as string != "FetchAuthorizedSoldiers")
				return;

			var val = values["response_value"] as string;

			if (string.IsNullOrEmpty(val))
				return;

			string[] ads = CPluginVariable.DecodeStringArray(val);

			Admins.Clear();
			foreach (var admin in ads.Where(admin => Guids.ContainsKey(admin)))
				Admins.Add(admin);
		}

		public void ConsoleWrite(string message)
		{
			ExecuteCommand("procon.protected.pluginconsole.write", message);
		}

		public void Say(string message)
		{
			if (message.StartsWith("{{"))
			{
				var msg = message.Substring(2);
				int end = msg.IndexOf("}}");

				if (end >= 0)
				{
					var cstring = msg.Substring(0, end);
					message = msg.Substring(end + 2);

					ExecuteCommand("procon.protected.chat.write", String.Format("(CountrySay {0}) {1}", cstring, message));

					bool invert = cstring.StartsWith("-");
					if (invert)
						cstring = cstring.Substring(1);

					var countries = cstring.ToLowerFast().Split(',');
					foreach (var player in Countries.Where(player => countries.Contains(player.Value) != invert))
					{
						LowLevelPlayerSay(player.Key, message, false);
					}
				}
			}
			else
			{
				ExecuteCommand("procon.protected.send", "admin.say", message, "all");
				ExecuteCommand("procon.protected.chat.write", message);
			}
		}
		public void PlayerSay(string player, string message)
		{
			if (message.StartsWith("{{"))
			{
				var msg = message.Substring(2);
				int end = msg.IndexOf("}}");

				if (end >= 0)
				{
					var cstring = msg.Substring(0, end);
					message = msg.Substring(end + 2);

					var cstringbak = cstring;

					bool invert = cstring.StartsWith("-");
					if (invert)
						cstring = cstring.Substring(1);

					var countries = cstring.ToLowerFast().Split(',');
					if (Countries.ContainsKey(player) && invert != countries.Contains(Countries[player]))
					{
						ExecuteCommand("procon.protected.chat.write", String.Format("(CountryPlayerSay {0} {1}) {2}", player, cstringbak, message));
						LowLevelPlayerSay(player, message, false);
					}
				}
			}
			else
			{
				LowLevelPlayerSay(player, message);
			}
		}

		public void LowLevelPlayerSay(string player, string message)
		{
			//No default values in procon compiler
			LowLevelPlayerSay(player, message, true);
		}

		public void LowLevelPlayerSay(string player, string message, bool log)
		{
			ExecuteCommand("procon.protected.send", "admin.say", message, "player", player);
			if (log)
				ExecuteCommand("procon.protected.chat.write", String.Format("(PlayerSay {0}) {1}", player, message));
		}

		public string GetReason(string player, string[] processedMessages)
		{
			var country = Countries.ContainsKey(player) ? Countries[player] : "??";
			foreach (var message in processedMessages)
			{
				if (message.StartsWith("{{"))
				{
					var msg = message.Substring(2);
					int end = msg.IndexOf("}}");

					if (end >= 0)
					{
						var cstring = msg.Substring(0, end);
						msg = msg.Substring(end + 2);

						bool invert = cstring.StartsWith("-");
						if (invert)
							cstring = cstring.Substring(1);

						var countries = cstring.ToLowerFast().Split(',');
						if (invert != countries.Contains(country))
							return msg;
					}
				}
				else
				{
					return message;
				}
			}
			return processedMessages.FirstOrDefault() ?? "";
		}

		public void AdminSay(string message)
		{
			foreach (var admin in Admins)
			{
				PlayerSay(admin, message);
			}
		}

		public void PlayerYell(string player, string message, uint yellTime)
		{
			if (message.StartsWith("{{"))
			{
				var msg = message.Substring(2);
				int end = msg.IndexOf("}}");

				if (end >= 0)
				{
					var cstring = msg.Substring(0, end);
					message = msg.Substring(end + 2);

					var cstringbak = cstring;

					bool invert = cstring.StartsWith("-");
					if (invert)
						cstring = cstring.Substring(1);

					var countries = cstring.ToLowerFast().Split(',');
					if (Countries.ContainsKey(player) && invert != countries.Contains(Countries[player]))
					{
						ExecuteCommand("procon.protected.chat.write", String.Format("(CountryPlayerYell {0} {1}) {2}", player, cstringbak, message));
						LowLevelPlayerYell(player, message, yellTime, false);
					}
				}
			}
			else
			{
				LowLevelPlayerYell(player, message, yellTime);
			}
		}

		public void LowLevelPlayerYell(string player, string message, uint yellTime)
		{
			LowLevelPlayerYell(player, message, yellTime, true);
		}

		public void LowLevelPlayerYell(string player, string message, uint yellTime, bool log)
		{
			ExecuteCommand("procon.protected.send", "admin.yell", message, yellTime.ToString(CultureInfo.InvariantCulture), "player", player);
			if(log)
				ExecuteCommand("procon.protected.chat.write", String.Format("(PlayerYell {0}) ", player) + message);
		}

		public void KillPlayer(string player)
		{
			//no default values allowed in the procon compiler -.-
			KillPlayer(player, _killDelay);
		}
		public void KillPlayer(string player, uint wait)
		{
			string guid = Guids.ContainsKey(player) ? Guids[player] : "unknown";
			WriteLog(String.Format("LanguageEnforcer: Player {0} killed. GUID = {1}", player, guid));
			ThreadPool.QueueUserWorkItem(callback =>
			{
				try
				{
					Thread.Sleep((int) wait);
					ExecuteCommand("procon.protected.send", "admin.killPlayer", player);
				}
				catch (Exception exc)
				{
					WriteLog(exc.ToString());
				}
			}, null);
		}

		public void KickPlayer(string player, string reason)
		{
			string guid = Guids.ContainsKey(player) ? Guids[player] : "unknown";
			WriteLog(String.Format("LanguageEnforcer: Player {0} kicked. GUID = {1}", player, guid));
			ThreadPool.QueueUserWorkItem(callback =>
			{
				try
				{
					Thread.Sleep(500);
					if (_useAdKatsBan || _useAdKatsPunish)
					{
						var requestHashtable = new Hashtable {
							{"caller_identity", GetType().Name},
							{"response_requested", false},
							{"command_type", "player_kick"},
							{"source_name", GetType().Name},
							{"target_name", player},
							{"record_message", reason},
						};
						if (Guids.ContainsKey(player))
							requestHashtable.Add("target_guid", Guids[player]);

						ExecuteCommand("procon.protected.plugins.call", "AdKats", "IssueCommand", GetType().Name, JSON.JsonEncode(requestHashtable));
					}
					else
						ExecuteCommand("procon.protected.send", "admin.kickPlayer", player, reason);
				}
				catch (Exception exc)
				{
					WriteLog(exc.ToString());
				}
			}, null);
		}

		public void TBanPlayer(string player, string reason, uint minutes)
		{
			WriteLog(String.Format("LanguageEnforcer: Player {0} temp banned for {1:0.##}min", player, minutes));
			ThreadPool.QueueUserWorkItem(callback =>
			{
				try
				{
					Thread.Sleep(500);
					if (_useAdKatsBan || _useAdKatsPunish)
					{
						var requestHashtable = new Hashtable {
							{"caller_identity", GetType().Name},
							{"response_requested", false},
							{"command_type", "player_ban_temp"},
							{"source_name", GetType().Name},
							{"target_name", player},
							{"record_message", reason},
							{"command_numeric", minutes},
						};
						if (Guids.ContainsKey(player))
							requestHashtable.Add("target_guid", Guids[player]);

						ExecuteCommand("procon.protected.plugins.call", "AdKats", "IssueCommand", GetType().Name, JSON.JsonEncode(requestHashtable));
					}
					else
					{
						var time = minutes * 60;
						if (Guids.ContainsKey(player))
						{
							ExecuteCommand("procon.protected.send", "banList.add", "guid", Guids[player], "seconds", time.ToString(CultureInfo.InvariantCulture), reason);
						}
						else
						{
							ExecuteCommand("procon.protected.send", "banList.add", "name", player, "seconds", time.ToString(CultureInfo.InvariantCulture), reason);
						}
						ExecuteCommand("procon.protected.send", "banList.save");
						ExecuteCommand("procon.protected.send", "admin.kickPlayer", player, reason);
					}
				}
				catch (Exception exc)
				{
					WriteLog(exc.ToString());
				}
			}, null);
		}

		public void BanPlayer(string player, string reason)
		{
			WriteLog(String.Format("LanguageEnforcer: Player {0} permanantly banned", player));
			ThreadPool.QueueUserWorkItem(callback =>
			{
				try
				{
					Thread.Sleep(500);
					if (_useAdKatsBan || _useAdKatsPunish)
					{
						var requestHashtable = new Hashtable {
							{"caller_identity", GetType().Name},
							{"response_requested", false},
							{"command_type", "player_ban_perm"},
							{"source_name", GetType().Name},
							{"target_name", player},
							{"record_message", reason}
						};
						if (Guids.ContainsKey(player))
							requestHashtable.Add("target_guid", Guids[player]);

						ExecuteCommand("procon.protected.plugins.call", "AdKats", "IssueCommand", GetType().Name, JSON.JsonEncode(requestHashtable));
					}
					else if (Guids.ContainsKey(player))
					{
						ExecuteCommand("procon.protected.send", "banList.add", "guid", Guids[player], "perm", reason);
					}
					else
					{
						ExecuteCommand("procon.protected.send", "banList.add", "name", player, "perm", reason);
					}
					ExecuteCommand("procon.protected.send", "banList.save");
					ExecuteCommand("procon.protected.send", "admin.kickPlayer", player, reason);
				}
				catch (Exception exc)
				{
					WriteLog(exc.ToString());
				}
			}, null);
		}

		protected internal void ProconRulzExecuteCommand(string s, int counter)
		{
			s = s.Trim();
			if (string.IsNullOrEmpty(s))
				return;

			if (s.StartsWith("le.isMinCounter "))
			{
				s = s.Substring(16);
				var compString = Regex.Match(s, "(\\d+)").Value;
				var comp = int.Parse(compString) - 1;

				if (counter <= comp)
					return;

				s = s.Substring(compString.Length + 1);
			}

			// We need to make a string array out of 'procon.protected.send' 
			// and the action message
			// Note that we delay the %% substitutions until we have 'split' 
			// the message in case we have spaces in subst values
			var parms_list = new List<string>();
			// v39b.1 modification - Use command directly if it begins 'procon.'
			if (!s.ToLower().StartsWith("procon."))
			{
				parms_list.Add("procon.protected.send");
			}
			// if this is a punkbuster command then concatenate pb command into a single string
			// e.g. pb_sv_getss "bambam"
			if (s.ToLower().StartsWith("punkbuster.pb_sv_command"))
			{
				parms_list.Add("punkBuster.pb_sv_command");
				parms_list.Add(s.Substring(25).TrimStart());
			}
			else // for non-punkbuster commands each param is its own string...
			{
				parms_list.AddRange(quoted_split(s));
			}

			ExecuteCommand(parms_list.ToArray());

			ConsoleWrite(String.Format("LanguageEnforcer: Executed command [{0}]", String.Join(",", parms_list.ToArray())));
		}

		// split a string into elements separated by spaces, binding quoted strings into one element
		// e.g. Exec vars.serverName "OFc Server - no nubs" will be parsed to [vars.serverName,"OFc Server - no nubs"]
		private IEnumerable<string> quoted_split(string str)
		{
			string quoted_str = null; // var to accumulate full string, quoted or not
			char? quote_char = null; // ? makes char accept nulls --  null or opennig quote char of current quoted string
			// quote_char != null used as flag to confirm we are mid-quoted-string

			var result = new List<string>();

			if (str == null)
				return result;

			foreach (string s in str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (s.StartsWith("\"") || s.StartsWith("\'")) // start of quoted string - allow matching " or'
				{
					quote_char = s[0];
				}
				if (quote_char == null) // NOT in quoted-string so just add this element to list
				{
					result.Add(s);
				}
				else //we're in a quoted string so accumulate
				{
					if (quoted_str == null)
					{
						quoted_str = s; // no accumulated quoted string so far so start with s
					}
					else
					{
						quoted_str += " " + s; // append s to accumulated quoted_str
					}
				}
				// check if we just ended a quoted string
				if (quote_char != null && s.EndsWith(quote_char.ToString())) // end of quoted string
				{
					result.Add(quoted_str.Substring(1).Substring(0, quoted_str.Length - 2));
					quoted_str = null;
					quote_char = null; // quoted_str is now complete
				}
			}
			// check to see if we've ended with an incomplete quoted string... if so add it
			if (quote_char != null && quoted_str != null)
			{
				result.Add(quoted_str);
			}
			return result;
		}

		#endregion
		#region chat methods
		protected abstract void OnChat(string speaker, string message);

		public override void OnGlobalChat(string speaker, string message)
		{
			OnChat(speaker, message);
		}
		public override void OnTeamChat(string speaker, string message, int teamId)
		{
			OnChat(speaker, message);
		}
		#endregion

		public void WriteLog(string message)
		{
			//ExecuteCommand("procon.protected.pluginconsole.write", message);
			string m = message.Replace("{", "~(").Replace("}", ")~");
			switch (LogTarget)
			{
				case LoggingTarget.PluginConsole:
					ConsoleWrite(m);
					break;
				case LoggingTarget.Console:
					ExecuteCommand("procon.protected.console.write", m);
					break;
				case LoggingTarget.Chat:
					ExecuteCommand("procon.protected.chat.write", m);
					break;
				case LoggingTarget.Events:
					ExecuteCommand("procon.protected.events.write", m);
					break;
			}

			try
			{
				File.AppendAllText(PluginFolder + "LangEnforcer.log", "[" + DateTime.Now + "] " + message + Environment.NewLine);
			}
			catch { }
		}

		#region plugin info
		protected bool Enabled { get; private set; }
		public void OnPluginDisable()
		{
			ConsoleWrite("^bLanguage Enforcer ^1Disabled!");
			Enabled = false;
			RunUpdateTask = false;
			if (SaveCounters)
				WriteCounters();
		}
		public void OnPluginEnable()
		{
			ConsoleWrite("^bLanguage Enforcer ^2Enabled!");
			WriteLog("Language Enforcer: Using folder \"" + PluginFolder + "\" for File operations");
			Enabled = true;
			RunUpdateTask = _lookForUpdates;
			if (SaveCounters)
				LoadCounters();
		}

		public string GetPluginName()
		{
			return "LanguageEnforcer";
		}
		public string GetPluginVersion()
		{
			return "1.0.3.0";
		}
		public string GetPluginAuthor()
		{
			return "[SH] PacmanTRT";
		}

		public string GetPluginWebsite()
		{
			return "bitbucket.org/bsattmann/procon-languageenforcer/";
		}

		public string GetPluginDescription()
		{
			return @"
<style>
blockquote{background-color: #EEE; border-left:4px solid #777;}
h2{color:#920000; border-bottom:1px solid #ACACAB;}
h3{color:#920000; border-bottom:1px solid #ECECEB;}
.settings-header{ font-weight: bold; font-size: 1.1em;}
.category-label{ margin-left: 10px; line-height: 1.5; color: #666;}
blockquote > h4{line-height: 1.5;}
.table-head{font-weight: bold; white-space: nowrap}
</style>

<form action=""https://www.paypal.com/cgi-bin/webscr"" method=""post"" target=""_blank"" style=""display: inline;"">
<input type=""hidden"" name=""cmd"" value=""_donations"">
<input type=""hidden"" name=""business"" value=""b.sattmann@gmail.com"">
<input type=""hidden"" name=""lc"" value=""US"">
<input type=""hidden"" name=""item_name"" value=""PacmanTRT"">
<input type=""hidden"" name=""item_number"" value=""LangEnforcer"">
<input type=""hidden"" name=""no_note"" value=""0"">
<input type=""hidden"" name=""currency_code"" value=""EUR"">
<input type=""hidden"" name=""bn"" value=""PP-DonationsBF:btn_donate_LG.gif:NonHostedGuest"">
<input type=""image"" src=""https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif"" border=""0"" name=""submit"" alt=""PayPal - The safer, easier way to pay online!"" style=""height: 20px; margin-left: 135px; margin-top: -28px; position: absolute;"">
<img alt="""" border=""0"" src=""https://www.paypalobjects.com/en_US/i/scr/pixel.gif"" width=""1"" height=""1"">
</form>

<h2>Description</h2>
	<p>This plugin will watch your server's ingame-chat for badwords and does a few other things which can be disabled. 
	<p>Also this plugin actually will wait to kill players if they currently are dead. Admins and the according player will be informed when this is done.</p>
	<p>I have to say that some of the basics in the code are stolen from the awesome <b>ProconRulz</b> Plugin.</p>
<h2>Commands</h2>
	<blockquote style=""margin: 1px 10px 1px 5px; border-left:4px solid #900; background-color: #F7F7F7;"">Admin only</blockquote>
	<blockquote style=""margin: 1px 10px 1px 5px; border-left:4px solid #090; background-color: #F7F7F7;"">Everyone</blockquote>
	
	<blockquote style=""border-left:4px solid #900;""><h4>!langreset / !langr</h4> Resets the Player's badword history. Requires only a fragment of the Player's name. Casing does not matter.</blockquote>
	<blockquote style=""border-left:4px solid #900""><h4>!langpunish / !langp</h4> Manually punishes a player. Requires only a fragment of the Player's name. Casing does not matter.</blockquote>
	<blockquote style=""border-left:4px solid #900""><h4>!langcounter / !langc</h4> Gets or sets the counter for a player. If provided with only a name it will tell you the according counter. If you provide an additional number (same formatting as the cooldown: 0.00) you can set the counter without the need to reset/punish a player</blockquote>
	<blockquote style=""border-left:4px solid #090""><h4>!langinfo</h4> Tells the Player some basics about this plugin.</blockquote>
	<h3>Extended Functions</h3>
	<blockquote style=""border-left:4px solid #090""><h4>!admin</h4> Lists online players with admin rights.</blockquote>
	<blockquote style=""border-left:4px solid #090""><h4>!rules / !help</h4> Displays a configurable helptext.</blockquote>
<br/>
<h2>Settings</h2>
	<blockquote><span class=""settings-header"">Cooldown steps per day</span><span class=""category-label"">Category 2</span><br/> Configures the cooldown. That way a player that curses occasionally will not have to be afraid of a ban. This is continuous and will <b>not</b> actually be execuded at midnight</blockquote></div>
	<blockquote><span class=""settings-header"">Look for updates</span><span class=""category-label"">Category 2</span><br/> Will contact a server to get the current version number and display a message if enabled</blockquote>
	<blockquote><span class=""settings-header"">Enable latent kills</span><span class=""category-label"">Category 2</span><br/> If enabled the plugin will wait for a dead player to spawn when a kill is triggered. Prevents ""HAHA! I already am dead! What now?!""-messages in a very impressive way ^^.</blockquote>
	<blockquote><span class=""settings-header"">Load/Save counters to disk</span><span class=""category-label"">Category 2</span><br/>If enabled the plugin will write the counters to a file when the server is empty.</blockquote>
	<blockquote><span class=""settings-header"">Log to</span><span class=""category-label"">Category 2</span><br/> Specifies where the Plugin should deposit a summary of its actions. The Plugin will also try to log to the LangEnforcer.log file in the plugin folder.</blockquote>
	<blockquote><span class=""settings-header"">Use AdKats punishment</span><span class=""category-label"">Category 2</span><br/> Tell AdKats to punish a player instead of using the successive measures. This will set ""Use AdKats to issue Kills and bans"" to Yes and the ""Allow higher measures"" settings of the overrides to false.</blockquote>
	<blockquote><span class=""settings-header"">Use AdKats to issue Kills and bans</span><span class=""category-label"">Category 2</span><br/> Send Kills, Kicks and Bans to AdKats instead of issuing them within the LanguageEnforcer.</blockquote>
	<blockquote><span class=""settings-header"">Get Admins from textfile</span><span class=""category-label"">Category 2</span><br/> Admins are determined by the <b>le_admins.txt</b> textfile.</blockquote>
	<blockquote><span class=""settings-header"">Section severity</span><span class=""category-label"">Category 6</span><br/> Modifies the value by which the counter will be increased</blockquote>
	<blockquote><span class=""settings-header"">Section measure</span><span class=""category-label"">Category 6</span><br/> Overrides the automatic measure. Below it you can define if it is teated as an absolute or as a minimum.</blockquote>
	<blockquote><span class=""settings-header"">Enable Extended Functions</span><span class=""category-label"">Category 7</span><br/> Enables the !admin, !rules and !help commands</blockquote>
	<blockquote><span class=""settings-header"">Manually punish Player</span><span class=""category-label"">Category 0</span><br/> works just like the ingame-command</blockquote>
	<blockquote><span class=""settings-header"">Manually reset Player</span><span class=""category-label"">Category 0</span><br/> works just like the ingame-command</blockquote>
	<blockquote><span class=""settings-header"">Whitelist</span><span class=""category-label"">Category 4</span><br/> Put all player names here, that should be ignored by the plugin.</blockquote>
	<blockquote><span class=""settings-header"">Warn Whitelisted</span><span class=""category-label"">Category 4</span><br/> If set to ""Yes"" a whitelisted player will recieve warnings (first configured measure)</blockquote>
	<blockquote><span class=""settings-header"">Ignore Squad Chat</span><span class=""category-label"">Category 4</span><br/> If set to ""Yes"" squad chat will be excluded from badword checks. Commands still can be triggered from there.</blockquote>
	<blockquote><span class=""settings-header"">!langinfo message</span><span class=""category-label"">Category 5</span><br/>Message to be sent to the player for the according command. You may use the variables like in the measure-messages (see table below).</blockquote>
	<br/>	
	<h3>Measures</h3>
	A List of measures ending with the entry ""ListEnd"" to be able to add measures. The last actual entry will be executed endlessly. There are different options depending on what measure you choose.<br/>
	<blockquote><h4 id=""Category 3 - "">Repeat X times</h4> Treat this entry as X equal entries.</blockquote>
	<blockquote><h4 id=""Category 3 - "">Custom Command</h4> Execute commands on the Procon console. (Thanks to ProconRulz)</blockquote>
	<br/>	
	<h3>Wordlists</h3>
	<blockquote><h4 id=""Category 1 - "">Badwords</h4> The plugin will punish players when their message contains one of these words. Casing doesn't matter here. <p style=""font-size:11px; padding: 0; margin: 0;""><b>Important:</b> Please note that e.g. ""ass"" will also match ""asshole"" or ""smartass"", but it will also match the german word ""wasser"" which just means water.</p></blockquote>
	<blockquote><h4 id=""Category 1 - "">Regex-Badwords</h4> A far more advanced but also slower way of matching badwords. A <b>simple example</b> is ""noo+b"". The plus means, that the previous letter may be typed more than once. That way the plugin also will recognize when a player writes ""noooob"". If you like the idea, googling for ""C# Regex cheat sheet"" may help you. Casing doesn't matter here either.</blockquote>
	The plugin tries to persist the wordlists in the according .txt-files. These files are attempted to be loaded when the plugin is enabled.
	<br/><br/>	
	<h3>Measure overrides</h3>
	The measures taken against a player can be modified based on sections in the wordlist.</br>
	To define a section you must insert a line that begins and ends with curly brackets. Only letters and digits are allowed to define a section.</br>
	The sample section defined in the default wordlist is <b>{racism}</b></br>
	If you have defined a section the according settings will appear.</br>
	If you set a section to disabled, you might lose its settings.
	<br/><br/>	
	<h3>Messages</h3>
	Following variables will be replaced in ANY configured message:
	<table>
		<tr><td class=""table-head"">%player%</td><td>The player name</td></tr>
		<tr><td class=""table-head"">%time%</td><td>The minutes of a TBan or seconds to Yell</td></tr>
		<tr><td class=""table-head"">%count%</td><td>The counter value of a player</td></tr>
		<tr><td class=""table-head"">%cooldown%</td><td>The cooldown setting</td></tr>
		<tr><td class=""table-head"">%quote%</td><td>The exact thing that was written by the player</td></tr>
		<tr><td class=""table-head"">{any text}</td><td>Text between curly brackets will only be shown if the next measure is different from the current one (e.g. warn is not kill). This is only done for measure related messages.</td></tr>
		<tr><td class=""table-head"">{{countrycode}}</td><td>Must stand in front of a message. For instance a message starting with {{at,ch}} will only be visible for players from Austria and Switzerland.</td></tr>
		<tr><td class=""table-head"">{{-countrycode}}</td><td>Must stand in front of a message. For instance a message starting with {{-at,ch}} will NOT be visible for players from Austria and Switzerland, but to all others.</td></tr>
	</table>
<br/>
<h2>Special thanks</h2>
	<b>IAF SDS</b> for the the ideas regarding Successive Countermeasures and persisting of badword counters.<br/>
	<b>DarkZerO_AT</b> for testing AdKats support.<br/>
	<b>PapaCharlie9</b> for the the highly valuable help regarding the settings mechanism of procon<br/>
	<b>Doom69</b> for the the ideas regarding multiple wordlists.<br/>
	<b>Zaeed</b> for the the ideas regarding the update mechanism.<br/>
	<b>guapoloko</b> for the the ideas I currently cannot implement.

"
				;
		}
		#endregion
	}

	/// <summary>
	/// contains helper methods and faster string functions
	/// </summary>
	public static class ProconUtil
	{
		public static string ToMeasureList(this IEnumerable<BadwordAction> measures)
		{
			var sb = new StringBuilder();
			foreach (var measure in measures)
			{
				sb.Append(measure);
				sb.Append(", ");
			}
			sb.Remove(sb.Length - 2, 2);
			return sb.ToString();
		}

		public static string CreateEnumString(string name, string[] valueList)
		{
			return string.Format("enum.{0}_{1}({2})", "ProconEvents.LanguageEnforcer", name, string.Join("|", valueList));
		}

		internal static string CreateEnumString<T>()
		{
			return CreateEnumString(typeof(T).Name, Enum.GetNames(typeof(T)));
		}

		public static string ToLowerFast(this string value)
		{
			char[] output = value.ToCharArray();
			for (int i = 0; i < output.Length; i++)
			{
				if (output[i] >= 'A' && output[i] <= 'Z')
				{
					output[i] = (char)(output[i] + 32);
				}
			}
			return new string(output);
		}
		public static bool ContainsIgnoreCaseFast(this string haystack, string needle)
		{
			return haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
		}
		public static bool ContainsFast(this string haystack, string needle)
		{
			return haystack.IndexOf(needle, StringComparison.Ordinal) >= 0;
		}
		public static bool StartsWithOneOf(this string haystack, char[] needles)
		{
			if (string.IsNullOrEmpty(haystack))
				return false;
			char start = haystack[0];
			return needles.Any(c => c == start);
		}

		public static string[] ProcessMessages(string[] message, string player, bool showNext, uint time, string quote, bool isMeasureMessage, LanguageEnforcer le)
		{
			var ret = message.ToArray(); //Clone
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = ProcessMessage(ret[i], player, showNext, time, quote, isMeasureMessage, le);
			}
			return ret;
		}

		public static string ProcessMessage(string message, string player, bool showNext, uint time, string quote, bool isMeasureMessage, LanguageEnforcer le)
		{
			if (message.Contains('%'))
				message = Regex.Replace(message, "%\\w*?%", match => ReplaceVariable(match.Value, player, time, quote, le));

			if (isMeasureMessage && message.Contains('{') && showNext)
				message = Regex.Replace(message, "{.*?}", match => {
					if (match.Value.StartsWith("{{"))
						return match.Value;
					return match.Value.Substring(1, match.Value.Length - 2);
				});
			else
				message = Regex.Replace(message, "{.*?}", match => {
					if (match.Value.StartsWith("{{"))
						return match.Value;
					return "";
				});
			return message;
		}

		public static string ProcessCommand(string command, string player, LanguageEnforcer le)
		{
			if (command.Contains('%'))
				command = Regex.Replace(command, "%\\w*?%", match => ReplaceVariable(match.Value, player, 0, "", le));

			return command;
		}

		private static string ReplaceVariable(string match, string player, uint time, string quote, LanguageEnforcer le)
		{
			if (match == "%player%")
				return player;
			if (match == "%quote%")
				return quote;
			if (match == "%time%")
				return time.ToString();
			if (match == "%count%" || match == "%counter%")
				return le.GetCounter(player).ToString();
			if (match == "%cooldown%")
				return le.GetCooldown(player).ToString("0.##", CultureInfo.InvariantCulture);
			return match;
		}

		public static string SavePrepare(this string data, bool prepare)
		{
			return prepare ? CPluginVariable.Encode(data) : data;
		}

		public static string[] SavePrepare(this IEnumerable<string> data, bool prepare)
		{
			if (data == null)
				data = new string[0];
			return prepare ? data.Select(line => CPluginVariable.Encode(line)).ToArray() : (data as string[]) ?? data.ToArray();
		}
	}

	internal class PlayerInfo
	{
		public double Heat;
		public DateTime LastAction;
		public bool Dead;
	}

	public enum BadwordAction { ListEnd, Warn, Kill, Kick, TBan, PermBan, Custom, ShowRules }

	public class SuccessiveMeasure
	{
		public uint Count = 1;
		public BadwordAction Action = BadwordAction.ListEnd;
		public uint TBanTime = 60;
		public uint YellTime = 30;
		public string[] PrivateMessage; // PlayerSay or Kick-Reason
		public string[] PublicMessage; //visible for anyone
		public string[] YellMessage; //PlayerYell
		public string[] Command;

		public void TakeMeasure(LanguageEnforcer le, string player, bool showNext, string quote, MeasureOverride mo)
		{
			if (mo == null)
				mo = MeasureOverride.NoOverride;

			var act = GetAction(mo, le._useAdKatsPunish);
			var pubMsg = mo.PublicMessage ?? PublicMessage;
			var privMsg = mo.PrivateMessage ?? PrivateMessage;
			var yellMsg = mo.YellMessage ?? YellMessage;
			var yellTime = mo.YellTime ?? YellTime;
			var tbanTime = mo.TBanTime ?? TBanTime;
			var command = mo.Command ?? Command;

			if (le._useAdKatsPunish && !mo.IsWhitelisted && !mo.NoAdKats)
			{
				var requestHashtable = new Hashtable {
					{"caller_identity", GetType().Name},
					{"response_requested", false},
					{"command_type", "player_punish"},
					{"source_name", GetType().Name},
					{"target_name", player},
					{"record_message", string.Join(" ", ProconUtil.ProcessMessages(pubMsg, player, false, tbanTime, quote, true, le))},
				};
				if (le.Guids.ContainsKey(player))
					requestHashtable.Add("target_guid", le.Guids[player]);

				le.ExecuteCommand("procon.protected.plugins.call", "AdKats", "IssueCommand", GetType().Name, JSON.JsonEncode(requestHashtable));
				ExecuteCommands(command, player, le);
			}
			else
				switch (act)
				{
					case BadwordAction.Kill:
						le.KillOrLatentKillPlayer(player, le.GetReason(player, ProconUtil.ProcessMessages(privMsg, player, showNext, 0, quote, true, le)));
						goto case BadwordAction.Warn; //please don't hit me
					case BadwordAction.Warn:
						if (pubMsg != null)
							foreach (var msg in pubMsg)
								le.Say(ProconUtil.ProcessMessage(msg, player, showNext, yellTime, quote, true, le));
						if (privMsg != null)
							foreach (var msg in privMsg)
								le.PlayerSay(player, ProconUtil.ProcessMessage(msg, player, showNext, yellTime, quote, true, le));
						if (yellMsg != null)
							foreach (var msg in yellMsg)
								le.PlayerYell(player, ProconUtil.ProcessMessage(msg, player, showNext, yellTime, quote, true, le), yellTime);
						break;
					case BadwordAction.Kick:
						if (pubMsg != null)
							foreach (var msg in pubMsg)
								le.Say(ProconUtil.ProcessMessage(msg, player, showNext, 0, quote, true, le));
						le.KickPlayer(player, le.GetReason(player, ProconUtil.ProcessMessages(privMsg, player, showNext, 0, quote, true, le)));
						break;
					case BadwordAction.TBan:
						if (pubMsg != null)
							foreach (var msg in pubMsg)
								le.Say(ProconUtil.ProcessMessage(msg, player, showNext, tbanTime, quote, true, le));
						le.TBanPlayer(player, le.GetReason(player, ProconUtil.ProcessMessages(privMsg, player, showNext, tbanTime, quote, true, le)), tbanTime);
						break;
					case BadwordAction.PermBan:
						if (pubMsg != null)
							foreach (var msg in pubMsg)
								le.Say(ProconUtil.ProcessMessage(msg, player, showNext, 0, quote, true, le));
						le.BanPlayer(player, le.GetReason(player, ProconUtil.ProcessMessages(privMsg, player, showNext, 0, quote, true, le)));
						break;
					case BadwordAction.ShowRules:
						le.ShowRules(player);
						break;
					case BadwordAction.Custom:
						ExecuteCommands(command, player, le);
						break;
				}
			if (act != BadwordAction.Custom)
				ExecuteCommands(command, player, le);

		}

		private void ExecuteCommands(IEnumerable<string> command, string player, LanguageEnforcer le)
		{
			if (command == null)
				return;

			int counter = 0;
			if (le._players.ContainsKey(player))
			{
				var p = le._players[player];
				counter = (int)Math.Floor(p.Heat);
			}

			foreach (var c in command)
			{
				le.ProconRulzExecuteCommand(ProconUtil.ProcessCommand(c, player, le), counter);
			}
		}

		public BadwordAction GetAction(MeasureOverride mo, bool overrideAlwaysUseMinAction)
		{
			if (overrideAlwaysUseMinAction || mo.AlwaysUseMinAction)
				return mo.MinimumAction;
			if (mo.MinimumAction > Action)
				return mo.MinimumAction;
			return Action;
		}
	}

	public class MeasureOverride
	{
		public static readonly MeasureOverride NoOverride = new MeasureOverride();

		public uint? TBanTime;
		public uint? YellTime;
		public string[] PrivateMessage; // PlayerSay or Kick-Reason
		public string[] PublicMessage; //visible for anyone
		public string[] YellMessage; //PlayerYell
		public string[] Command;
		public BadwordAction MinimumAction = BadwordAction.Warn;
		public bool AlwaysUseMinAction;
		public float MinimumCounter = -1;
		public float Severity = 1;

		public bool IsWhitelisted;
		public bool NoAdKats;
	}

	public enum LoggingTarget
	{
		PluginConsole,
		Console,
		Chat,
		Events,
		DiscardLogMessages
	}
}