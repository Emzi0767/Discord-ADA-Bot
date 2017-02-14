using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Emzi0767.Ada.Config;
using Emzi0767.Ada.Core;
using Emzi0767.Ada.Plugins;
using Emzi0767.Ada.Sql;

namespace Emzi0767.Ada
{
    public sealed class AdaBotProgram
    {
        public ConcurrentDictionary<int, AdaClient> Clients { get; private set; }

        public AdaConfigurationManager ConfigurationManager { get; private set; }
        public AdaSqlManager SqlManager { get; private set; }
        public AdaPluginManager PluginManager { get; private set; }

        private ManualResetEvent RunHandle { get; set; }
        private AutoResetEvent[] RunHandles { get; set; }

        public void Run()
        {
            Console.CancelKeyPress += OnCancel;

            L.W("ADA CORE", "Creating shard tracker");
            this.Clients = new ConcurrentDictionary<int, AdaClient>();

            L.W("ADA CORE", "Creating configuration manager");
            this.ConfigurationManager = new AdaConfigurationManager();
            this.ConfigurationManager.Initialize();

            L.W("ADA CORE", "Creating SQL manager");
            this.SqlManager = this.ConfigurationManager.CreateSqlManager();

            L.W("ADA CORE", "Creating plugin manager");
            this.PluginManager = new AdaPluginManager();
            this.PluginManager.Initialize();

            var sc = this.ConfigurationManager.BotConfiguration.ShardCount;
            this.RunHandle = new ManualResetEvent(false);
            this.RunHandles = new AutoResetEvent[sc];
            for (int i = 0; i < sc; i++)
            {
                Thread.Sleep(5000);

                var are = new AutoResetEvent(false);
                this.RunHandles[i] = new AutoResetEvent(false);

                ThreadPool.QueueUserWorkItem(this.CreateShardThread, new ThreadData { ShardId = i, WaitHandle = are });

                are.WaitOne();
            }

            this.RunHandle.WaitOne();

            for (int i = 0; i < sc; i++)
                this.RunHandles[i].WaitOne();
        }

        private void CreateShardThread(object _)
        {
            var __ = (ThreadData)_;

            var client = this.CreateShardAsync(__.ShardId).GetAwaiter().GetResult();
            this.Clients[__.ShardId] = client;

            __.WaitHandle.Set();
            this.RunHandle.WaitOne();

            client.Deinitialize().GetAwaiter().GetResult();
            this.RunHandles[__.ShardId].Set();
        }

        private async Task<AdaClient> CreateShardAsync(int id)
        {
            var cfg = this.ConfigurationManager.BotConfiguration;

            var ada = new AdaClient(id, this.ConfigurationManager, this.PluginManager, this.SqlManager);
            await ada.InitializeAsync();

            return ada;
        }

        private void OnCancel(object sender, ConsoleCancelEventArgs e)
        {
            this.RunHandle.Set();
        }

        internal static void Main(string[] args)
        {
            L.D(Debugger.IsAttached);
            L.R(Console.Out);

            var bot = new AdaBotProgram();
            bot.Run();
        }
    }

    internal struct ThreadData
    {
        public int ShardId { get; set; }
        public AutoResetEvent WaitHandle { get; set; }
    }
}
