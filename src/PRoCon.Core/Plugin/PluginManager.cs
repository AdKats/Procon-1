using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using PRoCon.Core.Accounts;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;
using PRoCon.Core.Players;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Remote;
using PRoCon.Core.Remote.Layer;
using PRoCon.Core.TextChatModeration;
using PRoCon.Core.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace PRoCon.Core.Plugin
{
    public class PluginManager
    {
        public static string PluginsDirectoryName = "Plugins";

        #region Private member attributes

        protected readonly object MatchedInGameCommandsLocker = new object();

        protected AppDomain AppDomainSandbox;

        protected CPRoConPluginCallbacks PluginCallbacks;

        protected CPRoConPluginLoaderFactory PluginFactory;

        protected PRoConClient ProconClient;

        /// <summary>
        ///     Queue of plugin invocations.
        ///     I doubt this will ever actually have more than one invocation on it
        ///     unless we make plugin calls asynchronous in the future.
        ///     This however could break existing plugins that require all calls
        ///     to be synchronous.
        /// </summary>
        protected List<PluginInvocation> Invocations { get; set; }

        /// <summary>
        ///     Thread to handle the looping of the timeout checker.
        ///     This thread will check if any invocations have taken longer than five seconds.
        ///     If they have it will destroy the AppDomain, rebuilding it but not loading
        ///     the faulty plugin.
        /// </summary>
        protected Timer InvocationTimeoutTimer { get; set; }

        /// <summary>
        /// Bool specifying if a check is currently occuring for plugin invocation timeouts. We
        /// ignore the check instead of blocking.
        /// </summary>
        protected bool InvocationTimeoutCheckRunning { get; set; }

        /// <summary>
        /// The maximum time span a single invocation can run
        /// </summary>
        protected TimeSpan InvocationMaxRuntime { get; set; }

        /// <summary>
        ///     A list of plugin class names that have previously been deemed as
        ///     broken and should be ignored during compile/load time.
        ///     If a plugin is the cause of the manager going into a panic this
        ///     will serve for the manager to ignore the plugin when/if the manager
        ///     is reloaded because of the panic.
        /// </summary>
        public List<String> IgnoredPluginClassNames { get; protected set; }

        /// <summary>
        /// The cache file, storing all the details of compiled plugins.
        /// </summary>
        public PluginCache PluginCache { get; set; }

        #endregion

        #region Events and Delegates

        public delegate void PluginEmptyParameterHandler(string strClassName);

        public delegate void PluginEventHandler();

        public delegate void PluginOutputHandler(string strOutput);

        public delegate void PluginVariableAlteredHandler(PluginDetails spdNewDetails);

        /// <summary>
        ///     The plugin manager has gone into panic, the plugins should be
        ///     reloaded as the AppDomain has been compromised.
        ///     Panics can occur when a plugin invocation times out, possibly leaning
        ///     towards a runaway call within the AppDomain that would be chewing
        ///     a lot of resources.
        /// </summary>
        public event PluginEventHandler PluginPanic;

        public event PluginOutputHandler PluginOutput;

        public event PluginEmptyParameterHandler PluginLoaded;
        public event PluginEmptyParameterHandler PluginEnabled;
        public event PluginEmptyParameterHandler PluginDisabled;

        public event PluginVariableAlteredHandler PluginVariableAltered;

        #endregion

        #region Properties

        public PluginDictionary Plugins { get; private set; }

        // TO DO: Move to seperate command control class with events captured by PluginManager.
        public Dictionary<string, MatchCommand> MatchedInGameCommands { get; private set; }
        private ConfirmationDictionary CommandsNeedingConfirmation { get; set; }

        public string PluginBaseDirectory
        {
            get { return Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PluginsDirectoryName), ProconClient.GameType); }
        }

        public string PluginDebugTempDirectory
        {
            get { return Path.Combine(this.PluginBaseDirectory, "Temp"); }
        }

        #endregion

        public PluginManager(PRoConClient cpcClient)
        {
            AppDomainSandbox = null;
            Plugins = new PluginDictionary();
            //this.m_dicLoadedPlugins = new Dictionary<string, IPRoConPluginInterface>();
            //this.m_dicEnabledPlugins = new Dictionary<string, IPRoConPluginInterface>();

            //this.CacheFailCompiledPluginVariables = new Dictionary<string, Dictionary<string, string>>();

            ProconClient = cpcClient;
            //this.LoadedClassNames = new List<string>();
            MatchedInGameCommands = new Dictionary<string, MatchCommand>();
            CommandsNeedingConfirmation = new ConfirmationDictionary();

            // Handle plugin invocation timeouts.
            Invocations = new List<PluginInvocation>();
            IgnoredPluginClassNames = new List<String>();

            this.InvocationMaxRuntime = ProconClient.PluginMaxRuntimeSpan;

            if (this.InvocationMaxRuntime.TotalMilliseconds < 10)
            {
                this.InvocationMaxRuntime = PluginInvocation.MaximumRuntime;
            }

            // Default maximum runtime = 5 seconds, divided by 20
            // check every 250 milliseconds.
            this.InvocationTimeoutTimer = new Timer(state => this.InvocationTimeoutCheck(), null, (int)(this.InvocationMaxRuntime.TotalMilliseconds / 20.0), (int)(this.InvocationMaxRuntime.TotalMilliseconds / 20.0));

            AssignEventHandler();
        }

        private void InvocationTimeoutCheck()
        {
            if (this.InvocationTimeoutCheckRunning == false)
            {
                this.InvocationTimeoutCheckRunning = true;

                PluginInvocation invocation = Invocations.FirstOrDefault(); // .OrderBy(x => x.Runtime())

                if (invocation != null)
                {
                    if (invocation.Runtime() >= this.InvocationMaxRuntime)
                    {
                        WritePluginConsole("^1^bPlugin manager entering panic..");

                        // Prevent the plugin from being loaded again during this instance
                        // of the plugin manager.
                        IgnoredPluginClassNames.Add(invocation.Plugin.ClassName);

                        String faultText = invocation.FormatInvocationFault("Call exceeded maximum execution time of {0}", this.InvocationMaxRuntime);

                        // Log the error so we might alert a plugin developer that
                        // a call to their plugin has caused the plugin manager to go
                        // into a panic.
                        File.AppendAllText("PLUGIN_DEBUG.txt", faultText);

                        WritePluginConsole("^1^bPlugin invocation timeout: ");
                        WritePluginConsole("^1" + faultText);

                        if (PluginPanic != null)
                        {
                            PluginPanic();
                        }
                    }

                    this.InvocationTimeoutCheckRunning = false;
                }
            }
        }

        /// <summary>
        ///     Adds an invocation to our list.
        ///     It's titled Enqueue for this may change in the future if this class
        ///     is modified to handle asynchronous calls to plugins it will have to handle
        ///     in a sort-of queue like fashion.
        /// </summary>
        /// <param name="plugin">The plugin being invoked</param>
        /// <param name="methodName">The method name within the plugin that is being invoked</param>
        /// <param name="parameters">The parameters being passed to the method within the plugin being invoked.</param>
        protected void EnqueueInvocation(Plugin plugin, String methodName, Object[] parameters)
        {
            lock (this)
            {
                Invocations.Add(new PluginInvocation()
                {
                    Plugin = plugin,
                    MethodName = methodName,
                    Parameters = parameters
                });
            }
        }

        /// <summary>
        ///     Removes all occurences of the invocation
        ///     It's titled Dequeue for this may change in the future if this class
        ///     is modified to handle asynchronous calls to plugins it will have to handle
        ///     in a sort-of queue like fashion.
        /// </summary>
        /// <param name="plugin">The plugin that was invoked</param>
        /// <param name="methodName">The method name within the plugin that was invoked</param>
        /// <param name="parameters">The parameters being passed to the method within the plugin that was invoked.</param>
        protected void DequeueInvocation(Plugin plugin, String methodName, Object[] parameters)
        {
            lock (this)
            {
                Invocations.RemoveAll(x => x.Plugin == plugin && x.MethodName == methodName);
            }
        }

        // TO DO: Move to seperate command control class with events captured by PluginManager.
        public void RegisterCommand(MatchCommand mtcCommand)
        {
            lock (MatchedInGameCommandsLocker)
            {
                if (mtcCommand.RegisteredClassname.Length > 0 && mtcCommand.RegisteredMethodName.Length > 0 && mtcCommand.Command.Length > 0)
                {
                    if (MatchedInGameCommands.ContainsKey(mtcCommand.ToString()) == true)
                    {
                        if (String.CompareOrdinal(MatchedInGameCommands[mtcCommand.ToString()].RegisteredClassname, mtcCommand.RegisteredClassname) != 0)
                        {
                            WritePluginConsole("^1^bIdentical command registration on class {0} overwriting class {1} command {2}", mtcCommand.RegisteredClassname, MatchedInGameCommands[mtcCommand.ToString()].RegisteredClassname, MatchedInGameCommands[mtcCommand.ToString()].ToString());
                        }

                        MatchedInGameCommands[mtcCommand.ToString()] = mtcCommand;
                    }
                    else
                    {
                        MatchedInGameCommands.Add(mtcCommand.ToString(), mtcCommand);

                        InvokeOnAllEnabled("OnRegisteredCommand", mtcCommand);
                    }
                }
            }
        }

        public void UnregisterCommand(MatchCommand mtcCommand)
        {
            lock (MatchedInGameCommandsLocker)
            {
                if (MatchedInGameCommands.ContainsKey(mtcCommand.ToString()) == true)
                {
                    MatchedInGameCommands.Remove(mtcCommand.ToString());
                    InvokeOnAllEnabled("OnUnregisteredCommand", mtcCommand);
                }
            }
        }

        public List<MatchCommand> GetRegisteredCommands()
        {
            return new List<MatchCommand>(MatchedInGameCommands.Values);
        }

        private void WritePluginConsole(string strFormat, params object[] arguments)
        {
            if (PluginOutput != null)
            {
                this.PluginOutput(String.Format(strFormat, arguments));
            }
        }

        public void EnablePlugin(string className)
        {
            if (Plugins.IsLoaded(className) == true && Plugins.IsEnabled(className) == false)
            {
                Plugins[className].IsEnabled = true;

                try
                {
                    Plugins[className].Type.Invoke("OnPluginEnable");

                    if (PluginEnabled != null)
                    {
                        this.PluginEnabled(className);
                    }
                }
                catch (Exception e)
                {
                    WritePluginConsole("{0}.EnablePlugin(): {1}", className, e.Message);
                }
            }
        }

        public void DisablePlugin(string className)
        {
            if (Plugins.IsLoaded(className) == true && Plugins.IsEnabled(className) == true)
            {
                Plugins[className].IsEnabled = false;

                try
                {
                    Plugins[className].Type.Invoke("OnPluginDisable");

                    if (PluginDisabled != null)
                    {
                        this.PluginDisabled(className);
                    }
                }
                catch (Exception e)
                {
                    WritePluginConsole("{0}.DisablePlugin(): {1}", className, e.Message);
                }
            }
        }

        public PluginDetails GetPluginDetails(string strClassName)
        {
            return new PluginDetails
            {
                ClassName = strClassName,
                Name = InvokeOnLoaded_String(strClassName, "GetPluginName"),
                Author = InvokeOnLoaded_String(strClassName, "GetPluginAuthor"),
                Version = InvokeOnLoaded_String(strClassName, "GetPluginVersion"),
                Website = InvokeOnLoaded_String(strClassName, "GetPluginWebsite"),
                Description = InvokeOnLoaded_String(strClassName, "GetPluginDescription"),
                DisplayPluginVariables = InvokeOnLoaded_CPluginVariables(strClassName, "GetDisplayPluginVariables"),
                PluginVariables = InvokeOnLoaded_CPluginVariables(strClassName, "GetPluginVariables")
            };
        }

        public void SetPluginVariable(string strClassName, string strVariable, string strValue, bool notification = true)
        {
            // FailCompiledPlugins

            if (Plugins.Contains(strClassName) == true && Plugins[strClassName].IsLoaded == true)
            {
                InvokeOnLoaded(strClassName, "SetPluginVariable", new object[] { strVariable, strValue });

                if (PluginVariableAltered != null && notification == true)
                {
                    this.PluginVariableAltered(GetPluginDetails(strClassName));
                }
            }
            else if (Plugins.IsLoaded(strClassName) == false)
            {
                Plugins.SetCachedPluginVariable(strClassName, strVariable, strValue);

                /*
                if (this.CacheFailCompiledPluginVariables[strClassName].ContainsKey(strVariable) == true) {
                    this.CacheFailCompiledPluginVariables[strClassName][strVariable] = strValue;
                }
                else {
                    this.CacheFailCompiledPluginVariables[strClassName].Add(strVariable, strValue);
                }
                */
            }
        }

        public PluginDetails GetPluginDetailsCon(string strClassName)
        {
            return new PluginDetails
            {
                ClassName = strClassName,
                Name = InvokeOnLoaded_String(strClassName, "GetPluginName"),
                Author = InvokeOnLoaded_String(strClassName, "GetPluginAuthor"),
                Version = InvokeOnLoaded_String(strClassName, "GetPluginVersion"),
                Website = InvokeOnLoaded_String(strClassName, "GetPluginWebsite"),
                Description = InvokeOnLoaded_String(strClassName, "GetPluginDescription"),
                PluginVariables = InvokeOnLoaded_CPluginVariables(strClassName, "GetPluginVariables")
            };
        }

        public void SetPluginVariableCon(string strClassName, string strVariable, string strValue, bool notification = true)
        {
            // FailCompiledPlugins

            if (Plugins.Contains(strClassName) == true && Plugins[strClassName].IsLoaded == true)
            {
                InvokeOnLoaded(strClassName, "SetPluginVariable", new object[] { strVariable, strValue });

                if (PluginVariableAltered != null && notification == true)
                {
                    this.PluginVariableAltered(GetPluginDetailsCon(strClassName));
                }
            }
            else if (Plugins.IsLoaded(strClassName) == false)
            {
                Plugins.SetCachedPluginVariable(strClassName, strVariable, strValue);
            }
        }

        public void InvokeOnLoaded(string strClassName, string strMethod, params object[] parameters)
        {
            try
            {
                if (Plugins.Contains(strClassName) == true && Plugins[strClassName].IsLoaded == true)
                {
                    EnqueueInvocation(Plugins[strClassName], strMethod, parameters);

                    Plugins[strClassName].Type.Invoke(strMethod, parameters);

                    DequeueInvocation(Plugins[strClassName], strMethod, parameters);
                }
            }
            catch (Exception e)
            {
                WritePluginConsole("{0}.{1}(): {2}", strClassName, strMethod, e.Message);
            }
        }

        public string InvokeOnLoaded_String(string strClassName, string strMethod, params object[] parameters)
        {
            string strReturn = String.Empty;

            try
            {
                if (Plugins.Contains(strClassName) == true && Plugins[strClassName].IsLoaded == true)
                {
                    EnqueueInvocation(Plugins[strClassName], strMethod, parameters);

                    strReturn = (string)Plugins[strClassName].Type.Invoke(strMethod, parameters);

                    DequeueInvocation(Plugins[strClassName], strMethod, parameters);
                }
            }
            catch (Exception e)
            {
                WritePluginConsole("{0}.{1}(): {2}", strClassName, strMethod, e.Message);
            }

            return strReturn;
        }

        public List<CPluginVariable> GetPluginVariables(string strClassName)
        {
            return InvokeOnLoaded_CPluginVariables(strClassName, "GetPluginVariables");
        }

        private List<CPluginVariable> InvokeOnLoaded_CPluginVariables(string strClassName, string strMethod, params object[] parameters)
        {
            List<CPluginVariable> lstReturn = null;

            try
            {
                if (Plugins.Contains(strClassName) == true && Plugins[strClassName].IsLoaded == true)
                {
                    EnqueueInvocation(Plugins[strClassName], strMethod, parameters);

                    lstReturn = (List<CPluginVariable>)Plugins[strClassName].Type.Invoke(strMethod, parameters);

                    DequeueInvocation(Plugins[strClassName], strMethod, parameters);
                }
            }
            catch (Exception e)
            {
                WritePluginConsole("{0}.{1}(): {2}", strClassName, strMethod, e.Message);
            }

            // If a problem occured return an empty list.
            if (lstReturn == null)
            {
                lstReturn = new List<CPluginVariable>();
            }

            return lstReturn;
        }

        public object InvokeOnEnabled(string strClassName, string strMethod, params object[] parameters)
        {
            object returnObject = null;

            try
            {
                if (Plugins.Contains(strClassName) == true && Plugins[strClassName].IsEnabled == true)
                {
                    EnqueueInvocation(Plugins[strClassName], strMethod, parameters);

                    returnObject = Plugins[strClassName].Type.Invoke(strMethod, parameters);

                    DequeueInvocation(Plugins[strClassName], strMethod, parameters);
                }
            }
            catch (Exception e)
            {
                WritePluginConsole("{0}.{1}(): {2}", strClassName, strMethod, e.Message);
            }

            return returnObject;
        }

        public void InvokeOnAllLoaded(string strMethod, params object[] parameters)
        {
            foreach (Plugin plugin in Plugins)
            {
                if (plugin.IsLoaded == true)
                {
                    try
                    {
                        EnqueueInvocation(plugin, strMethod, parameters);

                        plugin.ConditionalInvoke(strMethod, parameters);

                        DequeueInvocation(plugin, strMethod, parameters);
                    }
                    catch (Exception e)
                    {
                        WritePluginConsole("{0}.{1}(): {2}", plugin.ClassName, strMethod, e.Message);
                    }
                }
            }
        }

        public void InvokeOnAllEnabled(string methodName, params object[] parameters)
        {
            List<String> types = Plugins.Where(plugin => plugin.IsEnabled && plugin.CanConditionallyInvoke(methodName)).Select(plugin => plugin.ClassName).ToList();

            if (types.Count > 0)
            {
                PluginFactory.ConditionallyInvokeOn(types, methodName, parameters);
            }
        }

        private void PreparePluginsDirectory()
        {
            try
            {
                if (Directory.Exists(PluginBaseDirectory) == false)
                {
                    Directory.CreateDirectory(PluginBaseDirectory);
                }

                File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRoCon.Core.dll"), Path.Combine(PluginBaseDirectory, "PRoCon.Core.dll"), true);
                File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MySql.Data.dll"), Path.Combine(PluginBaseDirectory, "MySql.Data.dll"), true);
                File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRoCon.Core.pdb"), Path.Combine(PluginBaseDirectory, "PRoCon.Core.pdb"), true);

                // Clean up temp directory
                if (Directory.Exists(PluginDebugTempDirectory) == true)
                {
                    foreach (string file in Directory.GetFiles(PluginDebugTempDirectory))
                    {
                        File.Delete(file);
                    }
                }

                // Remove PDB files from plugin directory
                foreach (string file in Directory.GetFiles(PluginBaseDirectory, "*.pdb"))
                {
                    File.Delete(file);
                }
            }
            catch
            {
            }
        }

        private void MoveLegacyPlugins()
        {
            try
            {
                string legacyPluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PluginsDirectoryName);
                string legacyPluginDestinationDirectory = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PluginsDirectoryName), "BFBC2");

                var legacyPluginsDirectoryInfo = new DirectoryInfo(legacyPluginDirectory);
                FileInfo[] legacyPluginsInfo = legacyPluginsDirectoryInfo.GetFiles("*.cs");

                foreach (FileInfo legacyPlugin in legacyPluginsInfo)
                {
                    try
                    {
                        File.Move(legacyPlugin.FullName, Path.Combine(legacyPluginDestinationDirectory, legacyPlugin.Name));
                    }
                    catch (Exception e)
                    {
                        WritePluginConsole("^1PluginManager.MoveLegacyPlugins(): Move: \"{0}\"; Keeping /Plugins/BFBC2/ version.  Warning: {1};", legacyPlugin.Name, e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                WritePluginConsole("^1PluginManager.MoveLegacyPlugins(): Error: {0};", e.Message);
            }
        }

        private CSharpCompilationOptions GetCSharpCompilationOptions(bool enableDebugging)
        {
            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithOverflowChecks(true);

            if (enableDebugging)
            {
                compilationOptions = compilationOptions.WithOptimizationLevel(OptimizationLevel.Debug);
            }
            else
            {
                compilationOptions = compilationOptions.WithOptimizationLevel(OptimizationLevel.Release);
            }

            return compilationOptions;
        }

        private CSharpParseOptions GetCSharpParseOptions()
        {
            return CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
        }

        private IEnumerable<MetadataReference> GetCSharpCompilationReferences()
        {
            string dotnetRuntimePath = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "{0}.dll");
            string proconRuntimePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "{0}.dll");

            return new[]
            {
                MetadataReference.CreateFromFile(string.Format(dotnetRuntimePath, "mscorlib")),
                MetadataReference.CreateFromFile(string.Format(dotnetRuntimePath, "System")),
                MetadataReference.CreateFromFile(string.Format(dotnetRuntimePath, "System.Core")),
                MetadataReference.CreateFromFile(string.Format(dotnetRuntimePath, "System.Data")),
                MetadataReference.CreateFromFile(string.Format(dotnetRuntimePath, "System.Windows.Forms")),
                MetadataReference.CreateFromFile(string.Format(dotnetRuntimePath, "System.Xml")),

                MetadataReference.CreateFromFile(string.Format(proconRuntimePath, "MySql.Data")),
                MetadataReference.CreateFromFile(string.Format(proconRuntimePath, "PRoCon.Core"))
            };
        }

        private void PrintPluginResults(FileInfo pluginFile, EmitResult pluginResults)
        {
            // Produce compiler errors (if any)
            if (pluginResults.Success == false)
            {
                WritePluginConsole("Compiling {0}... ^1Errors^0 or ^3Warnings", pluginFile.Name);

                foreach (Diagnostic diagnostic in pluginResults.Diagnostics)
                {
                    if (diagnostic.Severity == DiagnosticSeverity.Error)
                    {
                        WritePluginConsole("\t^1{0}: {1}", pluginFile.Name, diagnostic.ToString());
                    }
                }
            }
            else
            {
                WritePluginConsole("Compiling {0}... ^2Done", pluginFile.Name);
            }
        }

        private string PrecompileDirectives(string source)
        {
            MatchCollection matches;
            int replacementDepth = 0;

            do
            {
                matches = Regex.Matches(source, "#include \"(?<file>.*?)\"");

                foreach (Match match in matches)
                {
                    try
                    {
                        string fileContents = File.ReadAllText(Path.Combine(PluginBaseDirectory, match.Groups["file"].Value.Replace("%GameType%", ProconClient.GameType)));

                        source = source.Replace(match.Value, fileContents);
                    }
                    catch (Exception e)
                    {
                        WritePluginConsole("^PluginManager.PrecompileDirectives(): #include File Error: {0};", e.Message);
                    }
                }

                replacementDepth++;
            } while (matches.Count > 0 && replacementDepth <= 5);

            if (replacementDepth > 5)
            {
                WritePluginConsole("^PluginManager.PrecompileDirectives(): #include Recursion Error: Invalid depth of {0}", replacementDepth);
            }


            return source;
        }

        /// <summary>
        /// Opens a source file, gets the contents and processes it for any legacy using directives and procon's
        /// crappy pre-compile directives.
        /// </summary>
        /// <param name="pluginFile"></param>
        /// <returns></returns>
        protected String BuildPluginSource(FileInfo pluginFile)
        {
            String fullPluginSource = File.ReadAllText(pluginFile.FullName);

            fullPluginSource = PrecompileDirectives(fullPluginSource);

            fullPluginSource = fullPluginSource.Replace("using PRoCon.Plugin;", "using PRoCon.Core.Plugin;");

            if (fullPluginSource.Contains("using PRoCon.Core;") == false)
            {
                fullPluginSource = fullPluginSource.Insert(fullPluginSource.IndexOf("using PRoCon.Core.Plugin;", StringComparison.Ordinal), "\r\nusing PRoCon.Core;\r\n");
            }

            if (fullPluginSource.Contains("using PRoCon.Core.Players;") == false)
            {
                fullPluginSource = fullPluginSource.Insert(fullPluginSource.IndexOf("using PRoCon.Core.Plugin;", StringComparison.Ordinal), "\r\nusing PRoCon.Core.Players;\r\n");
            }

            return fullPluginSource;
        }

        private void CompilePlugin(FileInfo pluginFile, string pluginClassName, CSharpCompilationOptions compilationOptions)
        {

            // 1. Grab the full source of this plugin
            String fullPluginSource = this.BuildPluginSource(pluginFile);

            // 2. Compute the hash of this plugin source.
            String pluginSourceHash = MD5.String(fullPluginSource);

            // 2. See if we require this plugin to be recompiled.
            bool requiresRecompiling = this.PluginCache.IsModified(pluginClassName, pluginSourceHash);

            String outputAssembly = Path.Combine(PluginBaseDirectory, pluginClassName + ".dll");
            // we don't need the pdb or xml file if plugin debugging is disabled
            String pdbPath = this.ProconClient.Parent.OptionsSettings.EnablePluginDebugging ? Path.Combine(PluginBaseDirectory, pluginClassName + ".pdb") : null;
            String xmlDocPath = this.ProconClient.Parent.OptionsSettings.EnablePluginDebugging ? Path.Combine(PluginBaseDirectory, pluginClassName + ".xml") : null;

            // 2.1: check if plugin debugging is enabled, always force compilation if true
            if (requiresRecompiling == true || File.Exists(outputAssembly) == false || this.ProconClient.Parent.OptionsSettings.EnablePluginDebugging == true)
            {
                // 3. If a compiled plugin exists already, remove it now.
                if (File.Exists(outputAssembly) == true)
                {
                    try
                    {
                        File.Delete(outputAssembly);
                    }
                    catch
                    {
                        WritePluginConsole("Error removing file {0}... Skipping", outputAssembly);
                    }
                }

                try
                {
                    // 4. Prepare compilation of the plugin
                    CSharpParseOptions parseOptions = this.GetCSharpParseOptions();

                    // create a list of syntax trees and add the main source file by default
                    List<SyntaxTree> syntaxTrees = new List<SyntaxTree> { CSharpSyntaxTree.ParseText(fullPluginSource, parseOptions, pluginFile.FullName, Encoding.UTF8) };

                    // get all related partial source files and add them to the list too
                    DirectoryInfo pluginsDirectoryInfo = new DirectoryInfo(PluginBaseDirectory);
                    foreach (FileInfo partialPluginFile in pluginsDirectoryInfo.GetFiles(pluginClassName + ".*.cs"))
                    {
                        string partialPluginSource = File.ReadAllText(partialPluginFile.FullName);
                        syntaxTrees.Add(CSharpSyntaxTree.ParseText(partialPluginSource, parseOptions, partialPluginFile.FullName, Encoding.UTF8));
                    }

                    IEnumerable<MetadataReference> compilationReferences = this.GetCSharpCompilationReferences();
                    CSharpCompilation compilation = CSharpCompilation.Create(pluginClassName, syntaxTrees, compilationReferences, compilationOptions);

                    // 4.1. Now compile the plugin
                    this.PrintPluginResults(pluginFile, compilation.Emit(outputAssembly, pdbPath, xmlDocPath));

                    // 5. Add/Update the storage cache for this plugin.
                    this.PluginCache.Cache(new PluginCacheEntry()
                    {
                        ClassName = pluginClassName,
                        Hash = pluginSourceHash,
                        SourcePath = pluginFile.FullName,
                        DestinationPath = outputAssembly
                    });
                }
                catch (Exception e)
                {
                    WritePluginConsole("Error compiling {0}: {1}", pluginClassName, e.Message);
                }
            }
            else
            {
                WritePluginConsole("Compiling {0}... ^2Using Cache", pluginFile.Name);
            }
        }

        private void LoadPlugin(string pluginClassName, CPRoConPluginLoaderFactory pluginFactory, bool blSandboxDisabled)
        {
            bool blSandboxEnabled = (blSandboxDisabled == true) ? false : true;
            string outputAssembly = Path.Combine(PluginBaseDirectory, pluginClassName + ".dll");

            if (File.Exists(outputAssembly) == true)
            {
                IPRoConPluginInterface pluginRemoteInterface = pluginFactory.Create(outputAssembly, "PRoConEvents." + pluginClassName, null);

                // Indirectely invoke registercallbacks since the delegates cannot go in the interface.
                pluginRemoteInterface.Invoke("RegisterCallbacks", new object[] { new CPRoConMarshalByRefObject.ExecuteCommandHandler(PluginCallbacks.ExecuteCommand_Callback), new CPRoConMarshalByRefObject.GetAccountPrivilegesHandler(PluginCallbacks.GetAccountPrivileges_Callback), new CPRoConMarshalByRefObject.GetVariableHandler(PluginCallbacks.GetVariable_Callback), new CPRoConMarshalByRefObject.GetVariableHandler(PluginCallbacks.GetSvVariable_Callback), new CPRoConMarshalByRefObject.GetMapDefinesHandler(PluginCallbacks.GetMapDefines_Callback), new CPRoConMarshalByRefObject.TryGetLocalizedHandler(PluginCallbacks.TryGetLocalized_Callback), new CPRoConMarshalByRefObject.RegisterCommandHandler(PluginCallbacks.RegisterCommand_Callback), new CPRoConMarshalByRefObject.UnregisterCommandHandler(PluginCallbacks.UnregisterCommand_Callback), new CPRoConMarshalByRefObject.GetRegisteredCommandsHandler(PluginCallbacks.GetRegisteredCommands_Callback), new CPRoConMarshalByRefObject.GetWeaponDefinesHandler(PluginCallbacks.GetWeaponDefines_Callback), new CPRoConMarshalByRefObject.GetSpecializationDefinesHandler(PluginCallbacks.GetSpecializationDefines_Callback), new CPRoConMarshalByRefObject.GetLoggedInAccountUsernamesHandler(PluginCallbacks.GetLoggedInAccountUsernames_Callback), new CPRoConMarshalByRefObject.RegisterEventsHandler(PluginCallbacks.RegisterEvents_Callback) });

                Plugins.AddLoadedPlugin(pluginClassName, pluginRemoteInterface);

                /*
                if (this.m_dicLoadedPlugins.ContainsKey(pluginClassName) == false) {
                    this.m_dicLoadedPlugins.Add(pluginClassName, loRemote);
                }
                else {
                    this.m_dicLoadedPlugins[pluginClassName] = loRemote;
                }

                this.LoadedClassNames.Add(pluginClassName);
                */

                var pluginEnvironment = new List<string>() {
                    Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    ProconClient.GameType,
                    ProconClient.CurrentServerInfo.GameMod.ToString(),
                    blSandboxEnabled.ToString()
                };

                InvokeOnLoaded(pluginClassName, "OnPluginLoadingEnv", pluginEnvironment);

                WritePluginConsole("Loading {0}... ^2Loaded", pluginClassName);

                InvokeOnLoaded(pluginClassName, "OnPluginLoaded", ProconClient.HostName, ProconClient.Port.ToString(CultureInfo.InvariantCulture), Assembly.GetExecutingAssembly().GetName().Version.ToString());

                if (PluginLoaded != null)
                {
                    this.PluginLoaded(pluginClassName);
                }
            }
            //else {
            //    this.CacheFailCompiledPluginVariables.Add(pluginClassName, new Dictionary<string, string>());
            //}
        }

        public void RegisterPluginEvents(string className, List<string> events)
        {
            if (Plugins.IsLoaded(className) == true)
            {
                Plugins[className].RegisteredEvents = events;
            }
        }

        /// <summary>
        /// Builds the strong name of a loaded assembly
        /// </summary>
        /// <param name="assembly">The assembly to fetch the strong name of</param>
        /// <returns></returns>
        protected static StrongName GetStrongName(Assembly assembly)
        {
            AssemblyName name = assembly.GetName();

            return new StrongName(new StrongNamePublicKeyBlob(name.GetPublicKey()), name.Name, name.Version);
        }

        public void CompilePlugins(PermissionSet pluginSandboxPermissions, List<String> ignoredPluginClassNames = null)
        {
            try
            {

                if (File.Exists(Path.Combine(this.PluginBaseDirectory, "PluginCache.xml")) == true)
                {
                    WritePluginConsole("Loading plugin cache..");

                    try
                    {
                        this.PluginCache = PluginCache.Load(Path.Combine(this.PluginBaseDirectory, "PluginCache.xml"));
                    }
                    catch (Exception e)
                    {
                        WritePluginConsole("Error loading plugin cache: {0}", e.Message);
                    }
                }

                // Recover from exceptions or logic errors if the document parsed correctly, but didn't deserialize correctly.
                if (this.PluginCache == null)
                {
                    this.PluginCache = new PluginCache();
                }

                // Make sure we ignore any plugins passed in. These won't even be loaded again.
                if (ignoredPluginClassNames != null)
                {
                    IgnoredPluginClassNames = ignoredPluginClassNames;
                }

                // Clear out all invocations if this is a reload.
                Invocations.Clear();

                WritePluginConsole("Preparing plugins directory..");
                PreparePluginsDirectory();

                WritePluginConsole("Moving legacy plugins..");
                MoveLegacyPlugins();

                WritePluginConsole("Creating and configuring compiler..");
                CSharpCompilationOptions compilationOptions = this.GetCSharpCompilationOptions(this.ProconClient.Parent.OptionsSettings.EnablePluginDebugging);
                // AppDomainSetup domainSetup = new AppDomainSetup() { ApplicationBase = this.PluginBaseDirectory };
                // Start of XpKillers mono workaround

                AppDomainSetup domainSetup = null;
                Type t = Type.GetType("Mono.Runtime");
                if (t != null)
                {
                    //Console.WriteLine("You are running with the Mono VM");
                    WritePluginConsole("Running with Mono VM..");
                    //AppDomain.CurrentDomain.BaseDirectory
                    domainSetup = new AppDomainSetup()
                    {
                        ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
                    };
                    domainSetup.PrivateBinPath = PluginBaseDirectory;
                }
                else
                {
                    // Console.WriteLine("You are running something else (native .Net)");
                    WritePluginConsole("Running with native .Net..");
                    domainSetup = new AppDomainSetup()
                    {
                        ApplicationBase = PluginBaseDirectory
                    };
                }
                // Workaround end

                WritePluginConsole("Building sandbox..");
                var hostEvidence = new Evidence();
                hostEvidence.AddHost(new Zone(SecurityZone.MyComputer));

                AppDomainSandbox = AppDomain.CreateDomain(ProconClient.HostName + ProconClient.Port + "Engine", hostEvidence, domainSetup, pluginSandboxPermissions, PluginManager.GetStrongName(AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.GetName().Name == "MySql.Data")));

                WritePluginConsole("Configuring sandbox..");
                // create the factory class in the secondary app-domain
                PluginFactory = (CPRoConPluginLoaderFactory)AppDomainSandbox.CreateInstance("PRoCon.Core", "PRoCon.Core.Plugin.CPRoConPluginLoaderFactory").Unwrap();
                PluginCallbacks = new CPRoConPluginCallbacks(ProconClient.ExecuteCommand, ProconClient.GetAccountPrivileges, ProconClient.GetVariable, ProconClient.GetSvVariable, ProconClient.GetMapDefines, ProconClient.TryGetLocalized, RegisterCommand, UnregisterCommand, GetRegisteredCommands, ProconClient.GetWeaponDefines, ProconClient.GetSpecializationDefines, ProconClient.Layer.GetLoggedInAccountUsernames, RegisterPluginEvents);

                WritePluginConsole("Compiling and loading plugins..");

                if (this.ProconClient.Parent.OptionsSettings.EnablePluginDebugging == true)
                {
                    WritePluginConsole("^b^1*** PLUGIN DEBUGGING ENABLED ***^0^n");
                    WritePluginConsole("^b^1If you're not actively testing or debugging a plugin, please disable this setting in Procon's options!^0^n");
                }

                var pluginsDirectoryInfo = new DirectoryInfo(PluginBaseDirectory);
                string className = string.Empty;

                foreach (FileInfo pluginFile in pluginsDirectoryInfo.GetFiles("*.cs"))
                {
                    try
                    {
                        className = Regex.Replace(pluginFile.Name, "\\.cs$", "");

                        // skip partial classes (only continue with the "main" partial class, the rest is getting added on compilation)
                        // files containing a dot (.) will be treated as additional partial classes/files, the main file does not contain any dots
                        // Example: MyPlugin.cs => Main file, so the file name equals the class name
                        //			MyPlugin.Additional.cs => additional file(s)
                        if (className.Contains("."))
                        {
                            continue;
                        }

                        if (IgnoredPluginClassNames.Contains(className) == false)
                        {
                            CompilePlugin(pluginFile, className, compilationOptions);

                            LoadPlugin(className, PluginFactory, pluginSandboxPermissions.IsUnrestricted());
                        }
                        else
                        {
                            WritePluginConsole("Compiling {0}... ^1^bIgnored", className);
                        }
                    }
                    catch (Exception e)
                    {
                        WritePluginConsole("Compiling {0}... ^1^bException", className);
                        WritePluginConsole(e.ToString());
                    }
                }

                this.PluginCache.Save(Path.Combine(this.PluginBaseDirectory, "PluginCache.xml"));
            }
            catch (Exception e)
            {
                WritePluginConsole(e.Message);
            }
        }

        ~PluginManager()
        {
            AppDomainSandbox = null;
            //this.m_dicEnabledPlugins = null;
            //this.m_dicLoadedPlugins = null;
            //this.LoadedClassNames = null;
            ProconClient = null;

            this.InvocationTimeoutTimer.Dispose();
        }

        public void Unload()
        {
            UnassignEventHandler();

            this.InvocationTimeoutTimer.Dispose();

            try
            {
                if (AppDomainSandbox != null)
                {
                    AppDomain.Unload(AppDomainSandbox);
                }
            }
            catch (Exception e)
            {
                if (ProconClient != null)
                {
                    WritePluginConsole("^1{0}", e.Message);
                }
            }

            GC.Collect();
        }

        #region Event Assignments

        private void UnassignEventHandler()
        {
            ProconClient.Login -= new PRoConClient.EmptyParamterHandler(m_prcClient_CommandLogin);
            ProconClient.Logout -= new PRoConClient.EmptyParamterHandler(m_prcClient_CommandLogout);
            ProconClient.Game.Quit -= new FrostbiteClient.EmptyParamterHandler(m_prcClient_CommandQuit);

            ProconClient.ConnectionClosed -= new PRoConClient.EmptyParamterHandler(m_prcClient_ConnectionClosed);

            ProconClient.Game.PlayerJoin -= new FrostbiteClient.PlayerEventHandler(m_prcClient_PlayerJoin);
            ProconClient.Game.PlayerLeft -= new FrostbiteClient.PlayerLeaveHandler(m_prcClient_PlayerLeft);
            ProconClient.Game.PlayerDisconnected -= new FrostbiteClient.PlayerDisconnectedHandler(m_prcClient_PlayerDisconnected);
            ProconClient.Game.PlayerAuthenticated -= new FrostbiteClient.PlayerAuthenticatedHandler(m_prcClient_PlayerAuthenticated);
            ProconClient.Game.PlayerKicked -= new FrostbiteClient.PlayerKickedHandler(m_prcClient_PlayerKicked);
            ProconClient.Game.PlayerChangedTeam -= new FrostbiteClient.PlayerTeamChangeHandler(m_prcClient_PlayerChangedTeam);
            ProconClient.Game.PlayerChangedSquad -= new FrostbiteClient.PlayerTeamChangeHandler(m_prcClient_PlayerChangedSquad);
            ProconClient.PlayerKilled -= new PRoConClient.PlayerKilledHandler(m_prcClient_PlayerKilled);

            ProconClient.Game.GlobalChat -= new FrostbiteClient.GlobalChatHandler(m_prcClient_GlobalChat);
            ProconClient.Game.TeamChat -= new FrostbiteClient.TeamChatHandler(m_prcClient_TeamChat);
            ProconClient.Game.SquadChat -= new FrostbiteClient.SquadChatHandler(m_prcClient_SquadChat);
            ProconClient.Game.PlayerChat -= new FrostbiteClient.PlayerChatHandler(m_prcClient_PlayerChat);

            ProconClient.Game.ResponseError -= new FrostbiteClient.ResponseErrorHandler(m_prcClient_ResponseError);
            ProconClient.Game.Version -= new FrostbiteClient.VersionHandler(m_prcClient_Version);
            ProconClient.Game.Help -= new FrostbiteClient.HelpHandler(m_prcClient_Help);

            ProconClient.Game.RunScript -= new FrostbiteClient.RunScriptHandler(m_prcClient_RunScript);
            ProconClient.Game.RunScriptError -= new FrostbiteClient.RunScriptErrorHandler(m_prcClient_RunScriptError);

            ProconClient.Game.PunkbusterMessage -= new FrostbiteClient.PunkbusterMessageHandler(m_prcClient_PunkbusterMessage);
            ProconClient.Game.LoadingLevel -= new FrostbiteClient.LoadingLevelHandler(m_prcClient_LoadingLevel);
            ProconClient.Game.LevelStarted -= new FrostbiteClient.EmptyParamterHandler(m_prcClient_LevelStarted);
            ProconClient.Game.LevelLoaded -= new FrostbiteClient.LevelLoadedHandler(m_prcClient_LevelLoaded);

            ProconClient.Game.ServerInfo -= new FrostbiteClient.ServerInfoHandler(m_prcClient_ServerInfo);
            ProconClient.Game.Yelling -= new FrostbiteClient.YellingHandler(m_prcClient_Yelling);
            ProconClient.Game.Saying -= new FrostbiteClient.SayingHandler(m_prcClient_Saying);

            ProconClient.Game.RunNextRound -= new FrostbiteClient.EmptyParamterHandler(m_prcClient_RunNextLevel);
            ProconClient.Game.CurrentLevel -= new FrostbiteClient.CurrentLevelHandler(m_prcClient_CurrentLevel);
            ProconClient.Game.RestartRound -= new FrostbiteClient.EmptyParamterHandler(m_prcClient_RestartLevel);
            ProconClient.Game.SupportedMaps -= new FrostbiteClient.SupportedMapsHandler(m_prcClient_SupportedMaps);

            ProconClient.Game.PlaylistSet -= new FrostbiteClient.PlaylistSetHandler(m_prcClient_PlaylistSet);
            ProconClient.Game.ListPlaylists -= new FrostbiteClient.ListPlaylistsHandler(m_prcClient_ListPlaylists);

            ProconClient.Game.ListPlayers -= new FrostbiteClient.ListPlayersHandler(m_prcClient_ListPlayers);

            ProconClient.Game.BanListLoad -= new FrostbiteClient.EmptyParamterHandler(m_prcClient_BanListLoad);
            ProconClient.Game.BanListSave -= new FrostbiteClient.EmptyParamterHandler(m_prcClient_BanListSave);
            ProconClient.Game.BanListAdd -= new FrostbiteClient.BanListAddHandler(m_prcClient_BanListAdd);
            ProconClient.Game.BanListRemove -= new FrostbiteClient.BanListRemoveHandler(m_prcClient_BanListRemove);
            ProconClient.Game.BanListClear -= new FrostbiteClient.EmptyParamterHandler(m_prcClient_BanListClear);
            ProconClient.FullBanListList -= new PRoConClient.FullBanListListHandler(m_prcClient_BanListList);

            ProconClient.Game.ReservedSlotsConfigFile -= new FrostbiteClient.ReserverdSlotsConfigFileHandler(m_prcClient_ReservedSlotsConfigFile);
            ProconClient.Game.ReservedSlotsLoad -= new FrostbiteClient.EmptyParamterHandler(m_prcClient_ReservedSlotsLoad);
            ProconClient.Game.ReservedSlotsSave -= new FrostbiteClient.EmptyParamterHandler(m_prcClient_ReservedSlotsSave);
            ProconClient.Game.ReservedSlotsPlayerAdded -= new FrostbiteClient.ReservedSlotsPlayerHandler(m_prcClient_ReservedSlotsPlayerAdded);
            ProconClient.Game.ReservedSlotsPlayerRemoved -= new FrostbiteClient.ReservedSlotsPlayerHandler(m_prcClient_ReservedSlotsPlayerRemoved);
            ProconClient.Game.ReservedSlotsCleared -= new FrostbiteClient.EmptyParamterHandler(m_prcClient_ReservedSlotsCleared);
            ProconClient.Game.ReservedSlotsList -= new FrostbiteClient.ReservedSlotsListHandler(m_prcClient_ReservedSlotsList);

            ProconClient.Game.MapListConfigFile -= new FrostbiteClient.MapListConfigFileHandler(m_prcClient_MapListConfigFile);
            ProconClient.Game.MapListLoad -= new FrostbiteClient.EmptyParamterHandler(m_prcClient_MapListLoad);
            ProconClient.Game.MapListSave -= new FrostbiteClient.EmptyParamterHandler(m_prcClient_MapListSave);
            ProconClient.Game.MapListMapAppended -= new FrostbiteClient.MapListAppendedHandler(m_prcClient_MapListMapAppended);
            ProconClient.Game.MapListNextLevelIndex -= new FrostbiteClient.MapListLevelIndexHandler(m_prcClient_MapListNextLevelIndex);
            ProconClient.Game.MapListGetMapIndices -= new FrostbiteClient.MapListGetMapIndicesHandler(m_prcClient_MapListGetMapIndices);
            ProconClient.Game.MapListGetRounds -= new FrostbiteClient.MapListGetRoundsHandler(m_prcClient_MapListGetRounds);
            ProconClient.Game.MapListMapRemoved -= new FrostbiteClient.MapListLevelIndexHandler(m_prcClient_MapListMapRemoved);
            ProconClient.Game.MapListMapInserted -= new FrostbiteClient.MapListMapInsertedHandler(m_prcClient_MapListMapInserted);
            ProconClient.Game.MapListCleared -= new FrostbiteClient.EmptyParamterHandler(m_prcClient_MapListCleared);
            ProconClient.Game.MapListListed -= new FrostbiteClient.MapListListedHandler(m_prcClient_MapListListed);

            ProconClient.Game.TextChatModerationListAddPlayer -= new FrostbiteClient.TextChatModerationListAddPlayerHandler(Game_TextChatModerationListAddPlayer);
            ProconClient.Game.TextChatModerationListRemovePlayer -= new FrostbiteClient.TextChatModerationListRemovePlayerHandler(Game_TextChatModerationListRemovePlayer);
            ProconClient.Game.TextChatModerationListClear -= new FrostbiteClient.EmptyParamterHandler(Game_TextChatModerationListClear);
            ProconClient.Game.TextChatModerationListSave -= new FrostbiteClient.EmptyParamterHandler(Game_TextChatModerationListSave);
            ProconClient.Game.TextChatModerationListLoad -= new FrostbiteClient.EmptyParamterHandler(Game_TextChatModerationListLoad);
            ProconClient.FullTextChatModerationListList -= new PRoConClient.FullTextChatModerationListListHandler(m_client_FullTextChatModerationListList);

            ProconClient.Game.GamePassword -= new FrostbiteClient.PasswordHandler(m_prcClient_GamePassword);
            ProconClient.Game.Punkbuster -= new FrostbiteClient.IsEnabledHandler(m_prcClient_Punkbuster);
            ProconClient.Game.Hardcore -= new FrostbiteClient.IsEnabledHandler(m_prcClient_Hardcore);
            ProconClient.Game.Ranked -= new FrostbiteClient.IsEnabledHandler(m_prcClient_Ranked);
            ProconClient.Game.RankLimit -= new FrostbiteClient.LimitHandler(m_prcClient_RankLimit);
            ProconClient.Game.TeamBalance -= new FrostbiteClient.IsEnabledHandler(m_prcClient_TeamBalance);
            ProconClient.Game.FriendlyFire -= new FrostbiteClient.IsEnabledHandler(m_prcClient_FriendlyFire);
            ProconClient.Game.MaxPlayerLimit -= new FrostbiteClient.LimitHandler(m_prcClient_MaxPlayerLimit);
            ProconClient.Game.CurrentPlayerLimit -= new FrostbiteClient.LimitHandler(m_prcClient_CurrentPlayerLimit);
            ProconClient.Game.PlayerLimit -= new FrostbiteClient.LimitHandler(m_prcClient_PlayerLimit);
            ProconClient.Game.BannerUrl -= new FrostbiteClient.BannerUrlHandler(m_prcClient_BannerUrl);
            ProconClient.Game.ServerDescription -= new FrostbiteClient.ServerDescriptionHandler(m_prcClient_ServerDescription);
            ProconClient.Game.ServerMessage -= new FrostbiteClient.ServerMessageHandler(m_prcClient_ServerMessage);
            ProconClient.Game.KillCam -= new FrostbiteClient.IsEnabledHandler(m_prcClient_KillCam);
            ProconClient.Game.MiniMap -= new FrostbiteClient.IsEnabledHandler(m_prcClient_MiniMap);
            ProconClient.Game.CrossHair -= new FrostbiteClient.IsEnabledHandler(m_prcClient_CrossHair);
            ProconClient.Game.ThreeDSpotting -= new FrostbiteClient.IsEnabledHandler(m_prcClient_ThreeDSpotting);
            ProconClient.Game.MiniMapSpotting -= new FrostbiteClient.IsEnabledHandler(m_prcClient_MiniMapSpotting);
            ProconClient.Game.ThirdPersonVehicleCameras -= new FrostbiteClient.IsEnabledHandler(m_prcClient_ThirdPersonVehicleCameras);

            // BF3
            ProconClient.Game.RoundRestartPlayerCount -= new FrostbiteClient.LimitHandler(m_prcClient_RoundRestartPlayerCount);
            ProconClient.Game.RoundStartPlayerCount -= new FrostbiteClient.LimitHandler(m_prcClient_RoundStartPlayerCount);
            ProconClient.Game.GameModeCounter -= new FrostbiteClient.LimitHandler(m_prcClient_GameModeCounter);
            ProconClient.Game.CtfRoundTimeModifier -= new FrostbiteClient.LimitHandler(m_prcClient_CtfRoundTimeModifier);
            ProconClient.Game.RoundTimeLimit -= new FrostbiteClient.LimitHandler(m_prcClient_RoundTimeLimit);
            ProconClient.Game.TicketBleedRate -= new FrostbiteClient.LimitHandler(m_prcClient_TicketBleedRate);

            ProconClient.Game.TextChatModerationMode -= new FrostbiteClient.TextChatModerationModeHandler(Game_TextChatModerationMode);
            ProconClient.Game.TextChatSpamCoolDownTime -= new FrostbiteClient.LimitHandler(Game_TextChatSpamCoolDownTime);
            ProconClient.Game.TextChatSpamDetectionTime -= new FrostbiteClient.LimitHandler(Game_TextChatSpamDetectionTime);
            ProconClient.Game.TextChatSpamTriggerCount -= new FrostbiteClient.LimitHandler(Game_TextChatSpamTriggerCount);

            ProconClient.Game.UnlockMode -= new FrostbiteClient.UnlockModeHandler(m_prcClient_UnlockMode);
            ProconClient.Game.BF4preset -= new FrostbiteClient.BF4presetHandler(m_prcClient_BF4preset);
            ProconClient.Game.GunMasterWeaponsPreset -= new FrostbiteClient.GunMasterWeaponsPresetHandler(Game_GunMasterWeaponsPreset);

            ProconClient.Game.ReservedSlotsListAggressiveJoin -= new FrostbiteClient.IsEnabledHandler(Game_ReservedSlotsListAggressiveJoin);
            ProconClient.Game.RoundLockdownCountdown -= new FrostbiteClient.LimitHandler(Game_RoundLockdownCountdown);
            ProconClient.Game.RoundWarmupTimeout -= new FrostbiteClient.LimitHandler(Game_RoundWarmupTimeout);

            ProconClient.Game.PremiumStatus -= new FrostbiteClient.IsEnabledHandler(Game_PremiumStatus);

            ProconClient.Game.VehicleSpawnAllowed -= new FrostbiteClient.IsEnabledHandler(Game_VehicleSpawnAllowed);
            ProconClient.Game.VehicleSpawnDelay -= new FrostbiteClient.LimitHandler(Game_VehicleSpawnDelay);
            ProconClient.Game.BulletDamage -= new FrostbiteClient.LimitHandler(Game_BulletDamage);
            ProconClient.Game.OnlySquadLeaderSpawn -= new FrostbiteClient.IsEnabledHandler(Game_OnlySquadLeaderSpawn);
            ProconClient.Game.SoldierHealth -= new FrostbiteClient.LimitHandler(Game_SoldierHealth);
            ProconClient.Game.PlayerManDownTime -= new FrostbiteClient.LimitHandler(Game_PlayerManDownTime);
            ProconClient.Game.PlayerRespawnTime -= new FrostbiteClient.LimitHandler(Game_PlayerRespawnTime);
            ProconClient.Game.Hud -= new FrostbiteClient.IsEnabledHandler(Game_Hud);
            ProconClient.Game.NameTag -= new FrostbiteClient.IsEnabledHandler(Game_NameTag);

            ProconClient.Game.PlayerIdleState -= new FrostbiteClient.PlayerIdleStateHandler(Game_PlayerIdleState);
            ProconClient.Game.PlayerIsAlive -= new FrostbiteClient.PlayerIsAliveHandler(Game_PlayerIsAlive);
            ProconClient.Game.PlayerPingedByAdmin -= new FrostbiteClient.PlayerPingedByAdminHandler(Game_PlayerPingedByAdmin);

            ProconClient.Game.SquadLeader -= new FrostbiteClient.SquadLeaderHandler(Game_SquadLeader);
            ProconClient.Game.SquadListActive -= new FrostbiteClient.SquadListActiveHandler(Game_SquadListActive);
            ProconClient.Game.SquadListPlayers -= new FrostbiteClient.SquadListPlayersHandler(Game_SquadListPlayers);
            ProconClient.Game.SquadIsPrivate -= new FrostbiteClient.SquadIsPrivateHandler(Game_SquadIsPrivate);

            ProconClient.Game.TeamFactionOverride -= new FrostbiteClient.TeamFactionOverrideHandler(Game_TeamFactionOverride);

            #region MoHW

            ProconClient.Game.AllUnlocksUnlocked -= new FrostbiteClient.IsEnabledHandler(Game_AllUnlocksUnlocked);
            ProconClient.Game.BuddyOutline -= new FrostbiteClient.IsEnabledHandler(Game_BuddyOutline);
            ProconClient.Game.HudBuddyInfo -= new FrostbiteClient.IsEnabledHandler(Game_HudBuddyInfo);
            ProconClient.Game.HudClassAbility -= new FrostbiteClient.IsEnabledHandler(Game_HudClassAbility);
            ProconClient.Game.HudCrosshair -= new FrostbiteClient.IsEnabledHandler(Game_HudCrosshair);
            ProconClient.Game.HudEnemyTag -= new FrostbiteClient.IsEnabledHandler(Game_HudEnemyTag);
            ProconClient.Game.HudExplosiveIcons -= new FrostbiteClient.IsEnabledHandler(Game_HudExplosiveIcons);
            ProconClient.Game.HudGameMode -= new FrostbiteClient.IsEnabledHandler(Game_HudGameMode);
            ProconClient.Game.HudHealthAmmo -= new FrostbiteClient.IsEnabledHandler(Game_HudHealthAmmo);
            ProconClient.Game.HudMinimap -= new FrostbiteClient.IsEnabledHandler(Game_HudMinimap);
            ProconClient.Game.HudObiturary -= new FrostbiteClient.IsEnabledHandler(Game_HudObiturary);
            ProconClient.Game.HudPointsTracker -= new FrostbiteClient.IsEnabledHandler(Game_HudPointsTracker);
            ProconClient.Game.HudUnlocks -= new FrostbiteClient.IsEnabledHandler(Game_HudUnlocks);
            ProconClient.Game.Playlist -= new FrostbiteClient.PlaylistSetHandler(Game_Playlist);

            #endregion

            // R13
            ProconClient.Game.ServerName -= new FrostbiteClient.ServerNameHandler(m_prcClient_ServerName);
            ProconClient.Game.TeamKillCountForKick -= new FrostbiteClient.LimitHandler(m_prcClient_TeamKillCountForKick);
            ProconClient.Game.TeamKillKickForBan -= new FrostbiteClient.LimitHandler(m_prcClient_TeamKillKickForBan);
            ProconClient.Game.TeamKillValueIncrease -= new FrostbiteClient.LimitHandler(m_prcClient_TeamKillValueIncrease);
            ProconClient.Game.TeamKillValueDecreasePerSecond -= new FrostbiteClient.LimitHandler(m_prcClient_TeamKillValueDecreasePerSecond);
            ProconClient.Game.TeamKillValueForKick -= new FrostbiteClient.LimitHandler(m_prcClient_TeamKillValueForKick);
            ProconClient.Game.IdleTimeout -= new FrostbiteClient.LimitHandler(m_prcClient_IdleTimeout);
            ProconClient.Game.IdleBanRounds -= new FrostbiteClient.LimitHandler(m_prcClient_IdleBanRounds);
            ProconClient.Game.ProfanityFilter -= new FrostbiteClient.IsEnabledHandler(m_prcClient_ProfanityFilter);

            ProconClient.PlayerSpawned -= new PRoConClient.PlayerSpawnedHandler(m_prcClient_PlayerSpawned);
            ProconClient.Game.RoundOver -= new FrostbiteClient.RoundOverHandler(m_prcClient_RoundOver);
            ProconClient.Game.RoundOverPlayers -= new FrostbiteClient.RoundOverPlayersHandler(m_prcClient_RoundOverPlayers);
            ProconClient.Game.RoundOverTeamScores -= new FrostbiteClient.RoundOverTeamScoresHandler(m_prcClient_RoundOverTeamScores);
            ProconClient.Game.EndRound -= new FrostbiteClient.EndRoundHandler(m_prcClient_EndRound);

            ProconClient.Game.LevelVariablesGet -= new FrostbiteClient.LevelVariableGetHandler(m_prcClient_LevelVariablesGet);
            ProconClient.Game.LevelVariablesSet -= new FrostbiteClient.LevelVariableHandler(m_prcClient_LevelVariablesSet);
            ProconClient.Game.LevelVariablesClear -= new FrostbiteClient.LevelVariableHandler(m_prcClient_LevelVariablesClear);
            ProconClient.Game.LevelVariablesEvaluate -= new FrostbiteClient.LevelVariableGetHandler(m_prcClient_LevelVariablesEvaluate);
            ProconClient.Game.LevelVariablesList -= new FrostbiteClient.LevelVariableListHandler(m_prcClient_LevelVariablesList);

            ProconClient.ReceiveProconVariable -= new PRoConClient.ReceiveProconVariableHandler(m_prcClient_ReceiveProconVariable);

            ProconClient.PunkbusterBeginPlayerInfo -= new PRoConClient.EmptyParamterHandler(m_client_PunkbusterBeginPlayerInfo);
            ProconClient.PunkbusterEndPlayerInfo -= new PRoConClient.EmptyParamterHandler(m_client_PunkbusterEndPlayerInfo);
            ProconClient.PunkbusterPlayerInfo -= new PRoConClient.PunkbusterPlayerInfoHandler(m_prcClient_PunkbusterPlayerInfo);
            ProconClient.PunkbusterPlayerBanned -= new PRoConClient.PunkbusterBanHandler(m_prcClient_PunkbusterPlayerBanned);
            ProconClient.PunkbusterPlayerUnbanned -= new PRoConClient.PunkbusterBanHandler(m_prcClient_PunkbusterPlayerUnbanned);

            ProconClient.Layer.AccountPrivileges.AccountPrivilegeAdded -= new AccountPrivilegeDictionary.AccountPrivilegeAlteredHandler(AccountPrivileges_AccountPrivilegeAdded);
            ProconClient.Layer.AccountPrivileges.AccountPrivilegeRemoved -= new AccountPrivilegeDictionary.AccountPrivilegeAlteredHandler(AccountPrivileges_AccountPrivilegeRemoved);

            ProconClient.MapGeometry.MapZoneTrespassed -= new MapGeometry.MapZoneTrespassedHandler(MapGeometry_MapZoneTrespassed);

            #region Admin actions on players

            ProconClient.Game.PlayerKickedByAdmin -= new FrostbiteClient.PlayerKickedHandler(Game_PlayerKickedByAdmin);
            ProconClient.Game.PlayerKilledByAdmin -= new FrostbiteClient.PlayerKilledByAdminHandler(Game_PlayerKilledByAdmin);
            ProconClient.Game.PlayerMovedByAdmin -= new FrostbiteClient.PlayerMovedByAdminHandler(Game_PlayerMovedByAdmin);

            #endregion

            #region Layer Accounts

            foreach (ILayerClient client in new List<ILayerClient>(ProconClient.Layer.Clients.Values))
            {
                client.Login -= Layer_LayerClientLogin;
                client.Logout -= Layer_LayerClientLogout;
            }
            ProconClient.Layer.ClientConnected -= Layer_ClientConnected;

            #region BF4

            ProconClient.Game.SpectatorListCleared -= new FrostbiteClient.EmptyParamterHandler(Game_SpectatorListCleared);
            ProconClient.Game.SpectatorListList -= new FrostbiteClient.SpectatorListListHandler(Game_SpectatorListList);
            ProconClient.Game.SpectatorListLoad -= new FrostbiteClient.EmptyParamterHandler(Game_SpectatorListLoad);
            ProconClient.Game.SpectatorListPlayerAdded -= new FrostbiteClient.SpectatorListPlayerHandler(Game_SpectatorListPlayerAdded);
            ProconClient.Game.SpectatorListPlayerRemoved -= new FrostbiteClient.SpectatorListPlayerHandler(Game_SpectatorListPlayerRemoved);
            ProconClient.Game.SpectatorListSave -= new FrostbiteClient.EmptyParamterHandler(Game_SpectatorListSave);

            ProconClient.Game.GameAdminCleared -= new FrostbiteClient.EmptyParamterHandler(Game_GameAdminCleared);
            ProconClient.Game.GameAdminList -= new FrostbiteClient.GameAdminListHandler(Game_GameAdminList);
            ProconClient.Game.GameAdminLoad -= new FrostbiteClient.EmptyParamterHandler(Game_GameAdminLoad);
            ProconClient.Game.GameAdminPlayerAdded -= new FrostbiteClient.GameAdminPlayerHandler(Game_GameAdminPlayerAdded);
            ProconClient.Game.GameAdminPlayerRemoved -= new FrostbiteClient.GameAdminPlayerHandler(Game_GameAdminPlayerRemoved);
            ProconClient.Game.GameAdminSave -= new FrostbiteClient.EmptyParamterHandler(Game_GameAdminSave);

            ProconClient.Game.MaxSpectators -= new FrostbiteClient.LimitHandler(Game_MaxSpectators);

            ProconClient.Game.FairFight -= new FrostbiteClient.IsEnabledHandler(Game_FairFight);

            ProconClient.Game.IsHitIndicator -= new FrostbiteClient.IsEnabledHandler(Game_IsHitIndicator);

            ProconClient.Game.IsCommander -= new FrostbiteClient.IsEnabledHandler(Game_IsCommander);
            ProconClient.Game.AlwaysAllowSpectators -= new FrostbiteClient.IsEnabledHandler(Game_AlwaysAllowSpectators);
            ProconClient.Game.IsForceReloadWholeMags -= new FrostbiteClient.IsEnabledHandler(Game_IsForceReloadWholeMags);
            ProconClient.Game.ServerType -= new FrostbiteClient.VarsStringHandler(Game_ServerType);

            #endregion

            #endregion

            #region Battlefield: Hardline

            ProconClient.Game.RoundStartReadyPlayersNeeded -= Game_RoundStartReadyPlayersNeeded;

            #endregion
        }

        private void AssignEventHandler()
        {
            ProconClient.Login += new PRoConClient.EmptyParamterHandler(m_prcClient_CommandLogin);
            ProconClient.Logout += new PRoConClient.EmptyParamterHandler(m_prcClient_CommandLogout);
            ProconClient.Game.Quit += new FrostbiteClient.EmptyParamterHandler(m_prcClient_CommandQuit);

            ProconClient.ConnectionClosed += new PRoConClient.EmptyParamterHandler(m_prcClient_ConnectionClosed);

            ProconClient.Game.PlayerJoin += new FrostbiteClient.PlayerEventHandler(m_prcClient_PlayerJoin);
            ProconClient.Game.PlayerLeft += new FrostbiteClient.PlayerLeaveHandler(m_prcClient_PlayerLeft);
            ProconClient.Game.PlayerDisconnected += new FrostbiteClient.PlayerDisconnectedHandler(m_prcClient_PlayerDisconnected);
            ProconClient.Game.PlayerAuthenticated += new FrostbiteClient.PlayerAuthenticatedHandler(m_prcClient_PlayerAuthenticated);
            ProconClient.Game.PlayerKicked += new FrostbiteClient.PlayerKickedHandler(m_prcClient_PlayerKicked);
            ProconClient.Game.PlayerChangedTeam += new FrostbiteClient.PlayerTeamChangeHandler(m_prcClient_PlayerChangedTeam);
            ProconClient.Game.PlayerChangedSquad += new FrostbiteClient.PlayerTeamChangeHandler(m_prcClient_PlayerChangedSquad);
            ProconClient.PlayerKilled += new PRoConClient.PlayerKilledHandler(m_prcClient_PlayerKilled);

            ProconClient.Game.GlobalChat += new FrostbiteClient.GlobalChatHandler(m_prcClient_GlobalChat);
            ProconClient.Game.TeamChat += new FrostbiteClient.TeamChatHandler(m_prcClient_TeamChat);
            ProconClient.Game.SquadChat += new FrostbiteClient.SquadChatHandler(m_prcClient_SquadChat);
            ProconClient.Game.PlayerChat += new FrostbiteClient.PlayerChatHandler(m_prcClient_PlayerChat);

            ProconClient.Game.ResponseError += new FrostbiteClient.ResponseErrorHandler(m_prcClient_ResponseError);
            ProconClient.Game.Version += new FrostbiteClient.VersionHandler(m_prcClient_Version);
            ProconClient.Game.Help += new FrostbiteClient.HelpHandler(m_prcClient_Help);

            ProconClient.Game.RunScript += new FrostbiteClient.RunScriptHandler(m_prcClient_RunScript);
            ProconClient.Game.RunScriptError += new FrostbiteClient.RunScriptErrorHandler(m_prcClient_RunScriptError);

            ProconClient.Game.PunkbusterMessage += new FrostbiteClient.PunkbusterMessageHandler(m_prcClient_PunkbusterMessage);
            ProconClient.Game.LoadingLevel += new FrostbiteClient.LoadingLevelHandler(m_prcClient_LoadingLevel);
            ProconClient.Game.LevelStarted += new FrostbiteClient.EmptyParamterHandler(m_prcClient_LevelStarted);
            ProconClient.Game.LevelLoaded += new FrostbiteClient.LevelLoadedHandler(m_prcClient_LevelLoaded);

            ProconClient.Game.ServerInfo += new FrostbiteClient.ServerInfoHandler(m_prcClient_ServerInfo);
            ProconClient.Game.Yelling += new FrostbiteClient.YellingHandler(m_prcClient_Yelling);
            ProconClient.Game.Saying += new FrostbiteClient.SayingHandler(m_prcClient_Saying);

            ProconClient.Game.RunNextRound += new FrostbiteClient.EmptyParamterHandler(m_prcClient_RunNextLevel);
            ProconClient.Game.CurrentLevel += new FrostbiteClient.CurrentLevelHandler(m_prcClient_CurrentLevel);
            ProconClient.Game.RestartRound += new FrostbiteClient.EmptyParamterHandler(m_prcClient_RestartLevel);
            ProconClient.Game.SupportedMaps += new FrostbiteClient.SupportedMapsHandler(m_prcClient_SupportedMaps);

            ProconClient.Game.PlaylistSet += new FrostbiteClient.PlaylistSetHandler(m_prcClient_PlaylistSet);
            ProconClient.Game.ListPlaylists += new FrostbiteClient.ListPlaylistsHandler(m_prcClient_ListPlaylists);

            ProconClient.Game.ListPlayers += new FrostbiteClient.ListPlayersHandler(m_prcClient_ListPlayers);

            ProconClient.Game.BanListLoad += new FrostbiteClient.EmptyParamterHandler(m_prcClient_BanListLoad);
            ProconClient.Game.BanListSave += new FrostbiteClient.EmptyParamterHandler(m_prcClient_BanListSave);
            ProconClient.Game.BanListAdd += new FrostbiteClient.BanListAddHandler(m_prcClient_BanListAdd);
            ProconClient.Game.BanListRemove += new FrostbiteClient.BanListRemoveHandler(m_prcClient_BanListRemove);
            ProconClient.Game.BanListClear += new FrostbiteClient.EmptyParamterHandler(m_prcClient_BanListClear);
            ProconClient.FullBanListList += new PRoConClient.FullBanListListHandler(m_prcClient_BanListList);

            ProconClient.Game.ReservedSlotsConfigFile += new FrostbiteClient.ReserverdSlotsConfigFileHandler(m_prcClient_ReservedSlotsConfigFile);
            ProconClient.Game.ReservedSlotsLoad += new FrostbiteClient.EmptyParamterHandler(m_prcClient_ReservedSlotsLoad);
            ProconClient.Game.ReservedSlotsSave += new FrostbiteClient.EmptyParamterHandler(m_prcClient_ReservedSlotsSave);
            ProconClient.Game.ReservedSlotsPlayerAdded += new FrostbiteClient.ReservedSlotsPlayerHandler(m_prcClient_ReservedSlotsPlayerAdded);
            ProconClient.Game.ReservedSlotsPlayerRemoved += new FrostbiteClient.ReservedSlotsPlayerHandler(m_prcClient_ReservedSlotsPlayerRemoved);
            ProconClient.Game.ReservedSlotsCleared += new FrostbiteClient.EmptyParamterHandler(m_prcClient_ReservedSlotsCleared);
            ProconClient.Game.ReservedSlotsList += new FrostbiteClient.ReservedSlotsListHandler(m_prcClient_ReservedSlotsList);

            ProconClient.Game.MapListConfigFile += new FrostbiteClient.MapListConfigFileHandler(m_prcClient_MapListConfigFile);
            ProconClient.Game.MapListLoad += new FrostbiteClient.EmptyParamterHandler(m_prcClient_MapListLoad);
            ProconClient.Game.MapListSave += new FrostbiteClient.EmptyParamterHandler(m_prcClient_MapListSave);
            ProconClient.Game.MapListMapAppended += new FrostbiteClient.MapListAppendedHandler(m_prcClient_MapListMapAppended);
            ProconClient.Game.MapListNextLevelIndex += new FrostbiteClient.MapListLevelIndexHandler(m_prcClient_MapListNextLevelIndex);
            ProconClient.Game.MapListGetMapIndices += new FrostbiteClient.MapListGetMapIndicesHandler(m_prcClient_MapListGetMapIndices);
            ProconClient.Game.MapListGetRounds += new FrostbiteClient.MapListGetRoundsHandler(m_prcClient_MapListGetRounds);
            ProconClient.Game.MapListMapRemoved += new FrostbiteClient.MapListLevelIndexHandler(m_prcClient_MapListMapRemoved);
            ProconClient.Game.MapListMapInserted += new FrostbiteClient.MapListMapInsertedHandler(m_prcClient_MapListMapInserted);
            ProconClient.Game.MapListCleared += new FrostbiteClient.EmptyParamterHandler(m_prcClient_MapListCleared);
            ProconClient.Game.MapListListed += new FrostbiteClient.MapListListedHandler(m_prcClient_MapListListed);

            ProconClient.Game.TextChatModerationListAddPlayer += new FrostbiteClient.TextChatModerationListAddPlayerHandler(Game_TextChatModerationListAddPlayer);
            ProconClient.Game.TextChatModerationListRemovePlayer += new FrostbiteClient.TextChatModerationListRemovePlayerHandler(Game_TextChatModerationListRemovePlayer);
            ProconClient.Game.TextChatModerationListClear += new FrostbiteClient.EmptyParamterHandler(Game_TextChatModerationListClear);
            ProconClient.Game.TextChatModerationListSave += new FrostbiteClient.EmptyParamterHandler(Game_TextChatModerationListSave);
            ProconClient.Game.TextChatModerationListLoad += new FrostbiteClient.EmptyParamterHandler(Game_TextChatModerationListLoad);
            ProconClient.FullTextChatModerationListList += new PRoConClient.FullTextChatModerationListListHandler(m_client_FullTextChatModerationListList);

            ProconClient.Game.GamePassword += new FrostbiteClient.PasswordHandler(m_prcClient_GamePassword);
            ProconClient.Game.Punkbuster += new FrostbiteClient.IsEnabledHandler(m_prcClient_Punkbuster);
            ProconClient.Game.Hardcore += new FrostbiteClient.IsEnabledHandler(m_prcClient_Hardcore);
            ProconClient.Game.Ranked += new FrostbiteClient.IsEnabledHandler(m_prcClient_Ranked);
            ProconClient.Game.RankLimit += new FrostbiteClient.LimitHandler(m_prcClient_RankLimit);
            ProconClient.Game.TeamBalance += new FrostbiteClient.IsEnabledHandler(m_prcClient_TeamBalance);
            ProconClient.Game.FriendlyFire += new FrostbiteClient.IsEnabledHandler(m_prcClient_FriendlyFire);
            ProconClient.Game.MaxPlayerLimit += new FrostbiteClient.LimitHandler(m_prcClient_MaxPlayerLimit);
            ProconClient.Game.CurrentPlayerLimit += new FrostbiteClient.LimitHandler(m_prcClient_CurrentPlayerLimit);
            ProconClient.Game.PlayerLimit += new FrostbiteClient.LimitHandler(m_prcClient_PlayerLimit);
            ProconClient.Game.BannerUrl += new FrostbiteClient.BannerUrlHandler(m_prcClient_BannerUrl);
            ProconClient.Game.ServerDescription += new FrostbiteClient.ServerDescriptionHandler(m_prcClient_ServerDescription);
            ProconClient.Game.ServerMessage += new FrostbiteClient.ServerMessageHandler(m_prcClient_ServerMessage);
            ProconClient.Game.KillCam += new FrostbiteClient.IsEnabledHandler(m_prcClient_KillCam);
            ProconClient.Game.MiniMap += new FrostbiteClient.IsEnabledHandler(m_prcClient_MiniMap);
            ProconClient.Game.CrossHair += new FrostbiteClient.IsEnabledHandler(m_prcClient_CrossHair);
            ProconClient.Game.ThreeDSpotting += new FrostbiteClient.IsEnabledHandler(m_prcClient_ThreeDSpotting);
            ProconClient.Game.MiniMapSpotting += new FrostbiteClient.IsEnabledHandler(m_prcClient_MiniMapSpotting);
            ProconClient.Game.ThirdPersonVehicleCameras += new FrostbiteClient.IsEnabledHandler(m_prcClient_ThirdPersonVehicleCameras);

            // BF3
            ProconClient.Game.RoundRestartPlayerCount += new FrostbiteClient.LimitHandler(m_prcClient_RoundRestartPlayerCount);
            ProconClient.Game.RoundStartPlayerCount += new FrostbiteClient.LimitHandler(m_prcClient_RoundStartPlayerCount);
            ProconClient.Game.GameModeCounter += new FrostbiteClient.LimitHandler(m_prcClient_GameModeCounter);
            ProconClient.Game.CtfRoundTimeModifier += new FrostbiteClient.LimitHandler(m_prcClient_CtfRoundTimeModifier);
            ProconClient.Game.RoundTimeLimit += new FrostbiteClient.LimitHandler(m_prcClient_RoundTimeLimit);
            ProconClient.Game.TicketBleedRate += new FrostbiteClient.LimitHandler(m_prcClient_TicketBleedRate);

            ProconClient.Game.TextChatModerationMode += new FrostbiteClient.TextChatModerationModeHandler(Game_TextChatModerationMode);
            ProconClient.Game.TextChatSpamCoolDownTime += new FrostbiteClient.LimitHandler(Game_TextChatSpamCoolDownTime);
            ProconClient.Game.TextChatSpamDetectionTime += new FrostbiteClient.LimitHandler(Game_TextChatSpamDetectionTime);
            ProconClient.Game.TextChatSpamTriggerCount += new FrostbiteClient.LimitHandler(Game_TextChatSpamTriggerCount);

            ProconClient.Game.UnlockMode += new FrostbiteClient.UnlockModeHandler(m_prcClient_UnlockMode);
            ProconClient.Game.BF4preset += new FrostbiteClient.BF4presetHandler(m_prcClient_BF4preset);
            ProconClient.Game.GunMasterWeaponsPreset += new FrostbiteClient.GunMasterWeaponsPresetHandler(Game_GunMasterWeaponsPreset);

            ProconClient.Game.ReservedSlotsListAggressiveJoin += new FrostbiteClient.IsEnabledHandler(Game_ReservedSlotsListAggressiveJoin);
            ProconClient.Game.RoundLockdownCountdown += new FrostbiteClient.LimitHandler(Game_RoundLockdownCountdown);
            ProconClient.Game.RoundWarmupTimeout += new FrostbiteClient.LimitHandler(Game_RoundWarmupTimeout);

            ProconClient.Game.PremiumStatus += new FrostbiteClient.IsEnabledHandler(Game_PremiumStatus);

            ProconClient.Game.VehicleSpawnAllowed += new FrostbiteClient.IsEnabledHandler(Game_VehicleSpawnAllowed);
            ProconClient.Game.VehicleSpawnDelay += new FrostbiteClient.LimitHandler(Game_VehicleSpawnDelay);
            ProconClient.Game.BulletDamage += new FrostbiteClient.LimitHandler(Game_BulletDamage);
            ProconClient.Game.OnlySquadLeaderSpawn += new FrostbiteClient.IsEnabledHandler(Game_OnlySquadLeaderSpawn);
            ProconClient.Game.SoldierHealth += new FrostbiteClient.LimitHandler(Game_SoldierHealth);
            ProconClient.Game.PlayerManDownTime += new FrostbiteClient.LimitHandler(Game_PlayerManDownTime);
            ProconClient.Game.PlayerRespawnTime += new FrostbiteClient.LimitHandler(Game_PlayerRespawnTime);
            ProconClient.Game.Hud += new FrostbiteClient.IsEnabledHandler(Game_Hud);
            ProconClient.Game.NameTag += new FrostbiteClient.IsEnabledHandler(Game_NameTag);

            ProconClient.Game.PlayerIdleState += new FrostbiteClient.PlayerIdleStateHandler(Game_PlayerIdleState);
            ProconClient.Game.PlayerIsAlive += new FrostbiteClient.PlayerIsAliveHandler(Game_PlayerIsAlive);
            ProconClient.Game.PlayerPingedByAdmin += new FrostbiteClient.PlayerPingedByAdminHandler(Game_PlayerPingedByAdmin);

            ProconClient.Game.SquadLeader += new FrostbiteClient.SquadLeaderHandler(Game_SquadLeader);
            ProconClient.Game.SquadListActive += new FrostbiteClient.SquadListActiveHandler(Game_SquadListActive);
            ProconClient.Game.SquadListPlayers += new FrostbiteClient.SquadListPlayersHandler(Game_SquadListPlayers);
            ProconClient.Game.SquadIsPrivate += new FrostbiteClient.SquadIsPrivateHandler(Game_SquadIsPrivate);

            #region MoHW

            ProconClient.Game.AllUnlocksUnlocked += new FrostbiteClient.IsEnabledHandler(Game_AllUnlocksUnlocked);
            ProconClient.Game.BuddyOutline += new FrostbiteClient.IsEnabledHandler(Game_BuddyOutline);
            ProconClient.Game.HudBuddyInfo += new FrostbiteClient.IsEnabledHandler(Game_HudBuddyInfo);
            ProconClient.Game.HudClassAbility += new FrostbiteClient.IsEnabledHandler(Game_HudClassAbility);
            ProconClient.Game.HudCrosshair += new FrostbiteClient.IsEnabledHandler(Game_HudCrosshair);
            ProconClient.Game.HudEnemyTag += new FrostbiteClient.IsEnabledHandler(Game_HudEnemyTag);
            ProconClient.Game.HudExplosiveIcons += new FrostbiteClient.IsEnabledHandler(Game_HudExplosiveIcons);
            ProconClient.Game.HudGameMode += new FrostbiteClient.IsEnabledHandler(Game_HudGameMode);
            ProconClient.Game.HudHealthAmmo += new FrostbiteClient.IsEnabledHandler(Game_HudHealthAmmo);
            ProconClient.Game.HudMinimap += new FrostbiteClient.IsEnabledHandler(Game_HudMinimap);
            ProconClient.Game.HudObiturary += new FrostbiteClient.IsEnabledHandler(Game_HudObiturary);
            ProconClient.Game.HudPointsTracker += new FrostbiteClient.IsEnabledHandler(Game_HudPointsTracker);
            ProconClient.Game.HudUnlocks += new FrostbiteClient.IsEnabledHandler(Game_HudUnlocks);
            ProconClient.Game.Playlist += new FrostbiteClient.PlaylistSetHandler(Game_Playlist);

            #endregion

            // R13
            ProconClient.Game.ServerName += new FrostbiteClient.ServerNameHandler(m_prcClient_ServerName);
            ProconClient.Game.TeamKillCountForKick += new FrostbiteClient.LimitHandler(m_prcClient_TeamKillCountForKick);
            ProconClient.Game.TeamKillKickForBan += new FrostbiteClient.LimitHandler(m_prcClient_TeamKillKickForBan);
            ProconClient.Game.TeamKillValueIncrease += new FrostbiteClient.LimitHandler(m_prcClient_TeamKillValueIncrease);
            ProconClient.Game.TeamKillValueDecreasePerSecond += new FrostbiteClient.LimitHandler(m_prcClient_TeamKillValueDecreasePerSecond);
            ProconClient.Game.TeamKillValueForKick += new FrostbiteClient.LimitHandler(m_prcClient_TeamKillValueForKick);
            ProconClient.Game.IdleTimeout += new FrostbiteClient.LimitHandler(m_prcClient_IdleTimeout);
            ProconClient.Game.IdleBanRounds += new FrostbiteClient.LimitHandler(m_prcClient_IdleBanRounds);
            ProconClient.Game.ProfanityFilter += new FrostbiteClient.IsEnabledHandler(m_prcClient_ProfanityFilter);

            ProconClient.PlayerSpawned += new PRoConClient.PlayerSpawnedHandler(m_prcClient_PlayerSpawned);
            ProconClient.Game.RoundOver += new FrostbiteClient.RoundOverHandler(m_prcClient_RoundOver);
            ProconClient.Game.RoundOverPlayers += new FrostbiteClient.RoundOverPlayersHandler(m_prcClient_RoundOverPlayers);
            ProconClient.Game.RoundOverTeamScores += new FrostbiteClient.RoundOverTeamScoresHandler(m_prcClient_RoundOverTeamScores);
            ProconClient.Game.EndRound += new FrostbiteClient.EndRoundHandler(m_prcClient_EndRound);

            ProconClient.Game.LevelVariablesGet += new FrostbiteClient.LevelVariableGetHandler(m_prcClient_LevelVariablesGet);
            ProconClient.Game.LevelVariablesSet += new FrostbiteClient.LevelVariableHandler(m_prcClient_LevelVariablesSet);
            ProconClient.Game.LevelVariablesClear += new FrostbiteClient.LevelVariableHandler(m_prcClient_LevelVariablesClear);
            ProconClient.Game.LevelVariablesEvaluate += new FrostbiteClient.LevelVariableGetHandler(m_prcClient_LevelVariablesEvaluate);
            ProconClient.Game.LevelVariablesList += new FrostbiteClient.LevelVariableListHandler(m_prcClient_LevelVariablesList);

            ProconClient.ReceiveProconVariable += new PRoConClient.ReceiveProconVariableHandler(m_prcClient_ReceiveProconVariable);

            ProconClient.PunkbusterBeginPlayerInfo += new PRoConClient.EmptyParamterHandler(m_client_PunkbusterBeginPlayerInfo);
            ProconClient.PunkbusterEndPlayerInfo += new PRoConClient.EmptyParamterHandler(m_client_PunkbusterEndPlayerInfo);
            ProconClient.PunkbusterPlayerInfo += new PRoConClient.PunkbusterPlayerInfoHandler(m_prcClient_PunkbusterPlayerInfo);
            ProconClient.PunkbusterPlayerBanned += new PRoConClient.PunkbusterBanHandler(m_prcClient_PunkbusterPlayerBanned);
            ProconClient.PunkbusterPlayerUnbanned += new PRoConClient.PunkbusterBanHandler(m_prcClient_PunkbusterPlayerUnbanned);

            ProconClient.Layer.AccountPrivileges.AccountPrivilegeAdded += new AccountPrivilegeDictionary.AccountPrivilegeAlteredHandler(AccountPrivileges_AccountPrivilegeAdded);
            ProconClient.Layer.AccountPrivileges.AccountPrivilegeRemoved += new AccountPrivilegeDictionary.AccountPrivilegeAlteredHandler(AccountPrivileges_AccountPrivilegeRemoved);

            ProconClient.MapGeometry.MapZoneTrespassed += new MapGeometry.MapZoneTrespassedHandler(MapGeometry_MapZoneTrespassed);

            #region Admin actions on players

            ProconClient.Game.PlayerKickedByAdmin += new FrostbiteClient.PlayerKickedHandler(Game_PlayerKickedByAdmin);
            ProconClient.Game.PlayerKilledByAdmin += new FrostbiteClient.PlayerKilledByAdminHandler(Game_PlayerKilledByAdmin);
            ProconClient.Game.PlayerMovedByAdmin += new FrostbiteClient.PlayerMovedByAdminHandler(Game_PlayerMovedByAdmin);

            #endregion

            #region Layer Accounts

            foreach (ILayerClient client in new List<ILayerClient>(ProconClient.Layer.Clients.Values))
            {
                client.Login += Layer_LayerClientLogin;
                client.Logout += Layer_LayerClientLogout;
            }
            ProconClient.Layer.ClientConnected += Layer_ClientConnected;

            #endregion

            #region BF4

            ProconClient.Game.SpectatorListCleared += new FrostbiteClient.EmptyParamterHandler(Game_SpectatorListCleared);
            ProconClient.Game.SpectatorListList += new FrostbiteClient.SpectatorListListHandler(Game_SpectatorListList);
            ProconClient.Game.SpectatorListLoad += new FrostbiteClient.EmptyParamterHandler(Game_SpectatorListLoad);
            ProconClient.Game.SpectatorListPlayerAdded += new FrostbiteClient.SpectatorListPlayerHandler(Game_SpectatorListPlayerAdded);
            ProconClient.Game.SpectatorListPlayerRemoved += new FrostbiteClient.SpectatorListPlayerHandler(Game_SpectatorListPlayerRemoved);
            ProconClient.Game.SpectatorListSave += new FrostbiteClient.EmptyParamterHandler(Game_SpectatorListSave);

            ProconClient.Game.GameAdminCleared += new FrostbiteClient.EmptyParamterHandler(Game_GameAdminCleared);
            ProconClient.Game.GameAdminList += new FrostbiteClient.GameAdminListHandler(Game_GameAdminList);
            ProconClient.Game.GameAdminLoad += new FrostbiteClient.EmptyParamterHandler(Game_GameAdminLoad);
            ProconClient.Game.GameAdminPlayerAdded += new FrostbiteClient.GameAdminPlayerHandler(Game_GameAdminPlayerAdded);
            ProconClient.Game.GameAdminPlayerRemoved += new FrostbiteClient.GameAdminPlayerHandler(Game_GameAdminPlayerRemoved);
            ProconClient.Game.GameAdminSave += new FrostbiteClient.EmptyParamterHandler(Game_GameAdminSave);

            ProconClient.Game.MaxSpectators += new FrostbiteClient.LimitHandler(Game_MaxSpectators);

            ProconClient.Game.FairFight += new FrostbiteClient.IsEnabledHandler(Game_FairFight);

            ProconClient.Game.IsHitIndicator += new FrostbiteClient.IsEnabledHandler(Game_IsHitIndicator);

            ProconClient.Game.IsCommander += new FrostbiteClient.IsEnabledHandler(Game_IsCommander);
            ProconClient.Game.AlwaysAllowSpectators += new FrostbiteClient.IsEnabledHandler(Game_AlwaysAllowSpectators);
            ProconClient.Game.IsForceReloadWholeMags += new FrostbiteClient.IsEnabledHandler(Game_IsForceReloadWholeMags);
            ProconClient.Game.ServerType += new FrostbiteClient.VarsStringHandler(Game_ServerType);

            ProconClient.Game.TeamFactionOverride += new FrostbiteClient.TeamFactionOverrideHandler(Game_TeamFactionOverride);

            #endregion

            #region Battlefield: Hardline

            ProconClient.Game.RoundStartReadyPlayersNeeded += Game_RoundStartReadyPlayersNeeded;

            #endregion
        }

        #endregion

        #region Events

        #region BF4

        void Game_ServerType(FrostbiteClient sender, string value)
        {
            InvokeOnAllEnabled("OnServerType", value);
        }

        void Game_IsCommander(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnCommander", isEnabled);
        }

        void Game_AlwaysAllowSpectators(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnAlwaysAllowSpectators", isEnabled);
        }

        void Game_IsForceReloadWholeMags(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnForceReloadWholeMags", isEnabled);
        }

        void Game_FairFight(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnFairFight", isEnabled);
        }

        void Game_IsHitIndicator(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnIsHitIndicator", isEnabled);
        }

        void Game_MaxSpectators(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnMaxSpectators", limit);
        }

        void Game_GameAdminSave(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnGameAdminSave");
        }

        void Game_GameAdminPlayerRemoved(FrostbiteClient sender, string soldierName)
        {
            InvokeOnAllEnabled("OnGameAdminPlayerRemoved", soldierName);
        }

        void Game_GameAdminPlayerAdded(FrostbiteClient sender, string soldierName)
        {
            InvokeOnAllEnabled("OnGameAdminPlayerAdded", soldierName);
        }

        void Game_GameAdminLoad(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnGameAdminLoad");
        }

        void Game_GameAdminList(FrostbiteClient sender, List<string> soldierNames)
        {
            InvokeOnAllEnabled("OnGameAdminList", soldierNames);
        }

        void Game_GameAdminCleared(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnGameAdminCleared");
        }

        void Game_SpectatorListSave(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnSpectatorListSave");
        }

        void Game_SpectatorListPlayerRemoved(FrostbiteClient sender, string soldierName)
        {
            InvokeOnAllEnabled("OnSpectatorListPlayerRemoved", soldierName);
        }

        void Game_SpectatorListPlayerAdded(FrostbiteClient sender, string soldierName)
        {
            InvokeOnAllEnabled("OnSpectatorListPlayerAdded", soldierName);
        }

        void Game_SpectatorListLoad(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnSpectatorListLoad");
        }

        void Game_SpectatorListList(FrostbiteClient sender, List<string> soldierNames)
        {
            InvokeOnAllEnabled("OnSpectatorListList", soldierNames);
        }

        void Game_SpectatorListCleared(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnSpectatorListCleared");
        }


        #endregion

        #region Battlefield: Hardline

        void Game_RoundStartReadyPlayersNeeded(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnRoundStartReadyPlayersNeeded", limit);
        }

        #endregion

        #region Layer Accounts

        private void Layer_ClientConnected(ILayerClient client)
        {
            client.Login += Layer_LayerClientLogin;
            client.Logout += Layer_LayerClientLogout;
        }

        private void Layer_LayerClientLogout(ILayerClient sender)
        {
            InvokeOnAllEnabled("OnAccountLogout", sender.Username, sender.IPPort, sender.Privileges);
        }

        private void Layer_LayerClientLogin(ILayerClient sender)
        {
            InvokeOnAllEnabled("OnAccountLogin", sender.Username, sender.IPPort, sender.Privileges);
        }

        #endregion

        private void AccountPrivileges_AccountPrivilegeRemoved(AccountPrivilege item)
        {
            InvokeOnAllLoaded("OnAccountDeleted", item.Owner.Name);

            item.AccountPrivilegesChanged -= new AccountPrivilege.AccountPrivilegesChangedHandler(item_AccountPrivilegesChanged);
        }

        private void AccountPrivileges_AccountPrivilegeAdded(AccountPrivilege item)
        {
            item.AccountPrivilegesChanged += new AccountPrivilege.AccountPrivilegesChangedHandler(item_AccountPrivilegesChanged);

            InvokeOnAllLoaded("OnAccountCreated", item.Owner.Name);
        }

        private void item_AccountPrivilegesChanged(AccountPrivilege item)
        {
            InvokeOnAllLoaded("OnAccountPrivilegesUpdate", item.Owner.Name, item.Privileges);
        }

        private void m_prcClient_PunkbusterPlayerUnbanned(PRoConClient sender, CBanInfo cbiUnbannedPlayer)
        {
            InvokeOnAllEnabled("OnPunkbusterUnbanInfo", cbiUnbannedPlayer);
        }

        private void m_prcClient_PunkbusterPlayerBanned(PRoConClient sender, CBanInfo cbiBannedPlayer)
        {
            InvokeOnAllEnabled("OnPunkbusterBanInfo", cbiBannedPlayer);
        }

        private void m_prcClient_PunkbusterPlayerInfo(PRoConClient sender, CPunkbusterInfo pbInfo)
        {
            InvokeOnAllEnabled("OnPunkbusterPlayerInfo", pbInfo);
        }

        private void m_client_PunkbusterEndPlayerInfo(PRoConClient sender)
        {
            InvokeOnAllEnabled("OnPunkbusterBeginPlayerInfo");
        }

        private void m_client_PunkbusterBeginPlayerInfo(PRoConClient sender)
        {
            InvokeOnAllEnabled("OnPunkbusterEndPlayerInfo");
        }

        private void m_prcClient_ReceiveProconVariable(PRoConClient sender, string strVariable, string strValue)
        {
            InvokeOnAllEnabled("OnReceiveProconVariable", strVariable, strValue);
        }

        private void m_prcClient_CommandLogin(PRoConClient sender)
        {
            InvokeOnAllEnabled("OnLogin");
        }

        private void m_prcClient_CommandLogout(PRoConClient sender)
        {
            InvokeOnAllEnabled("OnLogout");
        }

        private void m_prcClient_CommandQuit(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnQuit");
        }

        private void m_prcClient_ConnectionClosed(PRoConClient sender)
        {
            InvokeOnAllEnabled("OnConnectionClosed");
        }

        private void m_prcClient_PlayerJoin(FrostbiteClient sender, string playerName)
        {
            InvokeOnAllEnabled("OnPlayerJoin", playerName);
        }

        private void m_prcClient_PlayerLeft(FrostbiteClient sender, string playerName, CPlayerInfo cpiPlayer)
        {
            if (cpiPlayer != null)
            {
                InvokeOnAllEnabled("OnPlayerLeft", cpiPlayer);
            }

            // OBSOLETE
            //this.InvokeOnAllEnabled("OnPlayerLeft", playerName);
        }

        private void m_prcClient_PlayerDisconnected(FrostbiteClient sender, string playerName, string reason)
        {
            InvokeOnAllEnabled("OnPlayerDisconnected", playerName, reason);
        }

        private void m_prcClient_PlayerAuthenticated(FrostbiteClient sender, string playerName, string playerGuid)
        {
            InvokeOnAllEnabled("OnPlayerAuthenticated", playerName, playerGuid);
        }

        private void m_prcClient_PlayerKicked(FrostbiteClient sender, string strSoldierName, string strReason)
        {
            InvokeOnAllEnabled("OnPlayerKicked", strSoldierName, strReason);
        }

        private void m_prcClient_PlayerChangedTeam(FrostbiteClient sender, string strSoldierName, int iTeamID, int iSquadID)
        {
            InvokeOnAllEnabled("OnPlayerTeamChange", strSoldierName, iTeamID, iSquadID);
        }

        private void m_prcClient_PlayerChangedSquad(FrostbiteClient sender, string strSoldierName, int iTeamID, int iSquadID)
        {
            InvokeOnAllEnabled("OnPlayerSquadChange", strSoldierName, iTeamID, iSquadID);
        }

        private void m_prcClient_PlayerKilled(PRoConClient sender, Kill kKillerVictimDetails)
        {
            InvokeOnAllEnabled("OnPlayerKilled", new object[] { kKillerVictimDetails });

            // Obsolete. This was deprecated for BF3, it's now being taken away from BF4.
            // InvokeOnAllEnabled("OnPlayerKilled", new object[] {kKillerVictimDetails.Killer.SoldierName, kKillerVictimDetails.Victim.SoldierName});
        }

        /// <summary>
        ///     This will check from the dictionary of registered commands to see if some text is matched
        ///     against a registered command.  The return is prioritized for whatever command matches more
        ///     arguments.
        /// </summary>
        /// <param name="playerName">Who executed the command</param>
        /// <param name="message">The message they sent</param>
        /// <param name="matchedCommand"></param>
        /// <param name="returnCommand"></param>
        private bool CheckInGameCommands(string playerName, string message, out MatchCommand matchedCommand, out CapturedCommand returnCommand)
        {
            bool isMatch = false;
            returnCommand = null;
            matchedCommand = null;

            lock (MatchedInGameCommandsLocker)
            {
                CapturedCommand capMatched = null;

                // If this player has a command stored that requires confirmation.
                if (CommandsNeedingConfirmation.Contains(playerName) == true)
                {
                    if ((capMatched = CommandsNeedingConfirmation[playerName].MatchedCommand.Requirements.ConfirmationCommand.Matches(message)) != null)
                    {
                        //capReturnCommand = capMatched;
                        returnCommand = CommandsNeedingConfirmation[playerName].ConfirmationDetails;
                        matchedCommand = CommandsNeedingConfirmation[playerName].MatchedCommand;
                        returnCommand.IsConfirmed = true;
                        isMatch = true;
                    }
                }

                // If it was not a confirmation to a previously matched command.
                if (isMatch == false)
                {
                    foreach (var kvpCommand in MatchedInGameCommands)
                    {
                        // Only care if the plugin is enabled.
                        if (Plugins.IsEnabled(kvpCommand.Value.RegisteredClassname) == true)
                        {
                            capMatched = kvpCommand.Value.Matches(message);

                            if (capMatched != null)
                            {
                                if (kvpCommand.Value.Requirements.HasValidPermissions(ProconClient.GetAccountPrivileges(playerName)) == true)
                                {
                                    // if (this.ValidateRequirements(playerName, kvpCommand.Value.Requirements) == true) {

                                    // If it's the first match we've found
                                    if (returnCommand == null)
                                    {
                                        returnCommand = capMatched;
                                        matchedCommand = kvpCommand.Value;
                                        isMatch = true;
                                    }
                                    else if (capMatched.CompareTo(returnCommand) > 0)
                                    {
                                        // We've found a command with that is a closer match to its arguments
                                        returnCommand = capMatched;
                                        matchedCommand = kvpCommand.Value;
                                        isMatch = true;
                                    }

                                    /*
                                    // If we've found a better match than before (more arguments matched)
                                    else if (capReturnCommand != null && capMatched.MatchedArguments.Count > capReturnCommand.MatchedArguments.Count) {
                                        capReturnCommand = capMatched;
                                        mtcMatchedCommand = kvpCommand.Value;
                                        isMatch = true;
                                    }
                                    // If we've found another match, check if this one is "matchier" (has a lower score)
                                    else if (capReturnCommand != null && capMatched.MatchedArguments.Count == capReturnCommand.MatchedArguments.Count && capMatched.AggregateMatchScore < capReturnCommand.AggregateMatchScore) {
                                        // We've found a command with the same amount of matched data but the new command is closer to it's own dictionary.
                                        capReturnCommand = capMatched;
                                        mtcMatchedCommand = kvpCommand.Value;
                                        isMatch = true;
                                    }*/
                                }
                                else
                                {
                                    ProconClient.Game.SendAdminSayPacket(kvpCommand.Value.Requirements.FailedRequirementsMessage, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Player, playerName));
                                    // this.m_prcClient.SendRequest(new List<string>() { "admin.say", kvpCommand.Value.Requirements.FailedRequirementsMessage, "player", playerName });
                                }
                            }
                        }
                    }
                }
            }

            return isMatch;
        }

        private void DispatchMatchedCommand(string playerName, string message, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subset)
        {
            bool isConfirmationRequired = false;

            if (capCommand.IsConfirmed == false)
            {
                foreach (MatchArgument mtcArgument in capCommand.MatchedArguments)
                {
                    if (mtcArgument.MatchScore > mtcCommand.Requirements.MinimumMatchSimilarity)
                    {
                        isConfirmationRequired = true;
                        capCommand.IsConfirmed = false;
                        break;
                    }
                }
            }

            if (isConfirmationRequired == true && capCommand.IsConfirmed == false)
            {
                if (CommandsNeedingConfirmation.Contains(playerName) == true)
                {
                    CommandsNeedingConfirmation.Remove(playerName);
                }

                CommandsNeedingConfirmation.Add(new ConfirmationEntry(playerName, message, mtcCommand, capCommand, subset));

                ProconClient.Game.SendAdminSayPacket(String.Format("Did you mean {0}?", capCommand), new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Player, playerName));
            }
            else
            {
                InvokeOnEnabled(mtcCommand.RegisteredClassname, mtcCommand.RegisteredMethodName, playerName, message, mtcCommand, capCommand, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));

                InvokeOnAllEnabled("OnAnyMatchRegisteredCommand", playerName, message, mtcCommand, capCommand, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
            }
        }

        private void m_prcClient_GlobalChat(FrostbiteClient sender, string playerName, string message)
        {
            InvokeOnAllEnabled("OnGlobalChat", playerName, message);

            CapturedCommand capCommand = null;
            MatchCommand mtcCommand = null;

            if (String.Compare(playerName, "server", StringComparison.OrdinalIgnoreCase) != 0 && CheckInGameCommands(playerName, message, out mtcCommand, out capCommand) == true)
            {
                DispatchMatchedCommand(playerName, message, mtcCommand, capCommand, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
                //this.InvokeOnEnabled(mtcCommand.RegisteredClassname, "OnMatchRegisteredCommand", playerName, message, mtcCommand, capCommand, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
            }
        }

        private void m_prcClient_TeamChat(FrostbiteClient sender, string playerName, string message, int teamId)
        {
            InvokeOnAllEnabled("OnTeamChat", playerName, message, teamId);

            CapturedCommand capCommand = null;
            MatchCommand mtcCommand = null;

            if (String.Compare(playerName, "server", StringComparison.OrdinalIgnoreCase) != 0 && CheckInGameCommands(playerName, message, out mtcCommand, out capCommand) == true)
            {
                DispatchMatchedCommand(playerName, message, mtcCommand, capCommand, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Team, teamId));
                //this.InvokeOnEnabled(mtcCommand.RegisteredClassname, "OnMatchRegisteredCommand", playerName, message, mtcCommand, capCommand, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Team, teamId));
            }
        }

        private void m_prcClient_SquadChat(FrostbiteClient sender, string playerName, string message, int teamId, int squadId)
        {
            InvokeOnAllEnabled("OnSquadChat", playerName, message, teamId, squadId);

            CapturedCommand capCommand = null;
            MatchCommand mtcCommand = null;

            if (String.Compare(playerName, "server", StringComparison.OrdinalIgnoreCase) != 0 && CheckInGameCommands(playerName, message, out mtcCommand, out capCommand) == true)
            {
                DispatchMatchedCommand(playerName, message, mtcCommand, capCommand, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Squad, teamId, squadId));
                //this.InvokeOnEnabled(mtcCommand.RegisteredClassname, "OnMatchRegisteredCommand", playerName, message, mtcCommand, capCommand, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Squad, teamId, squadId));
            }
        }

        private void m_prcClient_PlayerChat(FrostbiteClient sender, string playerName, string message, string targetPlayer)
        {
            InvokeOnAllEnabled("OnPlayerChat", playerName, message, targetPlayer);

            CapturedCommand capCommand = null;
            MatchCommand mtcCommand = null;

            if (String.Compare(playerName, "server", StringComparison.OrdinalIgnoreCase) != 0 && CheckInGameCommands(playerName, message, out mtcCommand, out capCommand) == true)
            {
                DispatchMatchedCommand(playerName, message, mtcCommand, capCommand, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Player, targetPlayer));
                //this.InvokeOnEnabled(mtcCommand.RegisteredClassname, "OnMatchRegisteredCommand", playerName, message, mtcCommand, capCommand, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Squad, teamId, squadId));
            }
        }

        private void m_prcClient_ResponseError(FrostbiteClient sender, Packet originalRequest, string errorMessage)
        {
            InvokeOnAllEnabled("OnResponseError", new List<string>(originalRequest.Words), errorMessage);
        }

        private void m_prcClient_Version(FrostbiteClient sender, string serverType, string serverVersion)
        {
            InvokeOnAllEnabled("OnVersion", serverType, serverVersion);
        }

        private void m_prcClient_Help(FrostbiteClient sender, List<string> lstCommands)
        {
            InvokeOnAllEnabled("OnHelp", lstCommands);
        }

        private void m_prcClient_RunScript(FrostbiteClient sender, string scriptFileName)
        {
            InvokeOnAllEnabled("OnRunScript", scriptFileName);
        }

        private void m_prcClient_RunScriptError(FrostbiteClient sender, string strScriptFileName, int iLineError, string strErrorDescription)
        {
            InvokeOnAllEnabled("OnRunScriptError", strScriptFileName, iLineError, strErrorDescription);
        }

        private void m_prcClient_PunkbusterMessage(FrostbiteClient sender, string punkbusterMessage)
        {
            InvokeOnAllEnabled("OnPunkbusterMessage", punkbusterMessage);
        }

        private void m_prcClient_LoadingLevel(FrostbiteClient sender, string mapFileName, int roundsPlayed, int roundsTotal)
        {
            InvokeOnAllEnabled("OnLoadingLevel", mapFileName, roundsPlayed, roundsTotal);

            // DEPRECATED
            InvokeOnAllEnabled("OnLoadingLevel", mapFileName);
        }

        private void m_prcClient_LevelStarted(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnLevelStarted");
        }

        private void m_prcClient_LevelLoaded(FrostbiteClient sender, string mapFileName, string gamemode, int roundsPlayed, int roundsTotal)
        {
            InvokeOnAllEnabled("OnLevelLoaded", mapFileName, gamemode, roundsPlayed, roundsTotal);
        }

        private void m_prcClient_ServerInfo(FrostbiteClient sender, CServerInfo csiServerInfo)
        {
            InvokeOnAllEnabled("OnServerInfo", csiServerInfo);
        }

        private void m_prcClient_Yelling(FrostbiteClient sender, string strMessage, int iMessageDuration, List<string> lstSubsetWords)
        {
            InvokeOnAllEnabled("OnYelling", strMessage, iMessageDuration, new CPlayerSubset(lstSubsetWords));
        }

        private void m_prcClient_Saying(FrostbiteClient sender, string strMessage, List<string> lstSubsetWords)
        {
            InvokeOnAllEnabled("OnSaying", strMessage, new CPlayerSubset(lstSubsetWords));
        }

        private void m_prcClient_RunNextLevel(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnRunNextLevel");
        }

        private void m_prcClient_CurrentLevel(FrostbiteClient sender, string currentLevel)
        {
            InvokeOnAllEnabled("OnCurrentLevel", currentLevel);
        }

        private void m_prcClient_RestartLevel(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnRestartLevel");
        }

        private void m_prcClient_SupportedMaps(FrostbiteClient sender, string strPlaylist, List<string> lstSupportedMaps)
        {
            InvokeOnAllEnabled("OnSupportedMaps", strPlaylist, lstSupportedMaps);
        }

        private void m_prcClient_PlaylistSet(FrostbiteClient sender, string playlist)
        {
            InvokeOnAllEnabled("OnPlaylistSet", playlist);
        }

        private void m_prcClient_ListPlaylists(FrostbiteClient sender, List<string> lstPlaylists)
        {
            InvokeOnAllEnabled("OnListPlaylists", lstPlaylists);
        }

        private void m_prcClient_ListPlayers(FrostbiteClient sender, List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset)
        {
            InvokeOnAllEnabled("OnListPlayers", lstPlayers, cpsSubset);
        }

        private void m_prcClient_ReservedSlotsConfigFile(FrostbiteClient sender, string configFilename)
        {
            InvokeOnAllEnabled("OnReservedSlotsConfigFile", configFilename);
        }

        private void m_prcClient_ReservedSlotsLoad(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnReservedSlotsLoad");
        }

        private void m_prcClient_ReservedSlotsSave(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnReservedSlotsSave");
        }

        private void m_prcClient_ReservedSlotsPlayerAdded(FrostbiteClient sender, string strSoldierName)
        {
            InvokeOnAllEnabled("OnReservedSlotsPlayerAdded", strSoldierName);
        }

        private void m_prcClient_ReservedSlotsPlayerRemoved(FrostbiteClient sender, string strSoldierName)
        {
            InvokeOnAllEnabled("OnReservedSlotsPlayerRemoved", strSoldierName);
        }

        private void m_prcClient_ReservedSlotsCleared(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnReservedSlotsCleared");
        }

        private void m_prcClient_ReservedSlotsList(FrostbiteClient sender, List<string> soldierNames)
        {
            InvokeOnAllEnabled("OnReservedSlotsList", soldierNames);
        }

        private void m_prcClient_MapListConfigFile(FrostbiteClient sender, string strConfigFilename)
        {
            InvokeOnAllEnabled("OnMaplistConfigFile", strConfigFilename);
        }

        private void m_prcClient_MapListLoad(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnMaplistLoad");
        }

        private void m_prcClient_MapListSave(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnMaplistSave");
        }

        private void m_prcClient_MapListMapAppended(FrostbiteClient sender, MaplistEntry mapEntry)
        {
            InvokeOnAllEnabled("OnMaplistMapAppended", mapEntry.MapFileName);
        }

        private void m_prcClient_MapListNextLevelIndex(FrostbiteClient sender, int mapIndex)
        {
            InvokeOnAllEnabled("OnMaplistNextLevelIndex", mapIndex);
        }

        private void m_prcClient_MapListGetMapIndices(FrostbiteClient sender, int mapIndex, int nextIndex)
        {
            InvokeOnAllEnabled("OnMaplistGetMapIndices", mapIndex, nextIndex);
        }

        private void m_prcClient_MapListGetRounds(FrostbiteClient sender, int currentRound, int totalRounds)
        {
            InvokeOnAllEnabled("OnMaplistGetRounds", currentRound, totalRounds);
        }

        private void m_prcClient_MapListMapRemoved(FrostbiteClient sender, int mapIndex)
        {
            InvokeOnAllEnabled("OnMaplistMapRemoved", mapIndex);
        }

        private void m_prcClient_MapListMapInserted(FrostbiteClient sender, MaplistEntry mapEntry)
        {
            // int mapIndex, string mapFileName, int rounds) {
            InvokeOnAllEnabled("OnMaplistMapInserted", mapEntry.Index, mapEntry.MapFileName, mapEntry.Rounds);
        }

        private void m_prcClient_MapListCleared(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnMaplistCleared");
        }

        private void m_prcClient_MapListListed(FrostbiteClient sender, List<MaplistEntry> lstMaplist)
        {
            InvokeOnAllEnabled("OnMaplistList", lstMaplist);

            // OBSOLETE
            //var lstMapFileNames = new List<string>();
            //foreach (MaplistEntry mleEntry in lstMaplist) {
            //    lstMapFileNames.Add(mleEntry.MapFileName);
            //}

            // OBSOLETE
            //InvokeOnAllEnabled("OnMaplistList", lstMapFileNames);
        }

        private void m_prcClient_GamePassword(FrostbiteClient sender, string password)
        {
            InvokeOnAllEnabled("OnGamePassword", password);
        }

        private void m_prcClient_Punkbuster(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnPunkbuster", isEnabled);
        }

        private void m_prcClient_Hardcore(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnHardcore", isEnabled);
        }

        private void m_prcClient_Ranked(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnRanked", isEnabled);
        }

        private void m_prcClient_RankLimit(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnRankLimit", limit);
        }

        private void m_prcClient_TeamBalance(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnTeamBalance", isEnabled);
        }

        private void m_prcClient_FriendlyFire(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnFriendlyFire", isEnabled);
        }

        private void m_prcClient_MaxPlayerLimit(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnMaxPlayerLimit", limit);
        }

        private void m_prcClient_CurrentPlayerLimit(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnCurrentPlayerLimit", limit);
        }

        private void m_prcClient_PlayerLimit(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnPlayerLimit", limit);
            InvokeOnAllEnabled("OnMaxPlayers", limit);
        }

        private void m_prcClient_BannerUrl(FrostbiteClient sender, string url)
        {
            InvokeOnAllEnabled("OnBannerURL", url);
        }

        private void m_prcClient_ServerDescription(FrostbiteClient sender, string serverDescription)
        {
            InvokeOnAllEnabled("OnServerDescription", serverDescription);
        }

        private void m_prcClient_ServerMessage(FrostbiteClient sender, string serverMessage)
        {
            InvokeOnAllEnabled("OnServerMessage", serverMessage);
        }

        private void m_prcClient_KillCam(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnKillCam", isEnabled);
        }

        private void m_prcClient_MiniMap(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnMiniMap", isEnabled);
        }

        private void m_prcClient_CrossHair(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnCrossHair", isEnabled);
        }

        private void m_prcClient_ThreeDSpotting(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("On3dSpotting", isEnabled);
        }

        private void m_prcClient_MiniMapSpotting(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnMiniMapSpotting", isEnabled);
        }

        private void m_prcClient_ThirdPersonVehicleCameras(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnThirdPersonVehicleCameras", isEnabled);
        }

        private void m_prcClient_ProfanityFilter(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnProfanityFilter", new object[] { isEnabled });
        }

        private void m_prcClient_IdleTimeout(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnIdleTimeout", new object[] { limit });
        }

        private void m_prcClient_IdleBanRounds(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnIdleBanRounds", new object[] { limit });
        }

        private void m_prcClient_TeamKillValueForKick(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnTeamKillValueForKick", new object[] { limit });
        }

        private void m_prcClient_TeamKillKickForBan(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnTeamKillKickForBan", new object[] { limit });
        }

        private void m_prcClient_TeamKillValueDecreasePerSecond(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnTeamKillValueDecreasePerSecond", new object[] { limit });
        }

        private void m_prcClient_TeamKillValueIncrease(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnTeamKillValueIncrease", new object[] { limit });
        }

        private void m_prcClient_TeamKillCountForKick(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnTeamKillCountForKick", new object[] { limit });
        }

        // BF3
        private void m_prcClient_GameModeCounter(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnGameModeCounter", new object[] { limit });
        }

        private void m_prcClient_CtfRoundTimeModifier(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnCtfRoundTimeModifier", new object[] { limit });
        }

        private void m_prcClient_RoundTimeLimit(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnRoundTimeLimit", new object[] { limit });
        }

        private void m_prcClient_TicketBleedRate(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnTicketBleedRate", new object[] { limit });
        }

        private void m_prcClient_RoundRestartPlayerCount(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnRoundRestartPlayerCount", new object[] { limit });
        }

        private void m_prcClient_RoundStartPlayerCount(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnRoundStartPlayerCount", new object[] { limit });
        }

        private void m_prcClient_UnlockMode(FrostbiteClient sender, string mode)
        {
            InvokeOnAllEnabled("OnUnlockMode", new object[] { mode });
        }

        private void m_prcClient_BF4preset(FrostbiteClient sender, string mode, bool isLocked)
        {
            InvokeOnAllEnabled("OnPreset", new object[] { mode, isLocked });
        }

        private void Game_TeamFactionOverride(FrostbiteClient sender, int teamId, int faction)
        {
            InvokeOnAllEnabled("OnTeamFactionOverride", new object[] { teamId, faction });
        }

        private void Game_GunMasterWeaponsPreset(FrostbiteClient sender, int preset)
        {
            InvokeOnAllEnabled("OnGunMasterWeaponsPreset", new object[] { preset });
        }

        private void Game_ReservedSlotsListAggressiveJoin(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnReservedSlotsListAggressiveJoin", isEnabled);
        }

        private void Game_RoundLockdownCountdown(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnRoundLockdownCountdown", new object[] { limit });
        }

        private void Game_RoundWarmupTimeout(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnRoundWarmupTimeout", new object[] { limit });
        }

        private void Game_PremiumStatus(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnPremiumStatus", isEnabled);
        }

        private void Game_VehicleSpawnAllowed(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnVehicleSpawnAllowed", isEnabled);
        }

        private void Game_VehicleSpawnDelay(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnVehicleSpawnDelay", new object[] { limit });
        }

        private void Game_BulletDamage(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnBulletDamage", new object[] { limit });
        }

        private void Game_OnlySquadLeaderSpawn(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnOnlySquadLeaderSpawn", isEnabled);
        }

        private void Game_SoldierHealth(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnSoldierHealth", new object[] { limit });
        }

        private void Game_PlayerManDownTime(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnPlayerManDownTime", new object[] { limit });
        }

        private void Game_PlayerRespawnTime(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnPlayerRespawnTime", new object[] { limit });
        }

        private void Game_Hud(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnHud", isEnabled);
        }

        private void Game_NameTag(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnNameTag", isEnabled);
        }

        private void m_prcClient_ServerName(FrostbiteClient sender, string strServerName)
        {
            InvokeOnAllEnabled("OnServerName", new object[] { strServerName });
        }

        private void m_prcClient_EndRound(FrostbiteClient sender, int iWinningTeamID)
        {
            InvokeOnAllEnabled("OnEndRound", new object[] { iWinningTeamID });
        }

        private void m_prcClient_RoundOverTeamScores(FrostbiteClient sender, List<TeamScore> lstTeamScores)
        {
            InvokeOnAllEnabled("OnRoundOverTeamScores", new object[] { lstTeamScores });
        }

        private void m_prcClient_RoundOverPlayers(FrostbiteClient sender, List<CPlayerInfo> lstPlayers)
        {
            InvokeOnAllEnabled("OnRoundOverPlayers", new object[] { lstPlayers });
        }

        private void m_prcClient_RoundOver(FrostbiteClient sender, int iWinningTeamID)
        {
            InvokeOnAllEnabled("OnRoundOver", new object[] { iWinningTeamID });
        }

        private void m_prcClient_PlayerSpawned(PRoConClient sender, string soldierName, Inventory spawnedInventory)
        {
            InvokeOnAllEnabled("OnPlayerSpawned", new object[] { soldierName, spawnedInventory });
        }

        private void m_prcClient_LevelVariablesList(FrostbiteClient sender, LevelVariable lvRequestedContext, List<LevelVariable> lstReturnedValues)
        {
            InvokeOnAllEnabled("OnLevelVariablesList", new object[] { lvRequestedContext, lstReturnedValues });
        }

        private void m_prcClient_LevelVariablesEvaluate(FrostbiteClient sender, LevelVariable lvRequestedContext, LevelVariable lvReturnedValue)
        {
            InvokeOnAllEnabled("OnLevelVariablesEvaluate", new object[] { lvRequestedContext, lvReturnedValue });
        }

        private void m_prcClient_LevelVariablesClear(FrostbiteClient sender, LevelVariable lvRequestedContext)
        {
            InvokeOnAllEnabled("OnLevelVariablesClear", new object[] { lvRequestedContext });
        }

        private void m_prcClient_LevelVariablesSet(FrostbiteClient sender, LevelVariable lvRequestedContext)
        {
            InvokeOnAllEnabled("OnLevelVariablesSet", new object[] { lvRequestedContext });
        }

        private void m_prcClient_LevelVariablesGet(FrostbiteClient sender, LevelVariable lvRequestedContext, LevelVariable lvReturnedValue)
        {
            InvokeOnAllEnabled("OnLevelVariablesGet", new object[] { lvRequestedContext, lvReturnedValue });
        }

        private void MapGeometry_MapZoneTrespassed(CPlayerInfo cpiSoldier, ZoneAction action, MapZone sender, Point3D pntTresspassLocation, float flTresspassPercentage, object trespassState)
        {
            InvokeOnAllEnabled("OnZoneTrespass", new[] { cpiSoldier, action, sender, pntTresspassLocation, flTresspassPercentage, trespassState });
        }

        #region Admin actions on players

        private void Game_PlayerKilledByAdmin(FrostbiteClient sender, string soldierName)
        {
            InvokeOnAllEnabled("OnPlayerKilledByAdmin", soldierName);
        }

        private void Game_PlayerKickedByAdmin(FrostbiteClient sender, string strSoldierName, string strReason)
        {
            InvokeOnAllEnabled("OnPlayerKickedByAdmin", strSoldierName, strReason);
        }

        private void Game_PlayerMovedByAdmin(FrostbiteClient sender, string soldierName, int destinationTeamId, int destinationSquadId, bool forceKilled)
        {
            InvokeOnAllEnabled("OnPlayerMovedByAdmin", soldierName, destinationTeamId, destinationSquadId, forceKilled);
        }

        #endregion

        #region player/squad cmds

        private void Game_PlayerIdleState(FrostbiteClient sender, string soldierName, int idleTime)
        {
            InvokeOnAllEnabled("OnPlayerIdleDuration", soldierName, idleTime);
        }

        private void Game_PlayerIsAlive(FrostbiteClient sender, string soldierName, bool isAlive)
        {
            InvokeOnAllEnabled("OnPlayerIsAlive", soldierName, isAlive);
        }

        private void Game_PlayerPingedByAdmin(FrostbiteClient sender, string soldierName, int ping)
        {
            InvokeOnAllEnabled("OnPlayerPingedByAdmin", soldierName, ping);
        }

        private void Game_SquadLeader(FrostbiteClient sender, int teamId, int squadId, string soldierName)
        {
            InvokeOnAllEnabled("OnSquadLeader", teamId, squadId, soldierName);
        }

        private void Game_SquadListActive(FrostbiteClient sender, int teamId, int squadCount, List<int> squadList)
        {
            InvokeOnAllEnabled("OnSquadListActive", teamId, squadCount, squadList);
        }

        private void Game_SquadListPlayers(FrostbiteClient sender, int teamId, int squadId, int playerCount, List<string> playersInSquad)
        {
            InvokeOnAllEnabled("OnSquadListPlayers", teamId, squadId, playerCount, playersInSquad);
        }

        private void Game_SquadIsPrivate(FrostbiteClient sender, int teamId, int squadId, bool isPrivate)
        {
            InvokeOnAllEnabled("OnSquadIsPrivate", teamId, squadId, isPrivate);
        }

        #endregion

        #region MoHW vars setting events

        private void Game_AllUnlocksUnlocked(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnAllUnlocksUnlocked", isEnabled);
        }

        private void Game_BuddyOutline(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnBuddyOutline", isEnabled);
        }

        private void Game_HudBuddyInfo(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnHudBuddyInfo", isEnabled);
        }

        private void Game_HudClassAbility(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnHudClassAbility", isEnabled);
        }

        private void Game_HudCrosshair(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnCrosshair", isEnabled);
            InvokeOnAllEnabled("OnHudCrosshair", isEnabled);
        }

        private void Game_HudEnemyTag(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnHudEnemyTag", isEnabled);
        }

        private void Game_HudExplosiveIcons(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnHudExplosiveIcons", isEnabled);
        }

        private void Game_HudGameMode(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnHudGameMode", isEnabled);
        }

        private void Game_HudHealthAmmo(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnHudHealthAmmo", isEnabled);
        }

        private void Game_HudMinimap(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnHudMinimap", isEnabled);
        }

        private void Game_HudObiturary(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnHudObiturary", isEnabled);
        }

        private void Game_HudPointsTracker(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnHudPointsTracker", isEnabled);
        }

        private void Game_HudUnlocks(FrostbiteClient sender, bool isEnabled)
        {
            InvokeOnAllEnabled("OnHudUnlocks", isEnabled);
        }

        private void Game_Playlist(FrostbiteClient sender, string playlist)
        {
            InvokeOnAllEnabled("OnPlaylistSet", playlist);
            InvokeOnAllEnabled("OnPlaylist", playlist);
        }

        #endregion

        #region Text Chat Moderation Settings

        private void Game_TextChatSpamTriggerCount(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnTextChatSpamTriggerCount", limit);
        }

        private void Game_TextChatSpamDetectionTime(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnTextChatSpamDetectionTime", limit);
        }

        private void Game_TextChatSpamCoolDownTime(FrostbiteClient sender, int limit)
        {
            InvokeOnAllEnabled("OnTextChatSpamCoolDownTime", limit);
        }

        private void Game_TextChatModerationMode(FrostbiteClient sender, ServerModerationModeType mode)
        {
            InvokeOnAllEnabled("OnTextChatModerationMode", mode);
        }

        #endregion

        #region Banlist

        private void m_prcClient_BanListLoad(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnBanListLoad");
        }

        private void m_prcClient_BanListSave(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnBanListSave");
        }

        private void m_prcClient_BanListAdd(FrostbiteClient sender, CBanInfo cbiAddedBan)
        {
            InvokeOnAllEnabled("OnBanAdded", cbiAddedBan);
        }

        private void m_prcClient_BanListRemove(FrostbiteClient sender, CBanInfo cbiRemovedBan)
        {
            InvokeOnAllEnabled("OnBanRemoved", cbiRemovedBan);
        }

        private void m_prcClient_BanListClear(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnBanListClear");
        }

        private void m_prcClient_BanListList(PRoConClient sender, List<CBanInfo> lstBans)
        {
            InvokeOnAllEnabled("OnBanList", lstBans);
        }

        #endregion

        #region Text Chat Moderation

        private void m_client_FullTextChatModerationListList(PRoConClient sender, TextChatModerationDictionary moderationList)
        {
            InvokeOnAllEnabled("OnTextChatModerationList", moderationList);
        }

        private void Game_TextChatModerationListLoad(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnTextChatModerationLoad");
        }

        private void Game_TextChatModerationListClear(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnTextChatModerationClear");
        }

        private void Game_TextChatModerationListSave(FrostbiteClient sender)
        {
            InvokeOnAllEnabled("OnTextChatModerationSave");
        }

        private void Game_TextChatModerationListRemovePlayer(FrostbiteClient sender, TextChatModerationEntry playerEntry)
        {
            InvokeOnAllEnabled("OnTextChatModerationRemovePlayer", playerEntry);
        }

        private void Game_TextChatModerationListAddPlayer(FrostbiteClient sender, TextChatModerationEntry playerEntry)
        {
            InvokeOnAllEnabled("OnTextChatModerationAddPlayer", playerEntry);
        }

        #endregion

        #endregion
    }
}
