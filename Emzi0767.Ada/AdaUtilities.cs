// This file is part of ADA project
//
// Copyright 2018 Emzi0767
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Emzi0767.Ada.Data;
using Npgsql;

namespace Emzi0767.Ada
{
    /// <summary>
    /// Helper class containing various static helper methods and properties, as well as extension methods.
    /// </summary>
    public static class AdaUtilities
    {
        /// <summary>
        /// Gets the properly-configured UTF8 encoder.
        /// </summary>
        public static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);

        /// <summary>
        /// Converts this instance of PostgreSQL configuration section into a PostgreSQL connection string.
        /// </summary>
        /// <param name="config">Configuration section to convert.</param>
        /// <returns>PostgreSQL connection string.</returns>
        public static string ToPostgresConnectionString(this AdaConfigPostgres config)
        {
            // check if config is null
            if (config == null)
                throw new NullReferenceException();

            // build the connection string out of supplied parameters
            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = config.Hostname,
                Port = config.Port,

                Database = config.Database,
                Username = config.Username,
                Password = config.Password,

                SslMode = config.UseEncryption ? SslMode.Require : SslMode.Disable,
                TrustServerCertificate = config.TrustServerCertificate
            };
            return csb.ConnectionString;
        }

        /// <summary>
        /// Converts the string to a fixed-width string.
        /// </summary>
        /// <param name="s">String to fix the width of.</param>
        /// <param name="targetLength">Length that the string should be.</param>
        /// <returns>Adjusted string.</returns>
        public static string ToFixedWidth(this string s, int targetLength)
        {
            if (s == null)
                throw new NullReferenceException();

            if (s.Length < targetLength)
                return s.PadRight(targetLength, ' ');

            if (s.Length > targetLength)
                return s.Substring(0, targetLength);

            return s;
        }

        /// <summary>
        /// Gets the version of the bot's assembly.
        /// </summary>
        /// <returns>Bot version.</returns>
        public static string GetBotVersion()
        {
            var a = Assembly.GetExecutingAssembly();
            var av = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return av.InformationalVersion;
        }

        /// <summary>
        /// Converts given <see cref="TimeSpan"/> to a duration string.
        /// </summary>
        /// <param name="ts">Time span to convert.</param>
        /// <returns>Duration string.</returns>
        public static string ToDurationString(this TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return ts.ToString(@"h\:mm\:ss");
            return ts.ToString(@"m\:ss");
        }
        
        /// <summary>
        /// Extracts a substring token from supplied string.
        /// </summary>
        /// <param name="str">String to extract token from.</param>
        /// <param name="startPos">Starting position to search from.</param>
        /// <returns>Extracted token or null if no token could be extracted.</returns>
        private static string ExtractNextArgument(this string str, ref int startPos)
        {
            if (string.IsNullOrWhiteSpace(str))
                return null;

            var in_backtick = false;
            var in_triple_backtick = false;
            var in_quote = false;
            var in_escape = false;
            var remove = new List<int>(str.Length - startPos);

            var i = startPos;
            for (; i < str.Length; i++)
                if (!char.IsWhiteSpace(str[i]))
                    break;
            startPos = i;

            var ep = -1;
            var sp = startPos;
            for (i = sp; i < str.Length; i++)
            {
                if (char.IsWhiteSpace(str[i]) && !in_quote && !in_triple_backtick && !in_backtick && !in_escape)
                    ep = i;

                if (str[i] == '\\')
                {
                    if (!in_escape && !in_backtick && !in_triple_backtick)
                    {
                        in_escape = true;
                        if (str.IndexOf("\\`", i) == i || str.IndexOf("\\\"", i) == i || str.IndexOf("\\\\", i) == i || (str.Length >= i && char.IsWhiteSpace(str[i + 1])))
                            remove.Add(i - sp);
                        i++;
                    }
                    else if ((in_backtick || in_triple_backtick) && str.IndexOf("\\`", i) == i)
                    {
                        in_escape = true;
                        remove.Add(i - sp);
                        i++;
                    }
                }

                if (str[i] == '`' && !in_escape)
                {
                    var tritick = str.IndexOf("```", i) == i;
                    if (in_triple_backtick && tritick)
                    {
                        in_triple_backtick = false;
                        i += 2;
                    }
                    else if (!in_backtick && tritick)
                    {
                        in_triple_backtick = true;
                        i += 2;
                    }

                    if (in_backtick && !tritick)
                        in_backtick = false;
                    else if (!in_triple_backtick && tritick)
                        in_backtick = true;
                }

                if (str[i] == '"' && !in_escape && !in_backtick && !in_triple_backtick)
                {
                    remove.Add(i - sp);

                    if (!in_quote)
                        in_quote = true;
                    else
                        in_quote = false;
                }

                if (in_escape)
                    in_escape = false;

                if (ep != -1)
                {
                    startPos = ep;
                    if (sp != ep)
                        return str.Substring(sp, ep - sp).CleanupString(remove);
                    return null;
                }
            }

            startPos = str.Length;
            if (startPos != sp)
                return str.Substring(sp).CleanupString(remove);
            return null;
        }

        /// <summary>
        /// Cleans up input string.
        /// </summary>
        /// <param name="s">String to clean up.</param>
        /// <param name="indices">Indices of things to remove.</param>
        /// <returns>Cleaned-up string.</returns>
        internal static string CleanupString(this string s, IList<int> indices)
        {
            if (!indices.Any())
                return s;

            var li = indices.Last();
            var ll = 1;
            for (var x = indices.Count - 2; x >= 0; x--)
            {
                if (li - indices[x] == ll)
                {
                    ll++;
                    continue;
                }

                s = s.Remove(li - ll + 1, ll);
                li = indices[x];
                ll = 1;
            }

            return s.Remove(li - ll + 1, ll);
        }

        /// <summary>
        /// Splits supplied text into sentence items.
        /// </summary>
        /// <param name="str">String to split.</param>
        /// <returns>Split sentence enumerator.</returns>
        public static IEnumerable<string> SplitSentence(this string str)
        {
            var findpos = 0;
            var argv = "";

            while (true)
            {
                argv = str.ExtractNextArgument(ref findpos);
                if (argv == null)
                    yield break;

                yield return argv;
            }
        }
    }
}
