using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Emzi0767.Net.Discord.AdaBot.Attributes;
using Emzi0767.Tools.MicroLogger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Emzi0767.Net.Discord.Ada.AdvancedCommands
{
    [Plugin("Advanced Commands Plugin")]
    public class AdvancedCommandsPlugin
    {
        private static Dictionary<ulong, Dictionary<string, bool>> CommandConfiguration { get; set; }

        public static bool GetEnabledState(string command, ulong server)
        {
            if (!CommandConfiguration.ContainsKey(server))
                return true;

            var sd = CommandConfiguration[server];
            if (!sd.ContainsKey(command))
                return true;

            return sd[command];
        }

        public static void SetEnabledState(string command, ulong server, bool state)
        {
            if (!CommandConfiguration.ContainsKey(server))
                CommandConfiguration[server] = new Dictionary<string, bool>();

            var sd = CommandConfiguration[server];
            sd[command] = state;

            L.W("ADA DAC", "Command config updated");
            var jo = new JObject();
            foreach (var xsd in CommandConfiguration)
            {
                var xjo = new JObject();
                foreach (var xcc in xsd.Value)
                {
                    xjo.Add(xcc.Key, xcc.Value);
                }
                jo.Add(xsd.Key.ToString(), xjo);
            }
            var a = Assembly.GetEntryAssembly();
            var l = a.Location;
            l = Path.GetDirectoryName(l);
            l = Path.Combine(l, "advcmds.json");
            File.WriteAllText(l, jo.ToString(Formatting.None), new UTF8Encoding(false));
        }

        public static void Initialize()
        {
            L.W("ADA DAC", "Loading Advanced Commands Config");
            CommandConfiguration = new Dictionary<ulong, Dictionary<string, bool>>();
            var a = Assembly.GetEntryAssembly();
            var l = a.Location;
            l = Path.GetDirectoryName(l);
            l = Path.Combine(l, "advcmds.json");
            if (File.Exists(l))
            {
                var jo = JObject.Parse(File.ReadAllText(l, new UTF8Encoding(false)));
                foreach (var kvp in jo)
                {
                    var srv = ulong.Parse(kvp.Key);
                    var conf = (JObject)kvp.Value;
                    var dconf = new Dictionary<string, bool>();
                    foreach (var xkvp in conf)
                        dconf[xkvp.Key] = (bool)xkvp.Value;
                    CommandConfiguration[srv] = dconf;
                }
            }
            L.W("ADA DAC", "Done");
        }
    }
}
