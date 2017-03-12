using Discord.Commands;
using Emzi0767.Ada.Config;
using Emzi0767.Ada.Core;
using Emzi0767.Ada.Plugins;
using Emzi0767.Ada.Sql;

namespace Emzi0767.Ada.Commands
{
    public abstract class AdaModuleBase : ModuleBase<AdaCommandContext>
    {
        protected AdaConfigurationManager ConfigurationManager { get; set; }
        protected AdaPluginManager PluginManager { get; set; }
        protected AdaSqlManager SqlManager { get; set; }
        protected AdaUtilities Utilities { get; set; }

        public AdaModuleBase(AdaConfigurationManager cfg, AdaPluginManager plm, AdaSqlManager sql, AdaUtilities util)
        {
            this.ConfigurationManager = cfg;
            this.PluginManager = plm;
            this.SqlManager = sql;
            this.Utilities = util;
        }
    }
}
