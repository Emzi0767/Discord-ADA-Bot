using System;
using System.Diagnostics;
using System.Text;
using Emzi0767.Ada.Commands;
using Emzi0767.Ada.Config;
using Emzi0767.Ada.Core;
using Emzi0767.Ada.Plugins;

namespace Emzi0767.Ada
{
    public static class AdaBotCore
    {
        public static AdaCommandManager CommandManager { get; internal set; }
        public static AdaClient AdaClient { get; internal set; }
        public static int PluginCount { get { return PluginManager.PluginCount; } }
        public static AdaConfigManager ConfigManager { get; private set; }
        internal static AdaPluginManager PluginManager { get; set; }
        internal static UTF8Encoding UTF8 { get; set; }
        private static bool KeepRunning { get; set; }

        internal static void Main(string[] args)
        {
            // initialize self
            L.R(Console.Out);
            L.D(Debugger.IsAttached);
            UTF8 = new UTF8Encoding(false);
            Console.CancelKeyPress += Console_CancelKeyPress;

            // init discord
            L.W("ADA", "Initializing ADA Discord module");
            AdaClient = new AdaClient();
            AdaClient.Initialize();
            L.W("ADA", "ADA Discord module initialized");

            // load plugins
            L.W("ADA", "Loading ADA Plugins");
            PluginManager = new AdaPluginManager();
            PluginManager.LoadAssemblies();
            L.W("ADA", "ADA Plugins loaded");

            // init config
            L.W("ADA", "Initializing ADA Config module");
            ConfigManager = new AdaConfigManager();
            ConfigManager.Initialize();
            L.W("ADA", "ADA Config module initialized");

            // init plugins
            L.W("ADA", "Initializing ADA Plugins");
            PluginManager.Initialize();
            L.W("ADA", "ADA Plugins Initialized");

            // init commands
            L.W("ADA", "Initializing ADA Command module");
            CommandManager = new AdaCommandManager();
            CommandManager.Initialize();
            L.W("ADA", "ADA Command module initialized");

            // run
            L.W("ADA", "ADA is now running");
            KeepRunning = true;
            while (KeepRunning) { }

            // some shutdown signal and subsequent shutdown
            L.W("ADA", "Caught exit signal");
            AdaClient.Deinitialize();
            L.W("ADA", "Disposing logger");
            L.Q();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            KeepRunning = false;
        }
    }
}
