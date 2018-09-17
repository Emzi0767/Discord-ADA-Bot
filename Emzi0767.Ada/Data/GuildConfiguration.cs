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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Emzi0767.Ada.Data
{
    /// <summary>
    /// Represents a complete per-guild configuration.
    /// </summary>
    public sealed class GuildConfiguration
    {
        /// <summary>
        /// Gets the configuration for invite blocker.
        /// </summary>
        [JsonProperty("invite_blocker")]
        public GuildConfigurationInviteBlocker InviteBlocker { get; private set; } = new GuildConfigurationInviteBlocker();

        /// <summary>
        /// Gets the configuration for role state.
        /// </summary>
        [JsonProperty("rolestate")]
        public GuildConfigurationRoleState RoleState { get; private set; } = new GuildConfigurationRoleState();

        /// <summary>
        /// Gets the configuration for moderation log.
        /// </summary>
        [JsonProperty("modlog")]
        public GuildConfigurationModLog ModLog { get; private set; } = new GuildConfigurationModLog();

        /// <summary>
        /// Gets the configuration for moderation log.
        /// </summary>
        [JsonProperty("joinlog")]
        public GuildConfigurationJoinLog JoinLog { get; private set; } = new GuildConfigurationJoinLog();

        /// <summary>
        /// Gets the configuration for muting.
        /// </summary>
        [JsonProperty("muting")]
        public GuildConfigurationMuting Muting { get; private set; } = new GuildConfigurationMuting();

        /// <summary>
        /// Gets the configuration for automatic join role.
        /// </summary>
        [JsonProperty("autorole")]
        public GuildConfigurationAutoRole AutoRole { get; private set; } = new GuildConfigurationAutoRole();

        /// <summary>
        /// Gets the configuration for click-based role.
        /// </summary>
        [JsonProperty("clickrole")]
        public GuildConfigurationClickRole ClickRole { get; private set; } = new GuildConfigurationClickRole();

        /// <summary>
        /// Gets the configuration for unhoister.
        /// </summary>
        [JsonProperty("unhoister")]
        public GuildConfigurationUnhoister Unhoister { get; private set; } = new GuildConfigurationUnhoister();

        /// <summary>
        /// Gets the collection of commands disabled by server moderators.
        /// </summary>
        [JsonProperty("disabled_commands")]
        public HashSet<string> DisabledCommands { get; private set; } = new HashSet<string>();
        
        /// <summary>
        /// Gets the configuration for Stallman module.
        /// </summary>
        [JsonProperty("stallman")]
        public GuildConfigurationStallman Stallman { get; private set; } = new GuildConfigurationStallman();
    }

    /// <summary>
    /// Represents a set of users and roles exempt from processing.
    /// </summary>
    public sealed class GuildConfigurationExemptions
    {
        /// <summary>
        /// Gets Ids of users exempt from being processed.
        /// </summary>
        [JsonProperty("users")]
        public HashSet<ulong> UserIds { get; private set; } = new HashSet<ulong>();

        /// <summary>
        /// Gets Ids of roles exempt from being processed.
        /// </summary>
        [JsonProperty("roles")]
        public HashSet<ulong> RoleIds { get; private set; } = new HashSet<ulong>();

        /// <summary>
        /// Gets Ids of channels exempt from being processed.
        /// </summary>
        [JsonProperty("channels")]
        public HashSet<ulong> ChannelIds { get; private set; } = new HashSet<ulong>();
    }

    /// <summary>
    /// Represents configuration for built-in invite blocker.
    /// </summary>
    public sealed class GuildConfigurationInviteBlocker
    {
        /// <summary>
        /// Gets or sets whether invite blocker is enabled.
        /// </summary>
        [JsonProperty("enabled")]
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets exemptions from invite blocker.
        /// </summary>
        [JsonProperty("exemptions")]
        public GuildConfigurationExemptions Exemptions { get; private set; } = new GuildConfigurationExemptions();
    }
    
    /// <summary>
    /// Represents configuration for role state.
    /// </summary>
    public sealed class GuildConfigurationRoleState
    {
        /// <summary>
        /// Gets or sets whether role state is enabled.
        /// </summary>
        [JsonProperty("enabled")]
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets exemptions from role state.
        /// </summary>
        [JsonProperty("exemptions")]
        public GuildConfigurationExemptions Exemptions { get; private set; } = new GuildConfigurationExemptions();
    }

    /// <summary>
    /// Represents configuration for moderation log.
    /// </summary>
    public sealed class GuildConfigurationModLog
    {
        /// <summary>
        /// Gets or sets whether moderation log is enabled.
        /// </summary>
        [JsonProperty("enabled")]
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the channel for moderation log. 0 means disabled.
        /// </summary>
        [JsonProperty("channel")]
        public ulong ChannelId { get; set; } = 0;
    }

    /// <summary>
    /// Represents configuration for join log.
    /// </summary>
    public sealed class GuildConfigurationJoinLog
    {
        /// <summary>
        /// Gets or sets whether join log is enabled.
        /// </summary>
        [JsonProperty("enabled")]
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the channel for join log. 0 means disabled.
        /// </summary>
        [JsonProperty("channel")]
        public ulong ChannelId { get; set; } = 0;
    }

    /// <summary>
    /// Represents configuration for muting module.
    /// </summary>
    public sealed class GuildConfigurationMuting
    {
        /// <summary>
        /// Gets or sets whether muting is enabled.
        /// </summary>
        [JsonProperty("enabled")]
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the role used to mute users. 0 means disabled.
        /// </summary>
        [JsonProperty("role")]
        public ulong RoleId { get; set; } = 0;
    }
    
    /// <summary>
    /// Represents configuration for automatic join roles.
    /// </summary>
    public sealed class GuildConfigurationAutoRole
    {
        /// <summary>
        /// Gets or sets whether autorole is enabled.
        /// </summary>
        [JsonProperty("enabled")]
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the roles assigned to users who join the server.
        /// </summary>
        [JsonProperty("roles")]
        public HashSet<ulong> RoleIds { get; private set; } = new HashSet<ulong>();
    }

    /// <summary>
    /// Represents configuration for click-based roles.
    /// </summary>
    public sealed class GuildConfigurationClickRole
    {
        /// <summary>
        /// Gets or sets whether clickrole is enabled.
        /// </summary>
        [JsonProperty("enabled")]
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets the mapping of reaction->role Id mapping for roles to be assigned on reaction click.
        /// </summary>
        [JsonProperty("add_roles")]
        public Dictionary<string, ulong> AddRoles { get; private set; } = new Dictionary<string, ulong>();

        /// <summary>
        /// Gets the mapping of reaction->role Id mapping for roles to be removed on reaction click.
        /// </summary>
        [JsonProperty("remove_roles")]
        public Dictionary<string, ulong> RemoveRoles { get; private set; } = new Dictionary<string, ulong>();
    }

    /// <summary>
    /// Represents configuration for unhoister module.
    /// </summary>
    public sealed class GuildConfigurationUnhoister
    {
        /// <summary>
        /// Gets or sets whether unhoister is enabled.
        /// </summary>
        [JsonProperty("enabled")]
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets what to rename the user to upon unhoisting. If set to null or empty, the user will just have the offending characters removed.
        /// </summary>
        [JsonProperty("rename_to")]
        public string RenameTo { get; set; } = null;
    }

    /// <summary>
    /// Represents configuration for Stallman module.
    /// </summary>
    public sealed class GuildConfigurationStallman
    {
        /// <summary>
        /// Gets or sets whether Stallman module is enabled.
        /// </summary>
        [JsonProperty("enabled")]
        public bool IsEnabled { get; set; } = false;
    }
}
