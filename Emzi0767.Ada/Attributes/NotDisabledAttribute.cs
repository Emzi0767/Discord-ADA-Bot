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
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Emzi0767.Ada.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Emzi0767.Ada.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NotDisabledAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null)
                return Task.FromResult(false);
            
            var gid = (long)ctx.Guild.Id;

            var db = ctx.Services.GetService<DatabaseContext>();
            var cfg = db.GuildSettings.FirstOrDefault(x => x.GuildId == gid);
            if (cfg != null)
                return Task.FromResult(cfg.Settings.DisabledCommands.Contains(ctx.Command.QualifiedName.ToLowerInvariant()));
            else
                return Task.FromResult(true);
        }
    }
}
