// Copyright 2016 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;

namespace Emzi0767.Ada.Commands
{
    public class AdaTimeSpanReader : TypeReader
    {
        // Thanks to Joe4evr for pointing out this optimization
        private static Regex TimeSpanRegex { get; } = new Regex(@"^(?<days>\d+d)?(?<hours>\d{1,2}h)?(?<minutes>\d{1,2}m)?(?<seconds>\d{1,2}s)?$", RegexOptions.Compiled);
        private static string[] RegexGroups { get; } = new string[] { "days", "hours", "minutes", "seconds" };

        public override async Task<TypeReaderResult> Read(ICommandContext context, string input)
        {
            await Task.Yield();

            var result = TimeSpan.Zero;
            if (input == "0")
                return TypeReaderResult.FromSuccess((TimeSpan?)null);

            if (TimeSpan.TryParse(input, out result))
                return TypeReaderResult.FromSuccess(new TimeSpan?(result));
            
            var mtc = TimeSpanRegex.Match(input);
            if (!mtc.Success)
                return TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid TimeSpan string");

            var d = 0;
            var h = 0;
            var m = 0;
            var s = 0;
            foreach (var gp in RegexGroups)
            {
                var gpc = mtc.Groups[gp].Value;
                if (string.IsNullOrWhiteSpace(gpc))
                    continue;

                var gpt = gpc.Last();
                int.TryParse(gpc.Substring(0, gpc.Length - 1), out var val);
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
