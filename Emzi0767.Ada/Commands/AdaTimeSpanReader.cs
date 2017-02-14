using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;

namespace Emzi0767.Ada.Commands
{
    public class AdaTimeSpanReader : TypeReader
    {
        public override async Task<TypeReaderResult> Read(ICommandContext context, string input)
        {
            await Task.Yield();

            var result = TimeSpan.Zero;
            if (input == "0")
                return TypeReaderResult.FromSuccess((TimeSpan?)null);

            if (TimeSpan.TryParse(input, out result))
                return TypeReaderResult.FromSuccess(new TimeSpan?(result));

            var reg = new Regex(@"^(?<days>\d+d)?(?<hours>\d{1,2}h)?(?<minutes>\d{1,2}m)?(?<seconds>\d{1,2}s)?$", RegexOptions.Compiled);
            var gps = new string[] { "days", "hours", "minutes", "seconds" };
            var mtc = reg.Match(input);
            if (!mtc.Success)
                return TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid TimeSpan string");

            var d = 0;
            var h = 0;
            var m = 0;
            var s = 0;
            foreach (var gp in gps)
            {
                var val = 0;
                var gpc = mtc.Groups[gp].Value;
                if (string.IsNullOrWhiteSpace(gpc))
                    continue;

                var gpt = gpc.Last();
                int.TryParse(gpc.Substring(0, gpc.Length - 1), out val);
                switch (gpt)
                {
                    case 'd':
                        d = val;
                        break;

                    case 'h':
                        h = val;
                        break;

                    case 'm':
                        m = val;
                        break;

                    case 's':
                        s = val;
                        break;
                }
            }
            result = new TimeSpan(d, h, m, s);
            return TypeReaderResult.FromSuccess(new TimeSpan?(result));
        }
    }
}
