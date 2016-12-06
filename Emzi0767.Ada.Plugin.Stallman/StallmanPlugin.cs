using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Emzi0767.Ada.Config;
using Emzi0767.Ada.Plugins;

namespace Emzi0767.Ada.Plugin.Stallman
{
    public class StallmanPlugin : IAdaPlugin
    {
        public IAdaPluginConfig Config { get { return this.conf; } }
        public Type ConfigType { get { return typeof(StallmanPluginConfig); } }
        public string Name { get { return "GNU/Stallman Plugin"; } }

        public static StallmanPlugin Instance { get; private set; }

        private StallmanPluginConfig conf;
        private const string STALLMAN_COPYPASTA = "I'd just like to interject for a moment. What you’re referring to as Linux, is in fact, GNU/Linux, or as I’ve recently taken to calling it, GNU plus Linux. Linux is not an operating system unto itself, but rather another free component of a fully functioning GNU system made useful by the GNU corelibs, shell utilities and vital system components comprising a full OS as defined by POSIX.\n\nMany computer users run a modified version of the GNU system every day, without realizing it.Through a peculiar turn of events, the version of GNU which is widely used today is often called “Linux”, and many of its users are not aware that it is basically the GNU system, developed by the GNU Project.There really is a Linux, and these people are using it, but it is just a part of the system they use.\n\nLinux is the kernel: the program in the system that allocates the machine’s resources to the other programs that you run.The kernel is an essential part of an operating system, but useless by itself; it can only function in the context of a complete operating system.Linux is normally used in combination with the GNU operating system: the whole system is basically GNU with Linux added, or GNU/Linux.All the so-called “Linux” distributions are really distributions of GNU/Linux.";

        public void Initialize()
        {
            L.W("GNU/ADA", "Initializing GNU/Stallman Plugin");
            Instance = this;
            AdaBotCore.AdaClient.RegisterMessageHandler(MessageHandler);
            L.W("GNU/ADA", "Done");
        }

        public void Enable(ulong guild)
        {
            if (this.conf.DisabledGuilds.Contains(guild))
            {
                this.conf.DisabledGuilds.Remove(guild);
                AdaBotCore.ConfigManager.UpdateConfig(this);
            }
        }

        public void Disable(ulong guild)
        {
            if (!this.conf.DisabledGuilds.Contains(guild))
            {
                this.conf.DisabledGuilds.Add(guild);
                AdaBotCore.ConfigManager.UpdateConfig(this);
            }
        }

        public void LoadConfig(IAdaPluginConfig config)
        {
            var cfg = config as StallmanPluginConfig;
            if (cfg != null)
                this.conf = cfg;
        }

        private async Task MessageHandler(SocketMessage msg)
        {
            var chn = msg.Channel as SocketTextChannel;
            if (chn == null || this.conf.DisabledGuilds.Contains(chn.Guild.Id))
                return;

            var ct = msg.Content;
            if (string.IsNullOrWhiteSpace(ct))
                return;

            ct = ct.ToLower();
            if (ct.Contains("linux") && !ct.Contains("gnu/linux"))
            {
                var rep = string.Concat(msg.Author.Mention, ", ", STALLMAN_COPYPASTA);
                await chn.SendMessageAsync(rep);
            }
        }
    }
}
