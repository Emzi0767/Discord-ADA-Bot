﻿using System;
using System.Diagnostics;
using System.Text;
using Emzi0767.Net.Discord.AdaBot.Commands;
using Emzi0767.Net.Discord.AdaBot.Core;
using Emzi0767.Tools.MicroLogger;

namespace Emzi0767.Net.Discord.AdaBot
{
    public static class AdaBotCore
    {
        public static AdaCommandHandler Handler { get; internal set; }
        public static AdaDiscord Client { get; internal set; }
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
            Client = new AdaDiscord();
            Client.Initialize();
            L.W("ADA", "ADA Discord module initialized");

            // init plugins
            L.W("ADA", "Loading ADA Plugins");
            PluginManager = new AdaPluginManager();
            PluginManager.Initialize();
            L.W("ADA", "ADA Plugins loaded");

            // init commands
            L.W("ADA", "Initializing ADA Command module");
            Handler = new AdaCommandHandler();
            Handler.Initialize();
            L.W("ADA", "ADA Command module initialized");

            // run
            L.W("ADA", "ADA is now running");
            KeepRunning = true;
            while (KeepRunning) { }

            // some shutdown signal and subsequent shutdown
            L.W("ADA", "Caught exit signal");
            Client.Deinitialize();
            L.W("ADA", "Disposing logger");
            L.Q();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            KeepRunning = false;
        }
    }
}
